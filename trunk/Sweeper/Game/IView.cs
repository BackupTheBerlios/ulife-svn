using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public interface IView
    {
        void StartGame();
        void SetCurrentPlayer(Player player);

        void PlayerOutOfTurn(Player player);

        void GameTied();

        void PointAdded(Player player);

        Slot CreateSlot( bool mine );

        void ShowBoard(List<List<Slot>> board);
    }
}
