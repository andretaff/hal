using Hal.userInterface;
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
        private Uci uciInterface = new Uci();
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
            Environment.Exit(0);
        }

        void consoleRead()
        {
            string comando = "";
            while (true)
            {
                comando = Console.ReadLine();
                uciInterface.enviarComandoParaEngine(comando);
            }
        }
    }
}
