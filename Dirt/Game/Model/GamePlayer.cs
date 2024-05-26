namespace Dirt.Game.Model
{
    [System.Serializable]
    public class GamePlayer
    {
        public const string DebugFormat = "{0} ({1})";
        public int Number;
        public string Name;
        public GamePlayer()
        {
            Name = string.Empty;
        }

        public override string ToString()
        {
            return string.Format(DebugFormat, Name, Number);
        }
    }
}
