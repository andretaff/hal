using Hal.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hal.userInterface
{
    class Uci
    {
        private static readonly object _objectIn = new object();
        private static readonly object _objectOut = new object();
        private Queue<string> commIn = new Queue<string>();
        private Queue<string> commOut = new Queue<string>();
        private Game game;
        private Thread gameThread;



        public void run()
        {
            string comando = "";
            this.enviarComandoParaEngine("ucinewgame");
            while (true)
            {
                if (this.commIn.Count() > 0)
                {
                    lock (_objectIn)
                    {
                        comando = commIn.Dequeue();
                    }
                    if (comando=="uci")
                    {
                        this.cUci();
                    }

                    if (comando == "isready")
                    {

                    }
                    if (comando == "ucinewgame")
                        this.cUcinewGame();

                    if (comando.StartsWith("position "))
                        this.cUciPosition(comando);

                    if (comando.StartsWith("go"))
                    {
                        this.cUciGo(comando);
                    }

                    if (comando == "stop")
                    {
                        game.stop();
                    }


                    if (comando == "quit")
                    {
                        break;
                    }
                }
                if (this.commOut.Count() > 0)
                {
                    lock (_objectOut)
                    {
                        comando = commOut.Dequeue();
                        Console.Out.WriteLine(comando);
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void cUcinewGame()
        {
            game.newGame();
        }

        private void cUci()
        {
            this.enviarComandoParaConsole("id name " + GConstants.ENGINE_NAME);
            this.enviarComandoParaConsole("id author " + GConstants.AUTHOR);
            this.enviarComandoParaConsole("uciok");
        }

        private void cIsReady()
        {
            this.enviarComandoParaConsole("readyok");
        }

        private void cUciPosition(string comando)
        {
            if (comando.IndexOf("startpos") != -1)
            {
                game.newGame();
            }
            else
            {
                int movePos = comando.IndexOf("moves") - 1;
                if (movePos == -1)
                    movePos = comando.Length;
                string fenstr = comando.Substring(8,movePos);
                game.setFenPosition(fenstr);
            }
        }

        private void cUciGo(string comando)
        {
            if ((gameThread!=null) && (gameThread.IsAlive))
            {
                throw new SystemException("Engine já iniciada!");
            }
            if (comando.IndexOf("infinite") != -1)
            {
                game.start(tipoTempo.infinito, 0);
                gameThread = new Thread(new ThreadStart(game.run));
                gameThread.Start();
            }
            if (comando.IndexOf("movetime")!=-1)
            {
                uint msegundos = Convert.ToUInt32(comando.Substring(comando.IndexOf("movetime ") + 9));
                game.start(tipoTempo.milisecsPerMove, msegundos);
                gameThread = new Thread(new ThreadStart(game.run));
                gameThread.Start();
            }
        }

        public void enviarComandoParaEngine(string comando)
        {
            lock (_objectIn)
            {
                commIn.Enqueue(comando);
            }
        }

        public void enviarComandoParaConsole(string comando)
        {
            lock (_objectOut)
            {
                commOut.Enqueue(comando);
            }
        }

        public Uci()
        {
            this.game = new Game(this);
        }


    }
}
