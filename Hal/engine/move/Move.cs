using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.bitboard;

namespace Hal.engine.move
{
    enum tipoMovimento { MNORMAL = 0,
        MDUPLO = 1,
        MCAP = 2,
        MCAPENPASSANT=3,
        MROQUEQ = 4,
        MROQUEK = 5,
        MPROMO = 10,
        MPROMON = 11,
        MPROMOB = 12,
        MPROMOR = 13,
        MPROMOQ = 14,
        MPROMOCAP = 50,
        MPROMOCAPN = 51,
        MPROMOCAPB = 52,
        MPROMOCAPR = 53,
        MPROMOCAPQ = 54
    } 

    class Move
    {
        public ulong bbFrom;
        public ulong bbTo;
        public tipoPeca peca;
        public tipoPeca pecaCap;
        public tipoMovimento tipo;
        public int indiceDe;
        public int indicePara;
        public uint potencialRoque;
        public int  enPassant;

        public Move(ulong bbFrom, ulong bbTo, tipoMovimento tipo, tipoPeca peca, tipoPeca pecaCap, int indiceDe, int indicePara )
        {
            this.bbFrom = bbFrom;
            this.bbTo = bbTo;
            this.peca = peca;
            this.pecaCap = pecaCap;
            this.indiceDe = indiceDe;
            this.indicePara = indicePara;
            this.tipo = tipo;
            this.enPassant = -1;
            this.potencialRoque = 0;
        }

        public Move()
        {
            this.enPassant = -1;
            this.potencialRoque = 0;
        }

        public Move(Move copiar)
        {
            this.bbFrom = copiar.bbFrom;
            this.bbTo = copiar.bbTo;
            this.peca = copiar.peca;
            this.pecaCap = copiar.pecaCap;
            this.indiceDe = copiar.indiceDe;
            this.indicePara = copiar.indicePara;
            this.tipo = copiar.tipo;
            this.enPassant = copiar.enPassant;
            this.potencialRoque = copiar.potencialRoque;        }

        public void print(int ply = 0, int maxply = 0)
        {
            string saida = new string(' ', 5*(maxply-ply))+ bbConstants.sPecas[(int)this.peca]+ this.toAlgebra();
            Console.Out.WriteLine(saida);
        }

        public string toAlgebra()
        {
            string s="";
            tipoPeca pecaPromo = tipoPeca.NENHUMA;
            if (!((tipo == tipoMovimento.MROQUEK) || (tipo == tipoMovimento.MROQUEQ)))
            {
                s = BlackMagic.bbToString(bbFrom) + BlackMagic.bbToString(bbTo);
                if (tipo > tipoMovimento.MPROMOCAP)
                {
                    pecaPromo = (tipoPeca)((int)tipo - (int)tipoMovimento.MPROMOCAP);
                }
                else if (tipo > tipoMovimento.MPROMO)
                {
                    pecaPromo = (tipoPeca)((int)tipo - (int)tipoMovimento.MPROMO);
                }
                if (pecaPromo != tipoPeca.NENHUMA)
                {
                    s = s + bbConstants.sPecas[(int)pecaPromo];
                }
            }
            else
            {
                if (tipo == tipoMovimento.MROQUEK){
                    if ((int) peca % 2 == 0 )
                    {
                        s = "e1h1";
                    }
                    else
                    {
                        s = "e8h8";
                    }
                }
                else
                {
                    if ((int)peca % 2 == 0)
                    {
                        s = "e1a1";
                    }
                    else
                    {
                        s = "e8a8";
                    }
                }
            }
            return s;

        }
    }
}
