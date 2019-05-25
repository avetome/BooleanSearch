using BooleanRetrieval.Logic.Models;
using System.Collections.Generic;

namespace BooleanRetrieval.Logic.DataSource
{
    public interface INotebookDataSource
    {
        /// <summary>
        /// In real life we will read data piece by piece, but here we can read at all - we store it in RAM anyway.
        /// </summary>
        /// <returns></returns>
        Dictionary<int, Notebook> GetAllNotebook();

        List<int> GetAllIds();
    }
}