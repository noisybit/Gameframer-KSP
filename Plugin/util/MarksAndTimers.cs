using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gameframer
{
    public class MarksAndTimers
    {
        private static Dictionary<string, System.Diagnostics.Stopwatch> timers = new Dictionary<string, System.Diagnostics.Stopwatch>();
        private static Dictionary<string, bool> marks = new Dictionary<string, bool>();

        private static Dictionary<string, double> events = new Dictionary<string, double>();

        public static bool EventOkay(string name, double newTime)
        {
            double lastTime = -1;

            if (events.TryGetValue(name, out lastTime))
            {
                return(newTime - lastTime > 2);
            }

            return true;
        }

        public static void SetEventTime(string name, double newTime)
        {
            events.Add(name, newTime);
        }

        public static string DebugString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MARKS");
            marks.Keys.ToList<string>().ForEach(s => sb.AppendLine(s + ": " + marks[s]));
            sb.AppendLine("TIMERS");
            timers.Keys.ToList<string>().ForEach(s => sb.AppendLine(s + ": " + timers[s].ElapsedMilliseconds));

            return sb.ToString();
        }
        public static void ClearAll()
        {
            timers.Clear();
            marks.Clear();
        }
        public static void StopAndClear(string name)
        {
            StopTimer(name);
            ClearMark(name);
        }
        public static void DoMark(string markName)
        {
            GFLogger.Instance.AddDebugLog("Marked: " + markName);
            marks.Add(markName, true);
        }
        public static bool CheckMark(string markName)
        {
            var marked = false;
            marks.TryGetValue(markName, out marked);
            return marked;
        }
        public static void ClearMark(string markName)
        {
            marks.Remove(markName);
        }
        public static bool IsRunning(string name)
        {
            System.Diagnostics.Stopwatch sw;

            if (timers.TryGetValue(name, out sw))
            {
                return sw.IsRunning;
            }
            else
            {
                return false;
            }

        }
        public static bool StartTimer(string name)
        {
            System.Diagnostics.Stopwatch sw;

            if (timers.TryGetValue(name, out sw))
            {
                sw.Reset();
                sw.Start();

                return true;
            }
            else
            {
                sw = new System.Diagnostics.Stopwatch();
                sw.Reset();
                sw.Start();
                timers.Add(name, sw);
                return true;
            }
        }

        public static bool StopTimer(string name)
        {
            System.Diagnostics.Stopwatch sw;

            if (timers.TryGetValue(name, out sw))
            {
                sw.Stop();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static long CheckTimer(string name)
        {
            System.Diagnostics.Stopwatch sw;

            if (timers.TryGetValue(name, out sw))
            {
                return sw.ElapsedMilliseconds;
            }
            else
            {
                return -1;
            }
        }
    }
}
