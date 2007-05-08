using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    class ConsoleGame : Game
    {
        static void Main(string[] args)
        {
            ConsoleGame game = new ConsoleGame( 2  );
            game.TryAddPlayer(new ConsolePlayer("Stefan"));
            game.TryAddPlayer(new ConsolePlayer("Michael"));
            game.StartGame();
        }
                
        private ConsoleGame( int players ) : base( players )
        {
            m_view = new ConsoleView( this );
        }
    }
}
