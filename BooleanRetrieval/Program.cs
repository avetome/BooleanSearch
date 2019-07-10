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
using BooleanRetrieval.Logic.QueryParsing;

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

            var storage = new NotebooksFileDataSource(Filename);

            var index = BuildIndex(storage); 

            if (args.Count() == 1 && args[0] == "--demo")
            {
                DemoSearch(index, storage);
                return;
            }

            UserQueryMode(index, storage);

            Console.ReadKey();
        }

        private static IIndex BuildIndex(NotebooksFileDataSource storage)
        {
            Console.WriteLine($"Start indexing");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var indexBuilder = new InvertedIndexBuilder();
            var index = indexBuilder.BuildIndex(storage);

            stopWatch.Stop();
            Console.WriteLine($"Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

            Console.WriteLine();
            Console.WriteLine($"Notebooks: {storage.GetAllNotebook().Count}.");
            Console.WriteLine($"Inverted index terms size: {index.Size()}.");
            Console.WriteLine($"Inverted index memory size: {GetObjectSize(index)}.");
            Console.WriteLine();

            return index;
        }

        private static void UserQueryMode(IIndex index, NotebooksFileDataSource storage)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
            Stopwatch stopWatch = new Stopwatch();

            var searcher = new EtSearcher(index, storage);

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
                for (var r = 0; r < maxShow; r++)
                {
                    var notebook = storage.GetAllNotebook()[result[r]];
                    Console.WriteLine($"Id {result[r]}. Brand {notebook.Brand}, Model {notebook.Model}");
                }

                Console.WriteLine();
            }

            Console.WriteLine($"Total results: {result.Count}. Search time: {timeMeasure} ms.");
        }

        private static void DemoSearch(IIndex index, NotebooksFileDataSource storage)
        {
            var queries = new[] {
                "apple && 13",
                "iru || samsung",
                "apple air && ! 11 && ! 11.6" };

            Stopwatch stopWatch = new Stopwatch();

            var searcher = new EtSearcher(index, storage);

            foreach (var query in queries)
            {
                Console.WriteLine();
                Console.Write($"Searching {query}:");
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

        private static int GetObjectSize(object TestObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] Array;
            bf.Serialize(ms, TestObject);
            Array = ms.ToArray();
            return Array.Length;
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit application");

            Environment.Exit(0);
        }
    }
}
