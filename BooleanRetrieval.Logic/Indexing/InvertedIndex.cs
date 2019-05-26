using System;
using System.Collections.Generic;

namespace BooleanRetrieval.Logic.Indexing
{
    /// <summary>
    /// Serializable - just for memory statistics
    /// and for potential saving in file.
    /// </summary>
    [Serializable]
    public class InvertedIndex: Dictionary<string, HashSet<int>>
    {
        public List<int> FindInIndex(string text)
        {
            // make a copy for list just in case...
            List<int> result = new List<int>();

            if (ContainsKey(text))
            {
                result.AddRange(this[text]);
            }

            return result;
        }
    }
}
