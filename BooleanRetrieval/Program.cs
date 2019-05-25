using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.DataGenerating;
using BooleanRetrieval.Logic.Searching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BooleanRetrieval.Logic.DataSource;

namespace BooleanRetrieval
{
    /// <summary>
    /// TODO:
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
            // const string Filename = "notebooks_210000.csv";
            const string Filename = "notebooks.csv";

            /*var arguments = new List<string>() { "--generate", "210000" };
            args = arguments.ToArray();*/

            if (args.Count() >= 2 && args[0] == "--generate")
            {
                if (int.TryParse(args[1], out var number))
                {
                    ExampleDataGenerator.Generate(Filename, number);
                    return;
                }
            }
            
            Console.WriteLine($"Start indexing");
            
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var storage = new NotebooksFileDataSource(Filename);

            var indexer = new InvertedIndexer();
            indexer.BuildIndex(storage);

            stopWatch.Stop();
            Console.WriteLine($"Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

            Console.WriteLine();
            Console.WriteLine($"Notebooks: {storage.Notebooks.Count}.");
            Console.WriteLine($"Inverted index terms size: {indexer.Index.Keys.Count}.");
            Console.WriteLine($"Inverted index memory size: {GetObjectSize(indexer.Index)}.");
            Console.WriteLine();

            // just for fun
            PrintSomeStatistics(indexer.Index);

            Console.WriteLine();
            UserQueryMode(new SimpleSearcher(indexer, storage), storage);

            /*
            // Some demo searches
            DemoSearch("apple && 13", storage.Notebooks, () => {
                var r1 = indexer.FindInIndex("apple");
                var r2 = indexer.FindInIndex("13");

                return r1.Intersect(r2).ToList();
            });

            DemoSearch("iru || samsung", storage.Notebooks, () => {
                var r1 = indexer.FindInIndex("iru");
                var r2 = indexer.FindInIndex("samsung");

                return r1.Concat(r2).ToList();
            });

            DemoSearch("apple air && ! 11 && ! 11.6", storage.Notebooks, () =>
            {
                var r1 = indexer.FindInIndex("apple");
                var r2 = indexer.FindInIndex("air");
                var r3 = indexer.FindInIndex("11");
                var r4 = indexer.FindInIndex("11.6");

                return r1.Intersect(r2).Except(r3).Except(r4).ToList();
            });*/

            Console.ReadKey();
        }

        private static void UserQueryMode(SimpleSearcher searcher, NotebooksFileDataSource storage)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
            Stopwatch stopWatch = new Stopwatch();

            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter the query: ");
                string query = Console.ReadLine();
                stopWatch.Reset();
                stopWatch.Start();

                try
                {
                    var result = searcher.Search(query);

                    stopWatch.Stop();

                    PrintResults(result, stopWatch.ElapsedMilliseconds, storage);
                }
                catch
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid query format");
                    Console.WriteLine();
                }
            }
        }

        private static void PrintResults(List<int> result, long timeMeasure, NotebooksFileDataSource storage)
        {
            Console.WriteLine();
            var maxShow = Math.Min(result.Count, 10);

            if (maxShow > 0)
            {
                Console.WriteLine($"First {maxShow} results: ");
                for (var r = 0; r < 10; r++)
                {
                    var notebook = storage.Notebooks[result[r]];
                    Console.WriteLine($"Id {result[r]}. Brand {notebook.Brand}, Model {notebook.Model}");
                }

                Console.WriteLine();
            }

            Console.WriteLine($"Total results: {result.Count}. Search time: {timeMeasure} ms.");
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
            NotebooksFileDataSource storage,
            Func<List<int>> func)
        {
            Console.WriteLine($"For example, request \"{searchText}\": Press any key to run search...");
            Console.ReadKey();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = func();

            stopWatch.Stop();

            Console.WriteLine();

            PrintResults(result, stopWatch.ElapsedMilliseconds, storage);
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit application");

            Environment.Exit(0);
        }
    }
}
