using System;
using System.Collections.Generic;
using System.IO;

namespace BooleanRetrieval.Logic.DataGenerating
{
    public static class ExampleDataGenerator
    {
        public static void Generate(string fileWithOriginData, int dataCount)
        {
            var notebooks = new List<string>();

            var notebooksInFileCount = ReadNotebooks(fileWithOriginData, ref notebooks);

            var newfilename = $"notebooks_{dataCount}.csv";
            var rnd = new Random();
            var idHash = new HashSet<int>();

            var j = 0;

            using (StreamWriter file = new StreamWriter(newfilename))
            {
                for (var i = 0; i < dataCount; i++)
                {
                    if (j == notebooksInFileCount)
                    {
                        j = 0;
                    }

                    var id = rnd.Next(0, 90000000);

                    while (idHash.Contains(id))
                    {
                        id = rnd.Next(0, 90000000);
                    }

                    idHash.Add(id);

                    file.WriteLine($"{id},{notebooks[j++]}");
                }
            }
        }

        private static int ReadNotebooks(string filename, ref List<string> notebooks)
        {
            int counter = 0;

            foreach (var line in File.ReadLines(filename))
            {
                var i = 0;
                while (line[i] != ',' && i < line.Length)
                {
                    i++;
                }

                notebooks.Add(line.Substring(++i));

                counter++;
            }

            return counter;
        }
    }
}
