using System.Collections.Generic;

namespace BooleanRetrieval.Logic.Indexing
{
    public interface IIndex
    {
        List<int> Find(string text);

        void Add(string term, int id);

        int Size();
    }
}