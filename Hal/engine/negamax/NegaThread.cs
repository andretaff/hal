using Hal.engine.board;
using Hal.Interface;
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
        public uint nodes;
        Uci comm;
        public Move move;
        List<Move> moves;
        Avaliador avaliador;
        Thread temporizador;
        public int nota;


        public void Run()
        {
            this.nota = this.Nega(-99999999, +99999999, maxPly);

        }


        private int Nega(int alfa, int beta, int ply)
        {
            int alfaOriginal = alfa;
            int valor;
            Move melhorMov;
            List<Move> moves;

            if (!temporizador.IsAlive)
            {
                return 0;
            }
            if (ply == 0)
            {
                return this.avaliador.avaliar(tabuleiro);
            }
            valor = -99999999;
            moves = tabuleiro.gerarMovimentos();

            foreach (Move move in moves)
            {

            }

        }


        public NegaThread (Board tabuleiro, int profundidade, Uci comm, Avaliador avaliador, Thread temporizador)
        {
            this.tabuleiro = tabuleiro.DeepClone();
            this.maxPly = profundidade;
            this.comm = comm;
            this.avaliador = avaliador;
        }

    }
}
