using net.azirale.geosharer.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.azirale.geosharer.console
{
    static class Messaging
    {
        public static void Send(string text)
        {
            Console.WriteLine(text);
        }

        public static void ReceiveMessage(object sender, MessagePacket msg)
        {
            Send(DateTime.Now.ToString("HH:mm:ss> ") + msg.Text);
        }



        #region Progress Bar
        private static int cursorLeft = 0;
        private static int cursorTop = 0;
        private static DateTime lastUpdate = DateTime.Now;
        private static System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
        public static void ReceiveProgress(object sender, ProgressPacket progress)
        {
            lock (clock) { if (clock.ElapsedMilliseconds < 10 && clock.IsRunning) return; }
            
            cursorLeft = Console.CursorLeft;
            cursorTop = Console.CursorTop;
            int barWidth = Console.BufferWidth - 2 - 7;
            double pctD = (double)progress.Current / (double)progress.Maximum;
            int pct = (int)((barWidth * progress.Current) / progress.Maximum);
            int remaining = pct == barWidth ? 0 : barWidth - pct;
            Console.Write(pctD.ToString("000.0% ") + '[' + new string('=', pct) + new string(' ', remaining) + ']');
            Console.CursorLeft = cursorLeft;
            Console.CursorTop = cursorTop;
            lock (clock) { clock.Restart(); }
        }
        #endregion
    }
}
