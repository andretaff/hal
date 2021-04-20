using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hal.engine.utils;
using Hal.engine.bitboard;
using Hal.engine.move;

namespace Hal.engine.board
{
    enum tipoTranspItem { SCORE_NENHUM, SCORE_EXATO, SCORE_UPPER, SCORE_LOWER};
    class TranspItem
    {
        public tipoTranspItem tipo;
        public int score;
        public ulong chave;
        public uint idade;
        public int ply;
        public Move move;
        
        
        public TranspItem()
        {
            this.idade = 0;
            this.tipo = tipoTranspItem.SCORE_NENHUM;
            this.move = null;
        }


        public TranspItem clone()
        {
            TranspItem item = new TranspItem();
            item.tipo = tipo;
            item.ply = ply;
            item.idade = idade;
            item.chave = chave;
            item.score = score;
            item.move = move;
            return item;
        }
    }

    class TranspTable
    {
        public ulong[,] chaves;
        public ulong[] chavesRoque;
        private static object[] objLock;
        public ulong chaveBTM;
        private uint size;
        private TranspItem[] tabela;
        private List<movBusc>[] movsBuscados;
        private static object lockMove = new object();
        struct movBusc
        {
            public ulong moveHash;
            public ulong hash;
        }

        public ulong moveHash(Move move)
        {
            if ((int) move.peca < bbConstants.PECAS)
                return this.chaves[(int)move.peca, move.indiceDe] ^ this.chaves[(int)move.peca, move.indicePara];
            else
                return 1;

          
        }

        public void Clear()
        {
            for (int i = 0; i < this.size; i++)
                if (this.tabela[i]!= null)
                    this.tabela[i].chave = 0;
        }
        


        public TranspTable(uint size)
        {
            chaves = new ulong[bbConstants.PECAS+1,64];
            chavesRoque = new ulong[4];
            this.size = size;
            this.tabela = new TranspItem[size];
            objLock = new object[(size / 1000)+1];
            for (int i = 0; i < (size / 1000)+1; i++)
                objLock[i] = new object();
            read();

            movsBuscados = new List<movBusc>[bbConstants.MAXPLY];

            for (int i = 0; i < 4; i++)
            {
                lock (lockMove)
                {
                    movsBuscados[i] = new List<movBusc>();
                    for (int j = 0; j < bbConstants.MAXPLY; j++)
                    {
                        movBusc movB = new movBusc();
                        movB.hash = 0;
                        movsBuscados[i].Add(movB);
                    }
                }
            }
                    

        }

        public void reiniciarMovsBuscados()
        {
            movsBuscados = new List<movBusc>[bbConstants.MAXPLY];

            for (int i = 0; i < 4; i++)
            {
               // lock (lockMove)
                {
                    movsBuscados[i] = new List<movBusc>();
                    for (int j = 0; j < bbConstants.MAXPLY; j++)
                    {
                        movBusc movB = new movBusc();
                        movB.hash = 0;
                        movsBuscados[i].Add(movB);
                    }
                }
            }
        }

        public Boolean verificaSeBuscado(int id, ulong hash, ulong moveHash, int ply)
        {
            for (int i = 0; i < 4; i++)
                if (i != id)
                {
             //       lock (lockMove)
             //       {
                        if ((movsBuscados[i][ply].hash == 0))
                            break;
                        if ((movsBuscados[i][ply].hash == hash) && (movsBuscados[i][ply].moveHash == moveHash))
                        {
                            return false;
                        }
             //       }
                }
            movBusc moveB;
            moveB = new movBusc();
            moveB.hash = hash;
            moveB.moveHash = moveHash;
            //lock (lockMove)
           // {
                movsBuscados[id][ply] = moveB;
            //}

            return true;
        }

        public void retiraMov(int id, int ply)
        {
            movBusc moveB;
            moveB = new movBusc();
            moveB.hash = 0;
            movsBuscados[id][ply] = moveB;
        }

        public void armazenar(TranspItem item)
        {
            uint pos = (uint) (item.chave % (ulong) size);
            uint posLock = pos % 1000;
            lock (objLock[posLock])
            {
                tabela[pos] = item;
            }
        }

        public Tuple<bool,TranspItem> recuperar(ulong chave, int ply, uint idade)
        {
            TranspItem item;
            uint pos = (uint)(chave % (ulong)size);
            uint posLock = pos % 1000;
            bool ok = false;
            item = null;
            lock (objLock[posLock])
            {
                item = tabela[pos];
                if (item == null)
                {
                    return new Tuple<bool, TranspItem>(false, null);
                }
                if ((chave == item.chave) &&
                        (ply <= item.ply) &&
                        (idade <= item.idade + 1))
                {
                    item = item.clone();
                    ok = true;
                }
                else if (chave == item.chave)
                {
                    ok = false;
                    item = item.clone();
                }
                else
                    item = null;
            }
            return new Tuple<bool, TranspItem>(ok, item);
        }

            private void read()
        {
            System.IO.StreamReader file;
            try
            {
                file = new System.IO.StreamReader("chaves.txt");

            }
            catch (Exception)
            {
                // tenho que re-gerar as chaves
                Random random = new Random();
                System.IO.StreamWriter saida = new System.IO.StreamWriter("chaves.txt");
                for (int i = 0; i < (64 * (bbConstants.PECAS+1)) + 5; i++)
                    saida.WriteLine(Convert.ToString(random.NextULong()));
                saida.Close();
                file = new System.IO.StreamReader("chaves.txt");
            }
            for (int i = 0; i<bbConstants.PECAS+1; i++)
                for (int j= 0; j<64; j++)
                {
                    chaves[i, j] = Convert.ToUInt64(file.ReadLine().Trim());
                }
            for (int i =0; i<4; i++)
            {
                chavesRoque[i] = Convert.ToUInt64(file.ReadLine().Trim());
            }
            this.chaveBTM = Convert.ToUInt64(file.ReadLine().Trim());
            file.Close();
        }
    }
}
