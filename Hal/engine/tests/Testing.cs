using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.engine.tests
{
    class Testing
    {
        private DateTime inicio = DateTime.Now;
        public void startTime()
        {
            inicio = DateTime.Now;
        }

        public ulong timeEllapsed()
        {
            return (ulong) ((TimeSpan)(DateTime.Now - inicio)).TotalMilliseconds;
        }

        public void AssertEquals(string v1, string v2, string msg)
        {
            if (v1 != v2)
            {
                throw new Exception(msg);
            }
        }

        public void Assert(bool v1, string msg)
        {
            if (!v1)
            {
                throw new Exception(msg);
            }
        }


         
    }
}
