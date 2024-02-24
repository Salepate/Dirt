using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Dirt.Network
{
    public static class ClientCommandProcessor
    {
        public const int RequestTimeout = 5 * 1000; // in ms
        public const int MaxParameters = 4;
        public const string CommandNameParameter = "commandName";
        public const string SessionIDParameter = "sessid";

        public static string SessionID = null;

        private static string EndPoint = "http://127.0.0.1:8080/command";
        private static string[] ParameterNames;
        static ClientCommandProcessor()
        {
            ParameterNames = new string[MaxParameters];
            for (int i = 0; i < MaxParameters; ++i)
                ParameterNames[i] = $"param{i + 1}";
        }
        public static void SetEndPoint(string newEndPoint)
        {
            EndPoint = newEndPoint;
        }
        public static async Task<T> PerformRemoteGetCommand<T>(string commandName, System.Func<string, T> messageInterpreter, params object[] arguments)
        {
            WebRequest webReq = CreateRequest(commandName, arguments);
            HttpWebResponse resp = (HttpWebResponse)(await webReq.GetResponseAsync());
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                Stream dataStream = resp.GetResponseStream();
                StreamReader streamReader = new StreamReader(dataStream);
                string content = await streamReader.ReadToEndAsync();
                streamReader.Close();
                dataStream.Close();
                resp.Close();
                return messageInterpreter(content);
            }
            resp.Close();
            return default;
        }

        public static async Task<bool> PerformRemotePostCommand(string commandName, params object[] arguments)
        {
            WebRequest webReq = CreateRequest(commandName, arguments);
            HttpWebResponse resp = (HttpWebResponse)(await webReq.GetResponseAsync());
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                Stream dataStream = resp.GetResponseStream();
                StreamReader streamReader = new StreamReader(dataStream);
                string content = await streamReader.ReadToEndAsync();
                streamReader.Close();
                dataStream.Close();
                resp.Close();
                return content.CompareTo("1") == 0;
            }
            resp.Close();
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static WebRequest CreateRequest(string commandName, params object[] arguments)
        {
            WebRequest webReq = WebRequest.Create(EndPoint);
            webReq.Headers.Add(CommandNameParameter, commandName);
            webReq.Headers.Add(SessionIDParameter, SessionID);
            for (int i = 0; i < arguments.Length; ++i)
                webReq.Headers.Add(ParameterNames[i], arguments[i].ToString());
            webReq.Timeout = RequestTimeout;
            return webReq;
        }
    }
}
