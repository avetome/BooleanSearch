using BooleanRetrieval.Logic.DataSource;
using BooleanRetrieval.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BooleanRetrieval.Logic.Indexing
{
    /// <summary>
    /// Build a simple inverted index on file.
    /// </summary>
    public class InvertedIndexBuilder : IInvertedIndexBuilder
    {
        public IIndex BuildIndex(INotebookDataSource dataSource)
        {
            var index = new InvertedIndex();

            string line;

            var notebooks = dataSource.GetAllNotebook();

            foreach(var item in notebooks)
            {
                // We look on brand and model same way, just because we don't need any ranging yet
                line = item.Value.Brand + "," + item.Value.Model;

                int termStart = 0;

                var i = 0;

                while (true)
                {
                    if (char.IsLetterOrDigit(line[i]) || SearchOption.AcceptableSymbols.Contains(line[i]))
                    {
                        termStart++;
                    }
                    else
                    {
                        if (termStart > 0)
                        {
                            index.Add(line.Substring(i - termStart, termStart).ToLower(), item.Key);
                        }

                        termStart = 0;
                    }
                    
                    if (++i == line.Length)
                    {
                        if (termStart > 0)
                        {
                            index.Add(line.Substring(i - termStart, termStart).ToLower(), item.Key);
                        }

                        break;
                    }
                }
            }

            return index;
        }
    }
}
