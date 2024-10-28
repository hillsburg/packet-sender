using System;
using NLog;

namespace PacketSender
{
    public static class LogHelper
    {
        private static Logger _logger;

        static LogHelper()
        {
            try
            {
                _logger = NLog.LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void AddLog(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                    _logger.Error(message); break;
                case LogLevel.Warning:
                    _logger.Warn(message); break;
                case LogLevel.Info:
                    _logger.Info(message); break;
                case LogLevel.Debug:
                    _logger.Debug(message); break;
                default:
                    _logger.Info(message); break;
            }
        }
    }
}
