using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using OpenSim.Framework.Interfaces;
using OpenSim.Framework.Utilities;
using libsecondlife;

namespace OpenSim
{
    public class RegionInfo  // could inherit from SimProfileBase
    {
        public LLUUID SimUUID;
        public string RegionName;
        public uint RegionLocX;
        public uint RegionLocY;
        public ulong RegionHandle;
        public ushort RegionWaterHeight = 20;
        public bool RegionTerraform = true;

        public int IPListenPort;
        public string IPListenAddr;

        //following should be removed and the GenericConfig object passed around,
        //so each class (AssetServer, GridServer etc) can access what config data they want 
        public string AssetURL = "";
        public string AssetSendKey = "";

        public string GridURL = "";
        public string GridSendKey = "";
        public string GridRecvKey = "";
        public string UserURL = "";
        public string UserSendKey = "";
        public string UserRecvKey = "";
        private bool isSandbox;

        public string DataStore;

        public RegionInfo()
        {

        }

        public void InitConfig(bool sandboxMode, IGenericConfig configData)
        {
            this.isSandbox = sandboxMode;
            try
            {
                // Sim UUID
                string attri = "";
                attri = configData.GetAttribute("SimUUID");
                if (attri == "")
                {
                    this.SimUUID = LLUUID.Random();
                    configData.SetAttribute("SimUUID", this.SimUUID.ToString());
                }
                else
                {
                    this.SimUUID = new LLUUID(attri);
                }

                // Sim name
                attri = "";
                attri = configData.GetAttribute("SimName");
                if (attri == "")
                {
                    this.RegionName = OpenSim.Framework.Console.MainConsole.Instance.CmdPrompt("Name", "OpenSim test");
                    configData.SetAttribute("SimName", this.RegionName);
                }
                else
                {
                    this.RegionName = attri;
                }
                // Sim/Grid location X
                attri = "";
                attri = configData.GetAttribute("SimLocationX");
                if (attri == "")
                {
                    string location = OpenSim.Framework.Console.MainConsole.Instance.CmdPrompt("Grid Location X", "997");
                    configData.SetAttribute("SimLocationX", location);
                    this.RegionLocX = (uint)Convert.ToUInt32(location);
                }
                else
                {
                    this.RegionLocX = (uint)Convert.ToUInt32(attri);
                }
                // Sim/Grid location Y
                attri = "";
                attri = configData.GetAttribute("SimLocationY");
                if (attri == "")
                {
                    string location = OpenSim.Framework.Console.MainConsole.Instance.CmdPrompt("Grid Location Y", "996");
                    configData.SetAttribute("SimLocationY", location);
                    this.RegionLocY = (uint)Convert.ToUInt32(location);
                }
                else
                {
                    this.RegionLocY = (uint)Convert.ToUInt32(attri);
                }

                // Local storage datastore
                attri = "";
                attri = configData.GetAttribute("Datastore");
                if (attri == "")
                {
                    string datastore = OpenSim.Framework.Console.MainConsole.Instance.CmdPrompt("Filename for local storage", "localworld.yap");
                    configData.SetAttribute("Datastore", datastore);
                    this.DataStore = datastore;
                }
                else
                {
                    this.DataStore = attri;
                }

                //Sim Listen Port
                attri = "";
                attri = configData.GetAttribute("SimListenPort");
                if (attri == "")
                {
                    string port = OpenSim.Framework.Console.MainConsole.Instance.CmdPrompt("UDP port for client connections", "9000");
                    configData.SetAttribute("SimListenPort", port);
                    this.IPListenPort = Convert.ToInt32(port);
                }
                else
                {
                    this.IPListenPort = Convert.ToInt32(attri);
                }
                //Sim Listen Address
                attri = "";
                attri = configData.GetAttribute("SimListenAddress");
                if (attri == "")
                {
                    this.IPListenAddr = OpenSim.Framework.Console.MainConsole.Instance.CmdPrompt("IP Address to listen on for client connections", "127.0.0.1");
                    configData.SetAttribute("SimListenAddress", this.IPListenAddr);
                }
                else
                {
                    this.IPListenAddr = attri;
                }
                
                this.RegionHandle = Util.UIntsToLong((RegionLocX * 256), (RegionLocY * 256));
                
                configData.Commit();
            }
            catch (Exception e)
            {
                OpenSim.Framework.Console.MainConsole.Instance.WriteLine("Config.cs:InitConfig() - Exception occured");
                OpenSim.Framework.Console.MainConsole.Instance.WriteLine(e.ToString());
            }

            OpenSim.Framework.Console.MainConsole.Instance.WriteLine("Sim settings loaded:");
            OpenSim.Framework.Console.MainConsole.Instance.WriteLine("UUID: " + this.SimUUID.ToStringHyphenated());
            OpenSim.Framework.Console.MainConsole.Instance.WriteLine("Name: " + this.RegionName);
            OpenSim.Framework.Console.MainConsole.Instance.WriteLine("Region Location: [" + this.RegionLocX.ToString() + "," + this.RegionLocY + "]");
            OpenSim.Framework.Console.MainConsole.Instance.WriteLine("Region Handle: " + this.RegionHandle.ToString());
            OpenSim.Framework.Console.MainConsole.Instance.WriteLine("Listening on IP: " + this.IPListenAddr + ":" + this.IPListenPort);
            
        }
    }
}
