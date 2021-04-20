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
        public ulong[] bbs;
        public int corMover;
        private BlackMagic bm;
        private DNA dna = DNA.Instance;
        public int[] vMat;
        public uint potencialRoque = 0;
        public int enPassant = -1;
        private ulong chave = 0;
        private TranspTable transp;
        public int[] vPos;


        public Board(BlackMagic bm, TranspTable transp)
        {
            bbs = new ulong[bbConstants.todosBBs];
            this.bm = bm;
            this.corMover = 0;
            vMat = new int[2];
            vMat[0] = 0;
            vMat[1] = 0;
       
            vPos = new int[2];
            vPos[0] = 0;
            vPos[1] = 0;
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
            board.vPos[0] = vPos[0];
            board.vPos[1] = vPos[1];
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

        public void gerarMobilidade()
        {
            ulong amigas;
            ulong inimigas;
            ulong todas;
            ulong pecas;
            ulong pecaMover;
            ulong movs;
            ulong occ;
            int index;
            uint origem;

            this.bbs[bbConstants.ATB] = 0;
            this.bbs[bbConstants.ATP] = 0;


            todas = bbs[bbConstants.PECAS] | bbs[bbConstants.PECAS + 1];
            bbs[bbConstants.ATB] = (bbs[(int)tipoPeca.PEAO] >> 8) & ~todas;
            bbs[bbConstants.ATP] = (bbs[(int)tipoPeca.PP] << 8) & ~todas;
            bbs[bbConstants.ATB] = bbs[bbConstants.ATB] | (((bbs[bbConstants.ATB] & bbConstants.R6) >> 8) & ~todas);
            bbs[bbConstants.ATP] = bbs[bbConstants.ATP] | (((bbs[bbConstants.ATP] & bbConstants.R3) << 8) & ~todas);
            pecas = bbs[(int)tipoPeca.PEAO];

            while (pecas > 0)
            {
                pecaMover = (ulong)((long)pecas & -(long)pecas);
                index = BlackMagic.index(pecaMover);
                movs = bm.aPeao[0][index] & bbs[bbConstants.PECAS + 1];
                bbs[bbConstants.ATB] |= movs;
                pecas &= (pecas - 1);
            }
            pecas = bbs[(int)tipoPeca.PP];

            while (pecas > 0)
            {
                pecaMover = (ulong)((long)pecas & -(long)pecas);
                index = BlackMagic.index(pecaMover);
                movs = bm.aPeao[1][index] & bbs[bbConstants.PECAS];
                bbs[bbConstants.ATP] |= movs;
                pecas &= (pecas - 1);
            }

            for (int cor = 0; cor < 2; cor++)
            {
                amigas = this.bbs[bbConstants.PECAS+cor];
                inimigas = this.bbs[bbConstants.PECAS + 1-cor];

                pecas = this.bbs[(int)tipoPeca.CAVALO + cor];
                while (pecas > 0)
                {
                    pecaMover = (ulong)((long)pecas & -(long)pecas);
                    index = BlackMagic.index(pecaMover);
                    movs = bm.mCavalo[index] & ~amigas;
                    this.bbs[bbConstants.ATB + cor] |= movs;
                    pecas &= (pecas - 1);
                }

                pecas = this.bbs[(int)tipoPeca.BISPO + cor] | this.bbs[(int)tipoPeca.RAINHA + cor];
                while (pecas > 0)
                {
                    pecaMover = (ulong)((long)pecas & -(long)pecas);
                    index = BlackMagic.index(pecaMover);
                    origem = bm.bispo[index].posicao;
                    occ = bm.bispo[index].mascara | todas;
                    occ *= bm.bispo[index].fator;
                    occ >>= (64 - 9);
                    origem += (uint)occ;

                    movs = bm.tabela[origem] & ~amigas;
                    this.bbs[bbConstants.ATB + cor] |= movs;
                    pecas &= (pecas - 1);
                }
                pecas = this.bbs[(int)tipoPeca.TORRE + cor] | this.bbs[(int)tipoPeca.RAINHA + cor];
                while (pecas > 0)
                {
                    pecaMover = (ulong)((long)pecas & -(long)pecas);
                    index = BlackMagic.index(pecaMover);
                    origem = bm.torre[index].posicao;
                    occ = bm.torre[index].mascara | todas;
                    occ *= bm.torre[index].fator;
                    occ >>= (64 - 12);
                    origem += (uint)occ;

                    movs = bm.tabela[origem] & ~amigas;
                    this.bbs[bbConstants.ATB + cor] |= movs;
                    pecas &= (pecas - 1);
                }
                pecaMover = bbs[(int)tipoPeca.REI + cor];
                index = BlackMagic.index(pecaMover);
                movs = bm.mRei[index] & ~amigas;
                this.bbs[bbConstants.ATB + cor] |= movs;
            }
        }

        public void addPeca(ulong posicao, tipoPeca peca, int index)
        {
            int cor = (int)peca % 2;
            this.bbs[(int)peca] ^= posicao;
            this.bbs[cor + bbConstants.PECAS] |= posicao;
            vMat[cor] += dna.vPecas[(int)peca];
            if (peca<tipoPeca.REI)
                vPos[cor] += dna.pPos[(int)peca][index];
            this.chave ^= transp.chaves[(int)peca, index];
#if DEBUG
            if (posicao != bm.getBBIndex(index))
                posicao = posicao;

            if (bbs[bbConstants.PECAS] != (bbs[0] | bbs[2] | bbs[4] | bbs[6] | bbs[8] | bbs[10]))
                this.print();

            if (bbs[bbConstants.PECAS+1] != (bbs[1] | bbs[3] | bbs[5] | bbs[7] | bbs[9] | bbs[11]))
                this.print();
#endif

        }

        public void removePeca(ulong posicao, tipoPeca peca, int index)
        {
//            if (peca == tipoPeca.NENHUMA)
 //               return;

//            if (this.getPecaPosicao(posicao, (int)peca % 2) != peca)
//                return;
            int cor = (int)peca % 2;
            this.bbs[(int)peca] ^= posicao;
            this.bbs[cor + bbConstants.PECAS] &= ~posicao;
            vMat[cor] -= dna.vPecas[(int)peca];
            if (peca < tipoPeca.REI)
                vPos[cor] -= dna.pPos[(int)peca][index];

            this.chave ^= transp.chaves[(int)peca, index];
#if DEBUG
            if (posicao != bm.getBBIndex(index))
                posicao = posicao;

            if (bbs[bbConstants.PECAS] != (bbs[0] | bbs[2] | bbs[4] | bbs[6] | bbs[8] | bbs[10]))
                this.print();

            if (bbs[bbConstants.PECAS + 1] != (bbs[1] | bbs[3] | bbs[5] | bbs[7] | bbs[9] | bbs[11]))
                this.chave = 1;
#endif

        }

        public void print()
        {
            ulong posicao = 1;
            int index = 0;
            string linha = "";
            Console.Out.WriteLine(linha);
            Console.Out.WriteLine(linha);
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

                if ((index + 1) % 8 == 0)
                {
                    Console.Out.WriteLine(linha);
                    linha = "";
                }
                index++;
                posicao <<= 1;
            } while (index < 64);
            if (corMover == 0)
                Console.Out.WriteLine("Branco para mover");
            else
                Console.Out.WriteLine("Preto para mover");

            Console.Out.WriteLine("EnPass " + enPassant.ToString());
            Console.Out.WriteLine("Roque  " + this.potencialRoque.ToString());

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

            iFrom = BlackMagic.index(bb);

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
           // this.print();

            if ((bm.tabela[index] & (bbs[(int)tipoPeca.BISPO + corAtacante] | bbs[(int)tipoPeca.RAINHA + corAtacante])) != 0)
                return true;

            return false;

        }


        public ulong ataquesACasa(ulong bb, int iFrom)
        {
            ulong ataques = 0;
            uint index;
            ulong todas = bbs[bbConstants.PECAS] | bbs[bbConstants.PECAS + 1];
            ulong occ;

            ataques |= (bm.aPeao[1][iFrom] & bbs[(int)tipoPeca.PEAO]);
            ataques |= (bm.aPeao[0][iFrom] & bbs[(int)tipoPeca.PP]);
            ataques |= (bm.mRei[iFrom] &(bbs[(int)tipoPeca.REI] | bbs[(int)tipoPeca.KP]));
            ataques |= (bm.mCavalo[iFrom] & (bbs[(int)tipoPeca.CAVALO] |bbs[(int)tipoPeca.CP]));
            

            index = bm.torre[iFrom].posicao;
            occ = bm.torre[iFrom].mascara | todas;
            occ *= bm.torre[iFrom].fator;
            occ >>= (64 - 12);
            index = index + (uint)occ;

            ataques |= bm.tabela[index] & (bbs[(int)tipoPeca.TORRE] | bbs[(int)tipoPeca.TP]
                                                | bbs[(int)tipoPeca.RAINHA] | bbs[(int)tipoPeca.KP]);

            index = bm.bispo[iFrom].posicao;
            occ = bm.bispo[iFrom].mascara | todas;
            occ *= bm.bispo[iFrom].fator;
            occ >>= (64 - 9);
            index = index + (uint)occ;

            ataques |= bm.tabela[index] & (bbs[(int)tipoPeca.BISPO] | bbs[(int)tipoPeca.BP]
                                                | bbs[(int)tipoPeca.RAINHA] | bbs[(int)tipoPeca.KP]);

            return ataques;

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

            this.enPassant = -1;

            if ((potencialRoque & (bbConstants.ROQUE_REI_BRANCO | bbConstants.ROQUE_RAINHA_BRANCO))!=0)
            {
                if (move.peca == tipoPeca.REI)
                    this.potencialRoque &= (bbConstants.ROQUE_REI_PRETO | bbConstants.ROQUE_RAINHA_PRETO);
                if (((move.bbFrom | move.bbTo) & bbConstants.I56) != 0)
                    this.potencialRoque &= ~bbConstants.ROQUE_RAINHA_BRANCO;
                if (((move.bbFrom | move.bbTo) & bbConstants.I63) != 0)
                    this.potencialRoque &= ~bbConstants.ROQUE_REI_BRANCO;
            }
            if  ((potencialRoque & (bbConstants.ROQUE_REI_PRETO | bbConstants.ROQUE_RAINHA_PRETO)) != 0)
            {
                if (move.peca == tipoPeca.KP)
                    this.potencialRoque &= (bbConstants.ROQUE_REI_BRANCO | bbConstants.ROQUE_RAINHA_BRANCO);
                if (((move.bbFrom | move.bbTo) & bbConstants.I00) != 0)
                    this.potencialRoque &= ~bbConstants.ROQUE_RAINHA_PRETO;
                if (((move.bbFrom | move.bbTo) & bbConstants.I07) != 0)
                    this.potencialRoque &= ~bbConstants.ROQUE_REI_PRETO;
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
                        if (this.corMover == 0)
                            this.enPassant = move.indiceDe - 8;
                        else
                            this.enPassant = move.indiceDe + 8;
                    } break;
                case tipoMovimento.MCAP:
                    {
                        //if (move.pecaCap == tipoPeca.NENHUMA)
                        //{
                        //    this.print();
                        //    move.print();
                        //}
                        this.removePeca(move.bbFrom, move.peca, move.indiceDe);
                        this.removePeca(move.bbTo, move.pecaCap, move.indicePara);
                        this.addPeca(move.bbTo, move.peca, move.indicePara);
                    } break;
                case tipoMovimento.MCAPENPASSANT:
                    {
                        this.removePeca(move.bbFrom, move.peca, move.indiceDe);
                        this.addPeca(move.bbTo, move.peca, move.indicePara);
                        if (corMover == 0)
                            this.removePeca(move.bbTo<<8, move.pecaCap, move.indicePara+8);
                        else
                            this.removePeca(move.bbTo>>8, move.pecaCap, move.indicePara - 8);
                    } break;

                case tipoMovimento.MROQUEK:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            removePeca(bbConstants.I60, tipoPeca.REI, 60);
                            removePeca(bbConstants.I63, tipoPeca.TORRE, 63);
                            addPeca(bbConstants.I62, tipoPeca.REI, 62);
                            addPeca(bbConstants.I61, tipoPeca.TORRE, 61);
                            potencialRoque |= bbConstants.ROQUE_BRANCO;
                        }
                        else
                        {
                            removePeca(bbConstants.I04, tipoPeca.KP, 4);
                            removePeca(bbConstants.I07, tipoPeca.TP, 7);
                            addPeca(bbConstants.I06, tipoPeca.KP, 6);
                            addPeca(bbConstants.I05, tipoPeca.TP, 5);
                            potencialRoque |= bbConstants.ROQUE_PRETO;
                        }
                    } break;

                case tipoMovimento.MROQUEQ:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            removePeca(bbConstants.I60, tipoPeca.REI, 60);
                            removePeca(bbConstants.I56, tipoPeca.TORRE, 56);
                            addPeca(bbConstants.I58, tipoPeca.REI, 58);
                            addPeca(bbConstants.I59, tipoPeca.TORRE, 59);
                            potencialRoque |= bbConstants.ROQUE_BRANCO;
                        }
                        else
                        {
                            removePeca(bbConstants.I04, tipoPeca.KP, 4);
                            removePeca(bbConstants.I00, tipoPeca.TP, 0);
                            addPeca(bbConstants.I02, tipoPeca.KP, 2);
                            addPeca(bbConstants.I03, tipoPeca.TP, 3);
                            potencialRoque |= bbConstants.ROQUE_PRETO;
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
                case tipoMovimento.MCAPENPASSANT:
                    {
                        this.addPeca(move.bbFrom, move.peca, move.indiceDe);
                        this.removePeca(move.bbTo, move.peca, move.indicePara);
                        if ((int)move.peca%2 == 0)
                          this.addPeca(move.bbTo<<8, move.pecaCap, move.indicePara + 8); 
                        else
                         this.addPeca(move.bbTo>>8, move.pecaCap, move.indicePara - 8);
                                                                                        }
                    break;


                case tipoMovimento.MROQUEK:
                    {
                        int cor = (int)move.peca % 2;
                        if (cor == 0)
                        {
                            addPeca(bbConstants.I60, tipoPeca.REI, 60);
                            addPeca(bbConstants.I63, tipoPeca.TORRE, 63);
                            removePeca(bbConstants.I62, tipoPeca.REI, 62);
                            removePeca(bbConstants.I61, tipoPeca.TORRE, 61);
                        }
                        else
                        {
                            addPeca(bbConstants.I04, tipoPeca.KP, 4);
                            addPeca(bbConstants.I07, tipoPeca.TP, 7);
                            removePeca(bbConstants.I06, tipoPeca.KP, 6);
                            removePeca(bbConstants.I05, tipoPeca.TP, 5);
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
                            removePeca(bbConstants.I58, tipoPeca.REI, 58);
                            removePeca(bbConstants.I59, tipoPeca.TORRE, 59);
                        }
                        else
                        {
                            addPeca(bbConstants.I04, tipoPeca.KP, 4);
                            addPeca(bbConstants.I00, tipoPeca.TP, 0);
                            removePeca(bbConstants.I02, tipoPeca.KP, 2);
                            removePeca(bbConstants.I03, tipoPeca.TP, 3);
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


        public List<Move> gerarMovimentos(List<Move> moves, bool quiet)
        {
            this.genMovsPeao(true, moves,quiet);
            this.genMovsCavalo(true, moves,quiet);
            this.genMovsBispo(tipoPeca.BISPO, true, moves,quiet);
            this.genMovsTorre(tipoPeca.TORRE, true, moves,quiet);
            this.genMovsBispo(tipoPeca.RAINHA, true, moves,quiet);
            this.genMovsTorre(tipoPeca.RAINHA, true, moves,quiet);
            this.genMovsRei(true, moves);

            if (!quiet)
            {
                this.genMovsPeao(false, moves);
                this.genMovsCavalo(false, moves);
                this.genMovsBispo(tipoPeca.BISPO, false, moves);
                this.genMovsTorre(tipoPeca.TORRE, false, moves);
                this.genMovsBispo(tipoPeca.RAINHA, false, moves);
                this.genMovsTorre(tipoPeca.RAINHA, false, moves);
                this.genMovsRei(false, moves);
            }


            return moves;
        }

        private void genMovsTorre(tipoPeca peca, bool capturas, List<Move> moves, bool quiet = false)
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
                    iFrom = BlackMagic.index(pFrom);
                    index = bm.torre[iFrom].posicao;
                    occ = bm.torre[iFrom].mascara | todas;
                    occ *= bm.torre[iFrom].fator;
                    occ >>= (64 - 12);
                    index = index + (uint)occ;

                    movs = bm.tabela[index] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)peca + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        move.score = this.See(iTo, pTo, pecaAtacada, iFrom, pFrom, (tipoPeca)(int)peca + corMover);
                        if ((!quiet) || (move.score > 0))
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
                    iFrom = BlackMagic.index(pFrom);

                    index = bm.torre[iFrom].posicao;
                    occ = bm.torre[iFrom].mascara | todas;
                    occ *= bm.torre[iFrom].fator;
                    occ >>= (64 - 12);
                    index = index + (uint)occ;

                    movs = bm.tabela[index] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
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

        private void genMovsBispo(tipoPeca peca, bool capturas, List<Move> moves, bool quiet = false)
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
                    iFrom = BlackMagic.index(pFrom);
                    index = bm.bispo[iFrom].posicao;
                    occ = bm.bispo[iFrom].mascara | todas;
                    occ *= bm.bispo[iFrom].fator;
                    occ >>= (64 - 9);
                    index = index + (uint) occ;

                    movs = bm.tabela[index] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)peca + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        move.score = this.See(iTo, pTo, pecaAtacada, iFrom, pFrom, (tipoPeca)(int)peca + corMover);
                        if ((!quiet) || (move.score > 0))
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
                    iFrom = BlackMagic.index(pFrom);

                    index = bm.bispo[iFrom].posicao;
                    occ = bm.bispo[iFrom].mascara | todas;
                    occ *= bm.bispo[iFrom].fator;
                    occ >>= (64 - 9);
                    index = index + (uint)occ;

                    movs = bm.tabela[index] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
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
                    iFrom = BlackMagic.index(reis);
                    movs = bm.mRei[iFrom] & inimigas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
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
                iFrom = BlackMagic.index(reis);
                movs = bm.mRei[iFrom] & ~todas;
                while (movs > 0)
                {
                    pTo = (ulong)((long)movs & -(long)movs);
                    iTo = BlackMagic.index(pTo);
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
                            && (!casaAtacada(bbConstants.I04, 0))
                            && (!casaAtacada(bbConstants.I05, 0))
                            && (!casaAtacada(bbConstants.I06, 0))
                            && (!casaAtacada(bbConstants.I07, 0)))
                        {
                            move = new Move(reis, bbConstants.I07,
                                tipoMovimento.MROQUEK,
                                tipoPeca.KP, tipoPeca.NENHUMA,
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
                                tipoPeca.KP, tipoPeca.NENHUMA,
                                4, 0);
                            moves.Add(move);
                        }

                    }

                }
            }

        }

        private void genMovsCavalo(bool capturas, List<Move> moves, bool quiet = false)
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
                    iFrom = BlackMagic.index(pFrom);
                    movs = bm.mCavalo[iFrom] & inimigas;
                    while (movs>0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
                        pecaAtacada = this.getPecaPosicao(pTo, 1 - corMover);
                        move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                            (tipoPeca)(int)tipoPeca.CAVALO + corMover,
                            pecaAtacada,
                            iFrom,
                            iTo
                            );
                        move.score = this.See(iTo, pTo, pecaAtacada, iFrom, pFrom, (tipoPeca)(int)tipoPeca.CAVALO + corMover);
                        if ((!quiet) || (move.score > 0))
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
                    iFrom = BlackMagic.index(pFrom);
                    movs = bm.mCavalo[iFrom] & ~todas;
                    while (movs > 0)
                    {
                        pTo = (ulong)((long)movs & -(long)movs);
                        iTo = BlackMagic.index(pTo);
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

        private void genMovsPeao(bool capturas, List<Move> moves, bool quiet = false)
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

                if (this.enPassant > -1)
                    inimigas |= bm.getBBIndex(this.enPassant);

                while (peoes != 0) 
                {
                    pFrom = (ulong)((long)peoes & -(long)peoes);
                    iFrom = BlackMagic.index(pFrom);
                    ataques = bm.aPeao[corMover][iFrom] & inimigas;
                    if (corMover == 0)
                        promos = ataques & bbConstants.R1;
                    else
                        promos = ataques & bbConstants.R8;

                    ataques &= ~promos;

                    while (promos != 0)
                    {
                        pTo = (ulong)((long)promos & -(long)promos);
                        iTo = BlackMagic.index(pTo);
                        pAtacada = this.getPecaPosicao(pTo, (byte)1 - corMover);
                        for (j = (int)tipoPeca.RAINHA + corMover; j > (int) tipoPeca.PP; j -= 2)
                        {
                            move = new Move(pFrom, pTo, (tipoMovimento)((int)tipoMovimento.MPROMOCAP + j),
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                pAtacada,
                                iFrom,
                                iTo);
                            move.score = move.score = this.See(iTo, pTo, pAtacada, iFrom, pFrom, (tipoPeca)j);
                            if ((!quiet) || (move.score>0))
                                moves.Add(move);
                            
                        }

                        promos = promos & (promos - 1);
                    }

                    while (ataques != 0)
                    {
                        pTo = (ulong)((long)ataques & -(long)ataques);
                        iTo = BlackMagic.index(pTo);
                        if (iTo == this.enPassant)
                        {
                            move = new Move(pFrom, pTo, tipoMovimento.MCAPENPASSANT,
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                (tipoPeca)((int)tipoPeca.PEAO + 1 - corMover),
                                iFrom,
                                iTo);
                            move.score = move.score = this.See(iTo, pTo, (tipoPeca)((int)tipoPeca.PEAO + corMover), iFrom, pFrom, (tipoPeca)(int)tipoPeca.PEAO + corMover);
                        }
                        else
                        {
                            pAtacada = this.getPecaPosicao(pTo, (byte)1 - corMover);
                           // if (pAtacada == tipoPeca.NENHUMA)
                           //     this.print();
                            move = new Move(pFrom, pTo, tipoMovimento.MCAP,
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                pAtacada,
                                iFrom,
                                iTo);
                            move.score = move.score = this.See(iTo, pTo, pAtacada, iFrom, pFrom, (tipoPeca)(int)tipoPeca.PEAO + corMover);
                        }
                        if ((!quiet) || (move.score > 0))
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
                        iFrom = BlackMagic.index(pFrom);
                        iTo = iFrom - 8;
                        for (j = (int)tipoPeca.RAINHA + corMover; j > (int)tipoPeca.PP; j -= 2)
                        {
                            move = new Move(pFrom, pFrom >> 8, (tipoMovimento)((int)tipoMovimento.MPROMO + j),
                                (tipoPeca)((int)tipoPeca.PEAO + corMover),
                                tipoPeca.NENHUMA,
                                iFrom,
                                iTo);
                            moves.Add(move);
                            move.score = j - (int)tipoPeca.PEAO;

                        }
                        promos &= (promos - 1);
                    }


                    while (movsDuplos > 0)
                    {
                        pFrom = (ulong)((long)movsDuplos & -(long)movsDuplos)<<16;
                        iFrom = BlackMagic.index(pFrom);
                        iTo = iFrom - 16;
                        move = new Move(pFrom, pFrom >> 16, tipoMovimento.MDUPLO, tipoPeca.PEAO,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movsDuplos = movsDuplos & (movsDuplos - 1);
                    }

                    while (movs > 0)
                    {
                        pFrom = (ulong)((long)movs & -(long)movs)<<8;
                        iFrom = BlackMagic.index(pFrom);
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
                        iFrom = BlackMagic.index(pFrom);
                        iTo = iFrom + 8;
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
                        iFrom = BlackMagic.index(pFrom);
                        iTo = iFrom + 16;
                        move = new Move(pFrom, pFrom << 16, tipoMovimento.MDUPLO, tipoPeca.PP,
                            tipoPeca.NENHUMA, iFrom, iTo);
                        moves.Add(move);
                        movsDuplos = movsDuplos & (movsDuplos - 1);
                    }

                    while (movs > 0)
                    {
                        pFrom = (ulong)((long)movs & -(long)movs)>>8;
                        iFrom = BlackMagic.index(pFrom);
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

        ulong recuperarAtacanteMaisBarato(ulong ataques, int cor, ref tipoPeca peca)
        {
            int i;
            ulong ataque;
            for (i = (int)tipoPeca.PEAO + cor; i < bbConstants.PECAS; i += 2)
            {
                ataque = ataques & bbs[i];
                if (ataque != 0)
                {
                    peca = (tipoPeca) i;
                    return (ulong)((long)ataque & -(long)ataque);
                }
            }            
            return 0;
        }

        ulong ataquesRaioX(ulong ocupacao, int iFrom)
        {
            uint index;
            ulong occ;
            ulong resposta;
            index = bm.torre[iFrom].posicao;
            occ = bm.torre[iFrom].mascara | ocupacao;
            occ *= bm.torre[iFrom].fator;
            occ >>= (64 - 12);
            index = index + (uint)occ;

            ulong xRays = ocupacao & bbs[(int)tipoPeca.TORRE] | bbs[(int)tipoPeca.RAINHA] |
                                            bbs[(int)tipoPeca.TP] | bbs[(int)tipoPeca.RP];

            resposta = (bm.tabela[index] & (xRays));

            index = bm.torre[iFrom].posicao;
            occ = bm.torre[iFrom].mascara | ocupacao;
            occ *= bm.torre[iFrom].fator;
            occ >>= (64 - 9);
            index = index + (uint)occ;
            xRays = ocupacao & bbs[(int)tipoPeca.BISPO] | bbs[(int)tipoPeca.RAINHA] |
                                            bbs[(int)tipoPeca.BP] | bbs[(int)tipoPeca.RP];

            resposta |= (bm.tabela[index] & xRays);

            return resposta;

        }

        public int See(int toIndex, ulong toBB, tipoPeca alvo, int fromIndex, ulong fromBB, tipoPeca peca)
        {
            int d = 0;
            int cor = (int) peca % 2;
            int[] ganho = new int[32];
            ulong ataquesPossiveis = bbs[(int)(tipoPeca.PEAO)] | bbs[(int)(tipoPeca.PP)] | bbs[(int)(tipoPeca.BISPO)] | bbs[(int)(tipoPeca.BP)]
                        | bbs[(int)(tipoPeca.TORRE)] | bbs[(int)(tipoPeca.TP)] | bbs[(int)tipoPeca.RAINHA] | bbs[(int)(tipoPeca.RP)]; 



            ulong occ = bbs[(int)bbConstants.PECAS] | bbs[(int)bbConstants.PECAS+1];
            ulong ataques = ataquesACasa(toBB, toIndex);
            //this.print();
            ganho[d] = dna.vPecas[(int)alvo];
            do
            {
                d++;
                ganho[d] = dna.vPecas[(int)peca] - ganho[d - 1];
                if (Math.Max(-ganho[d - 1], ganho[d]) < 0) break;
                ataques ^= fromBB;
                occ ^= fromBB;
                if ((fromBB & ataquesPossiveis) != 0)
                    ataques |= ataquesRaioX(occ, fromIndex);
                ataquesPossiveis &= ~fromBB;
                fromBB = recuperarAtacanteMaisBarato(ataques, 1 - cor, ref peca);
                fromIndex = BlackMagic.index(fromBB);
                cor = 1 - cor;
            } while (fromBB>0);
            while ((--d) > 0)
                ganho[d - 1] = -Math.Max(-ganho[d - 1], ganho[d]);
           return ganho[0];
        }

    }
}
