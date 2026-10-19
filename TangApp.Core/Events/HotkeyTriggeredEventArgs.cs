using System;

namespace TangApp.Core.Events
{
    // سجل (Record) بسيط ومتوافق مع جميع المنصات لتمثيل إحداثيات الماوس
    public record MousePosition(int X, int Y);

    public class HotkeyTriggeredEventArgs : EventArgs
    {
        public MousePosition Position { get; }

        public HotkeyTriggeredEventArgs(int x, int y)
        {
            Position = new MousePosition(x, y);
        }
    }
}