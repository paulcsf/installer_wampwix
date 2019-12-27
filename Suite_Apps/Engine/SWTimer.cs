using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS
{
    public class SWTimer
    {
        private Stopwatch SW;
        private long Min = long.MaxValue;
        private long Max = 0;
        private Double Avg = 0;
        private long CancelCount = 0;
        private string Name;

        public SWTimer(string iName)
        {
            Name = iName;
            SW = new Stopwatch();
        }
        ~SWTimer()
        {
        }

        public void Start()
        {
            SW = Stopwatch.StartNew();
        }
        public void Stop()
        {
            SW.Stop();
            long E = SW.ElapsedMilliseconds;
            if (E > Max) Max = E;
            if (E < Min) Min = E;

            if (Avg == 0)
                Avg = E;
            else
            {
                Avg = Avg * .99f;
                Avg += (E * .01f);
            }
        }
        //Stop without updating the statistics, typically called from exception handlers.
        public void StopCancel()
        {
            SW.Stop();
            CancelCount++;
        }
        public string Stats()
        {
            return Name + ": Max: " + (((float)Max) / 1000f).ToString("0.000")
                + "s   Min: " + (Min==long.MaxValue ? "n/a" : (((float)Min) / 1000f).ToString("0.000"))
                + "s   Avg: " + (((float)Avg) / 1000f).ToString("0.000")
                + "s   Err: " + CancelCount.ToString();
        }
    }
}
