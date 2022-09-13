using System.Text;

namespace GZipTest.Core.Logging
{
    public class FileLogger : ILogger, IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly object _sync;

        public FileLogger(string directoryPath)
        {
            _sync = new object();
            string filePath = GenerateFilePath(directoryPath);
            _fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        }

        public void Exception(Exception exception, string message)
        {
            DateTime time = DateTime.Now;
            string combinedMessage = CombineExceptionMessageInternal(message, exception);
            WriteLineInternal(combinedMessage, time);
        }

        public void Message(string message)
        {
            DateTime time = DateTime.Now;
            WriteLineInternal(message, time);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }


        private static string GenerateFilePath(string directoryPath)
        {
            string fileName = $"GZipTest_{Guid.NewGuid()}.log";
            string filePath = Path.Combine(directoryPath, fileName);
            return filePath;
        }

        private void WriteLineInternal(string message, DateTime time)
        {
            StringBuilder lineBuilder = new StringBuilder();
            lineBuilder.AppendFormat("[{0:o}] ", time);
            lineBuilder.AppendFormat("<{0}> ", Environment.CurrentManagedThreadId);
            lineBuilder.AppendLine(message);
            string line = lineBuilder.ToString();

            byte[] lineBytes = Encoding.UTF8.GetBytes(line);

            lock (_sync)
            {
                _fileStream.Write(lineBytes);
                _fileStream.Flush();
            }
        }        


        private string CombineExceptionMessageInternal(string message, Exception rootException)
        {
            Stack<Exception> innerExceptions = new Stack<Exception>();
            innerExceptions.Push(rootException);
            StringBuilder combinedMessageBuilder = new StringBuilder(message);

            while (innerExceptions.Count > 0)
            {
                Exception exception = innerExceptions.Pop();
                if (exception is AggregateException aggregateException)
                {
                    foreach (Exception innerException in aggregateException.InnerExceptions.Reverse())
                    {
                        innerExceptions.Push(innerException);
                    }
                }
                else
                {
                    if (combinedMessageBuilder.Length > 0)
                        combinedMessageBuilder.AppendLine();
                    combinedMessageBuilder.Append(exception.ToString());
                }
            }

            return combinedMessageBuilder.ToString();
        }
    }
}
