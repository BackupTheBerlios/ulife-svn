using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public class Slot : ISlot
    {
        private bool m_mine = false;
        public bool Mine
        {
            get { return m_mine; }
        }

        private bool m_hidden = true;
        public bool Hidden
        {
            get { return m_hidden; }
        }
	
        public Slot( bool mine )
        {
            m_mine = mine;
        }
        
        public void Uncover()
        {
            m_hidden = false;
        }
    }
}
