using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.bitboard;

namespace Hal.engine.bitboard
{


    class blackMagic
    {
        public struct magic
        {
            public uint posicao;
            public ulong fator;
            public ulong mascara;
        }

        public ulong[] tabela;

        private ulong movsTorre(ulong pos, ulong occ)
        {
            ulong mascara = 0;
            ulong tPos = pos >> 1;
            while (((tPos & ( bbConstants.C1 | occ)) != 0))
            {
                mascara |= tPos;
                tPos >>= 1;
            }

            tPos = pos >> 1;
            while (((tPos & (bbConstants.R1 | occ)) != 0))
            {
                mascara |= tPos;
                tPos >>= 1;
            }

            tPos = pos << 8;
            while (((tPos & (bbConstants.R8 | occ)) != 0))
            {
                mascara |= tPos;
                tPos <<= 8;
            }

            tPos = pos << 1;
            while (((tPos & (bbConstants.C8 | occ)) != 0))
            {
                mascara |= tPos;
                tPos <<= 1;
            }

            return mascara;


        }

        private ulong movsBipo(ulong pos, ulong occ)
        {
            ulong mascara = 0;
            ulong tPos = pos >> 9;
            while (((tPos & (bbConstants.R1 | bbConstants.C1 | occ)) != 0))
            {
                mascara |= tPos;
                tPos >>= 9;
            }

            tPos = pos >> 7;
            while (((tPos & (bbConstants.R1 | bbConstants.C8 | occ)) != 0))
            {
                mascara |= tPos;
                tPos >>= 7;
            }

            tPos = pos << 9;
            while (((tPos & (bbConstants.R8 | bbConstants.C8 |occ)) != 0))
            {
                mascara |= tPos;
                tPos <<= 9;
            }

            tPos = pos << 7;
            while (((tPos & (bbConstants.R8 | bbConstants.C1 | occ)) != 0))
            {
                mascara |= tPos;
                tPos <<= 7;
            }

            return mascara;

        }

        public void iniciar()
        {
            int i;
            ulong pos;
            ulong mascara;
            ulong ataques;
            ulong oc;
            ulong posicao;

            this.tabela = new ulong[88508];

            for (i = 0; i < 64; i++)
            {
                pos = (ulong)Math.Pow(2, i);
                mascara = this.movsBipo(pos, 0);
                bispo[i].mascara = ~mascara;
                oc = 0;
                do
                {
                    this.gerarOcupacoes(mascara, ref oc);
                    ataques = this.movsBipo(pos, oc);

                    posicao = this.bispo[i].mascara | oc;
                    posicao *= this.bispo[i].fator;
                    posicao >>= (64 - 9);
                    posicao = posicao + this.bispo[i].posicao;
                    this.tabela[posicao] = ataques;
                 

                } while (oc != 0);


                mascara = this.movsTorre(pos, 0);
                torre[i].mascara = ~mascara;
                oc = 0;
                do
                {
                    this.gerarOcupacoes(mascara, ref oc);
                    ataques = this.movsTorre(pos, oc);

                    posicao = this.torre[i].mascara | oc;
                    posicao *= this.torre[i].fator;
                    posicao >>= (64-12);
                    posicao = posicao + this.torre[i].posicao;
                    this.tabela[posicao] = ataques;


                } while (oc != 0);


            }
        }

        private void gerarOcupacoes(ulong mascaraPrincipal, ref ulong mascaraAtual)
        {
            ushort[] bits;
            ushort[] atual;

            ulong posicao;
            int i;
            
            bits = new ushort[64];
            atual = new ushort[64];

            for (i = 0; i<64; i++)
            {
                posicao = (ulong) Math.Pow(2, i);
                if ((posicao & mascaraPrincipal) == 0)
                    bits[i] = 0;
                else
                    bits[i] = 1;

                if ((posicao & mascaraAtual) == 0)
                    atual[i] = 0;
                else
                    atual[i] = 1;

            }

            for (i = 0; i<64; i++)
            {
                if ((atual[i]==0) && (bits[i]==1))
                {
                    atual[i] = 1;
                    for (j=0; j<i; j++)
                    {
                        atual[i] = 0;
                    }
                    break;
                }
            }
            if (i == 64)
                mascaraAtual = 0;
            else
            {
                mascaraAtual = 0;
                for (i = 0; i < 64; i++)
                    mascaraAtual |= (ulong)Math.Pow(2, i) * atual[i];
            }
        }

        public magic[] bispo =          {   new magic      { fator = 0x107ac08050500bff, posicao = 66157 },
                                            new magic      { fator = 0x7fffdfdfd823fffd, posicao = 71730 },
                                            new magic      { fator = 0x0400c00fe8000200, posicao = 37781 },
                                            new magic      { fator = 0x103f802004000000, posicao = 21015 },
                                            new magic      { fator = 0xc03fe00100000000, posicao = 47590 },
                                            new magic      { fator = 0x24c00bffff400000, posicao =   835 },
                                            new magic      { fator = 0x0808101f40007f04, posicao = 23592 },
                                            new magic      { fator = 0x100808201ec00080, posicao = 30599 },
                                            new magic      { fator = 0xffa2feffbfefb7ff, posicao = 68776 },
                                            new magic      { fator = 0x083e3ee040080801, posicao = 19959 },
                                            new magic      { fator = 0x040180bff7e80080, posicao = 21783 },
                                            new magic      { fator = 0x0440007fe0031000, posicao = 64836 },
                                            new magic      { fator = 0x2010007ffc000000, posicao = 23417 },
                                            new magic      { fator = 0x1079ffe000ff8000, posicao = 66724 },
                                            new magic      { fator = 0x7f83ffdfc03fff80, posicao = 74542 },
                                            new magic      { fator = 0x080614080fa00040, posicao = 67266 },
                                            new magic      { fator = 0x7ffe7fff817fcff9, posicao = 26575 },
                                            new magic      { fator = 0x7ffebfffa01027fd, posicao = 67543 },
                                            new magic      { fator = 0x20018000c00f3c01, posicao = 24409 },
                                            new magic      { fator = 0x407e0001000ffb8a, posicao = 30779 },
                                            new magic      { fator = 0x201fe000fff80010, posicao = 17384 },
                                            new magic      { fator = 0xffdfefffde39ffef, posicao = 18778 },
                                            new magic      { fator = 0x7ffff800203fbfff, posicao = 65109 },
                                            new magic      { fator = 0x7ff7fbfff8203fff, posicao = 20184 },
                                            new magic      { fator = 0x000000fe04004070, posicao = 38240 },
                                            new magic      { fator = 0x7fff7f9fffc0eff9, posicao = 16459 },
                                            new magic      { fator = 0x7ffeff7f7f01f7fd, posicao = 17432 },
                                            new magic      { fator = 0x3f6efbbf9efbffff, posicao = 81040 },
                                            new magic      { fator = 0x0410008f01003ffd, posicao = 84946 },
                                            new magic      { fator = 0x20002038001c8010, posicao = 18276 },
                                            new magic      { fator = 0x087ff038000fc001, posicao =  8512 },
                                            new magic      { fator = 0x00080c0c00083007, posicao = 78544 },
                                            new magic      { fator = 0x00000080fc82c040, posicao = 19974 },
                                            new magic      { fator = 0x000000407e416020, posicao = 23850 },
                                            new magic      { fator = 0x00600203f8008020, posicao = 11056 },
                                            new magic      { fator = 0xd003fefe04404080, posicao = 68019 },
                                            new magic      { fator = 0x100020801800304a, posicao = 85965 },
                                            new magic      { fator = 0x7fbffe700bffe800, posicao = 80524 },
                                            new magic      { fator = 0x107ff00fe4000f90, posicao = 38221 },
                                            new magic      { fator = 0x7f8fffcff1d007f8, posicao = 64647 },
                                            new magic      { fator = 0x0000004100f88080, posicao = 61320 },
                                            new magic      { fator = 0x00000020807c4040, posicao = 67281 },
                                            new magic      { fator = 0x00000041018700c0, posicao = 79076 },
                                            new magic      { fator = 0x0010000080fc4080, posicao = 17115 },
                                            new magic      { fator = 0x1000003c80180030, posicao = 50718 },
                                            new magic      { fator = 0x2006001cf00c0018, posicao = 24659 },
                                            new magic      { fator = 0xffffffbfeff80fdc, posicao = 38291 },
                                            new magic      { fator = 0x000000101003f812, posicao = 30605 },
                                            new magic      { fator = 0x0800001f40808200, posicao = 37759 },
                                            new magic      { fator = 0x084000101f3fd208, posicao =  4639 },
                                            new magic      { fator = 0x080000000f808081, posicao = 21759 },
                                            new magic      { fator = 0x0004000008003f80, posicao = 67799 },
                                            new magic      { fator = 0x08000001001fe040, posicao = 22841 },
                                            new magic      { fator = 0x085f7d8000200a00, posicao = 66689 },
                                            new magic      { fator = 0xfffffeffbfeff81d, posicao = 62548 },
                                            new magic      { fator = 0xffbfffefefdff70f, posicao = 66597 },
                                            new magic      { fator = 0x100000101ec10082, posicao = 86749 },
                                            new magic      { fator = 0x7fbaffffefe0c02f, posicao = 69558 },
                                            new magic      { fator = 0x7f83fffffff07f7f, posicao = 61589 },
                                            new magic      { fator = 0xfff1fffffff7ffc1, posicao = 62533 },
                                            new magic      { fator = 0x0878040000ffe01f, posicao = 64387 },
                                            new magic      { fator = 0x005d00000120200a, posicao = 26581 },
                                            new magic      { fator = 0x0840800080200fda, posicao = 76355 },
                                            new magic      { fator = 0x100000c05f582008, posicao = 11140 }
                                        };

        public readonly magic[] torre ={    new magic      { fator = 0x80280013ff84ffff, posicao = 10890 },
	                                        new magic      { fator = 0x5ffbfefdfef67fff, posicao = 56054 },
	                                        new magic      { fator = 0xffeffaffeffdffff, posicao = 67495 },
	                                        new magic      { fator = 0x003000900300008a, posicao = 72797 },
	                                        new magic      { fator = 0x0030018003500030, posicao = 17179 },
	                                        new magic      { fator = 0x0020012120a00020, posicao = 63978 },
	                                        new magic      { fator = 0x0030006000c00030, posicao = 56650 },
	                                        new magic      { fator = 0xffa8008dff09fff8, posicao = 15929 },
	                                        new magic      { fator = 0x7fbff7fbfbeafffc, posicao = 55905 },
	                                        new magic      { fator = 0x0000140081050002, posicao = 26301 },
	                                        new magic      { fator = 0x0000180043800048, posicao = 78100 },
	                                        new magic      { fator = 0x7fffe800021fffb8, posicao = 86245 },
	                                        new magic      { fator = 0xffffcffe7fcfffaf, posicao = 75228 },
	                                        new magic      { fator = 0x00001800c0180060, posicao = 31661 },
	                                        new magic      { fator = 0xffffe7ff8fbfffe8, posicao = 38053 },
	                                        new magic      { fator = 0x0000180030620018, posicao = 37433 },
	                                        new magic      { fator = 0x00300018010c0003, posicao = 74747 },
	                                        new magic      { fator = 0x0003000c0085ffff, posicao = 53847 },
	                                        new magic      { fator = 0xfffdfff7fbfefff7, posicao = 70952 },
	                                        new magic      { fator = 0x7fc1ffdffc001fff, posicao = 49447 },
	                                        new magic      { fator = 0xfffeffdffdffdfff, posicao = 62629 },
	                                        new magic      { fator = 0x7c108007befff81f, posicao = 58996 },
	                                        new magic      { fator = 0x20408007bfe00810, posicao = 36009 },
	                                        new magic      { fator = 0x0400800558604100, posicao = 21230 },
	                                        new magic      { fator = 0x0040200010080008, posicao = 51882 },
	                                        new magic      { fator = 0x0010020008040004, posicao = 11841 },
	                                        new magic      { fator = 0xfffdfefff7fbfff7, posicao = 25794 },
	                                        new magic      { fator = 0xfebf7dfff8fefff9, posicao = 49689 },
	                                        new magic      { fator = 0xc00000ffe001ffe0, posicao = 63400 },
	                                        new magic      { fator = 0x2008208007004007, posicao = 33958 },
	                                        new magic      { fator = 0xbffbfafffb683f7f, posicao = 21991 },
	                                        new magic      { fator = 0x0807f67ffa102040, posicao = 45618 },
	                                        new magic      { fator = 0x200008e800300030, posicao = 70134 },
	                                        new magic      { fator = 0x0000008780180018, posicao = 75944 },
	                                        new magic      { fator = 0x0000010300180018, posicao = 68392 },
	                                        new magic      { fator = 0x4000008180180018, posicao = 66472 },
	                                        new magic      { fator = 0x008080310005fffa, posicao = 23236 },
	                                        new magic      { fator = 0x4000188100060006, posicao = 19067 },
	                                        new magic      { fator = 0xffffff7fffbfbfff, posicao =     0 },
	                                        new magic      { fator = 0x0000802000200040, posicao = 43566 },
	                                        new magic      { fator = 0x20000202ec002800, posicao = 29810 },
	                                        new magic      { fator = 0xfffff9ff7cfff3ff, posicao = 65558 },
	                                        new magic      { fator = 0x000000404b801800, posicao = 77684 },
	                                        new magic      { fator = 0x2000002fe03fd000, posicao = 73350 },
	                                        new magic      { fator = 0xffffff6ffe7fcffd, posicao = 61765 },
	                                        new magic      { fator = 0xbff7efffbfc00fff, posicao = 49282 },
	                                        new magic      { fator = 0x000000100800a804, posicao = 78840 },
	                                        new magic      { fator = 0xfffbffefa7ffa7fe, posicao = 82904 },
	                                        new magic      { fator = 0x0000052800140028, posicao = 24594 },
	                                        new magic      { fator = 0x00000085008a0014, posicao =  9513 },
	                                        new magic      { fator = 0x8000002b00408028, posicao = 29012 },
	                                        new magic      { fator = 0x4000002040790028, posicao = 27684 },
	                                        new magic      { fator = 0x7800002010288028, posicao = 27901 },
	                                        new magic      { fator = 0x0000001800e08018, posicao = 61477 },
	                                        new magic      { fator = 0x1890000810580050, posicao = 25719 },
	                                        new magic      { fator = 0x2003d80000500028, posicao = 50020 },
	                                        new magic      { fator = 0xfffff37eefefdfbe, posicao = 41547 },
	                                        new magic      { fator = 0x40000280090013c1, posicao =  4750 },
	                                        new magic      { fator = 0xbf7ffeffbffaf71f, posicao =  6014 },
	                                        new magic      { fator = 0xfffdffff777b7d6e, posicao = 41529 },
	                                        new magic      { fator = 0xeeffffeff0080bfe, posicao = 84192 },
	                                        new magic      { fator = 0xafe0000fff780402, posicao = 33433 },
	                                        new magic      { fator = 0xee73fffbffbb77fe, posicao =  8555 },
	                                        new magic      { fator = 0x0002000308482882, posicao =  1009 }
                                        };
    }
}
