using System;
using System.Collections.Generic;
using System.Text;
using OpenSim;
using libsecondlife.Packets;

namespace OpenSimSweeper
{
    public class SweeperPacketServer : PacketServer
    {
        public Dictionary<uint, SweeperSimClient> SweeperClientThreads = new Dictionary<uint, SweeperSimClient>();

        public SweeperPacketServer(OpenSimNetworkHandler networkHandler)
            : base(networkHandler)
        {

        }

        public override void ClientInPacket(uint circuitCode, Packet packet)
        {
            if (this.SweeperClientThreads.ContainsKey(circuitCode))
            {
                SweeperClientThreads[circuitCode].InPacket(packet);
            }
        }
    }
}
