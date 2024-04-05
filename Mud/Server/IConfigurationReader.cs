namespace Mud.Server
{
    public interface IConfigurationReader
    {
        int GetInt(string key);
        string GetString(string key);
    }
}
