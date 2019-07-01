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
        private TranspTable tabela;
        private ulong hits;

        private ThreadQueue<negaResult> resultados;

        private unsafe bool* bParar;

        public struct negaResult
        {
            public int nota;
            public uint nodes;
            public Move move;
            public ulong hits;
        };



        public void Run()
        {
            int nota;
            negaResult result;
            this.nodes = 0;
            this.hits = 0;
            nota = Nega(-99999999, +99999999, maxPly);
            result.nota = nota;
            result.move = this.move;
            result.nodes = this.nodes;
            result.hits = this.hits;
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
            Tuple<bool,TranspItem> retorno = tabela.recuperar(tabuleiro.getChave(), ply, 0);
            if (retorno.Item1)
            {
                this.hits++;
                if (this.maxPly == ply)
                    melhorMov = retorno.Item2.move;
                if (retorno.Item2.tipo == tipoTranspItem.SCORE_EXATO)
                {
                    return retorno.Item2.score;
                }
                else if (retorno.Item2.tipo == tipoTranspItem.SCORE_UPPER)
                {
                    beta = Math.Min(beta, retorno.Item2.score);

                }
                else if (retorno.Item2.tipo == tipoTranspItem.SCORE_LOWER)
                {
                    alfa = Math.Max(alfa, retorno.Item2.score);
                }
                if (alfa > beta)
                {
                    return retorno.Item2.score;
                }
                melhorMov.peca = tipoPeca.NENHUMA;
            }
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
                //if (ply == this.maxPly)
                //    tabuleiro.print();
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
                        melhorMov = move;
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
            TranspItem itemT = new TranspItem();
            itemT.move = melhorMov;
            itemT.idade = 0;
            itemT.score = alfa;
            itemT.chave = tabuleiro.getChave();
            itemT.ply = ply;

            if (alfa <= alfaOriginal)
                itemT.tipo = tipoTranspItem.SCORE_UPPER;
            else if (alfa > beta)
                itemT.tipo = tipoTranspItem.SCORE_LOWER;
            else
                itemT.tipo = tipoTranspItem.SCORE_EXATO;

            tabela.armazenar(itemT);

            return alfa;

        }


        unsafe

            public NegaThread(Board tabuleiro, int profundidade, Avaliador avaliador, Thread temporizador, bool* bParar, ThreadQueue<negaResult> resultados, TranspTable tabela)
        {
            this.tabuleiro = tabuleiro.clone();
            this.maxPly = profundidade;
            this.avaliador = avaliador;
            this.resultados = resultados;
            this.temporizador = temporizador;
            this.hits = 0;
            this.tabela = tabela;
            unsafe
            {
                this.bParar = bParar;
            }
        }
    


    }
}
