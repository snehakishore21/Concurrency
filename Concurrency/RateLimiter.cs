using System;
using System.Threading;
using System.Threading.Tasks;

//A rate limiter with the leaky bucket algorithm is a mechanism used to control the rate at which requests are processed or
//  transmitted in a system. The leaky bucket algorithm is a token-based algorithm that ensures a constant rate of traffic over time. 
/*In this example:

The LeakyBucketRateLimiter class is responsible for managing the leaky bucket algorithm. It initializes with a certain capacity and tokens per second,
and it continuously leaks tokens over time. The TryAcquire method is used to check if a certain number of tokens can be acquired. If there are enough tokens, 
it decrements the token count and returns true; otherwise, it returns false.
The LeakTokens method runs in a separate thread and periodically leaks tokens based on the elapsed time since the last leak.
In the Main method, a simple test scenario is set up where 20 requests are attempted with a delay between them. 
The rate limiter is configured with a capacity of 10 and a rate of 5 tokens per second.*/
class LeakyBucketRateLimiter
{
    private int capacity;
    private int tokensPerSec;
    private int tokens;
    private DateTime lastLeakTime;
    private readonly object tokenLock = new object();

    public LeakyBucketRateLimiter(int capacity, int tokensPerSecond)
    {
        this.capacity = capacity;
        this.tokens = capacity;
        this.lastLeakTime = DateTime.UtcNow;
        this.tokensPerSec = tokensPerSecond;
        Task.Run(() => LeakTokens(tokensPerSecond));
    }

    public bool TryAcquire(int tokensRequested)
    {
        lock (tokenLock)
        {
            LeakTokensInternal();
            if (tokens >= tokensRequested)
            {
                tokens -= tokensRequested;
                return true;
            }
            return false;
        }
    }

    private void LeakTokens(int tokensPerSecond)
    {
        while (true)
        {
            Thread.Sleep(1000);

            lock (tokenLock)
            {
                LeakTokensInternal();
            }
        }
    }

    private void LeakTokensInternal()
    {
        DateTime now = DateTime.UtcNow;
        double elapsedSeconds = (now - lastLeakTime).TotalSeconds;
        int newTokens = (int)(elapsedSeconds * tokensPerSec);

        if (newTokens > 0)
        {
            tokens = Math.Min(capacity, tokens + newTokens);
            lastLeakTime = now;
        }
    }
}       
class RateLimiterFunction
{
    public static async Task RateLimiterFuncMain()
    {
        LeakyBucketRateLimiter rateLimiter = new LeakyBucketRateLimiter(capacity: 10, tokensPerSecond: 5);

        Thread t1 = new Thread(() =>
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            for (int i = 0; i < 20; i++)
            {
                if (rateLimiter.TryAcquire(tokensRequested: 2))
                {
                    Console.WriteLine($"Request {i + 1}: Allowed");
                }
                else
                {
                    Console.WriteLine($"Request {i + 1}: Denied");
                }

                Thread.Sleep(100); // Simulate some processing time between requests
            }
        });
        t1.Start();

        t1.Join();  
    }

}