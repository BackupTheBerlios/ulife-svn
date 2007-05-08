using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public interface IView
    {
        void StartGame();
        void SetCurrentPlayer(IPlayer player);

        void PlayerOutOfTurn(IPlayer player);

        void GameTied();
    }
}
