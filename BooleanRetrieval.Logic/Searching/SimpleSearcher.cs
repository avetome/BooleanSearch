using BooleanRetrieval.Logic.DataSource;
using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.QueryParsing;
using System.Collections.Generic;
using System.Linq;

namespace BooleanRetrieval.Logic.Searching
{
    public class SimpleSearcher
    {
        private readonly InvertedIndex _index;
        private readonly INotebookDataSource _dataSource;

        public SimpleSearcher(InvertedIndex index, INotebookDataSource dataSource)
        {
            _index = index;
            _dataSource = dataSource;
        }

        public List<int> Search(string query)
        {
            var parser = new SimpleQueryParser(query);
            var parsedQuery = parser.Parse();

            if (parsedQuery.Length == 1)
            {
                return _index.FindInIndex(parsedQuery[0]);
            }

            var i = 0;
            var result = new List<int>();
            while (i < parsedQuery.Length)
            {
                if (i == 0)
                {
                    var invertedFirstArg = parsedQuery[i] == "NOT";

                    i = invertedFirstArg ? ++i : i;

                    if (invertedFirstArg)
                    {
                        result = _dataSource.GetAllIds().Except(_index.FindInIndex(parsedQuery[i++])).ToList();
                    }
                    else
                    {
                        result = _index.FindInIndex(parsedQuery[i++]);
                    }
                }

                if (parsedQuery[i] == "AND")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Except(_index.FindInIndex(parsedQuery[++i])).ToList();
                    }
                    else
                    {
                        result = result.Intersect(_index.FindInIndex(parsedQuery[i])).ToList();
                    }
                }
                else if (parsedQuery[i] == "OR")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Concat(_dataSource.GetAllIds().Except(_index.FindInIndex(parsedQuery[++i]))).ToList();
                    }
                    else
                    {
                        result = result.Concat(_index.FindInIndex(parsedQuery[i])).ToList();
                    }
                }

                i++;
            }

            return result;
        }
    }
}
