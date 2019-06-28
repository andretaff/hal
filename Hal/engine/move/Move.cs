using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.bitboard;

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

        public Move(ulong bbFrom, ulong bbTo, tipoMovimento tipo, tipoPeca peca, tipoPeca pecaCap, int indiceDe, int indicePara )
        {
            this.bbFrom = bbFrom;
            this.bbTo = bbTo;
            this.peca = peca;
            this.pecaCap = pecaCap;
            this.indiceDe = indiceDe;
            this.indicePara = indicePara;
            this.tipo = tipo;
        }

        private string bbToString(ulong bb)
        {
            string s = "";
            if ((bb & bbConstants.R1) != 0)
                s = "a";
            else if ((bb & bbConstants.R2) != 0)
                s = "b";
            else if ((bb & bbConstants.R3) != 0)
                s = "c";
            else if ((bb & bbConstants.R4) != 0)
                s = "d";
            else if ((bb & bbConstants.R5) != 0)
                s = "e";
            else if ((bb & bbConstants.R6) != 0)
                s = "f";
            else if ((bb & bbConstants.R7) != 0)
                s = "g";
            else if ((bb & bbConstants.R8) != 0)
                s = "h";

            if ((bb & bbConstants.C1) != 0)
                s = s+"1";
            else if ((bb & bbConstants.C2) != 0)
                s = s + "2";
            else if ((bb & bbConstants.C3) != 0)
                s = s + "3";
            else if ((bb & bbConstants.C4) != 0)
                s = s + "4";
            else if ((bb & bbConstants.C5) != 0)
                s = s + "5";
            else if ((bb & bbConstants.C6) != 0)
                s = s + "6";
            else if ((bb & bbConstants.C7) != 0)
                s = s + "7";
            else if ((bb & bbConstants.C8) != 0)
                s = s + "8";

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
            //else
            return s;

        }
    }
}
