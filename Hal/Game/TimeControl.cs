using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Hal.game
{
    enum tipoTempo { infinito, milisecsPerMove };
    class TimeControl
    {
        private tipoTempo tipo;
        private ulong miliSecsMax;
        private DateTime startTime;
        private bool stop;

        public void Run()
        {
            DateTime endTime;
            startTime = DateTime.Now;

            if (tipo == tipoTempo.infinito)
            {
                while (!stop)
                {
                    Thread.Sleep(10);
                }
            }
            else if (tipo == tipoTempo.milisecsPerMove)
            {
                endTime = DateTime.Now;
                while ((!stop) && (((TimeSpan)(endTime - startTime)).TotalMilliseconds<miliSecsMax-300))
                {
                    Thread.Sleep(10);
                    endTime = DateTime.Now;
                }
            }
        }

        public ulong ellapsedTime()
        {
            return (ulong) ((TimeSpan)(DateTime.Now - startTime)).TotalMilliseconds;
        }

        public void parar()
        {
            this.stop = true;
        }
        public TimeControl(tipoTempo tipo, ulong miliSecsMax)
        {
            stop = false;
            this.tipo = tipo;
            this.miliSecsMax = miliSecsMax;
        }
    }
}
