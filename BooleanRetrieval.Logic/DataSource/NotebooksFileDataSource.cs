using BooleanRetrieval.Logic.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BooleanRetrieval.Logic.DataSource
{
    public class NotebooksFileDataSource : INotebookDataSource
    {
        private Dictionary<int, Notebook> _notebooks;

        public NotebooksFileDataSource(string filename)
        {
            Load(filename);
        }

        public List<int> GetAllIds()
        {
            return _notebooks.Keys.ToList();
        }

        public Dictionary<int, Notebook> GetAllNotebook()
        {
            return _notebooks;
        }

        private void Load(string filename)
        {
            _notebooks = new Dictionary<int, Notebook>();

            foreach(var line in File.ReadLines(filename))
            {
                var strings = line.Split(',');
                if (strings.Length == 3 && int.TryParse(strings[0], out var id))
                {
                    _notebooks.Add(id, new Notebook(strings[1], strings[2]));
                }
            }
        }
    }
}
