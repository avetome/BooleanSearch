using System;
using System.Collections.Generic;

namespace BooleanRetrieval.Logic.Indexing
{
    /// <summary>
    /// Serializable - just for memory statistics
    /// and for potential saving in file.
    /// </summary>
    [Serializable]
    public class InvertedIndex: Dictionary<string, HashSet<int>>, IIndex
    {
        public List<int> Find(string text)
        {
            // make a copy for list just in case...
            List<int> result = new List<int>();

            if (ContainsKey(text))
            {
                result.AddRange(this[text]);
            }

            return result;
        }

        public void Add(string term, int id)
        {
            if (ContainsKey(term))
            {
                if (!this[term].Contains(id))
                {
                    this[term].Add(id);
                }
            }
            else
            {
                this[term] = new HashSet<int>() { id };
            }
        }

        public int Size()
        {
            return Keys.Count;
        }
    }
}
