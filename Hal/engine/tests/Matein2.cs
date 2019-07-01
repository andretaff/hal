using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.bitboard;
using Hal.engine.board;
using Hal.game;
using Hal.userInterface;
using Hal.engine.bitboard;

namespace Hal.engine.tests
{
    class Matein2 : Testing
    {
        private Uci uci;
        private Game game;

        public Matein2(Uci uci, Game game)
        {
            this.uci = uci;
            this.game = game;
        }
        public void test()
        {
            System.IO.StreamReader file;
            string linha;
            int estagio = 0;
            file = new System.IO.StreamReader("matein2.txt");
            while ((linha = file.ReadLine())!= null)
            {
                if ((estagio == 0) && Fen.isValida(linha))
                {
                    game.setFenPosition(linha);
                    estagio++;
                }
                else if (estagio == 1)
                {
                    string pecaMov = ""+linha[0];
                    string casas = linha.Substring(1);
                    uci.enviarComandoParaConsole(linha);
                    game.start(tipoTempo.infinito, 0);
                    game.run();
                    this.Assert((bbConstants.sPecas[(int)game.BestMove.peca].ToString().ToUpper() == pecaMov) &&
                        (BlackMagic.getBBFromAlg(casas) == game.BestMove.bbTo), "mov errado");
                    estagio = 0;
                    uci.enviarComandoParaConsole("ok");
                }
                else
                {
                    uci.enviarComandoParaConsole(linha);
                }
            }
        }

    }
}
