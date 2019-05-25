using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.QueryParsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace BooleanRetrieval
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
            //const string Filename = "notebooks_210000.csv";
            const string Filename = "notebooks.csv";

            /*var arguments = new List<string>() { "--generate", "210000" };
            args = arguments.ToArray();*/

            if (args.Count() >= 2 && args[0] == "--generate")
            {
                if (int.TryParse(args[1], out var number))
                {
                    GenerateDataFile(Filename, number);
                    return;
                }
            }

            Console.WriteLine($"Start indexing");
            
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var indexer = new InvertedIndexer();
            indexer.BuildIndex(Filename);

            stopWatch.Stop();
            Console.WriteLine($"Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

            Console.WriteLine();
            Console.WriteLine($"Notebooks: {indexer.Notebooks.Count}.");
            Console.WriteLine($"Inverted index terms size: {indexer.Index.Keys.Count}.");
            Console.WriteLine($"Inverted index memory size: {GetObjectSize(indexer.Index)}.");
            Console.WriteLine();

            // just for fun
            PrintSomeStatistics(indexer.Index);

            Console.WriteLine();

            var parser = new SimpleQueryParser("!iru && smsung");
            var parsedQuery = parser.SimpleParse();

            foreach (var parsed in parsedQuery)
            {
                Console.Write($"{parsed} ");
            }

            var i = 0;
            var result = new List<int>();
            while (i < parsedQuery.Length)
            {
                var invertedFirstArg = parsedQuery[i] == "NOT";

                i = invertedFirstArg ? ++i : i;

                if (invertedFirstArg)
                {
                    result = indexer.Notebooks.Keys.Except(indexer.FindInIndex(parsedQuery[i++])).ToList();
                }
                else
                {
                    result = indexer.FindInIndex(parsedQuery[i++]);
                }

                if (parsedQuery[i] == "AND")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Except(indexer.FindInIndex(parsedQuery[++i])).ToList();
                    }
                    else
                    {
                        result = result.Intersect(indexer.FindInIndex(parsedQuery[i])).ToList();
                    }
                }
                else if (parsedQuery[i] == "OR")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Concat(indexer.Notebooks.Keys.Except(indexer.FindInIndex(parsedQuery[++i]))).ToList();
                    }
                    else
                    {
                        result = result.Concat(indexer.FindInIndex(parsedQuery[i])).ToList();
                    }
                }

                i++;
            }

            Console.WriteLine();
            var maxShow = Math.Min(result.Count, 10);

            if (maxShow > 0)
            {
                Console.WriteLine($"First {maxShow} results: ");
                for (var r = 0; r < 10; r++)
                {
                    var notebook = indexer.Notebooks[result[r]];
                    Console.WriteLine($"Id {result[r]}. Brand {notebook.Brand}, Model {notebook.Model}");
                }

                Console.WriteLine();
            }

            Console.WriteLine($"Total results: {result.Count}.");
            Console.ReadLine();

            // Some demo searches
            DemoSearch("apple && 13", indexer.Notebooks, () => {
                var r1 = indexer.FindInIndex("apple");
                var r2 = indexer.FindInIndex("13");

                return r1.Intersect(r2).ToList();
            });

            DemoSearch("iru || samsung", indexer.Notebooks, () => {
                var r1 = indexer.FindInIndex("iru");
                var r2 = indexer.FindInIndex("samsung");

                return r1.Concat(r2).ToList();
            });

            DemoSearch("apple air && ! 11 && ! 11.6", indexer.Notebooks, () =>
            {
                var r1 = indexer.FindInIndex("apple");
                var r2 = indexer.FindInIndex("air");
                var r3 = indexer.FindInIndex("11");
                var r4 = indexer.FindInIndex("11.6");

                return r1.Intersect(r2).Except(r3).Except(r4).ToList();
            });

            Console.ReadKey();
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
        
        private static void PrintSomeStatistics(Dictionary<string, HashSet<int>> invertedIndex)
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
            Dictionary<int, Notebook> notebooks,
            Func<List<int>> func)
        {
            Console.WriteLine($"For example, request \"{searchText}\": Press any key to run search...");
            Console.ReadKey();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = func();

            stopWatch.Stop();

            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("First 10 results: ");
            for (var r = 0; r < 10; r++)
            {
                var notebook = notebooks[result[r]];
                Console.WriteLine($"Id {result[r]}. Brand {notebook.Brand}, Model {notebook.Model}");
            }

            Console.WriteLine();
            Console.WriteLine($"Total results: {result.Count}.");
        }

        private static void AddToInvertedIndex(string term, int id, ref Dictionary<string, HashSet<int>> invertedIndex)
        {
            if (invertedIndex.ContainsKey(term))
            {
                if (!invertedIndex[term].Contains(id))
                {
                    invertedIndex[term].Add(id);
                }
            }
            else
            {
                invertedIndex[term] = new HashSet<int>() { id };
            }
        }

        private static void GenerateDataFile(string filename, int count)
        {
            Console.WriteLine($"Generating data file with {count} notebooks");

            var notebooks = new List<string>();

            var notebooksInFileCount = ReadNotebooks(filename, ref notebooks);

            var newfilename = $"notebooks_{count}.csv";
            var rnd = new Random();
            var idHash = new HashSet<int>();

            var j = 0;

            using (StreamWriter file = new StreamWriter(newfilename))
            {
                for (var i = 0; i < count; i++)
                {
                    if (j == notebooksInFileCount)
                    {
                        j = 0;
                    }

                    var id =  rnd.Next(0, 90000000);

                    while(idHash.Contains(id))
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
            string line;

            using (StreamReader file = new StreamReader(filename))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var i = 0;
                    while (line[i] != ',' && i < line.Length)
                    {
                        i++;
                    }

                    notebooks.Add(line.Substring(++i));

                    counter++;
                }
            }

            return counter;
        }
    }
}
