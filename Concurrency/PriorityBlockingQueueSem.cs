using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrency
{
    public class BoundedBlockingQueueSem
    {
        public int capacity;

        private Semaphore enqueue;

        private Semaphore dequeue;

        private Queue<int> queue;
        public BoundedBlockingQueueSem(int capacity) 
        {
            this.enqueue = new Semaphore(capacity, capacity);
            this.dequeue = new Semaphore(capacity, capacity);
            queue = new Queue<int>();
        }

        public int Count { get { return capacity; } }

        public int Dequeue()
        {
            while (queue.Count == 0)
            { dequeue.WaitOne(); }
            int ans;
            lock (queue)
            {
                ans = queue.Dequeue();
            }
            return ans;
        }

        public void Enqueue(int item)
        {
            lock (queue)
            {
                queue.Enqueue(item);
            }
        }

        public int Size()
        {
            return queue.Count;
        }
    }
}
