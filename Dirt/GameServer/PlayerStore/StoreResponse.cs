using Newtonsoft.Json;

namespace Dirt.GameServer.PlayerStore
{
    [System.Serializable]
    public class StoreResponse
    {
        public bool Error;
        public bool Encoded;
        public int ErrorCode;
        public string Message;

        public static string RespondWithError(int error, string message)
        {
            StoreResponse resp = new StoreResponse()
            {
                Error = true,
                Encoded = false,
                ErrorCode = error,
                Message = message
            };
            return JsonConvert.SerializeObject(resp);
        }

        public static string RespondWithJson(object message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }

    public class StoreCredentialResponse
    {
        public string CompleteUsername;
    }

    public class StoreValidAuthResponse
    {
        public bool WasAuthed;

        public StoreValidAuthResponse()
        {

        }

        public StoreValidAuthResponse(bool auth)
        {
            WasAuthed = auth;
        }
    }
}
