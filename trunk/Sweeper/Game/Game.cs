using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public class Game
    {
        private List<Player> m_players;

        private int m_currentPlayer;
        private int m_maxPlayers;
        protected IView m_view;
        private bool m_gameOver;
        private int m_currentTurn;

        private List<List<Slot>> m_board;
        
        public Game(int maxPlayers)
        {
            m_maxPlayers = maxPlayers;
            m_players = new List<Player>();
        }

        public bool TryAddPlayer(Player player)
        {
            if (m_players.Count == m_maxPlayers)
            {
                return false;
            }

            m_players.Add(player);

            return true;
        }

        protected void StartGame()
        {
            m_view.StartGame();

            InitializeBoard();
            AddMines(25);

            ShowBoard( );
            
            while (!m_gameOver)
            {
                InitializeTurn();
            }
        }

        private void ShowBoard()
        {
            m_view.ShowBoard(m_board);
        }

        private void AddMines(int mines)
        {
            Random random = new Random( );
            
            for(int i=0;i<mines;i++)
            {
                while (true)
                {
                    int x = random.Next(10);
                    int y = random.Next(10);
                    
                    if( !m_board[y][x].Mine )
                    {
                        m_board[y][x] = m_view.CreateSlot( true );
                        break;
                    }
                }                
            }
        }

        private void InitializeBoard()
        {
            m_board = new List<List<Slot>>( 10 );
            
            for(int y=0;y<10;y++)
            {
                List<Slot> row = new List<Slot>();

                for (int x = 0; x < 10;x++ )
                {
                    row.Add( m_view.CreateSlot( false ) );
                }

                m_board.Add(row);
            }
        }

        private void InitializeTurn()
        {
            m_view.SetCurrentPlayer(m_players[m_currentPlayer]);
        }

        public bool PlayerResponse(Player player, int x, int y)
        {
            if (player != m_players[m_currentPlayer])
            {
                m_view.PlayerOutOfTurn(player);
                return false;
            }

            Slot slot = m_board[y][x];
           
            if( slot.Hidden )
            {
                slot.Uncover();
                
                if( slot.Mine )
                {
                    // Yay! Mine! Give point and continue
                    player.AddPoint();
                    m_view.PointAdded(player);
                }
                else
                {
                    // Not a mine, so hand over to next player
                    m_currentPlayer = (m_currentPlayer + 1) % m_maxPlayers;
                }
            }
            else
            {
                // Umm... already uncovered - do nothing.
            }
            

            // This ends game after 6 turns just to demonstrate an ending condition. (Normally, someone would win or something)

            m_currentTurn++;

            if (m_currentTurn >= 6)
            {
                m_view.GameTied();
                m_gameOver = true;
            }

            return true;
        }
    }
}
