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
        private OpenSimGame m_game;

        public SweeperLoginServer(IPEndPoint ep, uint regX, uint regY, OpenSimGame game)
            : base(null, ep.Address.ToString(), ep.Port, regX, regY, false)
        {
            m_game = game;
        }

        protected override bool Authenticate(string first, string last, string passwd)
        {
            SweeperPlayer player = new SweeperPlayer();
            return m_game.TryAddPlayer(player);
        }
    }
}
