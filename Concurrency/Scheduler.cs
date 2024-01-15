using System;
using System.Collections.Generic;
using System.Threading;

public class ScheduledExecutorService
{
    private PriorityQueue<ScheduledTask, double> taskQueue = new PriorityQueue<ScheduledTask, double>();
    private readonly object lockObject = new object();
    private Timer timer;
    /*To implement a scheduled executor service using a priority queue for the specified methods, 
     * you would need to prioritize tasks based on their execution times. Below is an example using a
     * PriorityQueue where tasks are sorted by their execution times.*/

    /*In this implementation, ScheduledTask includes a Period property, and tasks are enqueued in a priority queue based on their execution times.
     * The ScheduleNextTask and ExecuteNextTask methods are adjusted to consider the periodicity of tasks and update the next execution times accordingly.
     * This example uses a simple PriorityQueue implementation, 
     * and in a production environment, you might consider using a more robust data structure or a priority queue from a third-party library.*/
    public void Schedule(Action command, long delay, TimeUnit unit)
    {
        long millisecondsDelay = unit.ToMilliseconds(delay);
        var scheduledTask = new ScheduledTask(command, DateTime.UtcNow.AddMilliseconds(millisecondsDelay));

        lock (lockObject)
        {
            taskQueue.Enqueue(scheduledTask, GetUnixTimestamp(scheduledTask.ExecutionTime));
            ScheduleNextTask();
        }
    }

    public void ScheduleAtFixedRate(Action command, long initialDelay, long period, TimeUnit unit)
    {
        long millisecondsInitialDelay = unit.ToMilliseconds(initialDelay);
        long millisecondsPeriod = unit.ToMilliseconds(period);

        var scheduledTask = new ScheduledTask(command, DateTime.UtcNow.AddMilliseconds(millisecondsInitialDelay), millisecondsPeriod);

        lock (lockObject)
        {
            taskQueue.Enqueue(scheduledTask, GetUnixTimestamp(scheduledTask.ExecutionTime));
            ScheduleNextTask();
        }
    }

    public void ScheduleWithFixedDelay(Action command, long initialDelay, long delay, TimeUnit unit)
    {
        long millisecondsInitialDelay = unit.ToMilliseconds(initialDelay);
        long millisecondsDelay = unit.ToMilliseconds(delay);

        var scheduledTask = new ScheduledTask(command, DateTime.UtcNow.AddMilliseconds(millisecondsInitialDelay), millisecondsDelay);

        lock (lockObject)
        {
            taskQueue.Enqueue(scheduledTask, GetUnixTimestamp(scheduledTask.ExecutionTime));
            ScheduleNextTask();
        }
    }

    public void Stop()
    {
        lock (lockObject)
        {
            timer?.Dispose();
        }
    }

    private void ScheduleNextTask()
    {
        lock (lockObject)
        {
            if (timer != null)
                return;

            var nextTask = taskQueue.Peek();

            if (nextTask != null)
            {
                long delay = Math.Max(0, (long)(nextTask.ExecutionTime - DateTime.UtcNow).TotalMilliseconds);
                timer = new Timer(_ => ExecuteNextTask(), null, delay, Timeout.Infinite);
            }
        }
    }

    private void ExecuteNextTask()
    {
        lock (lockObject)
        {
            var nextTask = taskQueue.Dequeue();

            if (nextTask != null)
            {
                nextTask.Command.Invoke();

                if (nextTask.Period > 0)
                {
                    // If task is periodic, enqueue it for the next execution
                    nextTask.UpdateNextExecutionTime();
                    taskQueue.Enqueue(nextTask, GetUnixTimestamp(nextTask.ExecutionTime));
                }

                timer?.Dispose();
                timer = null;

                ScheduleNextTask();
            }
        }
    }

    private double GetUnixTimestamp(DateTime date)
    {
        return (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}

public class ScheduledTask : IComparable<ScheduledTask>
{
    public Action Command { get; }
    public DateTime ExecutionTime { get; private set; }
    public long Period { get; }

    public ScheduledTask(Action command, DateTime executionTime, long period = 0)
    {
        Command = command;
        ExecutionTime = executionTime;
        Period = period;
    }

    public void UpdateNextExecutionTime()
    {
        ExecutionTime = ExecutionTime.AddMilliseconds(Period);
    }

    public int CompareTo(ScheduledTask other)
    {
        return ExecutionTime.CompareTo(other.ExecutionTime);
    }
}

public enum TimeUnit
{
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days
}

public static class TimeUnitExtensions
{
    public static long ToMilliseconds(this TimeUnit unit, long value)
    {
        switch (unit)
        {
            case TimeUnit.Milliseconds:
                return value;
            case TimeUnit.Seconds:
                return value * 1000;
            case TimeUnit.Minutes:
                return value * 60 * 1000;
            case TimeUnit.Hours:
                return value * 60 * 60 * 1000;
            case TimeUnit.Days:
                return value * 24 * 60 * 60 * 1000;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
    }
}

class SchedulerFuntion
{
    public static void SchedulerMainFunc()
    {
        ScheduledExecutorService executor = new ScheduledExecutorService();

        // Example of scheduling a task with priority
        executor.Schedule(() => Console.WriteLine("High-priority task executed"), 2, TimeUnit.Seconds);
        executor.Schedule(() => Console.WriteLine("Low-priority task executed"), 2, TimeUnit.Seconds);

        // Example of scheduling a task at a fixed rate
        executor.ScheduleAtFixedRate(() => Console.WriteLine("Fixed-rate task executed"), 1, 3, TimeUnit.Seconds);

        // Example of scheduling a task with fixed delay between executions
        executor.ScheduleWithFixedDelay(() => Console.WriteLine("Fixed-delay task executed"), 1, 4, TimeUnit.Seconds);

        // Keep the application running for a while to observe task executions
        Console.ReadLine();

        // Stop the executor when the application is done
        executor.Stop();
    }
}