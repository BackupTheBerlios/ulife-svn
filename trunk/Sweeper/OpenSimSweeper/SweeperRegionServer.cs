using System;
using System.Collections.Generic;
using System.Text;
using OpenSim;
using System.Net;

namespace OpenSimSweeper
{
    public class SweeperRegionServer : OpenSimMain
    {
        private OpenSimGame m_game;

        public SweeperRegionServer( IPEndPoint ep, uint regX, uint regY, OpenSimGame game )
            : base(true, false, "", false, false, "")
        {
            m_game = game;
        }
        
        override public void StartUp()
        {
        }

        override protected void SetupLocalGridServers()
        {
        }

        override protected void SetupRemoteGridServers()
        {
        }

        override protected void SetupLocalWorld()
        {
        }

        override protected void SetupHttpListener()
        {
        }
        
        override protected void ConnectToRemoteGridServer()
        {
        }
        
        override public void Shutdown()
        {            
        }
    }       
}
