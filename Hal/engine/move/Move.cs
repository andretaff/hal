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

        public string toAlgebra()
        {
            return "";
        }
    }
}
