using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.bitboard
{
    enum tipoPeca
    {
        PEAO = 0,
        PP = 1,
        CAVALO = 2,
        CP = 3,
        BISPO = 4,
        BP =5,
        TORRE = 6,
        TP = 7,
        RAINHA = 8,
        RP = 9,
        REI = 10,
        KP = 11,
        NENHUMA = 15
    }

    static class bbConstants
    {
        public const byte todosBBs = 16;
        public const byte PECAS = 12;
        public const string sPecas = "PpNnBbRrQqKk";


        public const ulong R1 = 0x00000000000000FF;
        public const ulong R2 = 0x000000000000FF00;
        public const ulong R3 = 0x0000000000FF0000;
        public const ulong R4 = 0x00000000FF000000;
        public const ulong R5 = 0x000000FF00000000;
        public const ulong R6 = 0x0000FF0000000000;
        public const ulong R7 = 0x00FF000000000000;
        public const ulong R8 = 0xFF00000000000000;

        public const ulong C1 = 0x0101010101010101;
        public const ulong C2 = 0x0202020202020202;
        public const ulong C3 = 0x0404040404040404;
        public const ulong C4 = 0x0808080808080808;
        public const ulong C5 = 0x1010101010101010;
        public const ulong C6 = 0x2020202020202020;
        public const ulong C7 = 0x4040404040404040;
        public const ulong C8 = 0x8080808080808080;

        public const ulong I00 = C1 & R1;
        public const ulong I01 = C2 & R1;
        public const ulong I02 = C3 & R1;
        public const ulong I03 = C4 & R1;
        public const ulong I04 = C5 & R1;
        public const ulong I05 = C6 & R1;
        public const ulong I06 = C7 & R1;
        public const ulong I07 = C8 & R1;
        public const ulong I55 = C8 & R7;

        public const ulong I56 = C1 & R8;
        public const ulong I57 = C2 & R8;
        public const ulong I58 = C3 & R8;
        public const ulong I59 = C4 & R8;
        public const ulong I60 = C5 & R8;
        public const ulong I61 = C6 & R8;
        public const ulong I62 = C7 & R8;
        public const ulong I63 = C8 & R8;

        
    }
}
