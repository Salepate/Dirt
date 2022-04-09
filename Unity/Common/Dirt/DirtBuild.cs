using System;
using UnityEngine;

namespace Dirt
{
    public class DirtBuild : ScriptableObject
    {
        public string ProjectName;
        public string BinaryName;
        public string ContentPath;

        public int BuildNumber;
        public const string FileName = "ProjectBuilder";

        public Version GetVersion()
        {
            Version appVersion = Version.Parse(Application.version);
            return new Version(appVersion.Major, appVersion.Minor, BuildNumber);
        }

        public static DirtBuild LoadBuildInformation()
        {
            return Resources.Load<DirtBuild>(FileName);
        }
    }
}