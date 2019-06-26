using Hal.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hal
{
    class main
    {
        private uci uciInterface = new uci();
        private Thread tUci;
        private Thread tConsole;

        public void run()
        {
            tUci = new Thread(new ThreadStart(uciInterface.run));
            tConsole = new Thread(new ThreadStart(consoleRead));
            tUci.Start();
            tConsole.Start();
            while (tUci.IsAlive)
            {
                Thread.Sleep(10);
            }
            tConsole.Abort();
        }

        void consoleRead()
        {
            string comando = "";
            while (true)
            {
                Console.WriteLine("lendo");
                comando = Console.ReadLine();
                uciInterface.enviarComandoParaEngine(comando);

            }
        }
    }
}
