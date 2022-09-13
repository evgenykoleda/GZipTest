using GZipTest.Core.Application;
using GZipTest.Core.Logging;
using System.Reflection;

namespace GZipTest.ConsoleApp
{
    public class Program
    {
        private static FileLogger _logger;
        private static ApplicationPipeline _application;

        public static int Main(string[] args)
        {
            try
            {
                string applicationPath = GetApplicationPath();
                _logger = new FileLogger(applicationPath);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to initialize log" + Environment.NewLine + exception.ToString());
                return 1;
            }
            
            try
            {
                _logger.Message($"Application process started. Args: {string.Join(", ", args)}");                
                ApplicationSettings settings = ParseApplicationSettings(args);
                LogApplicationSettings(settings);

                using (_application = ApplicationPipeline.CreateApplication(settings, _logger))
                {
                    Console.CancelKeyPress += OnCancelKeyPress;
                    _application.Start();
                    _application.Wait();
                    Console.CancelKeyPress -= OnCancelKeyPress;
                }

                _logger.Message($"Application process finished");
                return 0;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception, "Application process failed");
                return 1;
            }
            finally
            {
                _logger.Dispose();
            }
        }

        private static string GetApplicationPath()
        {
            string applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrWhiteSpace(applicationPath))
                throw new Exception("Failed to determine application path");
            
            return applicationPath;
        }

        private static ApplicationSettings ParseApplicationSettings(string[] args)
        {
            string usageText =
                "Invalid application arguments." + Environment.NewLine +
                "Example of arguments to compress: GZipTest.exe compress \"C:\\in.vbk\" \"D:\\out.gz\"" + Environment.NewLine +
                "Example of arguments to decompress: GZipTest.exe decompress \"D:\\out.gz\" \"D:\\in_decompressed.vbk\"";

            if (args.Length < 3)
                throw new Exception(usageText);

            ApplicationSettings.EApplicationMode applicationMode;
            switch (args[0].ToLower())
            {
                case "compress":
                {
                    applicationMode = ApplicationSettings.EApplicationMode.Compress;
                    break;
                }
                case "decompress":
                {
                    applicationMode = ApplicationSettings.EApplicationMode.Decompress;
                    break;
                }
                default:
                    throw new Exception(usageText);
            }

            string sourceFilePath = args[1];
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new Exception(usageText);

            string targetFilePath = args[2];
            if (string.IsNullOrWhiteSpace(targetFilePath))
                throw new Exception(usageText);

            return ApplicationSettings.CreateDefault(applicationMode, sourceFilePath, targetFilePath);
        }

        private static void LogApplicationSettings(ApplicationSettings settings)
        {
            _logger.Message(
                $"Using application settings: " +
                $"ApplicationMode: {settings.ApplicationMode}, " +
                $"SourceFilePath: {settings.SourceFilePath}, " +
                $"TargetFilePath: {settings.TargetFilePath}, " +
                $"BlockSize: {settings.BlockSize}, " +
                $"ProcessorsCount: {settings.ProcessorsCount}, " +
                $"QueueMaxBlocksCount: {settings.QueueMaxBlocksCount}");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs eventArgs)
        {
            _logger.Message($"Cancelling application process");
            eventArgs.Cancel = true;
            _application.Cancel();
        }
    }
}