using System.Net;
using NLog;

namespace H.Infrastructure
{
    public class NetworkHelper
    {
        // NLog logger. Routes through the same NLog pipeline configured in NLog.config
        // so this class's lines share the unified "HH:mm:ss.ffff [LEVEL] [Class.Method] message"
        // format with the rest of the codebase.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static bool IsConnectedToInternet()
        {
            try
            {
#pragma warning disable SYSLIB0014 // WebClient is obsolete — HttpClient migration deferred
                using (var client = new WebClient())
#pragma warning restore SYSLIB0014
                using (client.OpenRead("https://google.com/generate_204"))
                {
                    _log.Info($"{nameof(NetworkHelper)}.{nameof(IsConnectedToInternet)} : Successfully connected to the internet");
                    return true;
                }
            }
            catch(Exception e)
            {
                _log.Error($"Exception thrown.");
                _log.Error($"{nameof(NetworkHelper)}.{nameof(IsConnectedToInternet)} : Could not connect to the internet.");
                _log.Error($"Inner Exception message: {e.InnerException}");
                return false;
            }
        }
    }
}
