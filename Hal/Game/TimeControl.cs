using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Hal.Game
{
    enum tipoTempo { infinito, milisecsPerMove };
    class TimeControl
    {
        private tipoTempo tipo;
        private ulong miliSecsMax;
        private bool stop;

        public void Run()
        {
            DateTime startTime, endTime;
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
                while ((!stop) && (((TimeSpan)(endTime - startTime)).TotalMilliseconds<miliSecsMax))
                {
                    Thread.Sleep(10);
                    endTime = DateTime.Now;
                }
            }
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
