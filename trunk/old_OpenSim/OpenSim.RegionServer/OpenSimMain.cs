/*
Copyright (c) OpenSim project, http://osgrid.org/

* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the <organization> nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY <copyright holder> ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
using OpenSim.world;
using OpenSim.Terrain;
using OpenSim.Framework.Interfaces;
using OpenSim.Framework.Types;
using OpenSim.UserServer;
using OpenSim.Assets;
using OpenSim.CAPS;
using OpenSim.Framework.Console;
using OpenSim.Physics.Manager;
using Nwc.XmlRpc;
using OpenSim.Servers;
using OpenSim.GenericConfig;

namespace OpenSim
{

    public class OpenSimMain : OpenSimNetworkHandler, conscmd_callback
    {
        private IGenericConfig localConfig;
        private PhysicsManager physManager;
        private Grid GridServers;
        private PacketServer _packetServer;
        private World LocalWorld;
        private AssetCache AssetCache;
        private InventoryCache InventoryCache;
        private Dictionary<EndPoint, uint> clientCircuits = new Dictionary<EndPoint, uint>();
        private DateTime startuptime;
        private RegionInfo regionData;

        public Socket Server;
        private IPEndPoint ServerIncoming;
        private byte[] RecvBuffer = new byte[4096];
        private byte[] ZeroBuffer = new byte[8192];
        private IPEndPoint ipeSender;
        private EndPoint epSender;
        private AsyncCallback ReceivedData;

        private System.Timers.Timer m_heartbeatTimer = new System.Timers.Timer();
        public string m_physicsEngine;
        public bool m_sandbox = false;
        public bool m_loginserver;
        public bool user_accounts = false;
        public bool gridLocalAsset = false;
        private bool configFileSetup = false;
        public string m_config;

        protected ConsoleBase m_console;

        public OpenSimMain(bool sandBoxMode, bool startLoginServer, string physicsEngine, bool useConfigFile, bool verbose, string configFile)
        {
            this.configFileSetup = useConfigFile;
            m_sandbox = sandBoxMode;
            m_loginserver = startLoginServer;
            m_physicsEngine = physicsEngine;
            m_config = configFile;

            m_console = new ConsoleBase("region-console-" + Guid.NewGuid().ToString() + ".log", "Region", this, verbose);
            OpenSim.Framework.Console.MainConsole.Instance = m_console;
        }

        /// <summary>
        /// Performs initialisation of the world, such as loading configuration from disk.
        /// </summary>
        public virtual void StartUp()
        {
            this.regionData = new RegionInfo();
            try
            {
                this.localConfig = new XmlConfig(m_config);
                this.localConfig.LoadData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
            m_console.WriteLine("Main.cs:Startup() - Loading configuration");
            this.regionData.InitConfig(this.m_sandbox, this.localConfig);
            this.localConfig.Close();//for now we can close it as no other classes read from it , but this should change


            GridServers = new Grid();
            if (m_sandbox)
            {
                GridServers.AssetDll = "OpenSim.GridInterfaces.Local.dll";
                GridServers.GridDll = "OpenSim.GridInterfaces.Local.dll";

                m_console.WriteLine("Starting in Sandbox mode");
            }
            else
            {
                if (this.gridLocalAsset)
                {
                    GridServers.AssetDll = "OpenSim.GridInterfaces.Local.dll";
                }
                else
                {
                    GridServers.AssetDll = "OpenSim.GridInterfaces.Remote.dll";
                }
                GridServers.GridDll = "OpenSim.GridInterfaces.Remote.dll";

                m_console.WriteLine("Starting in Grid mode");
            }

            try
            {
                GridServers.Initialise();
            }
            catch (Exception e)
            {
                m_console.WriteLine(e.Message + "\nSorry, could not setup the grid interface");
                Environment.Exit(1);
            }

            startuptime = DateTime.Now;

            try
            {
                AssetCache = new AssetCache(GridServers.AssetServer);
                InventoryCache = new InventoryCache();
            }
            catch (Exception e)
            {
                m_console.WriteLine(e.Message + "\nSorry, could not setup local cache");
                Environment.Exit(1);
            }

            //should be passing a IGenericConfig object to these so they can read the config data they want from it
            GridServers.AssetServer.SetServerInfo(regionData.AssetURL, regionData.AssetSendKey);
            IGridServer gridServer = GridServers.GridServer;
            gridServer.SetServerInfo(regionData.GridURL, regionData.GridSendKey, regionData.GridRecvKey);

            this.physManager = new OpenSim.Physics.Manager.PhysicsManager();
            this.physManager.LoadPlugins();

            this.CustomiseStartup();

            m_console.WriteLine("Main.cs:Startup() - Initialising HTTP server");
            // HttpServer = new SimCAPSHTTPServer(GridServers.GridServer, Cfg.IPListenPort);

            BaseHttpServer httpServer = new BaseHttpServer(regionData.IPListenPort);
            LoginServer loginServer = null;
            LoginServer adminLoginServer = null;

            bool sandBoxWithLoginServer = m_loginserver && m_sandbox;
            if (sandBoxWithLoginServer)
            {
                loginServer = new LoginServer(gridServer, regionData.IPListenAddr, regionData.IPListenPort,regionData.RegionLocX, regionData.RegionLocY, this.user_accounts);
                loginServer.Startup();

                if (user_accounts)
                {
                    //sandbox mode with loginserver using accounts
                    this.GridServers.UserServer = loginServer;
                    adminLoginServer = loginServer;

                    httpServer.AddXmlRPCHandler("login_to_simulator", loginServer.LocalUserManager.XmlRpcLoginMethod);
                }
                else
                {
                    //sandbox mode with loginserver not using accounts
                    httpServer.AddXmlRPCHandler("login_to_simulator", loginServer.XmlRpcLoginMethod);
                }
            }

            AdminWebFront adminWebFront = new AdminWebFront("Admin", LocalWorld, InventoryCache, adminLoginServer);
            adminWebFront.LoadMethods(httpServer);

            m_console.WriteLine("Main.cs:Startup() - Starting HTTP server");
            httpServer.Start();

            MainServerListener();

            m_heartbeatTimer.Enabled = true;
            m_heartbeatTimer.Interval = 100;
            m_heartbeatTimer.Elapsed += new ElapsedEventHandler(this.Heartbeat);
        }

        protected virtual void CustomiseStartup()
        {
            PacketServer packetServer = new PacketServer(this);

            m_console.WriteLine("Main.cs:Startup() - We are " + regionData.RegionName + " at " + regionData.RegionLocX.ToString() + "," + regionData.RegionLocY.ToString());
            m_console.WriteLine("Initialising world");
            LocalWorld = new World(this._packetServer.ClientThreads, regionData.RegionHandle, regionData.RegionName);
            LocalWorld.InventoryCache = InventoryCache;
            LocalWorld.AssetCache = AssetCache;

            this._packetServer.LocalWorld = LocalWorld;
            this._packetServer.RegisterClientPacketHandlers();

            LocalWorld.m_datastore = this.regionData.DataStore;

            LocalWorld.LoadStorageDLL("OpenSim.Storage.LocalStorageDb4o.dll"); //all these dll names shouldn't be hard coded.
            LocalWorld.LoadWorldMap();

            m_console.WriteLine("Main.cs:Startup() - Starting up messaging system");
            LocalWorld.PhysScene = this.physManager.GetPhysicsScene(this.m_physicsEngine);
            LocalWorld.PhysScene.SetTerrain(LocalWorld.Terrain.getHeights1D());
            LocalWorld.LoadPrimsFromStorage();
        }

        protected virtual void SetupFromConfigFile(IGenericConfig configData)
        {
           
        }

        protected virtual void OnReceivedData(IAsyncResult result)
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
                this._packetServer.ClientInPacket(this.clientCircuits[epSender], packet);
            }
            else if (packet.Type == PacketType.UseCircuitCode)
            { 
                // new client
                this.AddNewClient(packet);
            }
            else
            { // invalid client
                Console.Error.WriteLine("Main.cs:OnReceivedData() - WARNING: Got a packet from an invalid client - " + epSender.ToString());
            }

            Server.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref epSender, ReceivedData, null);
        }

        protected virtual void AddNewClient(Packet packet)
        {
            UseCircuitCodePacket useCircuit = (UseCircuitCodePacket)packet;
            this.clientCircuits.Add(epSender, useCircuit.CircuitCode.Code);
            bool isChildAgent = false;

            SimClient newuser = new SimClient(epSender, useCircuit, LocalWorld, _packetServer.ClientThreads, AssetCache, GridServers.GridServer, this, InventoryCache, m_sandbox, isChildAgent, this.regionData);
            if ((this.GridServers.UserServer != null) && (user_accounts))
            {
                newuser.UserServer = this.GridServers.UserServer;
            }
            //OpenSimRoot.Instance.ClientThreads.Add(epSender, newuser);
            this._packetServer.ClientThreads.Add(useCircuit.CircuitCode.Code, newuser);  
        }

        private void MainServerListener()
        {
            m_console.WriteLine("Main.cs:MainServerListener() - New thread started");
            m_console.WriteLine("Main.cs:MainServerListener() - Opening UDP socket on " + regionData.IPListenAddr + ":" + regionData.IPListenPort);

            ServerIncoming = new IPEndPoint(IPAddress.Any, regionData.IPListenPort);
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Server.Bind(ServerIncoming);

            m_console.WriteLine("Main.cs:MainServerListener() - UDP socket bound, getting ready to listen");

            ipeSender = new IPEndPoint(IPAddress.Any, 0);
            epSender = (EndPoint)ipeSender;
            ReceivedData = new AsyncCallback(this.OnReceivedData);
            Server.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref epSender, ReceivedData, null);

            m_console.WriteLine("Main.cs:MainServerListener() - Listening...");

        }

        public void RegisterPacketServer(PacketServer server)
        {
            this._packetServer = server;
        }

        public virtual void SendPacketTo(byte[] buffer, int size, SocketFlags flags, uint circuitcode)//EndPoint packetSender)
        {
            // find the endpoint for this circuit
            EndPoint sendto = null;
            foreach (KeyValuePair<EndPoint, uint> p in this.clientCircuits)
            {
                if (p.Value == circuitcode)
                {
                    sendto = p.Key;
                    break;
                }
            }
            if (sendto != null)
            {
                //we found the endpoint so send the packet to it
                this.Server.SendTo(buffer, size, flags, sendto);
            }
        }

        public virtual void RemoveClientCircuit(uint circuitcode)
        {
            foreach (KeyValuePair<EndPoint, uint> p in this.clientCircuits)
            {
                if (p.Value == circuitcode)
                {
                    this.clientCircuits.Remove(p.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// Performs any last-minute sanity checking and shuts down the region server
        /// </summary>
        public virtual void Shutdown()
        {
            m_console.WriteLine("Main.cs:Shutdown() - Closing all threads");
            m_console.WriteLine("Main.cs:Shutdown() - Killing listener thread");
            m_console.WriteLine("Main.cs:Shutdown() - Killing clients");
            // IMPLEMENT THIS
            m_console.WriteLine("Main.cs:Shutdown() - Closing console and terminating");
            LocalWorld.Close();
            GridServers.Close();
            m_console.Close();
            Environment.Exit(0);
        }

        /// <summary>
        /// Performs per-frame updates regularly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Heartbeat(object sender, System.EventArgs e)
        {
            LocalWorld.Update();
        }

        /// <summary>
        /// Runs commands issued by the server console from the operator
        /// </summary>
        /// <param name="command">The first argument of the parameter (the command)</param>
        /// <param name="cmdparams">Additional arguments passed to the command</param>
        public void RunCmd(string command, string[] cmdparams)
        {
            switch (command)
            {
                case "help":
                    m_console.WriteLine("show users - show info about connected users");
                    m_console.WriteLine("shutdown - disconnect all clients and shutdown");
                    break;

                case "show":
                    Show(cmdparams[0]);
                    break;

                case "terrain":
                    string result = "";
                    if (!LocalWorld.Terrain.RunTerrainCmd(cmdparams, ref result))
                    {
                        m_console.WriteLine(result);
                    }
                    break;

                case "shutdown":
                    Shutdown();
                    break;

                default:
                    m_console.WriteLine("Unknown command");
                    break;
            }
        }

        /// <summary>
        /// Outputs to the console information about the region
        /// </summary>
        /// <param name="ShowWhat">What information to display (valid arguments are "uptime", "users")</param>
        public void Show(string ShowWhat)
        {
            switch (ShowWhat)
            {
                case "uptime":
                    m_console.WriteLine("OpenSim has been running since " + startuptime.ToString());
                    m_console.WriteLine("That is " + (DateTime.Now - startuptime).ToString());
                    break;
                case "users":
                    OpenSim.world.Avatar TempAv;
                    m_console.WriteLine(String.Format("{0,-16}{1,-16}{2,-25}{3,-25}{4,-16}{5,-16}", "Firstname", "Lastname", "Agent ID", "Session ID", "Circuit", "IP"));
                    foreach (libsecondlife.LLUUID UUID in LocalWorld.Entities.Keys)
                    {
                        if (LocalWorld.Entities[UUID].ToString() == "OpenSim.world.Avatar")
                        {
                            TempAv = (OpenSim.world.Avatar)LocalWorld.Entities[UUID];
                            m_console.WriteLine(String.Format("{0,-16}{1,-16}{2,-25}{3,-25}{4,-16},{5,-16}", TempAv.firstname, TempAv.lastname, UUID, TempAv.ControllingClient.SessionID, TempAv.ControllingClient.CircuitCode, TempAv.ControllingClient.userEP.ToString()));
                        }
                    }
                    break;
            }
        }
    }


}
