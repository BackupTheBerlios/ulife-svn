using System;
using System.Collections.Generic;
using System.Text;
using Sweeper;

namespace OpenSimSweeper
{
    public class OpenSimGame : Game
    {
        static void Main(string[] args)
        {
            OpenSimGame game = new OpenSimGame( 2 );
        }

        private OpenSimGame(int players) : base(players)
        {

        }
    }
}
