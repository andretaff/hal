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
            lock (objLock[posLock])
            {
                item = tabela[pos];
                if (item == null)
                {
                    return new Tuple<bool, TranspItem>(false, null);
                }
                if ((chave == item.chave) &&
                        (ply <= item.ply) &&
                        (idade == item.idade))
                {
                    item = item.clone();
                    ok = true;
                }
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
