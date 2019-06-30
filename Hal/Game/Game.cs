using Hal.engine.bitboard;
using Hal.engine.board;
using Hal.userInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hal.engine.negamax;
using Hal.engine.move;

namespace Hal.game
{
    class Game
    {
        BlackMagic bm;
        Board tabuleiro;
        TimeControl temporizador;
        Uci uci;
        NegaMax negamax;
        Thread tTemporizador;

        public Game(Uci uci)
        {
            bm = new BlackMagic();
            tabuleiro = new Board(bm);
            this.uci = uci;
            this.negamax = new NegaMax(uci);
        }

        public void run()
        {
            Move move = negamax.go(tabuleiro,tTemporizador,temporizador);
            uci.enviarComandoParaConsole("bestmove " + move.toAlgebra());
        }

        public void newGame()
        {
            this.tabuleiro = Fen.tabuleiroPadrao(bm);
        }

        public void setFenPosition(string fenString)
        {
            this.tabuleiro = Fen.lerFen(bm, fenString);
        }

        public void start(tipoTempo tipo, ulong miliSecs )
        {
            temporizador = new TimeControl(tipo, miliSecs);
            tTemporizador = new Thread(new ThreadStart(temporizador.Run));
            tTemporizador.Start();    
                      

        }

        public void stop()
        {
            temporizador.parar();
        }
    }
}
