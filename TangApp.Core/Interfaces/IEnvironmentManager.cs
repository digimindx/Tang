using System;
using TangApp.Core.Events;

namespace TangApp.Core.Interfaces
{
    public interface IEnvironmentManager
    {
        void Initialize();
        void Shutdown();
        event EventHandler<HotkeyTriggeredEventArgs> ActionTriggered;
    }
}