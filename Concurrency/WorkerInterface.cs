using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IWorkerInterface
{
    Task SubmitWorkAsync(Func<Task> taskFunction);
    Task BlockUntilComplete();
}
public class WorkerInterface
{
    private readonly List<Task> tasks = new List<Task>();
    private readonly object lockObject = new object();

    public void SubmitWork(Action work)
    {
        lock (lockObject)
        {
            //Task.Run(work) is used to run the specified work(an Action or delegate) asynchronously on the ThreadPool.
            //It starts the execution of the specified delegate and returns a Task object(work queued in threadpool) representing that asynchronous operation.
            Task task = Task.Run(work);
            tasks.Add(task);
        }
    }

    public void BlockUntilComplete()
    {
        lock (lockObject)
        {
            // Wait for all submitted tasks to complete
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("All tasks are completed now block");
        }
    }
}


public class WorkerInterfaceMainFunction
{ 
    public static async Task WorkerInterfaceMainFunc()
    {
        WorkerInterface worker = new WorkerInterface();

        // Submitting some work
        worker.SubmitWork(() => Console.WriteLine("Task 1 is running"));
        worker.SubmitWork(() => Console.WriteLine("Task 2 is running"));
        worker.SubmitWork(() => Console.WriteLine("Task 3 is running"));

        // Block until all tasks are complete
        worker.BlockUntilComplete();

        Console.WriteLine("All tasks are complete.");
    }
}

/*I have a single Threaded client which calls the following interface:

class WorkerInterface{
	public void submitWork();// Accepts a Task and returns immediately
	public void blockUntilComplete(); //Upon Invocation the thread should be blocked until all submitted tasks are finished
}
I was allowed to modify the signatures to add necesssary params.*/

/*To enhance the WorkerInterface to handle asynchronous execution and blocking until all submitted tasks are completed, you can add an
 * asynchronous method to submit work and return a Task. The blockUntilComplete method can then await this task to ensure all submitted tasks are finished
 * In this example, the SubmitWorkAsync method accepts a Func<Task> representing the task to be executed asynchronously. 
 * It returns a Task representing the submitted task. The BlockUntilComplete 
 * method waits until all submitted tasks are finished using Task.WhenAll.

The Worker class maintains a list of tasks and a lock to ensure thread safety when modifying the list. 
Each submitted task is added to the list, and the BlockUntilComplete method awaits the completion of all tasks in the list..*/