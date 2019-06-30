using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.board;
using Hal.engine.move;
using System.Threading;
using Hal.userInterface;
using Hal.engine.avaliacao;
using Hal.game;

namespace Hal.engine.negamax
{
    class NegaMax
    {
        private Uci uci;
        private Avaliador avaliador;
        public NegaMax(Uci uci)
        {
            this.uci = uci;
            this.avaliador = new Avaliador();
        }
        public Move go (Board tabuleiro, Thread ttemporizador, TimeControl temporizador)
        {
            int numberOfThreads = 1;
            NegaThread.negaResult resultadoTemp, resultado;
            ThreadQueue<NegaThread.negaResult> results = new ThreadQueue<NegaThread.negaResult>();
            NegaThread negathread;
            int i;
            List<Thread> threads = new List<Thread>();
            Thread thread;
            int profundidade = 1;
            bool bAchou;
            bool bParar;

            resultado = new NegaThread.negaResult();
            resultadoTemp = resultado;

            while (ttemporizador.IsAlive)
            {
                bAchou = false;
                threads.Clear();
                results.Clear();
                profundidade = profundidade + 1;
                bParar = false;
                unsafe
                {
                    negathread = new NegaThread(tabuleiro, profundidade, avaliador, ttemporizador, &bParar, results);
                }

                for (i = 0; i < numberOfThreads; i++)
                {
                    thread = new Thread(new ThreadStart(negathread.Run));
                    thread.Start();
                    threads.Add(thread);
                }
                while (!bAchou)
                {
                    Thread.Sleep(10);
                    if (!results.isEmpty())
                    {
                        bParar = true;
                        resultadoTemp = results.get();
                        bAchou = true;

                    }
                }

                if ((ttemporizador.IsAlive))
                {
                    resultado = resultadoTemp;
                    uci.enviarComandoParaConsole("info " +
                                                 "score cp " + Convert.ToString(resultado.nota) + " " +
                                                 "depth " + Convert.ToString(profundidade) + " " +
                                                 "nodes " + Convert.ToString(resultado.nodes) + " " +
                                                 "time " + Convert.ToString(temporizador.ellapsedTime()) + " " +
                                                 "currmove " + resultado.move.toAlgebra());
                        
                }

             }
            return resultado.move;

        }
    }
}
