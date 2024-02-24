using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace GameContent
{
    public class Program
    {
        static void Main(string[] args)
        {
            if ( args.Length < 2 )
            {
                System.Console.WriteLine($"Usage: <input directory> <output filename> [<additionalDirA> <additionalDirB>]");
                return;
            }

            string input = GetRequiredArg(args, 0);
            string output = GetRequiredArg(args, 1);
            bool prettify = GetOption(args, "p");

            DirectoryInfo inputDir = new DirectoryInfo(input);
            FileInfo outputFile = new FileInfo(output);

            string outputFilename = outputFile.Name;

            Dictionary<string, string> filemap = new Dictionary<string, string>();

            AddFolder(inputDir, filemap, string.Empty);
            int addDirs = 2;
            string addDir = GetRequiredArg(args, addDirs);
            while(!string.IsNullOrEmpty(addDir))
            {
                string path = string.Empty;
                if (addDir.StartsWith(input))
                {
                    path = addDir.Substring(input.Length + 1) + "/";
                }

                AddFolder(new DirectoryInfo(addDir), filemap, path);
                addDir = GetRequiredArg(args, ++addDirs);
            }
            

            var gamecontent = new Dirt.Game.Content.GameContent()
            {
                FileMap = filemap
            };

            var outputStr =  JsonConvert.SerializeObject(gamecontent, prettify ? Formatting.Indented : Formatting.None);
            File.WriteAllText(output, outputStr);
        }

        private static void AddFolder(DirectoryInfo dir, Dictionary<string, string> filemap, string path = "")
        {
            var assets = dir.GetFiles("*.*");
            for(int i = 0; i < assets.Length; ++i)
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
