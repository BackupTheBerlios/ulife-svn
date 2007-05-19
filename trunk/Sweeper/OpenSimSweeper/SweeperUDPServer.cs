using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using OpenSim;
using OpenSim.Assets;
using OpenSim.Framework.Console;
using libsecondlife;
using libsecondlife.Packets;

namespace OpenSimSweeper
{
    public class SweeperUDPServer : UDPServer
    {
        private int maxNumPlayers = 2; //shouldn't be set to such low number, we should create a new PacketServer and Game (including world) for every 2 players
        private int currentNumPlayers = 0;
        private SweeperPacketServer _sweeperPacketServer;

        public SweeperUDPServer(int port, Grid gridServers, AssetCache assetCache, InventoryCache inventoryCache, RegionInfo _regionData, ConsoleBase console, AuthenticateSessionsBase authenticateClass)
            : base(port, gridServers, assetCache, inventoryCache, _regionData, true, false, console, authenticateClass)
        {
        }

        protected override void CreatePacketServer()
        {
            SweeperPacketServer packetServer = new SweeperPacketServer(this);
        }

        public override void RegisterPacketServer(PacketServer server)
        {
            this._sweeperPacketServer =(SweeperPacketServer) server;
        }

        protected override void OnReceivedData(IAsyncResult result)
        {
            ipeSender = new IPEndPoint(IPAddress.Any, 0);
            epSender = (EndPoint)ipeSender;
            Packet packet = null;
            int numBytes = Server.EndReceiveFrom(result, ref epSender);
            int packetEnd = numBytes - 1;

            packet = Packet.BuildPacket(RecvBuffer, ref packetEnd, ZeroBuffer);

            // do we already have a circuit for this endpoint
            if (this.clientCircuits.ContainsKey(epSender))
            {
                //if so then send packet to the packetserver
                this._sweeperPacketServer.ClientInPacket(this.clientCircuits[epSender], packet);
            }
            else if (packet.Type == PacketType.UseCircuitCode)
            {
                if (currentNumPlayers < maxNumPlayers)
                {
                    // new client
                    currentNumPlayers++;
                    this.AddNewClient(packet);
                }
                else
                {
                    //kill connection
                }
            }
            else
            { // invalid client
                Console.Error.WriteLine("UDPServer.cs:OnReceivedData() - WARNING: Got a packet from an invalid client - " + epSender.ToString());
            }

            Server.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref epSender, ReceivedData, null);
        }

        protected override void AddNewClient(Packet packet)
        {
            UseCircuitCodePacket useCircuit = (UseCircuitCodePacket)packet;
            this.clientCircuits.Add(epSender, useCircuit.CircuitCode.Code);
            bool isChildAgent = false;

            SweeperSimClient newuser = new SweeperSimClient(); //epSender, useCircuit, m_localWorld, _packetServer.ClientThreads, m_assetCache, m_gridServers.GridServer, this, m_inventoryCache, m_sandbox, isChildAgent, this.m_regionData, m_authenticateSessionsClass);
            this._sweeperPacketServer.SweeperClientThreads.Add(useCircuit.CircuitCode.Code, newuser);
        }
    }

}
