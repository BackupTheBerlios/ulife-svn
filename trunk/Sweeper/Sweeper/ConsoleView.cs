using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public class ConsoleView : IView
    {
        private Game m_game;

        public ConsoleView(Game game)
        {
            m_game = game;
        }

        public void StartGame()
        {
            Console.WriteLine("Game starting");
        }

        public void SetCurrentPlayer(Player player)
        {
            ConsolePlayer consolePlayer = player as ConsolePlayer;
            
            while (true)
            {
                Console.WriteLine("{0}, you're up - enter two-digit number: ", consolePlayer.Name);
                string response = Console.ReadLine();
                int xycoord;
                
                if (int.TryParse(response, out xycoord))
                {
                    int x = xycoord/10;
                    int y = xycoord%10;

                    if( m_game.PlayerResponse(consolePlayer, x, y) )
                    {                        
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Bad input, try again!");
                }
            }
        }

        public void PlayerOutOfTurn(Player player)
        {
            ConsolePlayer consolePlayer = player as ConsolePlayer;
            Console.WriteLine("{0}, it's not your turn!", consolePlayer.Name);
        }

        public void GameTied()
        {
            Console.WriteLine("Game tied!");
        }

        public void PointAdded(Player player)
        {
            ConsolePlayer consolePlayer = player as ConsolePlayer;            
            Console.WriteLine("{0} got a point, score is now {1}", consolePlayer.Name, consolePlayer.Score );
        }

        public Slot CreateSlot(bool mine)
        {
            return new Slot( mine );
        }

        public void ShowBoard(List<List<Slot>> board)
        {
            Console.WriteLine("--- BOARD ---");
            Console.WriteLine("Y 0 1 2 3 4 5 6 7 8 9 ");
            for (int y = 0; y < 10; y++)
            {
                Console.Write("{0}: ", y);
                for (int x = 0; x < 10;x++ )
                {
                    Slot slot = board[y][x];
                    char c = 'M';
                    
                    if( !slot.Mine )
                    {
                        c++;
                    }
                    
                    if( slot.Hidden )
                    {
                        c += (char)2;
                    }
                    
                    Console.Write( "{0} ", c );
                }
                Console.WriteLine( );
            }
            Console.WriteLine("Legend: M = Uncovered mine, N = Covered Mine, O = Uncovered empty, P = Covered empty ");
        }
    }
}
