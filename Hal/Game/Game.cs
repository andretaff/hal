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
        TranspTable tabela;
        Move bestMove;

        internal Move BestMove { get => bestMove; set => bestMove = value; }

        public Game(Uci uci)
        {
            bm = new BlackMagic();
            this.tabela = new TranspTable(9999999);
            tabuleiro = new Board(bm,tabela);
            this.uci = uci;
            this.negamax = new NegaMax(uci,tabela);
        }

        public void run()
        {
            bestMove = negamax.go(tabuleiro,tTemporizador,temporizador);
            uci.enviarComandoParaConsole("bestmove " + bestMove.toAlgebra());
        }

        public void newGame()
        {
            this.tabuleiro = Fen.tabuleiroPadrao(bm,tabela);
        }

        public void setFenPosition(string fenString)
        {
            this.tabuleiro = Fen.lerFen(bm, tabela, fenString);
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
