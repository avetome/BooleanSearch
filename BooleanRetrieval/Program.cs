using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.DataGenerating;
using BooleanRetrieval.Logic.Searching;
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
                    ExampleDataGenerator.Generate(Filename, number);
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

            stopWatch.Reset();
            stopWatch.Start();

            var searcher = new SimpleSearcher(indexer);
            var result = searcher.Search("iru || samsung");

            stopWatch.Stop();

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

            Console.WriteLine($"Total results: {result.Count}. Search time: {stopWatch.ElapsedMilliseconds} ms.");
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
            Console.WriteLine($"Total results: {result.Count}. Search time: {stopWatch.ElapsedMilliseconds} ms.");
        } 
    }
}
