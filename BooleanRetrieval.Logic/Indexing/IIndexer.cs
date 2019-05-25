using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.Indexing
{
    public interface IIndexer
    {
        void BuildIndex(string filename);

        List<int> FindInIndex(string text);
    }
}
