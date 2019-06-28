using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.engine.avaliacao
{
    class DNA
    {
        private static readonly DNA instance = new DNA();

        private DNA() { }

        public static DNA Instance
        {
            get
            {
                return instance;
            }
        }

        public readonly int[] vPecas = { 100, 100, 300, 300, 320, 320, 500, 500, 900, 900, 0, 0 };
    }
}
