using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IASLTimer : IASL
    {
        event Action<int> RequestNextTimerTick;
        void Tick(int elapsedTime);
        void SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata);
    }
}
