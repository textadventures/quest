using System;
using System.Collections.Generic;

namespace QuestViva.Common
{
    public interface IGameTimer : IGame
    {
        event Action<int>? RequestNextTimerTick;
        void Tick(int elapsedTime);
        void SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata);
    }
}
