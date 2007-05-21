using System;
using System.Collections.Generic;
using System.Text;
using Sweeper;
using OpenSim.world;
using libsecondlife.Packets;
using OpenSim;

namespace OpenSimSweeper
{
    public class OpenSimView : IView
    {
        private OpenSimGame m_game;
        private World m_world;
        
        public OpenSimView(OpenSimGame game, World world)
        {
            m_game = game;
            m_world = world;           
        }

        public void StartGame()
        {
            ChatFromViewerPacket chatPacket = new ChatFromViewerPacket();
            chatPacket.ChatData.Type = 2;
            
            Encoding enc = System.Text.Encoding.ASCII;
            
            chatPacket.ChatData.Message = enc.GetBytes("Game started.");

            m_world.SimChat(null, chatPacket);            
        }

        public void SetCurrentPlayer(Player player)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void PlayerOutOfTurn(Player player)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GameTied()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void PointAdded(Player player)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Slot CreateSlot(bool mine)
        {
            m_world.AddNewPrim( );
        }

        public void ShowBoard(List<List<ISlot>> board)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
