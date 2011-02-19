using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public delegate void UpdateTimerHandler(int nextTick);

    public interface IASLTimer
    {
        event UpdateTimerHandler UpdateTimer;
        void Tick();
    }
}
