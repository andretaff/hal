using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.board;

namespace Hal.engine.board
{
    static class fen
    {
        public static bool isValida(string fen)
        {
            return fen.Length-fen.Replace("/","").Length == 7;
        }


    }
}
