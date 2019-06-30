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
        MROQUEQ = 3,
        MROQUEK = 4,
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

    struct Move
    {
        public ulong bbFrom;
        public ulong bbTo;
        public tipoPeca peca;
        public tipoPeca pecaCap;
        public tipoMovimento tipo;
        public int indiceDe;
        public int indicePara;
        public uint potencialRoque;
        public ulong enPassant;

        public Move(ulong bbFrom, ulong bbTo, tipoMovimento tipo, tipoPeca peca, tipoPeca pecaCap, int indiceDe, int indicePara )
        {
            this.bbFrom = bbFrom;
            this.bbTo = bbTo;
            this.peca = peca;
            this.pecaCap = pecaCap;
            this.indiceDe = indiceDe;
            this.indicePara = indicePara;
            this.tipo = tipo;
            this.enPassant = 0;
            this.potencialRoque = 0;
        }

        public void print()
        {
            string saida = bbConstants.sPecas[(int)this.peca]+ this.toAlgebra();
            Console.Out.WriteLine(saida);
        }

        private string bbToString(ulong bb)
        {
            string s = "";
            if ((bb & bbConstants.C1) != 0)
                s = s + "a";
            else if ((bb & bbConstants.C2) != 0)
                s = s + "b";
            else if ((bb & bbConstants.C3) != 0)
                s = s + "c";
            else if ((bb & bbConstants.C4) != 0)
                s = s + "d";
            else if ((bb & bbConstants.C5) != 0)
                s = s + "e";
            else if ((bb & bbConstants.C6) != 0)
                s = s + "f";
            else if ((bb & bbConstants.C7) != 0)
                s = s + "g";
            else if ((bb & bbConstants.C8) != 0)
                s = s + "h";

            if ((bb & bbConstants.R1) != 0)
                s += "8";
            else if ((bb & bbConstants.R2) != 0)
                s += "7";
            else if ((bb & bbConstants.R3) != 0)
                s += "6";
            else if ((bb & bbConstants.R4) != 0)
                s += "5";
            else if ((bb & bbConstants.R5) != 0)
                s += "4";
            else if ((bb & bbConstants.R6) != 0)
                s += "3";
            else if ((bb & bbConstants.R7) != 0)
                s += "2";
            else if ((bb & bbConstants.R8) != 0)
                s += "1";

            return s;
        }

        public string toAlgebra()
        {
            string s="";
            tipoPeca peca =tipoPeca.NENHUMA;
            if (!((tipo == tipoMovimento.MROQUEK) || (tipo == tipoMovimento.MROQUEQ)))
            {
                s = this.bbToString(bbFrom) + this.bbToString(bbTo);
                if (tipo > tipoMovimento.MPROMOCAP)
                {
                    peca = (tipoPeca)((int)tipo - (int)tipoMovimento.MPROMOCAP);
                }
                else if (tipo > tipoMovimento.MPROMO)
                {
                    peca = (tipoPeca)((int)tipo - (int)tipoMovimento.MPROMO);
                }
                if (peca != tipoPeca.NENHUMA)
                {
                    s = s + bbConstants.sPecas[(int)peca];
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
