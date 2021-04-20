using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.board;
using Hal.engine.bitboard;
namespace Hal.engine.avaliacao
{
    
    class Avaliador
    {
        private DNA dna = DNA.Instance;
        public int avaliar(Board tabuleiro)
        {
            int mB, mP;
            mB = 0; mP = 0;

            if ((tabuleiro.vMat[0]+tabuleiro.vMat[1]>1000))
            {
                mB = tabuleiro.vPos[0] + dna.pPos[0][BlackMagic.index(tabuleiro.bbs[(int)tipoPeca.REI])];
                mP = tabuleiro.vPos[1] + dna.pPos[1][BlackMagic.index(tabuleiro.bbs[(int)tipoPeca.KP])];
            }
            else
            {
                mB = tabuleiro.vPos[0] + dna.pPos[0][BlackMagic.index(tabuleiro.bbs[(int)tipoPeca.REI+2])];
                mP = tabuleiro.vPos[1] + dna.pPos[1][BlackMagic.index(tabuleiro.bbs[(int)tipoPeca.KP+2])];
            }
            //tabuleiro.print();
            if (tabuleiro.corMover == 0)
                return tabuleiro.vMat[0] +1 - tabuleiro.vMat[1] +mB -mP;
            else
                return tabuleiro.vMat[1] +1 - tabuleiro.vMat[0] +mP -mB;

        }

        //private int avaliarPosicionamento(Board tabuleiro)
        //{
        //
//        }
        
    }
}
