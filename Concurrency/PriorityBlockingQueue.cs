using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrency
{
    public class BoundedBlockingQueue
    {
        public int capacity;

        private Queue<int> queue;
        public BoundedBlockingQueue(int capacity) 
        {
            this.capacity = capacity;
            queue = new Queue<int>();
        }

        public int Count { get { return capacity; } }

        public int Dequeue()
        {
            lock (queue) 
            {
                while (queue.Count == 0) 
                {
                    Console.WriteLine("Queue is empty");
                    return -1 ;
                } 
                int ans = queue.Dequeue();
                Console.WriteLine($"Dequeued: {ans}, Current Count: {this.Size()}");
                Monitor.PulseAll(queue);
                return ans;
            }
        }

        public void Enqueue(int item)
        {
            lock (queue)
            {
                while (queue.Count == capacity)
                { 
                    Console.WriteLine($"Queue is full, waiting to enqueue: {item}");
                    Monitor.Wait(queue); 
                }
                queue.Enqueue(item);
                Console.WriteLine($"Enqueued: {item}, Current Count: {this.Size()}");
                Monitor.PulseAll(queue);
            }
        }

        public int Size()
        {
            return queue.Count;
        }
    }

    public class BoundedBlockingQueueFunction
    {
        public static void BoundedBlockingQueueFuncMain()
        {
            BoundedBlockingQueue boundedQueue = new BoundedBlockingQueue(2);

            Thread producerThread = new Thread(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    boundedQueue.Enqueue(i);
                    Thread.Sleep(100); // Simulate some processing time
                }
            });

            Thread consumerThread = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    int item = boundedQueue.Dequeue();
                    Thread.Sleep(100); // Simulate some processing time
                }
            });

            Thread t2 = new Thread(() =>
            {
                Thread.Sleep(5000);
                for (int i = 0; i < 5; i++)
                {
                    int item = boundedQueue.Dequeue();
                }
            });

            producerThread.Start();
            consumerThread.Start();
            t2.Start();

            producerThread.Join();
            consumerThread.Join();
            t2.Join();
        }
    }
}
