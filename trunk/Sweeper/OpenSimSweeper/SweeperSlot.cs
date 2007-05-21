using System;
using System.Collections.Generic;
using System.Text;
using OpenSim.world;
using Sweeper;

namespace OpenSimSweeper
{
    public class SweeperSlot:Slot
    {
        Primitive2 m_prim;
        
        public SweeperSlot(Primitive2 prim, bool mine) : base( mine )
        {
            m_prim = prim;

            UpdateShape();
        }

        private void UpdateShape()
        {
            
        }
                
    }
}
