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
            this.genMovsCavalo(true, moves);
            this.genMovsBispo(tipoPeca.BISPO, true, moves);
            this.genMovsTorre(tipoPeca.TORRE, true, moves);
            this.genMovsBispo(tipoPeca.RAINHA, true, moves);
            this.genMovsTorre(tipoPeca.RAINHA, true, moves);
            this.genMovsRei(true, moves);

            this.genMovsPeao(false, moves);
            this.genMovsCavalo(false, moves);
            this.genMovsBispo(tipoPeca.BISPO, false, moves);
            this.genMovsTorre(tipoPeca.TORRE, false, moves);
            this.genMovsBispo(tipoPeca.RAINHA, false, moves);
            this.genMovsTorre(tipoPeca.RAINHA, false, moves);
            this.genMovsRei(false, moves);


            return moves;
        }

        private void genMovsTorre(tipoPeca peca, bool capturas, List<Move> moves)
        {

            ulong torres = bbs[(int)peca + corMover];
            ulong movs;
            ulong pFrom, pTo;
            int iFrom, iTo;
            tipoPeca pecaAtacada;
            ulong amigas = bbs[bbConstants.PECAS + corMover];
            ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
            ulong todas = amigas | inimigas;
            ulong occ;
            Move move;
            uint index;

            if (capturas)
            {
                while (torres > 0)
                {
                    pFrom = (ulong)((long)torres & -(long)torres);
                    iFrom = bm.index(pFrom);
                    index = bm.torre[iFrom].posicao;
                    occ = bm.torre[iFrom].mascara | todas;
                    occ *= bm.torre[iFrom].fator;
                    occ >>= (64 - 12);
                    index = index + (uint)occ;

                    movs = bm.tabela[index] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)peca + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    torres = torres & (torres - 1);
                }
            }
            else
            {
                while (torres > 0)
                {
                    pFrom = (ulong)((long)torres & -(long)torres);
                    iFrom = bm.index(pFrom);

                    index = bm.torre[iFrom].posicao;
                    occ = bm.torre[iFrom].mascara | todas;
                    occ *= bm.torre[iFrom].fator;
                    occ >>= (64 - 12);
                    index = index + (uint)occ;

                    movs = bm.tabela[index] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        move = new Move(pFrom, pTo, tipoMovimento.MNORMAL,
                            (tipoPeca)(int)peca + corMover,
                            tipoPeca.NENHUMA,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    torres = torres & (torres - 1);
                }
            }
        }

        private void genMovsBispo(tipoPeca peca, bool capturas, List<Move> moves)
        {

            ulong bispos = bbs[(int)peca + corMover];
            ulong movs;
            ulong pFrom, pTo;
            int iFrom, iTo;
            tipoPeca pecaAtacada;
            ulong amigas = bbs[bbConstants.PECAS + corMover];
            ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
            ulong todas = amigas | inimigas;
            ulong occ;
            Move move;
            uint index;

            if (capturas)
            {
                while (bispos > 0)
                {
                    pFrom = (ulong)((long)bispos & -(long)bispos);
                    iFrom = bm.index(pFrom);
                    index = bm.bispo[iFrom].posicao;
                    occ = bm.bispo[iFrom].mascara | todas;
                    occ *= bm.bispo[iFrom].fator;
                    occ >>= (64 - 9);
                    index = index + (uint) occ;

                    movs = bm.tabela[index] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)peca + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    bispos = bispos & (bispos - 1);
                }
            }
            else
            {
                while (bispos > 0)
                {
                    pFrom = (ulong)((long)bispos & -(long)bispos);
                    iFrom = bm.index(pFrom);

                    index = bm.bispo[iFrom].posicao;
                    occ = bm.bispo[iFrom].mascara | todas;
                    occ *= bm.bispo[iFrom].fator;
                    occ >>= (64 - 9);
                    index = index + (uint)occ;

                    movs = bm.tabela[index] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        move = new Move(pFrom, pTo, tipoMovimento.MNORMAL,
                            (tipoPeca)(int)peca + corMover,
                            tipoPeca.NENHUMA,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    bispos = bispos & (bispos - 1);
                }
            }
        }

        private void genMovsRei(bool capturas, List<Move> moves)
        {
            ulong reis= bbs[(int)tipoPeca.REI + corMover];
            ulong movs;
            ulong pFrom, pTo;
            int iFrom, iTo;
            tipoPeca pecaAtacada;
            ulong amigas = bbs[bbConstants.PECAS + corMover];
            ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
            ulong todas = amigas | inimigas;
            Move move;

            if (capturas)
            {
                while (reis > 0)
                {
                    pFrom = (ulong)((long)reis & -(long)reis);
                    iFrom = bm.index(pFrom);
                    movs = bm.mRei[iFrom] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)tipoPeca.REI + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    reis = reis & (reis- 1);
                }
            }
            else
            {
                while (reis> 0)
                {
                    pFrom = (ulong)((long)reis & -(long)reis);
                    iFrom = bm.index(pFrom);
                    movs = bm.mCavalo[iFrom] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        move = new Move(pFrom, pTo, tipoMovimento.MNORMAL,
                            (tipoPeca)(int)tipoPeca.REI + corMover,
                            tipoPeca.NENHUMA,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    reis = reis & (reis - 1);
                }
            }

        }

        private void genMovsCavalo(bool capturas, List<Move> moves)
        {
            ulong cavalos = bbs[(int)tipoPeca.CAVALO + corMover];
            ulong movs;
            ulong pFrom, pTo;
            int iFrom, iTo;
            tipoPeca pecaAtacada;
            ulong amigas = bbs[bbConstants.PECAS + corMover];
            ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
            ulong todas = amigas | inimigas;
            Move move;

            if (capturas)
            {
                while (cavalos > 0)
                {
                    pFrom = (ulong)((long)cavalos & -(long)cavalos);
                    iFrom = bm.index(pFrom);
                    movs = bm.mCavalo[iFrom] & inimigas;
                    while (movs>0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)tipoPeca.CAVALO + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    cavalos = cavalos & (cavalos - 1);
                }
            }
            else
            {
                while (cavalos > 0)
                {
                    pFrom = (ulong)((long)cavalos & -(long)cavalos);
                    iFrom = bm.index(pFrom);
                    movs = bm.mCavalo[iFrom] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        move = new Move(pFrom, pTo, tipoMovimento.MNORMAL,
                            (tipoPeca)(int)tipoPeca.CAVALO + corMover,
                            tipoPeca.NENHUMA,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                    cavalos = cavalos & (cavalos - 1);
                }
            }

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

                while (peoes != 0)
                {
                    pFrom = (ulong)((long)peoes & -(long)peoes);
                    iFrom = bm.index(pFrom);
                    ataques = bm.aPeao[corMover][iFrom] & inimigas;
                    if (corMover == 0)
                        promos = ataques & bbConstants.R1;
                    else
                        promos = ataques & bbConstants.R8;

                    while (promos != 0)
                    {
                        pTo = (ulong)((long)ataques & -(long)ataques);
                        iTo = bm.index(pTo);
                        pAtacada = this.getPecaPosicao(pTo, (byte)1 - corMover);
                        for (j = tipoPeca.RAINHA + corMover; j > tipoPeca.PP; j -= 2)
                        {
                            move = new Move(pFrom, pTo, (tipoMovimento)((int)tipoMovimento.MPROMOCAP + j),
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                pAtacada,
                                iFrom,
                                iTo);
                            moves.Add(move);
                            promos = promos & (promos - 1);
                        }

                        ataques = ataques & (ataques - 1);
                    }

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
                        moves.Add(move);
                        ataques = ataques & (ataques - 1);
                    }
                    peoes = peoes & (peoes - 1);
                }
            }
            else
            {
                if (corMover == 0)
                {
                    ulong amigas = bbs[bbConstants.PECAS + corMover];
                    ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
                    ulong todas = amigas | inimigas;

                    ulong movs = (bbs[(int)tipoPeca.PEAO] >> 8) & ~todas;
                    ulong movsDuplos = ((movs & bbConstants.R3) >> 8) & ~todas;
                    ulong pFrom, pTo;
                    int iFrom, iTo;
                    while (movsDuplos > 0)
                    {
                        pFrom = (ulong)((long)movsDuplos & -(long)movsDuplos);
                        iFrom = bm.index(pFrom);
                        iTo = iFrom - 16;
                        move = new Move(pFrom, pFrom >> 16, tipoMovimento.MDUPLO, tipoPeca.PEAO,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movsDuplos = movsDuplos & (movsDuplos - 1);
                    }

                    while (movs > 0)
                    {
                        pFrom = (ulong)((long)movs & -(long)movs);
                        iFrom = bm.index(pFrom);
                        iTo = iFrom - 8;
                        move = new Move(pFrom, pFrom >> 8, tipoMovimento.MNORMAL, tipoPeca.PEAO,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
                }
                else
                {
                    ulong amigas = bbs[bbConstants.PECAS + corMover];
                    ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
                    ulong todas = amigas | inimigas;

                    ulong movs = (bbs[(int)tipoPeca.PP] << 8) & ~todas;
                    ulong movsDuplos = ((movs & bbConstants.R3) << 8) & ~todas;
                    ulong pFrom, pTo;
                    int iFrom, iTo;
                    while (movsDuplos > 0)
                    {
                        pFrom = (ulong)((long)movsDuplos & -(long)movsDuplos);
                        iFrom = bm.index(pFrom);
                        iTo = iFrom + 16;
                        move = new Move(pFrom, pFrom << 16, tipoMovimento.MDUPLO, tipoPeca.PP,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movsDuplos = movsDuplos & (movsDuplos - 1);
                    }

                    while (movs > 0)
                    {
                        pFrom = (ulong)((long)movs & -(long)movs);
                        iFrom = bm.index(pFrom);
                        iTo = iFrom + 8;
                        move = new Move(pFrom, pFrom << 8, tipoMovimento.MNORMAL, tipoPeca.PP,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
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
