using System;
using System.Threading.Tasks;

public class Promise<T>
{
    //TaskCompletionSource: Represents the producer side of a Task<TResult> unbound to a delegate,
    //providing access to the consumer side through the Task property.

    // TaskCompletionSource is used to create and manage the underlying Task<T> associated with the promise.
    private TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

    // Task property exposes the Task associated with the promise.
    public Task<T> Task => tcs.Task;

    // Resolve method fulfills the promise with a result.
    public void Resolve(T result)
    {
        tcs.SetResult(result);
    }

    // Reject method rejects the promise with an exception.
    public void Reject(Exception error)
    {
        tcs.SetException(error);
    }
}

class PromiseMainFunction
{
    public static async Task PromiseMainFunc()
    { 
        // Create a promise for fetching data asynchronously.
        Promise<string> fetchDataPromise = FetchDataAsync();

        // Other work can be done here concurrently with the asynchronous operation

        try
        {
            // Wait for the asynchronous operation to complete and retrieve the result.
            string data = await fetchDataPromise.Task;
            Console.WriteLine("Data received: " + data);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occurred during the asynchronous operation.
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static Promise<string>  FetchDataAsync()
    {
        // Create a new promise for the asynchronous operation.
        Promise<string> promise = new Promise<string>();

        // Simulating an asynchronous operation (e.g., fetching data from a remote server).
        Task t =Task.Run(async () =>
        {
            // Simulating a delay before completing the operation.
            await Task.Delay(1000);

            // Simulating a result or an error based on some condition.
            if (new Random().NextDouble() > 0.5)
            {
                // Resolve the promise with a successful result.
                promise.Resolve("Success data");
            }
            else
            {
                // Reject the promise with an exception indicating a failure.
                promise.Reject(new Exception("Failed to fetch data"));
            }
        });

        Task.WaitAll(t);
        // Return the promise to the caller.
        return promise;
    }
}
