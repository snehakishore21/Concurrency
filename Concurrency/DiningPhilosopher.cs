using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrency
{
    internal class DiningPhilosopher
    {
        Thread[] thread;
        Semaphore[] forks;
        private readonly Semaphore waiter;
        int n;
        public DiningPhilosopher(int n)
        {
            this.n = n;
            this.thread = new Thread[n];
            this.forks = new Semaphore[n];
            for (int i = 0; i < n; i++)
            {
                forks[i] = new Semaphore(1,1);
            }
            for(int i = 0; i < n; i++)
            {
                int curr = i;
                thread[curr] = new Thread(()=>Philosopher(curr));
            }
            waiter = new Semaphore(n - 1, n - 1);
        }

        public void Start()
        {
            for(int i = 0; i < n; i++)
            {
                thread[i].Start();
            }
        }
        public void Stop()
        {
            for(int i = 0; i < n; i++)
            {
                thread[i].Abort();
            }
        }

        public void Philosopher(int i)
        {
            int left = i;
            int right = (i + 1) % n;

            while(true)
            {
                Console.WriteLine("Philosopher " + i + " is thinking");
                waiter.WaitOne();
                //Thread.Sleep(1000);
                forks[left].WaitOne();
                forks[right].WaitOne();
                waiter.Release();

                Console.WriteLine("Philosopher " + i + " is eating");
                //Thread.Sleep(1000);
                forks[left].Release();
                forks[right].Release();
            }
        }
    }

    public class DiningPhilosopherFunction
    {
        public static void DiningPhilosopherFuncMain()
        {
            DiningPhilosopher diningPhilosopher = new DiningPhilosopher(5);
            diningPhilosopher.Start();
        }
    }
}
