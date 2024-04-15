using Dirt.CLI;
using Newtonsoft.Json;

namespace GameContent
{
    public class Program
    {
        public static bool Prettify;
        static void Main(string[] args)
        {
            ProgramParser parser = new ProgramParser();
            Prettify = false;

            parser.HandleOption("p", "prettify", () => Prettify = true, "Prettiy the output JSON");

            if (!parser.Parse(args, 2, "Input", "Output"))
            {
                return;
            }

            string input = parser.Inputs[0];
            string output = parser.Inputs[1];

            DirectoryInfo inputDir = new DirectoryInfo(input);
            FileInfo outputFile = new FileInfo(output);

            string outputFilename = outputFile.Name;

            Dictionary<string, string> filemap = new Dictionary<string, string>();

            AddFolder(inputDir, filemap, string.Empty);

            for(int i = 2; i < parser.Inputs.Count; ++i)
            {
                string addDir = parser.Inputs[i];
                string path = string.Empty;
                if (addDir.StartsWith(input))
                {
                    path = addDir.Substring(input.Length + 1) + "/";
                }

                AddFolder(new DirectoryInfo(addDir), filemap, path);
            }


            var gamecontent = new Dirt.Game.Content.GameContent()
            {
                FileMap = filemap
            };

            var outputStr = JsonConvert.SerializeObject(gamecontent, Prettify ? Formatting.Indented : Formatting.None);
            File.WriteAllText(output, outputStr);
        }

        private static void AddFolder(DirectoryInfo dir, Dictionary<string, string> filemap, string path = "")
        {
            var assets = dir.GetFiles("*.*");
            for (int i = 0; i < assets.Length; ++i)
            {
                string ext = assets[i].Extension;
                string name = assets[i].Name.Substring(0, assets[i].Name.Length - ext.Length);

                if (filemap.ContainsKey(name))
                {
                    System.Console.WriteLine($"<Error> - Duplicate file {name} found in {dir.FullName}");
                    continue;
                }
                else
                {
                    filemap.Add(name, $"{path}{assets[i].Name}");
                }
            }
        }

        private static string GetRequiredArg(string[] args, int argNumber)
        {
            int c = 0;
            for (int i = 0; i < args.Length; ++i)
            {
                if (!args[i].StartsWith("-"))
                {
                    if (argNumber == c)
                        return args[i];
                    c++;
                }
            }
            return string.Empty;
        }

        private static bool GetOption(string[] args, string optionName)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("-"))
                {
                    string value = args[i].Substring(1);
                    if (value == optionName)
                        return true;
                }
            }
            return false;
        }
    }
}
