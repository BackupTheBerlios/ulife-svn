using System;
using System.Collections.Generic;
using System.Text;

namespace Sweeper
{
    public interface IPlayer
    {
        int Score { get; }
        string Name { get; }
    }
}
