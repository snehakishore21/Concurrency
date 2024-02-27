using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;

namespace Concurrency
{
//Compress the file to use minimal network bandwidth.
//Have an asynchronous setup to transfer chunks of the file.
//On the receiving end, devise a merge algorithm to stitch all chunks.
//In case a chunk fails, we perform a retry strategy - depending on the HttpStatusCode from receiver's end.
    internal class FileTransfer
    {
        private string sourceFilePath;
        private string destinationFolder;
        private string serverIpAddr;
        private int portInput;
        public FileTransfer(string src, string dest, string ipaddr, int port)
        {
            this.destinationFolder = dest;
            this.sourceFilePath = src;
            this.serverIpAddr = ipaddr;
            this.portInput = port;
        }
        public async Task SenderCode()
        {
            string filePath = sourceFilePath;
            string destinationIpAddress = serverIpAddr;
            int destinationPort = portInput;

            await SendFileAsync(filePath, destinationIpAddress, destinationPort);
            Task t = Task.Run(() => 
            {
                SendFileAsync(filePath, destinationIpAddress, destinationPort);
            });
            Task.WaitAll(t);

            Console.WriteLine("File transfer completed.");
        }

        async Task SendFileAsync(string filePath, string destinationIp, int destinationPort)
        {
            using (HttpClient client = new HttpClient())
            {
                // Compress the file
                string compressedFilePath = CompressFile(filePath);

                // Get the compressed file size
                long fileSize = new FileInfo(compressedFilePath).Length;

                // Set up chunk size (adjust as needed)
                int chunkSize = 1024 * 1024; // 1 MB

                // Calculate the total number of chunks
                int totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

                // Send file metadata
                await SendMetadataAsync(client, destinationIp, destinationPort, compressedFilePath, fileSize);

                // Send file chunks asynchronously
                await Task.Run(async () =>
                {
                    for (int chunkNumber = 0; chunkNumber < totalChunks; chunkNumber++)
                    {
                        await SendFileChunkAsync(client, destinationIp, destinationPort, compressedFilePath, chunkNumber, chunkSize);
                    }
                });
            }
        }

        string CompressFile(string filePath)
        {
            string compressedFilePath = Path.ChangeExtension(filePath, ".zip");

            using (FileStream originalFileStream = File.OpenRead(filePath))
            using (FileStream compressedFileStream = File.Create(compressedFilePath))
            using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
            {
                originalFileStream.CopyTo(compressionStream);
            }

            return compressedFilePath;
        }

        async Task SendMetadataAsync(HttpClient client, string destinationIp, int destinationPort, string filePath, long fileSize)
        {
            string destinationUrl = $"http://{destinationIp}:{destinationPort}/api/metadata";
            var metadata = new { FilePath = filePath, FileSize = fileSize };
            await client.PostAsJsonAsync(destinationUrl, metadata);
        }

        async Task SendFileChunkAsync(HttpClient client, string destinationIp, int destinationPort, string filePath, int chunkNumber, int chunkSize)
        {
            string destinationUrl = $"http://{destinationIp}:{destinationPort}/api/chunk?chunkNumber={chunkNumber}";
            byte[] chunkData;

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                fileStream.Seek(chunkNumber * chunkSize, SeekOrigin.Begin);

                int remainingBytes = (int)Math.Min(chunkSize, fileStream.Length - fileStream.Position);
                chunkData = new byte[remainingBytes];

                await fileStream.ReadAsync(chunkData, 0, remainingBytes);
            }

            using (MemoryStream chunkStream = new MemoryStream(chunkData))
            {
                await client.PostAsync(destinationUrl, new StreamContent(chunkStream));
            }
        }

        async Task RecieverCode()
        {
            int listenPort = portInput;

            await ReceiveAndMergeChunksAsync(listenPort);

            Console.WriteLine("File received and merged.");
        }
        private readonly ConcurrentDictionary<int, byte[]> ReceivedChunks = new ConcurrentDictionary<int, byte[]>();

        async Task ReceiveAndMergeChunksAsync(int listenPort)
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://+:{listenPort}/api/upload/");
                listener.Start();

                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    await ProcessRequestAsync(context);
                }
            }
        }

        async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                string fileName = context.Request.QueryString["fileName"];
                int chunkNumber = int.Parse(context.Request.Headers["Chunk-Number"]);

                using (Stream receivedStream = context.Request.InputStream)
                using (GZipStream decompressedStream = new GZipStream(receivedStream, CompressionMode.Decompress))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await decompressedStream.CopyToAsync(memoryStream);
                    ReceivedChunks[chunkNumber] = memoryStream.ToArray();
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();

                if (CheckAndMergeChunks(fileName))
                {
                    Console.WriteLine($"File {fileName} received and merged successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
        }

        bool CheckAndMergeChunks(string fileName)
        {
            int expectedChunkNumber = 0;
            byte[] chunk;

            while (ReceivedChunks.TryRemove(expectedChunkNumber, out chunk))
            {
                AppendChunkToFile(fileName, chunk);
                expectedChunkNumber++;
            }

            return expectedChunkNumber > 0; // Return true if at least one chunk was processed
        }

        void AppendChunkToFile(string fileName, byte[] chunk)
        {
            string destinationFilePath = Path.Combine(destinationFolder, fileName);

            using (FileStream destinationFileStream = new FileStream(destinationFilePath, FileMode.Append))
            {
                destinationFileStream.Write(chunk, 0, chunk.Length);
            }
        }
    }

    public class SendLargeFileFunction
    {
        public async static void SendLargeFile()
        {
          
        }
    }
}