using Hal.engine.board;
using Hal.engine.bitboard;
using Hal.userInterface;
using Hal.engine.move;
using Hal.engine.avaliacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace Hal.engine.negamax
{
    class NegaThread
    {
        Board tabuleiro;
        int maxPly;
        private uint nodes;
        private Move move;
        Avaliador avaliador;
        Thread temporizador;

        private ThreadQueue<negaResult> resultados;

        private unsafe bool* bParar;

        public struct negaResult
        {
            public int nota;
            public uint nodes;
            public Move move;
        };



        public void Run()
        {
            int nota;
            negaResult result;
            nota = Nega(-99999999, +99999999, maxPly);
            result.nota = nota;
            result.move = this.move;
            result.nodes = this.nodes;
            if (resultados.isEmpty())
                resultados.put(result);

        }


        private int Nega(int alfa, int beta, int ply)
        {
            int alfaOriginal = alfa;
            int valor;
            int melhorValor;
            Move melhorMov = new Move();
            List<Move> moves;
            melhorMov.peca = tipoPeca.NENHUMA;

            unsafe
            {
                if ((!temporizador.IsAlive) || (*bParar))
                {
                    return 0;
                }
            }

            this.nodes++;

            if (ply == 0)
            {
                return this.avaliador.avaliar(tabuleiro);
            }
            valor = -99999999;
            melhorValor = -99999999;
            moves = tabuleiro.gerarMovimentos();

            foreach (Move move in moves)
            {
                unsafe
                {
                    if ((!temporizador.IsAlive) || (*bParar))
                    {
                        return 0;
                    }
                }
                //tabuleiro.print();
                // move.print();
                this.tabuleiro.makeMove(move);
                //tabuleiro.print();
                if (!tabuleiro.isValido())
                {
                    this.tabuleiro.unmakeMove(move);
                }
                else
                {
                    valor = -Nega(-beta, -alfa, ply - 1);
                    if (valor > melhorValor)
                    {
                        if (ply == this.maxPly)
                        {
                            melhorMov = move;
                        }
                        melhorValor = valor;
                    }
                    if (valor > alfa)
                        alfa = valor;
                    if (alfa > beta)
                    {
                        tabuleiro.unmakeMove(move);
                        break;
                    }
                    tabuleiro.unmakeMove(move);
                }
            }

            if (melhorMov.peca == tipoPeca.NENHUMA)
            {
                if (tabuleiro.isChecked())
                {
                    return -99999;
                }
                else
                {
                    return -100; //stalemate
                }
            }

            if (this.maxPly == ply)
            {
                unsafe
                {
                    if ((temporizador.IsAlive) && (!*bParar))
                    {
                        this.move = melhorMov;
                    }
                }
            }
            return alfa;

        }


        unsafe

            public NegaThread(Board tabuleiro, int profundidade, Avaliador avaliador, Thread temporizador, bool* bParar, ThreadQueue<negaResult> resultados)
        {
            this.tabuleiro = tabuleiro.clone();
            this.maxPly = profundidade;
            this.avaliador = avaliador;
            this.resultados = resultados;
            this.temporizador = temporizador;
            unsafe
            {
                this.bParar = bParar;
            }
        }
    


    }
}
