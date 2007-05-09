using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public class ConsolePlayer : Player 
    {
        private string m_name;
        public string Name
        {
            get { return m_name; }
        }
	        
        public ConsolePlayer( string name )
        {
            m_name = name;
        }
    }
}
