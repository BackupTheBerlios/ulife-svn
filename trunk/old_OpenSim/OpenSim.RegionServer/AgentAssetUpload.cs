using System;
using System.Collections.Generic;
using System.Text;
using OpenSim.Assets;
using OpenSim.Framework.Types;
using OpenSim.Framework.Utilities;
using libsecondlife;
using libsecondlife.Packets;

namespace OpenSim
{
    public class AgentAssetUpload
    {
        private Dictionary<LLUUID, AssetTransaction> transactions = new Dictionary<LLUUID, AssetTransaction>();
        private SimClient ourClient;
        private AssetCache m_assetCache;
        private InventoryCache m_inventoryCache;

        public AgentAssetUpload(SimClient client, AssetCache assetCache, InventoryCache inventoryCache)
        {
            this.ourClient = client;
            m_assetCache = assetCache;
            m_inventoryCache = inventoryCache;
        }

        public void AddUpload(LLUUID transactionID, AssetBase asset)
        {
            AssetTransaction upload = new AssetTransaction();
            lock (this.transactions)
            {
                upload.Asset = asset;
                upload.TransactionID = transactionID;
                this.transactions.Add(transactionID, upload);
            }
            if (upload.Asset.Data.Length > 2)
            {
                //is complete
                upload.UploadComplete = true;
                AssetUploadCompletePacket response = new AssetUploadCompletePacket();
                response.AssetBlock.Type = asset.Type;
                response.AssetBlock.Success = true;
                response.AssetBlock.UUID = transactionID.Combine(this.ourClient.SecureSessionID);
                this.ourClient.OutPacket(response);
                m_assetCache.AddAsset(asset);
            }
            else
            {
                upload.UploadComplete = false;
                upload.XferID = Util.GetNextXferID();
                RequestXferPacket xfer = new RequestXferPacket();
                xfer.XferID.ID = upload.XferID;
                xfer.XferID.VFileType = upload.Asset.Type;
                xfer.XferID.VFileID = transactionID.Combine(this.ourClient.SecureSessionID);
                xfer.XferID.FilePath = 0;
                xfer.XferID.Filename = new byte[0];
                this.ourClient.OutPacket(xfer);
            }

        }

        public AssetBase GetUpload(LLUUID transactionID)
        {
            if (this.transactions.ContainsKey(transactionID))
            {
                return this.transactions[transactionID].Asset;
            }

            return null;
        }

        public void HandleUploadPacket(AssetUploadRequestPacket pack, LLUUID assetID)
        {
           // Console.Write("asset upload request , type = " + pack.AssetBlock.Type.ToString());
            AssetBase asset = null;
            if (pack.AssetBlock.Type == 0)
            {

                //first packet for transaction
                asset = new AssetBase();
                asset.FullID = assetID;
                asset.Type = pack.AssetBlock.Type;
                asset.InvType = asset.Type;
                asset.Name = "UploadedTexture" + Util.RandomClass.Next(1, 1000).ToString("000");
                asset.Data = pack.AssetBlock.AssetData;


            }
            else if (pack.AssetBlock.Type == 13 | pack.AssetBlock.Type == 5 | pack.AssetBlock.Type == 7)
            {

                asset = new AssetBase();
                asset.FullID = assetID;
                //  Console.WriteLine("skin asset id is " + assetID.ToStringHyphenated());
                asset.Type = pack.AssetBlock.Type;
                asset.InvType = asset.Type;
                asset.Name = "NewClothing" + Util.RandomClass.Next(1, 1000).ToString("000");
                asset.Data = pack.AssetBlock.AssetData;


            }

            if (asset != null)
            {
                this.AddUpload(pack.AssetBlock.TransactionID, asset);
            }
            else
            {

                //currently we don't support this asset type 
                //so lets just tell the client that the upload is complete
                AssetUploadCompletePacket response = new AssetUploadCompletePacket();
                response.AssetBlock.Type = pack.AssetBlock.Type;
                response.AssetBlock.Success = true;
                response.AssetBlock.UUID = pack.AssetBlock.TransactionID.Combine(this.ourClient.SecureSessionID);
                this.ourClient.OutPacket(response);
            }

        }

        #region Xfer packet system for larger uploads

        public void HandleXferPacket(SendXferPacketPacket xferPacket)
        {
            lock (this.transactions)
            {
                foreach (AssetTransaction trans in this.transactions.Values)
                {
                    if (trans.XferID == xferPacket.XferID.ID)
                    {
                        if (trans.Asset.Data.Length > 1)
                        {
                            byte[] newArray = new byte[trans.Asset.Data.Length + xferPacket.DataPacket.Data.Length];
                            Array.Copy(trans.Asset.Data, 0, newArray, 0, trans.Asset.Data.Length);
                            Array.Copy(xferPacket.DataPacket.Data, 0, newArray, trans.Asset.Data.Length, xferPacket.DataPacket.Data.Length);
                            trans.Asset.Data = newArray;
                        }
                        else
                        {
                            byte[] newArray = new byte[xferPacket.DataPacket.Data.Length - 4];
                            Array.Copy(xferPacket.DataPacket.Data, 4, newArray, 0, xferPacket.DataPacket.Data.Length - 4);
                            trans.Asset.Data = newArray;
                        }

                        if ((xferPacket.XferID.Packet & 2147483648) != 0)
                        {
                            //end of transfer
                            trans.UploadComplete = true;
                            AssetUploadCompletePacket response = new AssetUploadCompletePacket();
                            response.AssetBlock.Type = trans.Asset.Type;
                            response.AssetBlock.Success = true;
                            response.AssetBlock.UUID = trans.TransactionID.Combine(this.ourClient.SecureSessionID);
                            this.ourClient.OutPacket(response);

                            m_assetCache.AddAsset(trans.Asset);
                            //check if we should add it to inventory 
                            if (trans.AddToInventory)
                            {
                                // m_assetCache.AddAsset(trans.Asset);
                                m_inventoryCache.AddNewInventoryItem(this.ourClient, trans.InventFolder, trans.Asset);
                            }


                        }
                        break;
                    }

                }
            }

            ConfirmXferPacketPacket confirmXfer = new ConfirmXferPacketPacket();
            confirmXfer.XferID.ID = xferPacket.XferID.ID;
            confirmXfer.XferID.Packet = xferPacket.XferID.Packet;
            this.ourClient.OutPacket(confirmXfer);
        }

        #endregion

        public AssetBase AddUploadToAssetCache(LLUUID transactionID)
        {
            AssetBase asset = null;
            if (this.transactions.ContainsKey(transactionID))
            {
                AssetTransaction trans = this.transactions[transactionID];
                if (trans.UploadComplete)
                {
                    m_assetCache.AddAsset(trans.Asset);
                    asset = trans.Asset;
                }
            }

            return asset;
        }

        public void CreateInventoryItem(CreateInventoryItemPacket packet)
        {
            if (this.transactions.ContainsKey(packet.InventoryBlock.TransactionID))
            {
                AssetTransaction trans = this.transactions[packet.InventoryBlock.TransactionID];
                trans.Asset.Description = Util.FieldToString(packet.InventoryBlock.Description);
                trans.Asset.Name = Util.FieldToString(packet.InventoryBlock.Name);
                trans.Asset.Type = packet.InventoryBlock.Type;
                trans.Asset.InvType = packet.InventoryBlock.InvType;
                if (trans.UploadComplete)
                {
                    //already complete so we can add it to the inventory
                    //m_assetCache.AddAsset(trans.Asset);
                    m_inventoryCache.AddNewInventoryItem(this.ourClient, packet.InventoryBlock.FolderID, trans.Asset);
                }
                else
                {
                    trans.AddToInventory = true;
                    trans.InventFolder = packet.InventoryBlock.FolderID;
                }
            }
        }

        private class AssetTransaction
        {
            public uint XferID;
            public AssetBase Asset;
            public bool AddToInventory;
            public LLUUID InventFolder = LLUUID.Zero;
            public bool UploadComplete = false;
            public LLUUID TransactionID = LLUUID.Zero;

            public AssetTransaction()
            {

            }
        }
    }
}
