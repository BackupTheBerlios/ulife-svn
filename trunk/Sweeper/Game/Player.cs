using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public class Player
    {
        private int m_score;
        public int Score
        {
            get { return m_score; }
        }

        public void AddPoint()
        {
            m_score++;
        }
    }
}
