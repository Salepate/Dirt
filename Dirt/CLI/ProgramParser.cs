#pragma warning disable CS8625

using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace Dirt.CLI
{
    public class ProgramParser
    {
        public string[] Args { get; private set; }
        public List<string> Inputs { get; private set; }

        private Dictionary<string, Action> Options; // booleans
        private Dictionary<string, Action<string>> Parameters; // value specified
        private List<string> m_FormattedOptions;
        public ProgramParser()
        {
            Inputs = new List<string>();
            Args = Array.Empty<string>();
            Options = new Dictionary<string, Action>();
            Parameters = new Dictionary<string, Action<string>>();
            m_FormattedOptions = new List<string>();
        }

        public void HandleOption(string shortKey, string longKey, Action setter, string description = "")
        {
            Options.Add($"-{shortKey}", setter);
            Options.Add($"--{longKey}", setter);
            m_FormattedOptions.Add($"-{shortKey} --{longKey} {description}");
        }

        public void HandleParameter(string shortKey, string longKey, Action<string> setter, string description = "")
        {
            Parameters.Add($"-{shortKey}", setter);
            Parameters.Add($"--{longKey}", setter);
            m_FormattedOptions.Add($"-{shortKey} --{longKey} {description}");
        }

        public bool Parse(string[] args, int requiredInputs = 0, params string[] inputLabels)
        {
            Args = args;
            bool previousArgWasParam = false;

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i][0] != '-' || previousArgWasParam)
                {
                    if (!previousArgWasParam)
                    {
                        Inputs.Add(args[i]);
                    }
                    else
                    {
                        previousArgWasParam = false;
                    }
                }
                else
                {
                    if (Options.ContainsKey(args[i]))
                        Options[args[i]]();
                    if (Parameters.ContainsKey(args[i]))
                    {
                        if (i + 1 >= args.Length)
                            return false;

                        Parameters[args[i]](args[i + 1]);
                        previousArgWasParam = true;
                    }
                }
            }

            if (Inputs.Count < requiredInputs)
            {
                string binaryName = System.AppDomain.CurrentDomain.FriendlyName;
                string inputs = string.Empty;

                int count = requiredInputs;
                if (inputLabels != null)
                    count = Math.Max(requiredInputs, inputLabels.Length);

                for(int i = 0; i < count; ++i)
                {
                    if (inputLabels != null && i < inputLabels.Length)
                        inputs += $"<{inputLabels[i]}>";
                    else
                        inputs += $"<input{i}>";

                    if (i == requiredInputs - 1 && i < count - 1)
                        inputs += " [";
                    else if (i + 1 < count)
                        inputs += " ";
                }

                if (count > requiredInputs)
                    inputs += "]";

                Console.Error.WriteLine($"Usage: {binaryName}.exe [options] {inputs}");
                Console.WriteLine("------------------\nValid Options\n------------------");
                for(int i = 0; i < m_FormattedOptions.Count; ++i)
                {
                    Console.Error.WriteLine(m_FormattedOptions[i]);
                }
                Console.WriteLine("------------------");
                return false;
            }
            return true;
        }


        public FileInfo[] ConvertInputs(int inputIndex, System.Func<FileInfo, bool> matchPredicate)
        {
            List<FileInfo> files = new List<FileInfo>();
            string input = Inputs[inputIndex];
            var directoryInfo = new DirectoryInfo(input);
            FileInfo pathInfo = new FileInfo(input);

            if (directoryInfo.Exists)
            {
                FileInfo[] dirFiles = directoryInfo.GetFiles();
                for (int i = 0; i < dirFiles.Length; ++i)
                {
                    var file = dirFiles[i];
                    if (!matchPredicate(file))
                        continue;

                    files.Add(file);
                }
            }
            else if (pathInfo.Exists && matchPredicate(pathInfo))
            {
                files.Add(pathInfo);
            }

            return files.ToArray();
        }
    }
}
