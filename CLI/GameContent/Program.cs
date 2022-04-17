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
                System.Console.WriteLine($"Usage: <input directory> <output filename>");
                return;
            }

            string input = GetRequiredArg(args, 0);
            string output = GetRequiredArg(args, 1);
            bool prettify = GetOption(args, "p");

            DirectoryInfo inputDir = new DirectoryInfo(input);

            FileInfo outputFile = new FileInfo(output);

            string outputFilename = outputFile.Name;

            Dictionary<string, string> filemap = new Dictionary<string, string>();

            var jsons = inputDir.GetFiles("*.*");
            for(int i = 0; i < jsons.Length; ++i)
            {
                string ext = jsons[i].Extension;
                string name = jsons[i].Name.Substring(0, jsons[i].Name.Length - ext.Length);
                if (jsons[i].Name == outputFilename)
                    continue;
                try
                { 
                    filemap.Add(name, jsons[i].Name);
                }
                catch(System.Exception e)
                {
                    System.Console.WriteLine($"Error adding {jsons[i].Name}");
                    System.Console.WriteLine(e.Message);
                }
            }

            var gamecontent = new Dirt.Game.Content.GameContent()
            {
                FileMap = filemap
            };

            var outputStr =  JsonConvert.SerializeObject(gamecontent, prettify ? Formatting.Indented : Formatting.None);
            File.WriteAllText(output, outputStr);
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
