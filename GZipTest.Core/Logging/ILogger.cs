namespace GZipTest.Core.Logging
{
    public interface ILogger
    {
        public void Message(string message);
        public void Exception(Exception exception, string message);
    }
}