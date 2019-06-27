using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.bitboard;
using Hal.engine.bitboard;
using Hal.engine.move;

namespace Hal.engine.board
{
    class Board
    {
        private ulong[] bbs;
        private byte corMover;
        private BlackMagic bm;

        public Board(BlackMagic bm)
        {
            bbs = new ulong[bbConstants.todosBBs];
            this.bm = bm;
            this.corMover = 0;
        }

        public void addPeca(ulong posicao, tipoPeca peca)
        {
            this.bbs[(int)peca] ^= posicao;
            this.bbs[(int)peca % 2 + bbConstants.PECAS] ^= posicao;
        }

        public void removePeca(ulong posicao, tipoPeca peca)
        {
            this.bbs[(int)peca] ^= posicao;
            this.bbs[(int)peca % 2 + bbConstants.PECAS] ^= posicao;
        }


        public List<Move> gerarMovimentos()
        {
            List<Move> moves;

            moves = new List<Move>();
            this.genMovsPeao(true, moves);
            return moves;
        }

        private void genMovsPeao(bool capturas, List<Move> moves)
        {
            Move move;
            if (capturas)
            {
                ulong peoes = bbs[(byte)tipoPeca.PEAO + corMover];
                ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
                ulong pFrom;
                ulong ataques;
                ulong pTo;
                tipoPeca pAtacada;
                ulong promos;
                int iFrom, iTo;

                while (peoes!=0)
                {
                    pFrom = (ulong) ((long) peoes & -(long)peoes);
                    iFrom = bm.index(pFrom);
                    ataques = bm.aPeao[corMover][iFrom] & inimigas;
                    if (corMover == 0)
                        promos = ataques & bbConstants.R1;
                    else
                        promos = ataques & bbConstants.R8;

                    while (ataques != 0)
                    {
                        pTo = (ulong)((long)ataques & -(long)ataques);
                        iTo = bm.index(pTo);
                        pAtacada = this.getPecaPosicao(pTo, (byte)1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)((int)tipoPeca.PEAO + corMover),
                            pAtacada,
                            iFrom,
                            iTo);

                        ataques = ataques & (ataques - 1);
                    }
                    peoes = peoes & (peoes - 1);
                }
            }
        }

        tipoPeca getPecaPosicao(ulong posicao, int cor)
        {
            int i;

            for (i = cor; i<bbConstants.PECAS; i += 2)
            {
                if ((this.bbs[i] & posicao)!=0)
                {
                    return (tipoPeca)i;
                }
            }
            return tipoPeca.NENHUMA;
        }

    }
}
