using System;
using System.Collections.Generic;
using System.Text;
using Sweeper;
using OpenSim;
using OpenSim.world;
using System.Net;

namespace OpenSimSweeper
{
    public class OpenSimGame : Game
    {
        static void Main(string[] args)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 9000);
            uint regX = 996;
            uint regY = 997;
            
            OpenSimGame game = new OpenSimGame( 2 );
            SweeperRegionServer regionServer = new SweeperRegionServer(ep, regX, regY, game);
            regionServer.StartUp();
            
            SweeperLoginServer loginServer = new SweeperLoginServer( ep, regX, regY, game );
            loginServer.Startup();

            World world = regionServer.LocalWorld;
            
            OpenSimView view = new OpenSimView( game, world );
        }

        private OpenSimGame(int players) : base(players)
        {

        }
    }
}
