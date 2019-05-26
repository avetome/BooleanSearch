using BooleanRetrieval.Logic.DataSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.Indexing
{
    public interface IIndexer
    {
        void BuildIndex(INotebookDataSource dataSource);

        List<int> FindInIndex(string text);
    }
}
