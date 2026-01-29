using System.Linq;

namespace GlobalUtil
{
    public class Logger
    {
        private static Logger _instance;
        private string _prefix = "【】";

        public static void Init(string prefix)
        {
            _instance = new Logger();
            _instance.SetPrefix(prefix);
        }

        private void SetPrefix(string msg)
        {
            _prefix = msg;
        }

        private void LogInfo(params object[] args)
        {
            Debug.Log(GetMessage(args));
        }

        private void LogWarning(params object[] args)
        {
            Debug.LogWarning(GetMessage(args));
        }

        private void LogError(params object[] args)
        {
            Debug.LogError(GetMessage(args));
        }

        private string GetMessage(object[] args)
        {
            return _prefix + string.Join(" ", args.Select(arg => arg.ToString()));
        }

        public static void Info(params object[] args)
        {
            _instance.LogInfo(args);
        }

        public static void Warning(params object[] args)
        {
            _instance.LogWarning(args);
        }

        public static void Error(params object[] args)
        {
            _instance.LogError(args);
        }
    }
}