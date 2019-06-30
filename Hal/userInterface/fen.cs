using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.board;
using Hal.engine.bitboard;

namespace Hal.userInterface
{
    static class Fen
    {
        public static bool isValida(string fen)
        {
            return fen.Length-fen.Replace("/","").Length == 7;
        }

        public static Board tabuleiroPadrao(BlackMagic bm)
        {
            return lerFen(bm,"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq");
        }

        public static Board lerFen(BlackMagic bm, string fenString)
        {
            int tipo = 0;
            int i,j;
            Char c;

            Board tabuleiro = new Board(bm);

            i = 0;
            int posicao = 0;

            while (i < fenString.Length)
            {
                c = fenString[i++];
                if (c == ' ')
                {
                    tipo++;
                    continue;
                }
                if (tipo == 0)
                {
                    if (c == '/')
                        continue;

                    if (Char.IsDigit(c))
                        posicao += ((int)Char.GetNumericValue(c));
                    else
                    {
                        for (j = 0; j < bbConstants.PECAS; j++)
                            if (bbConstants.sPecas[j] == c)
                                break;
                        tabuleiro.addPecaHumana((tipoPeca)j, posicao);
                        posicao++;
                    }
                }
                if (tipo == 1) //roques
                {
                    if (c == 'w')
                        tabuleiro.corMover = 0;
                    else
                        tabuleiro.corMover = 1;
                }
                if (tipo == 2)
                {
                    switch (c)
                    {
                        case 'q':
                            tabuleiro.potencialRoque |= bbConstants.ROQUE_RAINHA_PRETO;
                            break;
                        case 'Q':
                            tabuleiro.potencialRoque |= bbConstants.ROQUE_RAINHA_BRANCO;
                            break;
                        case 'k':
                            tabuleiro.potencialRoque |= bbConstants.ROQUE_REI_PRETO;
                            break;
                        case 'K':
                            tabuleiro.potencialRoque |= bbConstants.ROQUE_REI_BRANCO;
                            break;
                        case '-':
                            break;
                    }
                }
            }

            return tabuleiro;

        }


    }
}
