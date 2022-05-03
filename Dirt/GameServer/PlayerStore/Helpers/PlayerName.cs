namespace Dirt.GameServer.PlayerStore.Helpers
{
    public static class PlayerName
    {
        public static bool FromTag(string tag, out string username, out uint number)
        {
            username = null;
            number = 0;
            int sepIdx = tag.IndexOf('#');
            if (sepIdx > 0 )
            {
                username = tag.Substring(0, sepIdx);
                return uint.TryParse(tag.Substring(sepIdx + 1), out number);
            }
            return false;
        }
    }
}
