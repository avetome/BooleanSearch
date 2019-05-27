using BooleanRetrieval.Logic.DataSource;
using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.QueryParsing;
using System.Collections.Generic;
using System.Linq;

namespace BooleanRetrieval.Logic.Searching
{
    public class SimpleSearcher
    {
        private readonly IIndex _index;
        private readonly INotebookDataSource _dataSource;

        public SimpleSearcher(IIndex index, INotebookDataSource dataSource)
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
                return _index.Find(parsedQuery[0]);
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
                        result = _dataSource.GetAllIds().Except(_index.Find(parsedQuery[i++])).ToList();
                    }
                    else
                    {
                        result = _index.Find(parsedQuery[i++]);
                    }
                }

                if (parsedQuery[i] == "AND")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Except(_index.Find(parsedQuery[++i])).ToList();
                    }
                    else
                    {
                        result = result.Intersect(_index.Find(parsedQuery[i])).ToList();
                    }
                }
                else if (parsedQuery[i] == "OR")
                {
                    i++;

                    if (parsedQuery[i] == "NOT")
                    {
                        result = result.Concat(_dataSource.GetAllIds().Except(_index.Find(parsedQuery[++i]))).ToList();
                    }
                    else
                    {
                        result = result.Concat(_index.Find(parsedQuery[i])).ToList();
                    }
                }

                i++;
            }

            return result;
        }
    }
}
