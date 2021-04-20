using Hal.engine.bitboard;
using Hal.game;
using Hal.userInterface;
using System;
using System.Threading;

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
            this.startTime();
            while ((linha = file.ReadLine())!= null)
            {
                if ((estagio == 0) && Fen.isValida(linha))
                {
                    game.setFenPosition(linha);
                    estagio++;
                }
                else if (estagio == 1)
                {
                    linha = linha.Replace("x", "");
                    Thread.Sleep(1000);
                    string pecaMov = ""+linha[0];
                    string casas = linha.Substring(1);

                  


                    uci.enviarComandoParaConsole(linha);
                    game.start(tipoTempo.infinito, 0);
                    game.run();
                    string movEngine = game.BestMove.toAlgebra();
                    uci.enviarComandoParaConsole("mov engine: " + movEngine);
                    Thread.Sleep(1000);

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
            file.Close();
            System.IO.StreamWriter arquivo = new System.IO.StreamWriter("matein2Results.txt", true);
            arquivo.WriteLine(DateTime.Now.ToString("dd/MM/yy", System.Globalization.DateTimeFormatInfo.InvariantInfo) +
                                " - tempo " + Convert.ToString(this.timeEllapsed()) + " - " + GConstants.ENGINE_NAME);

            uci.enviarComandoParaConsole(DateTime.Now.ToString("dd/MM/yy", System.Globalization.DateTimeFormatInfo.InvariantInfo) +
                                " - tempo " + Convert.ToString(this.timeEllapsed()) + " - " + GConstants.ENGINE_NAME);
            arquivo.Close();
        }


    }
}
