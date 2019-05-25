using BooleanRetrieval.Logic.DataSource;
using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.QueryParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BooleanRetrieval.Logic.Searching
{
    public class SimpleSearcher
    {
        private readonly IIndexer _indexer;
        private readonly INotebookDataSource _dataSource;

        public SimpleSearcher(IIndexer indexer, INotebookDataSource dataSource)
        {
            _indexer = indexer;
            _dataSource = dataSource;
        }

        public List<int> Search(string query)
        {
            var parser = new SimpleQueryParser(query);
            var parsedQuery = parser.SimpleParse();

            var i = 0;
            var result = new List<int>();
            while (i < parsedQuery.Length)
            {
                var invertedFirstArg = parsedQuery[i] == "NOT";

                i = invertedFirstArg ? ++i : i;

                if (invertedFirstArg)
                {
                    result = _dataSource.GetAllIds().Except(_indexer.FindInIndex(parsedQuery[i++])).ToList();
                }
                else
                {
                    result = _indexer.FindInIndex(parsedQuery[i++]);
                }

                if (parsedQuery[i] == "AND")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Except(_indexer.FindInIndex(parsedQuery[++i])).ToList();
                    }
                    else
                    {
                        result = result.Intersect(_indexer.FindInIndex(parsedQuery[i])).ToList();
                    }
                }
                else if (parsedQuery[i] == "OR")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Concat(_dataSource.GetAllIds().Except(_indexer.FindInIndex(parsedQuery[++i]))).ToList();
                    }
                    else
                    {
                        result = result.Concat(_indexer.FindInIndex(parsedQuery[i])).ToList();
                    }
                }

                i++;
            }

            return result;
        }
    }
}
