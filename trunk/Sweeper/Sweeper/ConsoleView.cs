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

        public void SetCurrentPlayer(IPlayer player)
        {
            while (true)
            {
                Console.WriteLine("{0}, you're up - enter two-digit number: ", player.Name);
                string response = Console.ReadLine();
                int xycoord;
                
                if (int.TryParse(response, out xycoord))
                {
                    int x = xycoord/10;
                    int y = xycoord%10;

                    if( m_game.PlayerResponse(player, x, y) )
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

        public void PlayerOutOfTurn(IPlayer player)
        {
            Console.WriteLine( "{0}, it's not your turn!", player.Name);
        }

        public void GameTied()
        {
            Console.WriteLine("Game tied!");
        }
    }
}
