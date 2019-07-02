using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.engine
{
    

    class ThreadQueue<T>
    {
        static object obj = new object();
        Queue<T> fila = new Queue<T>();

        public void put (T item)
        {
            lock (obj)
            {
                fila.Enqueue(item);
            }
        }

        public bool isEmpty() {
            return fila.Count == 0;
        }

        public T get()
        {
            lock (obj)
            {
                return fila.Dequeue();
            }
        }

        public void Clear()
        {
            fila.Clear();
        }




    }
}
