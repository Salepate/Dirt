using System.Collections.Generic;

namespace Dirt.GameServer.PlayerStore.Model
{
    [System.Serializable]
    public class RegistrationCodeTable
    {
        public const int CodeSize = 16;
        public const string ValidCharacters = "ABCDEFGHIJKMNPQRSTUVWXYZ0123456789";
        public string FileName { get; set; }
        public List<string> Codes;

        public RegistrationCodeTable()
        {
            Codes = new List<string>();
        }

        public static bool IsValidFormat(string code)
        {
            if (code.IndexOf('-') != -1)
            {
                code = code.Replace("-", string.Empty);
            }

            if (code.Length != CodeSize)
            {
                return false;
            }

            for(int i = 0; i < code.Length; ++i)
            {
                if (ValidCharacters.IndexOf(code[i]) == -1)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
