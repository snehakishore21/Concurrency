namespace Concurrency
{
    public class LRU
    {
        public int capacity;
        Dictionary<int, int> dict;
        LinkedList<int> list;
        ReaderWriterLockSlim ReaderWriterLock;
        public LRU(int cap) 
        {
            lock (this)
            {
                this.capacity = cap;
                this.dict = new Dictionary<int, int>();
                this.list = new();
                ReaderWriterLock = new ReaderWriterLockSlim();
            }
        }

        public void Put(int key, int value) 
        {
            ReaderWriterLock.EnterWriteLock();
            try
            {
                if (list.Count == capacity)
                {
                    int toBeRemoved = list.First();
                    list.RemoveFirst();
                    dict.Remove(toBeRemoved);
                }

                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                    list.Remove(key);
                }
                else
                {
                    dict.Add(key, value);
                }
                list.AddLast(key);
                Console.WriteLine($"Added {key}: {value}");
            }
            finally
            { ReaderWriterLock.ExitWriteLock(); }
        }

        public int Get(int key) 
        {
            ReaderWriterLock.EnterReadLock();
            int curr = key;
            int ans = -1;
            if (dict.ContainsKey(curr))
            {
                ans = dict[curr];
            }
            Console.WriteLine($"Fetched {key}:{ans}");
            ReaderWriterLock.ExitReadLock();

            ReaderWriterLock.EnterWriteLock();
            try
            {
                list.Remove(curr);
                list.AddLast(curr);
                return ans;
            }
            finally { ReaderWriterLock.ExitWriteLock();}
        }
    }

    public class LRUFunction
    {
        public static void LRUMainFunc()
        {
            LRU lRU = new LRU(7);

            Thread put = new Thread(() =>
            {
                for (int i = 0; i < 7; i++)
                {
                    int curr = i;
                    lRU.Put(curr, curr + 1);
                    Thread.Sleep(100);
                }
            }
            );
            Thread put2 = new Thread(() =>
            {
                for (int i = 0; i < 15; i++)
                {
                    int curr = i;
                    lRU.Put(curr, curr + 2);
                    Thread.Sleep(100);
                }
            }
           );
            Thread get = new Thread(() =>
            {
                for (int i = 0; i < 4; i++)
                {
                    int curr = i;
                    lRU.Get(curr);
                    Thread.Sleep(200);
                }
            });

            Thread get2 = new Thread(() =>
            {
                Thread.Sleep(2000);
                for (int i = 0; i < 4; i++)
                {
                    int curr = i;
                    lRU.Get(curr);
                }
            });


            put.Start();
            put2.Start();
            get.Start();
            put.Join();
            get.Join();
            put2.Join();
            get2.Start();
        }
    }
}
