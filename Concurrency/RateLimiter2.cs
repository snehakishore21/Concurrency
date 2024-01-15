using System;
using System.Collections.Generic;

class RateLimiter
{
    private readonly int capacity;
    private readonly double expireTimeInSeconds;
    private readonly PriorityQueue<double, double> requestQueue = new PriorityQueue<double, double>();
    private readonly object lockObject = new object();

    public RateLimiter(int capacity, double expireTimeInSeconds)
    {
        this.capacity = capacity;
        this.expireTimeInSeconds = expireTimeInSeconds;
    }

    public bool CanAccept()
    {
        lock (lockObject)
        {
            // Process expired requests
            ProcessExpiredRequests();

            if (requestQueue.Count < capacity)
            {
                // Add the current request to the queue
                double currentTime = GetUnixTimestamp();
                requestQueue.Enqueue(currentTime, -1*currentTime);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void ProcessExpiredRequests()
    {
        double currentTime = GetUnixTimestamp();
        while (requestQueue.Count > 0 && currentTime - requestQueue.Peek() > expireTimeInSeconds)
        {
            requestQueue.Dequeue();
        }
    }

    private double GetUnixTimestamp()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}

public class RateLimiter2Function
{
    public static void RateLimiter2()
    {
        RateLimiter rateLimiter = new RateLimiter(capacity: 5, expireTimeInSeconds: 10);

        // Simulate some requests using multiple threads
        System.Threading.Tasks.Parallel.For(0, 10, i =>
        {
            if (rateLimiter.CanAccept())
            {
                Console.WriteLine($"Thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: Request accepted");
            }
            else
            {
                Console.WriteLine($"Thread {System.Threading.Thread.CurrentThread.ManagedThreadId}: Request rejected");
            }
        });

        // Wait for some time to let requests expire
        System.Threading.Thread.Sleep(15000);

        // Try again after waiting using a single thread
        if (rateLimiter.CanAccept())
        {
            Console.WriteLine("Request accepted after waiting");
        }
        else
        {
            Console.WriteLine("Request rejected after waiting");
        }
    }
}
/*Certainly! In order to make the RateLimiter thread-safe, you need to use synchronization mechanisms
 * . Here, I'll modify the C# code to use the lock statement to ensure that the operations are thread-safe:
 * In this example, I added a lockObject to synchronize access to the shared data (the requestQueue). 
 * The lock statement ensures that only one thread can execute the critical section of code at a time, 
 * preventing race conditions and ensuring thread safety.*/

/*Implement a RateLimiter. Asked to implement canAccept method, and RateLimiter had an initial capacity. 
 * Also requests accepted will have an expireTime. After expireTime , RateLimiter can accept new requests.

I went with queue approach. Interviewer asked to implement a single execution first, then asked to implement concurrent scenario
. Then follow up question was how to implement shared resources in case of distributed system.*/