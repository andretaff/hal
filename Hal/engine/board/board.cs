using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.bitboard;
using Hal.engine.move;
using Hal.engine.avaliacao;


namespace Hal.engine.board
{
    class Board
    {
        private ulong[] bbs;
        public int corMover;
        private BlackMagic bm;
        private DNA dna = DNA.Instance;
        public int[] vMat;
        public uint potencialRoque = 0;
        public int enPassant = -1;
        private ulong chave = 0;
        private TranspTable transp;


        public Board(BlackMagic bm, TranspTable transp)
        {
            bbs = new ulong[bbConstants.todosBBs];
            this.bm = bm;
            this.corMover = 0;
            vMat = new int[2];
            this.transp = transp;
        }

        public Board clone()
        {
            Board board = new Board(bm, transp);
            board.corMover = corMover;
            board.potencialRoque = potencialRoque;
            board.enPassant = enPassant;
            board.chave = chave;
            board.vMat[0] = vMat[0];
            board.vMat[1] = vMat[1];
            int i;
            for (i=0; i<bbConstants.todosBBs; i++)
            {
                board.bbs[i] = bbs[i];
            }
            return board;
        }

        public ulong getChave()
        {
            ulong chavereal = this.chave;
            if ((this.potencialRoque & bbConstants.ROQUE_RAINHA_BRANCO) != 0)
                chavereal ^=  transp.chavesRoque[0];
            if ((this.potencialRoque & bbConstants.ROQUE_REI_BRANCO) != 0)
                chavereal ^=  transp.chavesRoque[1];
            if ((this.potencialRoque & bbConstants.ROQUE_RAINHA_PRETO) != 0)
                chavereal ^= transp.chavesRoque[2];
            if ((this.potencialRoque & bbConstants.ROQUE_REI_PRETO) != 0)
                chavereal ^= transp.chavesRoque[3];
            if ((this.enPassant != -1))
                chavereal ^= transp.chaves[bbConstants.PECAS, this.enPassant];
            if (this.corMover == 1)
                chavereal ^= transp.chaveBTM;
            return chavereal;

        }

        public void addPeca(ulong posicao, tipoPeca peca, int index)
        {
            int cor = (int)peca % 2;
            this.bbs[(int)peca] ^= posicao;
            this.bbs[cor + bbConstants.PECAS] |= posicao;
            vMat[cor] += dna.vPecas[(int)peca];
            this.chave ^= transp.chaves[(int)peca, index];

        }

        public void removePeca(ulong posicao, tipoPeca peca, int index)
        {
            if (peca == tipoPeca.NENHUMA)
                return;
            int cor = (int)peca % 2;
            this.bbs[(int)peca] ^= posicao;
            this.bbs[cor + bbConstants.PECAS] &= ~posicao;
            vMat[cor] -= dna.vPecas[(int)peca];
            this.chave ^= transp.chaves[(int)peca, index];
        }

        public void print()
        {
            ulong posicao = 1;
            int index = 0;
            string linha="";
            do
            {
                int i;
                for (i = 0; i < bbConstants.PECAS; i++)
                    if ((bbs[i] & posicao) != 0)
                    {
                        linha += bbConstants.sPecas[i];
                        break;
                    }
                if (i == bbConstants.PECAS)
                    linha += " ";

                if ((index+1) % 8 == 0)
                {
                    Console.Out.WriteLine(linha);
                    linha = "";
                }
                index++;
                posicao <<= 1;
            } while (index < 64);
        }

        public void addPecaHumana(tipoPeca peca, int index)
        {
            ulong posicao = (ulong)Math.Pow(2, index);
            this.addPeca(posicao, peca, index);
        }

        public bool casaAtacada(ulong bb,int corAtacante)
        {
            ulong atacantes = bbs[bbConstants.PECAS + corAtacante];
            uint index;
            ulong todas = bbs[bbConstants.PECAS] | bbs[bbConstants.PECAS + 1];
            int iFrom;
            ulong occ;

            iFrom = bm.index(bb);

            if ((bm.aPeao[1-corAtacante][iFrom] & bbs[(int)tipoPeca.PEAO + corAtacante]) != 0)
                return true;

            if ((bm.mRei[iFrom] & bbs[(int)tipoPeca.REI + corAtacante]) != 0)
                return true;

            if ((bm.mCavalo[iFrom] & bbs[(int)tipoPeca.CAVALO + corAtacante]) != 0)
                return true;

            index = bm.torre[iFrom].posicao;
            occ = bm.torre[iFrom].mascara | todas;
            occ *= bm.torre[iFrom].fator;
            occ >>= (64 - 12);
            index = index + (uint)occ;

            if ((bm.tabela[index] & (bbs[(int)tipoPeca.TORRE+corAtacante]| bbs[(int)tipoPeca.RAINHA + corAtacante]))!=0)
                return true;

            index = bm.bispo[iFrom].posicao;
            occ = bm.bispo[iFrom].mascara | todas;
            occ *= bm.bispo[iFrom].fator;
            occ >>= (64 - 9);
            index = index + (uint)occ;

            if ((bm.tabela[index] & (bbs[(int)tipoPeca.BISPO + corAtacante] | bbs[(int)tipoPeca.RAINHA + corAtacante])) != 0)
                return true;

            return false;

        }

        public bool isChecked()
        {
            return casaAtacada(bbs[(int)tipoPeca.REI + corMover], 1 - corMover);
        }
               
        public bool isValido()
        {
            return !casaAtacada(bbs[(int)tipoPeca.REI + 1 - corMover], corMover);
        }

        public void makeMove(Move move)
        {
            move.enPassant = this.enPassant;
            move.potencialRoque = this.potencialRoque;

            if ((potencialRoque & (bbConstants.ROQUE_REI_BRANCO | bbConstants.ROQUE_RAINHA_BRANCO))!=0)
            {
                if (move.peca == tipoPeca.REI)
                    move.potencialRoque &= (bbConstants.ROQUE_REI_PRETO | bbConstants.ROQUE_RAINHA_PRETO);
                if (((move.bbFrom | move.bbTo) & bbConstants.I56) != 0)
                    move.potencialRoque &= ~bbConstants.ROQUE_RAINHA_BRANCO;
                if (((move.bbFrom | move.bbTo) & bbConstants.I63) != 0)
                    move.potencialRoque &= ~bbConstants.ROQUE_REI_BRANCO;
            }
            if  ((potencialRoque & (bbConstants.ROQUE_REI_PRETO | bbConstants.ROQUE_RAINHA_PRETO)) != 0)
            {
                if (move.peca == tipoPeca.RP)
                    move.potencialRoque &= (bbConstants.ROQUE_REI_BRANCO | bbConstants.ROQUE_RAINHA_BRANCO);
                if (((move.bbFrom | move.bbTo) & bbConstants.I00) != 0)
                    move.potencialRoque &= ~bbConstants.ROQUE_RAINHA_PRETO;
                if (((move.bbFrom | move.bbTo) & bbConstants.I07) != 0)
                    move.potencialRoque &= ~bbConstants.ROQUE_REI_PRETO;
            }


            switch (move.tipo)
            {
                case tipoMovimento.MNORMAL:
                    {
                        this.removePeca(move.bbFrom, move.peca, move.indiceDe);
                        this.addPeca(move.bbTo, move.peca, move.indicePara);
                    } break;
                case tipoMovimento.MDUPLO:
                    {
                        this.removePeca(move.bbFrom, move.peca, move.indiceDe);
                        this.addPeca(move.bbTo, move.peca, move.indicePara);
                    } break;
                case tipoMovimento.MCAP:
                    {
                        this.removePeca(move.bbFrom, move.peca, move.indiceDe);
                        this.removePeca(move.bbTo, move.pecaCap, move.indicePara);
                        this.addPeca(move.bbTo, move.peca, move.indicePara);
                    } break;

                case tipoMovimento.MROQUEK:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            removePeca(bbConstants.I60, tipoPeca.REI, 60);
                            removePeca(bbConstants.I63, tipoPeca.TORRE, 63);
                            addPeca(bbConstants.I63, tipoPeca.REI, 63);
                            addPeca(bbConstants.I62, tipoPeca.TORRE, 62);
                        }
                        else
                        {
                            removePeca(bbConstants.I04, tipoPeca.RP, 4);
                            removePeca(bbConstants.I07, tipoPeca.TP, 7);
                            addPeca(bbConstants.I07, tipoPeca.RP, 7);
                            addPeca(bbConstants.I06, tipoPeca.TP, 6);
                         }
                    } break;

                case tipoMovimento.MROQUEQ:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            removePeca(bbConstants.I60, tipoPeca.REI, 60);
                            removePeca(bbConstants.I56, tipoPeca.TORRE, 56);
                            addPeca(bbConstants.I56, tipoPeca.REI, 56);
                            addPeca(bbConstants.I57, tipoPeca.TORRE, 57);
                        }
                        else
                        {
                            removePeca(bbConstants.I04, tipoPeca.RP, 4);
                            removePeca(bbConstants.I00, tipoPeca.TP, 0);
                            addPeca(bbConstants.I00, tipoPeca.RP, 0);
                            addPeca(bbConstants.I01, tipoPeca.TP, 1);
                        }
                    }
                    break;
                default:
                    {
                        if ((int) move.tipo> (int) tipoMovimento.MPROMOCAP)
                        {
                            tipoPeca promo = (tipoPeca)((int)move.tipo - (int)tipoMovimento.MPROMOCAP);
                            removePeca(move.bbFrom, move.peca, move.indiceDe);
                            removePeca(move.bbTo, move.pecaCap, move.indicePara);
                            addPeca(move.bbTo, promo, move.indicePara);
                        }
                        else
                        {
                            tipoPeca promo = (tipoPeca)((int)move.tipo - (int)tipoMovimento.MPROMO);
                            removePeca(move.bbFrom, move.peca, move.indiceDe);
                            addPeca(move.bbTo, promo, move.indicePara);
                        }

                    } break;

            }
            this.corMover = 1 - corMover;

        }

        public void unmakeMove(Move move)
        {
            this.enPassant = move.enPassant;
            this.potencialRoque = move.potencialRoque;
            switch (move.tipo){
                case tipoMovimento.MNORMAL:
                    {
                        this.addPeca(move.bbFrom, move.peca, move.indiceDe);
                        this.removePeca(move.bbTo, move.peca, move.indicePara);
                    } break;
                case tipoMovimento.MDUPLO:
                    {
                        this.addPeca(move.bbFrom, move.peca, move.indiceDe);
                        this.removePeca(move.bbTo, move.peca, move.indicePara);
                    } break;
                case tipoMovimento.MCAP:
                    {
                        this.addPeca(move.bbFrom, move.peca, move.indiceDe);
                        this.addPeca(move.bbTo, move.pecaCap, move.indicePara);
                        this.removePeca(move.bbTo, move.peca, move.indicePara);
                    }
                    break;
                case tipoMovimento.MROQUEK:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            addPeca(bbConstants.I60, tipoPeca.REI, 60);
                            addPeca(bbConstants.I63, tipoPeca.TORRE, 63);
                            removePeca(bbConstants.I63, tipoPeca.REI, 63);
                            removePeca(bbConstants.I62, tipoPeca.TORRE, 62);
                        }
                        else
                        {
                            addPeca(bbConstants.I04, tipoPeca.RP, 4);
                            addPeca(bbConstants.I07, tipoPeca.TP, 7);
                            removePeca(bbConstants.I07, tipoPeca.RP, 7);
                            removePeca(bbConstants.I06, tipoPeca.TP, 6);
                        }
                    }
                    break;

                case tipoMovimento.MROQUEQ:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            addPeca(bbConstants.I60, tipoPeca.REI, 60);
                            addPeca(bbConstants.I56, tipoPeca.TORRE, 56);
                            removePeca(bbConstants.I56, tipoPeca.REI, 56);
                            removePeca(bbConstants.I57, tipoPeca.TORRE, 57);
                        }
                        else
                        {
                            removePeca(bbConstants.I04, tipoPeca.RP, 4);
                            removePeca(bbConstants.I00, tipoPeca.TP, 0);
                            addPeca(bbConstants.I00, tipoPeca.RP, 0);
                            addPeca(bbConstants.I01, tipoPeca.TP, 1);
                        }
                    }
                    break;
                default:
                    {
                        if ((int)move.tipo > (int)tipoMovimento.MPROMOCAP)
                        {
                            tipoPeca promo = (tipoPeca)((int)move.tipo - (int)tipoMovimento.MPROMOCAP);
                            addPeca(move.bbFrom, move.peca, move.indiceDe);
                            addPeca(move.bbTo, move.pecaCap, move.indicePara);
                            removePeca(move.bbTo, promo, move.indicePara);
                        }
                        else
                        {
                            tipoPeca promo = (tipoPeca)((int)move.tipo - (int)tipoMovimento.MPROMO);
                            addPeca(move.bbFrom, move.peca, move.indiceDe);
                            removePeca(move.bbTo, promo, move.indicePara);
                        }

                    }
                    break;

            }
            this.corMover = 1 - corMover;

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
            ulong pTo;
            int iFrom, iTo;
            tipoPeca pecaAtacada;
            ulong amigas = bbs[bbConstants.PECAS + corMover];
            ulong inimigas = bbs[bbConstants.PECAS + 1 - corMover];
            ulong todas = amigas | inimigas;
            Move move;

            if (capturas)
            {
                    iFrom = bm.index(reis);
                    movs = bm.mRei[iFrom] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = bm.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(reis, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)tipoPeca.REI + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        moves.Add(move);
                        movs = movs & (movs - 1);
                    }
            }
            else
            {
                iFrom = bm.index(reis);
                movs = bm.mRei[iFrom] & ~todas;
                while (movs > 0)
                {
                    pTo = (ulong)((long)movs & -(long)movs);
                    iTo = bm.index(pTo);
                    move = new Move(reis, pTo, tipoMovimento.MNORMAL,
                        (tipoPeca)(int)tipoPeca.REI + corMover,
                        tipoPeca.NENHUMA,
                        iFrom,
                        iTo
                        );
                    moves.Add(move);
                    movs = movs & (movs - 1);
                }
                if ((this.corMover == 0) && (((this.potencialRoque & (bbConstants.ROQUE_RAINHA_BRANCO | bbConstants.ROQUE_REI_BRANCO))!=0)))
                {
                    if ((this.potencialRoque & bbConstants.ROQUE_REI_BRANCO) != 0)
                    {
                        if (((todas & (bbConstants.I62 | bbConstants.I61)) == 0)
                            && (!casaAtacada(bbConstants.I60, 1))
                            && (!casaAtacada(bbConstants.I61, 1))
                            && (!casaAtacada(bbConstants.I62, 1))
                            && (!casaAtacada(bbConstants.I63, 1)))
                        {
                            move = new Move(reis, bbConstants.I63,
                                tipoMovimento.MROQUEK,
                                tipoPeca.REI, tipoPeca.NENHUMA,
                                60, 63);
                            moves.Add(move);
                        }
                    }
                    if ((this.potencialRoque & bbConstants.ROQUE_RAINHA_BRANCO) != 0)
                    { 
                        if (((todas & (bbConstants.I57 | bbConstants.I58 | bbConstants.I59)) == 0)
                            && (!casaAtacada(bbConstants.I56, 1))
                            && (!casaAtacada(bbConstants.I57, 1))
                            && (!casaAtacada(bbConstants.I58, 1))
                            && (!casaAtacada(bbConstants.I59, 1))
                            && (!casaAtacada(bbConstants.I60, 1)))
                        {
                            move = new Move(reis, bbConstants.I56,
                                tipoMovimento.MROQUEQ,
                                tipoPeca.REI, tipoPeca.NENHUMA,
                                60, 56);
                            moves.Add(move);
                        }

                    }
                }
                else if ((this.corMover != 0) && (((this.potencialRoque & (bbConstants.ROQUE_RAINHA_PRETO| bbConstants.ROQUE_REI_PRETO)) != 0)))
                {
                    if ((this.potencialRoque & bbConstants.ROQUE_REI_PRETO) != 0)
                    {
                        if (((todas & (bbConstants.I05 | bbConstants.I06)) == 0)
                            && (!casaAtacada(bbConstants.I04, 1))
                            && (!casaAtacada(bbConstants.I05, 1))
                            && (!casaAtacada(bbConstants.I06, 1))
                            && (!casaAtacada(bbConstants.I07, 1)))
                        {
                            move = new Move(reis, bbConstants.I07,
                                tipoMovimento.MROQUEK,
                                tipoPeca.RP, tipoPeca.NENHUMA,
                                4, 7);
                            moves.Add(move);
                        }
                    }
                    if ((this.potencialRoque & bbConstants.ROQUE_RAINHA_PRETO) != 0)
                    {
                        if (((todas & (bbConstants.I03 | bbConstants.I02 | bbConstants.I01)) == 0)
                            && (!casaAtacada(bbConstants.I00, 0))
                            && (!casaAtacada(bbConstants.I01, 0))
                            && (!casaAtacada(bbConstants.I02, 0))
                            && (!casaAtacada(bbConstants.I03, 0))
                            && (!casaAtacada(bbConstants.I04, 0)))
                        {
                            move = new Move(reis, bbConstants.I56,
                                tipoMovimento.MROQUEQ,
                                tipoPeca.REI, tipoPeca.NENHUMA,
                                4, 0);
                            moves.Add(move);
                        }

                    }

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
                int j;

                while (peoes != 0) 
                {
                    pFrom = (ulong)((long)peoes & -(long)peoes);
                    iFrom = bm.index(pFrom);
                    ataques = bm.aPeao[corMover][iFrom] & inimigas;
                    if (corMover == 0)
                        promos = ataques & bbConstants.R1;
                    else
                        promos = ataques & bbConstants.R8;

                    ataques &= ~promos;

                    while (promos != 0)
                    {
                        pTo = (ulong)((long)promos & -(long)promos);
                        iTo = bm.index(pTo);
                        pAtacada = this.getPecaPosicao(pTo, (byte)1 - corMover);
                        for (j = (int)tipoPeca.RAINHA + corMover; j > (int) tipoPeca.PP; j -= 2)
                        {
                            move = new Move(pFrom, pTo, (tipoMovimento)((int)tipoMovimento.MPROMOCAP + j),
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                pAtacada,
                                iFrom,
                                iTo);
                            moves.Add(move);
                            
                        }

                        promos = promos & (promos - 1);
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
                    ulong movsDuplos = ((movs & bbConstants.R6) >> 8) & ~todas;
                    ulong promos = movs & bbConstants.R1;
                    ulong pFrom;
                    int iFrom, iTo,j;

                    movs &= ~promos;

                    while (promos != 0)
                    {
                        pFrom = (ulong)((long)promos & -(long)promos) << 8;
                        iFrom = bm.index(pFrom);
                        iTo = iFrom - 8;
                        for (j = (int)tipoPeca.RAINHA + corMover; j > (int)tipoPeca.PP; j -= 2)
                        {
                            move = new Move(pFrom, pFrom >> 8, (tipoMovimento)((int)tipoMovimento.MPROMO + j),
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                tipoPeca.NENHUMA,
                                iFrom,
                                iTo);
                            moves.Add(move);

                        }
                        promos &= (promos - 1);
                    }


                    while (movsDuplos > 0)
                    {
                        pFrom = (ulong)((long)movsDuplos & -(long)movsDuplos)<<16;
                        iFrom = bm.index(pFrom);
                        iTo = iFrom - 16;
                        move = new Move(pFrom, pFrom >> 16, tipoMovimento.MDUPLO, tipoPeca.PEAO,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movsDuplos = movsDuplos & (movsDuplos - 1);
                    }

                    while (movs > 0)
                    {
                        pFrom = (ulong)((long)movs & -(long)movs)<<8;
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
                    ulong promos = movs & bbConstants.R8;
ulong movsDuplos = ((movs & bbConstants.R3) << 8) & ~todas;
                    ulong pFrom;
                    int iFrom, iTo,j;

                    movs &= ~promos;

                    while (promos != 0)
                    {
                        pFrom = (ulong)((long)promos & -(long)promos) >> 8;
                        iFrom = bm.index(pFrom);
                        iTo = iFrom - 8;
                        for (j = (int)tipoPeca.RAINHA + corMover; j > (int)tipoPeca.PP; j -= 2)
                        {
                            move = new Move(pFrom, pFrom << 8, (tipoMovimento)((int)tipoMovimento.MPROMO + j),
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                tipoPeca.NENHUMA,
                                iFrom,
                                iTo);
                            moves.Add(move);

                        }
                        promos &= (promos - 1);
                    }

                    while (movsDuplos > 0)
                    {
                        pFrom = (ulong)((long)movsDuplos & -(long)movsDuplos)>>16;
                        iFrom = bm.index(pFrom);
                        iTo = iFrom + 16;
                        move = new Move(pFrom, pFrom << 16, tipoMovimento.MDUPLO, tipoPeca.PP,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movsDuplos = movsDuplos & (movsDuplos - 1);
                    }

                    while (movs > 0)
                    {
                        pFrom = (ulong)((long)movs & -(long)movs)>>8;
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
