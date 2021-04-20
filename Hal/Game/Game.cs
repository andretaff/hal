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
            //tabela.Clear();
        }

        public void newGame()
        {
            this.tabuleiro = Fen.tabuleiroPadrao(bm,tabela);
        }


        public void Print()
        {
            this.tabuleiro.print();
        }
        public void setFenPosition(string fenString)
        {
            this.tabuleiro = Fen.lerFen(bm, tabela, fenString);
            //this.tabuleiro.print();
        }

        public void start(tipoTempo tipo, ulong miliSecs )
        {
            temporizador = new TimeControl(tipo, miliSecs);
            tTemporizador = new Thread(new ThreadStart(temporizador.Run));
            tTemporizador.Start();    
                 

        }

        public void makeHumanMoves(string moves)
        {
            string move;
            int i;
            Move movimento;
            int pos;
            bool achou;
            List<Move> movel;
            while (moves.Trim() != "")
            {
                pos = moves.IndexOf(" ");
                if (pos < 0)
                    pos = moves.Length;

                move = moves.Substring(0, pos);
                moves = moves.Substring(move.Length, moves.Length- move.Length).Trim();
                movel = new List<Move>();

                movel = this.tabuleiro.gerarMovimentos(movel,false);
                achou = false;
                for (i = 0; i<movel.Count; i++)
                {
                    movimento = movel[i];
                    //Console.Out.WriteLine(" Testando "+move);
                    //tabuleiro.print();
                    if (movimento.toAlgebra().ToUpper() == move.ToUpper())
                    {
                        tabuleiro.makeMove(movimento);
                        achou = true;
                        //Console.Out.WriteLine(" ---------------------------- ");
                        break;
                    }
                    //else
                   //     movimento.print();
                }
                if (!achou)
                    throw new Exception("movimento errado");
                    


            }
            //tabuleiro.print();
        }

        public void Stop()
        {
            temporizador.parar();
        }

        public void ClearHash()
        {
            this.tabela.Clear();
        }
    }
}
