using System.Reflection.Metadata.Ecma335;

namespace Concurrency
{
    class DiningPhilosophers
    {
        static void Main(string[] args)
        {
            //DiningPhilosopherFunction.DiningPhilosopherFuncMain();
            //BoundedBlockingQueueFunction.BoundedBlockingQueueFuncMain();
            //LRUFunction.LRUMainFunc();
            //RateLimiterFunction.RateLimiterFuncMain();    
            //RateLimiter2Function.RateLimiter2();
            //SchedulerFuntion.SchedulerMainFunc();
            //SchedulerRunnableFunction.SchedulerRunnableFunc();
            //WorkerInterfaceMainFunction.WorkerInterfaceMainFunc();

            //PromiseMainFunction.PromiseMainFunc();
            //BlockBasedIOMainFunction.BlockBasedIOMainFunc();
            //SendLargeFileFunction.SendLargeFile();

            //FuncMain();
            //BoundedBlockingQueueSemFuncMain();
        }
       

        private static void FuncMain()
        {
            Task t1 = Task.Run(() => { Func1(); });
            Task t2 = Task.Run(() => { Func2(); });

            Console.WriteLine("m");
            Task.WaitAll(t1, t2);
        }

        static void Func1()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Func1");
        }

        static void Func2()
        {
            Console.WriteLine("Func2");
        }

        
        private static void BoundedBlockingQueueSemFuncMain()
        {
            BoundedBlockingQueueSem boundedQueue = new BoundedBlockingQueueSem(4);

            Thread producerThread = new Thread(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    boundedQueue.Enqueue(i);
                    Console.WriteLine($"Produced: {i}  {boundedQueue.Size()}");
                    //Thread.Sleep(100); // Simulate some processing time
                }
            });

            Thread consumerThread = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    int item = boundedQueue.Dequeue();
                    Console.WriteLine($"Consumed: {item} {boundedQueue.Size()}");
                    Thread.Sleep(200); // Simulate some processing time
                }
            });

            producerThread.Start();
            consumerThread.Start();

            producerThread.Join();
            consumerThread.Join();
        }
    }
}
