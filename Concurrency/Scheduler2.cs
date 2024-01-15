using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IScheduledExecutor
{
    void Schedule(Action runnable, TimeSpan waitTime);
}

public class ScheduledExecutor : IScheduledExecutor
{
    private readonly PriorityQueue<ScheduledTaskRunnable, double> taskQueue = new PriorityQueue<ScheduledTaskRunnable, double>();
    private readonly object lockObject = new object();

    // Used to cancel the poller, when the executor is stopped, or disposed. 
    // we use this because we want to stop the poller when the executor is stopped or disposed
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public ScheduledExecutor()
    {
        // Start the poller in the background
        Task.Run(() => Poller(cancellationTokenSource.Token));
    }

    public void Schedule(Action runnable, TimeSpan waitTime)
    {
        DateTime executionTime = DateTime.UtcNow.Add(waitTime);

        var task = new ScheduledTaskRunnable(runnable, executionTime);

        lock (lockObject)
        {
            taskQueue.Enqueue(task, GetUnixTimestamp(task.ExecutionTime));
        }
    }

    private async Task Poller(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000); // Poll every 1 second

            lock (lockObject)
            {
                while (taskQueue.Count > 0 && taskQueue.Peek().ExecutionTime <= DateTime.UtcNow)
                {
                    var task = taskQueue.Dequeue();
                    Task.Run(task.Runnable);
                }
            }
        }
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
    }
    private double GetUnixTimestamp(DateTime date)
    {
        return (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}

public class ScheduledTaskRunnable : IComparable<ScheduledTaskRunnable>
{
    public Action Runnable { get; }
    public DateTime ExecutionTime { get; }

    public ScheduledTaskRunnable(Action runnable, DateTime executionTime)
    {
        Runnable = runnable ?? throw new ArgumentNullException(nameof(runnable));
        ExecutionTime = executionTime;
    }

    public int CompareTo(ScheduledTaskRunnable other)
    {
        return ExecutionTime.CompareTo(other.ExecutionTime);
    }
}
public class SchedulerRunnableFunction
{
    public static void SchedulerRunnableFunc()
    {
        IScheduledExecutor executor = new ScheduledExecutor();

        // Schedule some tasks
        executor.Schedule(() => Console.WriteLine($"Task 1 executed {DateTime.Now}"), TimeSpan.FromSeconds(5));
        executor.Schedule(() => Console.WriteLine($"Task 2 executed {DateTime.Now}"), TimeSpan.FromSeconds(10));
        executor.Schedule(() => Console.WriteLine($"Task 3 executed {DateTime.Now}"), TimeSpan.FromSeconds(5));

        // Keep the application running
        Console.ReadLine();

        // Stop the executor (optional)
        if (executor is ScheduledExecutor scheduledExecutor)
        {
            scheduledExecutor.Stop();
        }
    }


}
/*Your approach of using a heap to store Runnables along with timestamps
 * is a reasonable way to implement a timer-based execution system. Let's discuss the basic implementation using C# and
 * Task for asynchronous execution:
 This code provides a basic implementation of a scheduled executor that allows you to schedule tasks to run after a specified wait time. 
The tasks are executed asynchronously in a background poller. 
Feel free to adapt and extend it based on your specific needs and requirements.*/

/*User
Expose an interface which will accept a Runnable object along with the waitTime . The implementation should run the Runnable object after waiting time is over.
The condition was that, invocation to the interface should not be blocking. Client will invoke the method, and it should exit immediately.

I was going with some random approach initially and then came up with below approach -

Store the Runnable object in a Heap along with the timestamp when it is going to be triggered (Current timestamp + waitTime)
Create a poller, which will check Heap every 1 second if any eligible Runnable needs to be triggered
I took too much time in this, and time was almost up, and interviewer didn't have much time to ask follow up question.
Follow up question : How to manage large number of threads? If you are planning to use ExecutorService, implement your own service.*/


/*Certainly! Based on your requirements, you can create an interface that accepts a Runnable object along with the waitTime. The implementation
 * uses a MinHeap to keep track of the scheduled Runnables. A separate poller runs in the background and checks the heap for any tasks that need 
 * to be executed.
 This example includes a basic implementation of a PriorityQueue to manage the scheduled tasks. 
The Scheduler class runs a background poller that continuously checks for tasks to execute. The Stop method is provided to 
stop the background poller when necessary. Note that this is a basic example, and you may need to enhance it based on your 
specific requirements and use cases.*/

/*Expose an interface which will accept a Runnable object along with the waitTime . The implementation should run the Runnable object after waiting time is over.
The condition was that, invocation to the interface should not be blocking. Client will invoke the method, and it should exit immediately.

I was going with some random approach initially and then came up with below approach -

Store the Runnable object in a Heap along with the timestamp when it is going to be triggered (Current timestamp + waitTime)
Create a poller, which will check Heap every 1 second if any eligible Runnable needs to be triggered
I took too much time in this, and time was almost up, and interviewer didn't have much time to ask follow up question.
Follow up question : How to manage large number of threads? If you are planning to use ExecutorService, implement your own service.

*/

