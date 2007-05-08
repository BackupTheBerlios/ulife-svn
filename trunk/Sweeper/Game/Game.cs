using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public class Game
    {
        private List<IPlayer> m_players;

        private int m_currentPlayer;
        private int m_maxPlayers;
        protected IView m_view;
        private bool m_gameOver;
        private int m_currentTurn;

        public Game(int maxPlayers)
        {
            m_maxPlayers = maxPlayers;
            m_players = new List<IPlayer>();
        }

        public bool TryAddPlayer(IPlayer player)
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

            while (!m_gameOver)
            {
                InitializeTurn();
            }
        }

        private void InitializeTurn()
        {
            m_view.SetCurrentPlayer(m_players[m_currentPlayer]);
        }

        public bool PlayerResponse(IPlayer player, int x, int y)
        {
            if (player != m_players[m_currentPlayer])
            {
                m_view.PlayerOutOfTurn(player);
                return false;
            }

            m_currentPlayer = (m_currentPlayer + 1) % m_maxPlayers;

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
