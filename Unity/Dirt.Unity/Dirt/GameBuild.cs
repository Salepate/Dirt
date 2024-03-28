using System;
using UnityEngine;

namespace Dirt
{
    public class GameBuild : ScriptableObject
    {
        public static readonly string FileName = "GameBuild";
        public int BuildNumber;
        public string ProjectName;
        public string BinaryName;
        public string ContentDestinationPath;
        public Version GetVersion()
        {
            Version appVersion = Version.Parse(Application.version);
            return new Version(appVersion.Major, appVersion.Minor, BuildNumber);
        }

        public static GameBuild LoadBuildInformation()
        {
            GameBuild res = Resources.Load<GameBuild>(FileName);
            if ( res == null )
            {
                Log.Console.Warning("No build file found");
                res = ScriptableObject.CreateInstance<GameBuild>();
            }
            return res;
        }
    }
}