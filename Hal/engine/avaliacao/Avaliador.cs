using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.board;

namespace Hal.engine.avaliacao
{
    
    class Avaliador
    {
        private DNA dna = DNA.Instance;
        public int avaliar(Board tabuleiro)
        {
            if (tabuleiro.corMover == 0)
                return tabuleiro.vMat[0] - tabuleiro.vMat[1];
            else
                return tabuleiro.vMat[1] - tabuleiro.vMat[0];

        }
        
    }
}
