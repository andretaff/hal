using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.Interface
{
    class uci
    {
        private static readonly object _objectIn = new object();
        private static readonly object _objectOut = new object();
        private Queue<string> commIn = new Queue<string>();
        private Queue<string> commOut = new Queue<string>();


        public void run()
        {
            string comando = "";
            while (true)
            {
                if (this.commIn.Count() > 0)
                {
                    lock (_objectIn)
                    {
                        comando = commIn.Dequeue();
                    }
                    if (comando == "quit")
                    {
                        break;
                    }
                }
            }
        }

        public void enviarComandoParaEngine(string comando)
        {
            lock (_objectIn)
            {
                commIn.Enqueue(comando);
            }
        }




    }
}
