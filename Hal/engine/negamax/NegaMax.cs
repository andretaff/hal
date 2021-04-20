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
        private TranspTable tabela;
        private Move finalResult;
        private uint age;


        public NegaMax(Uci uci, TranspTable tabela)
        {
            this.uci = uci;
            this.avaliador = new Avaliador();
            this.tabela = tabela;
            age = 0;
        }

        internal Move FinalResult { get => finalResult; set => finalResult = value; }

        public Move go (Board tabuleiro, Thread ttemporizador, TimeControl temporizador)
        {
#if DEBUG
            int numberOfThreads = 1;
#else
            int numberOfThreads = 1;
#endif
            age++;
            NegaThread.negaResult resultadoTemp, resultado;
            ThreadQueue<NegaThread.negaResult> results = new ThreadQueue<NegaThread.negaResult>();
            NegaThread[] negathread = new NegaThread[numberOfThreads];
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
                for (i = 0; i < numberOfThreads; i++)
                {
                    unsafe
                    {
                        negathread[i] = new NegaThread(i,tabuleiro, profundidade, avaliador, ttemporizador, &bParar, results, tabela,age);
                    }

                    thread = new Thread(new ThreadStart(negathread[i].Run));
                    thread.Start();
                    threads.Add(thread);
                }
                while (!bAchou)
                {
                    Thread.Sleep(5);
                    if (!results.isEmpty())
                    {
                        bParar = true;
                        resultadoTemp = results.get();
                        bAchou = true;
                        for (i = 0; i < numberOfThreads; i++)
                            threads[i].Abort();
                        Thread.Sleep(1);
                        tabela.reiniciarMovsBuscados();
                    }
                }

                if ((ttemporizador.IsAlive))
                {
                    resultado = resultadoTemp;
                    tabuleiro.makeMove(resultado.move);
                  
                    uci.enviarComandoParaConsole("info " +
                                                 "depth " + Convert.ToString(profundidade) + " " +
                                                 "score cp " + Convert.ToString(resultado.nota) + " " +
                                                 "nodes " + Convert.ToString(resultado.nodes) + " " +
                                                 "time " + Convert.ToString(temporizador.ellapsedTime()) + " " +
//                                                 "tbhits "+ Convert.ToString(resultado.hits) + " "+
                                                 "pv " + resultado.move.toAlgebra()+" "+this.printPV(tabuleiro));
                    tabuleiro.unmakeMove(resultado.move);     
                }
                if (resultado.nota>9990)
                {
                    break;
                }
             }
            //resultado.move.print();
            return resultado.move;

        }

        private string printPV(Board tabuleiro)
        {
            string saida = "";
            Move move;
            Tuple<bool, TranspItem> resultado;

            resultado = tabela.recuperar(tabuleiro.getChave(), 0, age);
            if ((resultado.Item2 != null) && (resultado.Item2.move != null) && (resultado.Item2.move.tipo != tipoMovimento.MOVNENHUM))
            {
                saida = resultado.Item2.move.toAlgebra();
                tabuleiro.makeMove(resultado.Item2.move);
                saida += " " +this.printPV(tabuleiro);
                tabuleiro.unmakeMove(resultado.Item2.move);
            }
            return saida;
 
        }
    }
}
