using BooleanRetrieval.Logic.DataSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.Indexing
{
    public interface IInvertedIndexBuilder
    {
        IIndex BuildIndex(INotebookDataSource dataSource);
    }
}
