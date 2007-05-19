using System;
using System.Collections.Generic;
using System.Text;
using OpenSim;
using System.Net;
using libsecondlife;
using libsecondlife.Packets;
using OpenSim.world;
//using OpenSim.Terrain;
using OpenSim.Framework.Interfaces;
using OpenSim.Framework.Types;
using OpenSim.UserServer;
using OpenSim.Assets;
using OpenSim.CAPS;
using OpenSim.Framework.Console;
//using OpenSim.Physics.Manager;
using Nwc.XmlRpc;
using OpenSim.Servers;
using OpenSim.GenericConfig;

namespace OpenSimSweeper
{
    public class SweeperRegionServer : RegionServerBase
    {
        private OpenSimGame m_game;
        private IPEndPoint m_endPoint;
        private uint m_regionX;
        private uint m_regionY;

        public SweeperRegionServer( IPEndPoint ep, uint regX, uint regY, OpenSimGame game )
            : base(true, false, "", false, false, "")
        {
            m_game = game;
            m_endPoint = ep;
            m_regionX = regX;
            m_regionY = regY;
        }
        
        override public void StartUp()
        {
            this.regionData = new RegionInfo();
            //need to set region info (should we even be using RegionInfo?)
           /* try
            {
                this.localConfig = new XmlConfig(m_config);
                this.localConfig.LoadData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            m_console.WriteLine(OpenSim.Framework.Console.LogPriority.LOW, "Main.cs:Startup() - Loading configuration");
            this.regionData.InitConfig(this.m_sandbox, this.localConfig);
            this.localConfig.Close();*/

            GridServers = new Grid();
            this.SetupLocalGridServers();
            AuthenticateSessionsLocal authen = new AuthenticateSessionsLocal();
            this.AuthenticateSessionsHandler = authen;

            startuptime = DateTime.Now;

            AssetCache = new AssetCache(GridServers.AssetServer);
            InventoryCache = new InventoryCache();
            m_udpServer = new UDPServer(this.regionData.IPListenPort, this.GridServers, this.AssetCache, this.InventoryCache, this.regionData, this.m_sandbox, this.user_accounts, this.m_console, this.AuthenticateSessionsHandler);

            this.SetupLocalWorld();
            this.SetupHttpListener();

            SweeperLoginServer loginServer = new SweeperLoginServer(m_endPoint, m_regionX, m_regionY);
            loginServer.Startup();
            loginServer.SetSessionHandler(((AuthenticateSessionsLocal)this.AuthenticateSessionsHandler).AddNewSession);
            httpServer.AddXmlRPCHandler("login_to_simulator", loginServer.XmlRpcLoginMethod);

            AdminWebFront adminWebFront = new AdminWebFront("Admin", LocalWorld, InventoryCache, null);
            adminWebFront.LoadMethods(httpServer);

            //Start http server
            m_console.WriteLine(OpenSim.Framework.Console.LogPriority.LOW, "Main.cs:Startup() - Starting HTTP server");
            httpServer.Start();

            // Start UDP server
            this.m_udpServer.ServerListener();

        }

        override protected void SetupLocalGridServers()
        {
            GridServers.AssetDll = "OpenSim.GridInterfaces.Local.dll";
            GridServers.GridDll = "OpenSim.GridInterfaces.Local.dll";

            m_console.WriteLine(OpenSim.Framework.Console.LogPriority.LOW, "Starting in Sandbox mode");

            try
            {
                GridServers.Initialise();
            }
            catch (Exception e)
            {
                m_console.WriteLine(OpenSim.Framework.Console.LogPriority.HIGH, e.Message + "\nSorry, could not setup the grid interface");
                Environment.Exit(1);
            }
        }

        override protected void SetupRemoteGridServers()
        {
        }

        override protected void SetupLocalWorld()
        {
        }

        override protected void SetupHttpListener()
        {
            httpServer = new BaseHttpServer(regionData.IPListenPort);
        }
        
        override protected void ConnectToRemoteGridServer()
        {
        }
        
        public void Shutdown()
        {            
        }
    }       
}
