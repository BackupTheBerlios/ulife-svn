using System;
using System.Collections.Generic;
using System.Text;
using OpenSim.UserServer;
using Sweeper;
using System.Net;

namespace OpenSimSweeper
{
    public class SweeperLoginServer : LoginServer
    {
        private int maxPlayers = 2;  //for now only two , once we support multiple games this will increase
        private int currentNumPlayers = 0;

        public SweeperLoginServer(IPEndPoint ep, uint regX, uint regY)
            : base(ep.Address.ToString(), ep.Port, regX, regY, false)
        {
        }

        protected override bool Authenticate(string first, string last, string passwd)
        {
            if (currentNumPlayers < maxPlayers)
            {
                currentNumPlayers++;
                return true;
            }

            return false;
        }
    }
}
