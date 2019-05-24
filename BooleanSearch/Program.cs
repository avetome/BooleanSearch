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
    /// It's just a example, so I don't want to implement jokers or checking typo in terms or do search query plan optimizing. It's a path of samurai, endless path.
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

            // Some demo searches
            DemoSearch("apple && 13", ref notebooks, () => {
                var r1 = FindInIndex("apple", ref invertedIndex);
                var r2 = FindInIndex("13", ref invertedIndex);

                return r1.Intersect(r2).ToList();
            });

            DemoSearch("iru || samsung", ref notebooks, () => {
                var r1 = FindInIndex("iru", ref invertedIndex);
                var r2 = FindInIndex("samsung", ref invertedIndex);

                return r1.Concat(r2).ToList();
            });

            DemoSearch("apple air && ! 11 && ! 11.6", ref notebooks, () =>
            {
                var r1 = FindInIndex("apple", ref invertedIndex);
                var r2 = FindInIndex("air", ref invertedIndex);
                var r3 = FindInIndex("11", ref invertedIndex);
                var r4 = FindInIndex("11.6", ref invertedIndex);

                return r1.Intersect(r2).Except(r3).Except(r4).ToList();
            });

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
            string searchText,
            ref Dictionary<int, Notebook> notebooks,
            Func<List<int>> func)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = func();

            stopWatch.Stop();

            Console.WriteLine();

            Console.WriteLine($"For example, request \"{searchText}\": Press any key to run search...");
            Console.ReadKey();

            foreach (var id in result)
            {
                var notebook = notebooks[id];
                Console.WriteLine($"Id {id}. Brand {notebooks[id].Brand}, Model {notebooks[id].Model}");
            }

            Console.WriteLine($"Finded {result.Count} results. Time: {stopWatch.ElapsedMilliseconds} ms.");
            Console.WriteLine();
        }
    }
}
