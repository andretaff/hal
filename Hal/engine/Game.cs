using Hal.bitboard;
using Hal.engine.bitboard;
using Hal.engine.board;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.engine
{
    class Game
    {
        BlackMagic bm;
        Board tabuleiro;

        public Game()
        {
            bm = new BlackMagic();
            tabuleiro = new Board(bm);
            tabuleiro.addPeca(bm.getBBIndex(34), tipoPeca.PEAO);
            tabuleiro.addPeca(bm.getBBIndex(25), tipoPeca.CP);
            tabuleiro.addPeca(bm.getBBIndex(27), tipoPeca.CP);
            tabuleiro.addPeca(bm.getBBIndex(41), tipoPeca.CP);
            tabuleiro.addPeca(bm.getBBIndex(43), tipoPeca.CP);
            tabuleiro.gerarMovimentos();
        }
    }
}
