using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace BooleanSearch
{
    /// <summary>
    /// First draft. Work in progress...
    /// 
    /// TODO:
    /// * Query parsing and build expression tree.
    /// * Move all code to it own classes, separate user interaction, getting data, indexing and searching from each other.
    /// * Save index on disk to restore it on next start. Check index file date and source file modify date and rebuild index if needed.
    /// * Write unit tests
    /// * Clean up the code.
    /// 
    /// It's just a example, so I don't want to implement jokers or checking typo in term or search query plan optimizing. It's a path of samurai, endless path.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            const string Filename = "notebooks.csv";

            var notebooks = new Dictionary<int, Notebook>();
            var invertedIndex = new Dictionary<string, List<int>>();

            Console.WriteLine($"Filename: {Filename}");
            Console.WriteLine();

            Console.WriteLine($"Start indexing...");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var counter = ParseFileAndBuildIndex(Filename, ref invertedIndex, ref notebooks);

            stopWatch.Stop();
            Console.WriteLine($"Indexing is finish in {stopWatch.ElapsedMilliseconds} ms");

            Console.WriteLine();
            Console.WriteLine($"Lines: {counter}.");
            Console.WriteLine($"Inverted index terms size: {invertedIndex.Keys.Count}.");
            Console.WriteLine($"Inverted index memory size: {GetObjectSize(invertedIndex)}.");
            Console.WriteLine();

            // just for fun
            PrintSomeStatistics(ref invertedIndex);

            // demo search "apple and 15"
            DemoSearch(ref invertedIndex, ref notebooks);

            // demo search "iru or samsung"
            DemoSearch2(ref invertedIndex, ref notebooks);

            Console.ReadKey();
        }

        private static int ParseFileAndBuildIndex(
            string filename,
            ref Dictionary<string, List<int>> invertedIndex,
            ref Dictionary<int, Notebook> notebooks)
        {
            int counter = 0;
            string line;
            string idStr = string.Empty;
            string brand = string.Empty;
            string model = string.Empty;

            using (StreamReader file = new StreamReader(filename))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var i = 0;
                    while (line[i] != ',' && i < line.Length)
                    {
                        i++;
                    }

                    idStr = line.Substring(0, i++);

                    // Ignore items without id. 
                    if (!int.TryParse(idStr, out var id))
                    {
                        continue;
                    }

                    var notebook = new Notebook() { };
                    var descriptionStart = i;

                    // TODO: reuse this array.
                    var terms = new List<string>();
                    int termStart = 0;

                    // We look on brand and model same way, just because we don't need any ranging yet
                    while (true)
                    {
                        // TODO: Move symbols to special constants
                        if (char.IsLetterOrDigit(line[i])
                            || line[i] == '-'
                            || line[i] == '.'
                            || line[i] == '/')
                        {
                            termStart++;
                        }
                        else
                        {
                            if (termStart > 0)
                            {
                                terms.Add(line.Substring(i - termStart, termStart).ToLower());
                            }

                            termStart = 0;
                        }

                        if (line[i] == ',')
                        {
                            notebook.Brand = line.Substring(descriptionStart, i - descriptionStart);
                            descriptionStart = i + 1;
                        }

                        if (++i == line.Length)
                        {
                            if (termStart > 0)
                            {
                                terms.Add(line.Substring(i - termStart, termStart).ToLower());
                            }

                            notebook.Model = line.Substring(descriptionStart, i - descriptionStart);
                            notebooks.Add(id, notebook);

                            break;
                        }
                    }

                    foreach (var t in terms)
                    {
                        if (invertedIndex.ContainsKey(t))
                        {
                            if (!invertedIndex[t].Any(l => l == id))
                            {
                                invertedIndex[t].Add(id);
                            }
                        }
                        else
                        {
                            invertedIndex[t] = new List<int>() { id };
                        }
                    }

                    counter++;
                }
            }

            return counter;
        }

        private static List<int> FindInIndex(string text, ref Dictionary<string, List<int>> index)
        {
            List<int> result = null;

            if (index.ContainsKey(text))
            {
                result = new List<int>();
                result.AddRange(index[text]);
            }

            return result;
        }

        private static int GetObjectSize(object TestObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] Array;
            bf.Serialize(ms, TestObject);
            Array = ms.ToArray();
            return Array.Length;
        }

        public struct Notebook
        {
            public string Brand { get; set; }

            public string Model { get; set; }
        }

        private static void PrintSomeStatistics(ref Dictionary<string, List<int>> invertedIndex)
        {
            var statsCountToShow = 10;
            var stats = invertedIndex.Select(index => (term: index.Key, count: index.Value.Count())).OrderByDescending(t => t.count).ToList();

            Console.WriteLine($"{statsCountToShow} most popular terms");

            for (var i = 0; i < statsCountToShow; i++)
            {
                Console.WriteLine($"#{i + 1}: \"{stats[i].term}\" Finded in {stats[i].count} sku.");
            }
        }

        private static void DemoSearch(
            ref Dictionary<string, List<int>> invertedIndex,
            ref Dictionary<int, Notebook> notebooks)
        {
            var appleRes = FindInIndex("apple", ref invertedIndex);
            var thirteenRes = FindInIndex("13", ref invertedIndex);

            var appleAndthirteen = appleRes.Intersect(thirteenRes).ToList();

            Console.WriteLine();
            Console.WriteLine("Demo version is limited. You can enter on one words on each condition. Conditional is '&', '||' and '!'. No brackets.");
            Console.WriteLine();

            // TODO: we don't care about "Apple MacBook 12" and "Apple MacBook 12.0" case yet.
            Console.WriteLine($"For example, request 'apple && 13'. Press any key to run search...");
            Console.ReadKey();

            foreach (var id in appleAndthirteen)
            {
                var notebook = notebooks[id];
                Console.WriteLine($"Id {id}. Brand {notebooks[id].Brand}, Model {notebooks[id].Model}");
            }

            Console.WriteLine($"Finded {appleAndthirteen.Count} results");
            Console.WriteLine();
        }

        private static void DemoSearch2(
            ref Dictionary<string, List<int>> invertedIndex,
            ref Dictionary<int, Notebook> notebooks)
        {
            var iruRes = FindInIndex("iru", ref invertedIndex);
            var samsungRes = FindInIndex("samsung", ref invertedIndex);

            var iruOrsamsung = samsungRes.Concat(iruRes).ToList();

            Console.WriteLine();

            // TODO: we don't care about "Apple MacBook 12" and "Apple MacBook 12.0" case yet.
            Console.WriteLine($"For example, request 'iru || samsung':. Press any key to run search...");
            Console.ReadKey();

            foreach (var id in iruOrsamsung)
            {
                var notebook = notebooks[id];
                Console.WriteLine($"Id {id}. Brand {notebooks[id].Brand}, Model {notebooks[id].Model}");
            }

            Console.WriteLine($"Finded {iruOrsamsung.Count} results");
            Console.WriteLine();
        }
    }
}
