using Hal.engine.board;
using Hal.engine.bitboard;
using Hal.userInterface;
using Hal.engine.move;
using Hal.engine.avaliacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;



namespace Hal.engine.negamax
{
    class NegaThread
    {
        const int RESULT_CHECKMATE = 9999;
        const int RESULT_STALEMATE = -100;
        const int QUIES_PLY = 5;

        Board tabuleiro;
        int maxPly;
        private uint nodes;
        private Move move;
        Avaliador avaliador;
        Thread temporizador;
        private TranspTable tabela;
        private ulong hits;
        private uint age;
        private int id;
        private long quiesNodes = 0;
        private ThreadQueue<negaResult> resultados;

        private unsafe bool* bParar;

        public struct negaResult
        {
            public int nota;
            public uint nodes;
            public Move move;
            public ulong hits;
        };



        public void Run()
        {
            int nota;
            negaResult result;
            this.nodes = 0;
            this.hits = 0;
            this.tabuleiro.print();
            Thread.Sleep(300);
            nota = Nega(-99999999, +99999999, maxPly, 0);
            result.nota = nota;
            result.move = this.move;
            result.nodes = this.nodes;
            result.hits = this.hits;
            //Console.Out.WriteLine("Q " + this.quiesNodes.ToString());
            unsafe
            {
                if ((!*bParar) &&(resultados.isEmpty()))
                    resultados.put(result);
            }
        }

        private int NegaQuiet(int alfa, int beta, int ply,int depth)
        {
            int alfaOriginal = alfa;
            int valor;
            List<Move> moves = new List<Move>(); 
            bool check;
            ulong chaveLocal = tabuleiro.getChave();
            bool moveu = false;

            unsafe
            {
                if ((!temporizador.IsAlive) || (*bParar))
                {
                    return 0;
                }
            }

            this.nodes++;
            Tuple<bool,TranspItem> retorno = tabela.recuperar(chaveLocal, 0, age);

  //          if (chaveLocal == 12841783601964796451)
  //              tabuleiro.print();
            
            if (retorno.Item1)
            {
                this.hits++;
                if (retorno.Item2.tipo == tipoTranspItem.SCORE_EXATO)
                {
                    return retorno.Item2.score;
                }
                else if (retorno.Item2.tipo == tipoTranspItem.SCORE_UPPER)
                {
                    beta = Math.Min(beta, retorno.Item2.score);

                }
                else if (retorno.Item2.tipo == tipoTranspItem.SCORE_LOWER)
                {
                    alfa = Math.Max(alfa, retorno.Item2.score);
                }
                if (alfa > beta)
                {
                    return retorno.Item2.score;
                }
            }
            valor = -99999999;

            this.quiesNodes++;
           


            MoveComparer comparador = new MoveComparer();
            if (ply == 0)
                return this.avaliador.avaliar(tabuleiro);
            check = tabuleiro.isChecked();

            moves = tabuleiro.gerarMovimentos(moves,!check);

            if ((moves.Count == 0) && (!check))
                return this.avaliador.avaliar(tabuleiro);
            else if ((moves.Count == 0) && (check))
                return - (RESULT_CHECKMATE - depth);
            //            Console.Out.WriteLine("-------------------------ANTES");
            //            foreach (Move move in moves)
            //                move.print();

            moves.Sort(comparador);
            //            Console.Out.WriteLine("-------------------------");
            //            foreach (Move move in moves)
            //               move.print();
           //tabuleiro.print();
            foreach (Move move in moves)
                {
             //   move.print();
                    unsafe
                    {
                        if ((!temporizador.IsAlive) || (*bParar))
                        {
                            return 0;
                        }
                    }
                    this.tabuleiro.makeMove(move);
                    if (!tabuleiro.isValido())
                    {
                  //  this.tabuleiro.print();
                        this.tabuleiro.unmakeMove(move);
#if DEBUG
                    if (chaveLocal != tabuleiro.getChave())
                        chaveLocal = 0;
#endif
                }
                    else
                    {
                        moveu = true;
                        valor = Math.Max(valor,-NegaQuiet(-beta, -alfa,ply-1,depth+1));

                        if (valor > alfa)
                            alfa = valor;
                        if (alfa > beta)
                        {
#if DEBUG
//                        if (chaveLocal != tabuleiro.getChave())
//                            chaveLocal = 0;
#endif

                        tabuleiro.unmakeMove(move);

                        break;
                        }
                    tabuleiro.unmakeMove(move);
#if DEBUG
                    if (chaveLocal != tabuleiro.getChave())
                    {
                        Console.Out.WriteLine("Errooooo");
                        move.print();
                        tabuleiro.print();
                    }
#endif

                }
            }
            if ((check)&& (!moveu))
                return -(RESULT_CHECKMATE -depth); 
            else if (!moveu)
            {
                return this.avaliador.avaliar(tabuleiro);
            }
            return alfa;
        }


        private int Nega(int alfa, int beta, int ply, int depth)
        {
            int alfaOriginal = alfa;
            int valor;
            int melhorValor;
            Move melhorMov = new Move();
            List<Move> moves = new List<Move>(); 
            Queue<Move> atrasados = new Queue<Move>();
            ulong moveHash = 0;
            bool check;
            bool visitado = false;
            ulong chaveLocal = tabuleiro.getChave();

            unsafe
            {
                if ((!temporizador.IsAlive) || (*bParar))
                {
                    return 0;
                }
            }

            this.nodes++;
//                       if (chaveLocal == 13861742866784470756)
//                           tabuleiro.print();
            Tuple<bool,TranspItem> retorno = tabela.recuperar(chaveLocal, ply, age);
            
            if (retorno.Item1)
            {
                this.hits++;
                if (depth==0)
                {
                    melhorMov = retorno.Item2.move;
                    this.move = melhorMov;
                }
                if (retorno.Item2.tipo == tipoTranspItem.SCORE_EXATO)
                {
                    return retorno.Item2.score;
                }
                else if (retorno.Item2.tipo == tipoTranspItem.SCORE_UPPER)
                {
                    beta = Math.Min(beta, retorno.Item2.score);

                }
                else if (retorno.Item2.tipo == tipoTranspItem.SCORE_LOWER)
                {
                    alfa = Math.Max(alfa, retorno.Item2.score);
                }
                if (alfa > beta)
                {
                    return retorno.Item2.score;
                }
                if ((retorno.Item2.move != null) && (retorno.Item2.move.tipo != tipoMovimento.MOVNENHUM))
                {
                    retorno.Item2.move.score = bbConstants.SCORE_MOVE_HASH;
                    moves.Add(retorno.Item2.move);
                    moveHash = tabela.moveHash(retorno.Item2.move);
                }
            }
            else if ((retorno.Item2 != null) && (retorno.Item2.move != null) && (retorno.Item2.move.tipo != tipoMovimento.MOVNENHUM))
            {
                    retorno.Item2.move.score = bbConstants.SCORE_MOVE_HASH;
                    moves.Add(retorno.Item2.move);
                    moveHash = tabela.moveHash(retorno.Item2.move);
            }
            valor = -99999999;
            melhorValor = -99999999;
            check = tabuleiro.isChecked();
            if (check)
                ply++;
//            if (ply == 0)
//                return avaliador.avaliar(tabuleiro);

            MoveComparer comparador = new MoveComparer();

            moves = tabuleiro.gerarMovimentos(moves,false);
//            Console.Out.WriteLine("-------------------------ANTES");
//            foreach (Move move in moves)
//                move.print();
            moves.Sort(comparador);
//            Console.Out.WriteLine("-------------------------");
//            foreach (Move move in moves)
 //               move.print();
            // if (chaveLocal == 10541650143722217845)
            // {
            //     tabuleiro.print();
            // chaveLocal = tabuleiro.getChave();
            //}
            do
            {
                if (atrasados.Count != 0)
                {
                    moves = new List<Move>();
                    while (atrasados.Count > 0)
                    {
                        moves.Add(atrasados.Dequeue());
                    }
                }
                foreach (Move move in moves)
                {
                    if ((visitado) && (moveHash == tabela.moveHash(move)))
                        continue;
                    unsafe
                    {
                        if ((!temporizador.IsAlive) || (*bParar))
                        {
                            return 0;
                        }
                    }
                    if (!tabela.verificaSeBuscado(id, chaveLocal, tabela.moveHash(move), ply))
                    {
                        atrasados.Enqueue(move);
                        continue;
                    }
                    //if (ply == this.maxPly)
                    //    tabuleiro.print();
                    // move.print();
                    //move.print(ply,10);
//                    if ((move.peca == tipoPeca.BISPO) && (chaveLocal == 15571111703642359256))
//                        Thread.Sleep(1);
                    this.tabuleiro.makeMove(move);
                    //tabuleiro.print();
                    if (!tabuleiro.isValido())
                    {
                        this.tabuleiro.unmakeMove(move);
                        tabela.retiraMov(id, ply);
                    }
                    else
                    {
                        if (ply > 1)
                            valor = -Nega(-beta, -alfa, ply - 1, depth+1);
                        else
                            valor = -NegaQuiet(-beta, -alfa, QUIES_PLY,depth+1);
                        if (valor > melhorValor)
                        {
                            melhorMov = move;
                            melhorValor = valor;
                        }
                        if (valor > alfa)
                            alfa = valor;
                        if (alfa > beta)
                        {

                            tabuleiro.unmakeMove(move);
                            tabela.retiraMov(id, ply);
                            atrasados.Clear();
                            //   if (chaveLocal != tabuleiro.getChave())
                            //   {
                            //       move.print();
                            //       tabuleiro.print();
                            //   }
                            break;
                        }
                        tabuleiro.unmakeMove(move);
                        tabela.retiraMov(id, ply);
                        // if (chaveLocal != tabuleiro.getChave())
                        // {
                        //     tabuleiro.print();
                        //     move.print();
                        // }
                    }
                    visitado = visitado || tabela.moveHash(move) == moveHash; 
                }
            } while (atrasados.Count != 0);


            if (melhorMov.tipo == tipoMovimento.MOVNENHUM)
            {
                if (check)
                {
                    return - (RESULT_CHECKMATE - depth);
                }
                else
                {
                    return RESULT_STALEMATE;
                }
            }

            if (depth==0)
            {
                unsafe
                {
                    if ((temporizador.IsAlive) && (!*bParar))
                    {
                        this.move = melhorMov;
                    }
                }
            }
            if (check)
                ply--;

            TranspItem itemT = new TranspItem();
            itemT.move = melhorMov;
            itemT.idade = age;
            itemT.score = melhorValor;
            itemT.chave = chaveLocal;
            itemT.ply = ply;

            if (alfa <= alfaOriginal)
                itemT.tipo = tipoTranspItem.SCORE_UPPER;
            else if (alfa > beta)
                itemT.tipo = tipoTranspItem.SCORE_LOWER;
            else
                itemT.tipo = tipoTranspItem.SCORE_EXATO;

            tabela.armazenar(itemT);

            return melhorValor;

        }


        unsafe

            public NegaThread(int id, Board tabuleiro, int profundidade, Avaliador avaliador, Thread temporizador, bool* bParar, ThreadQueue<negaResult> resultados, TranspTable tabela, uint age)
        {
            this.tabuleiro = tabuleiro.clone();
            this.maxPly = profundidade;
            this.avaliador = avaliador;
            this.resultados = resultados;
            this.temporizador = temporizador;
            this.hits = 0;
            this.tabela = tabela;
            this.age = age;
            this.id = id;
            unsafe
            {
                this.bParar = bParar;
            }
        }
    


    }
}
