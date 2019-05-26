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
        private string _filename;

        public NotebooksFileDataSource(string filename)
        {
            _filename = filename;
            Load();
        }

        public List<int> GetAllIds()
        {
            return _notebooks.Keys.ToList();
        }

        public Dictionary<int, Notebook> GetAllNotebook()
        {
            return _notebooks;
        }

        private void Load()
        {
            _notebooks = new Dictionary<int, Notebook>();

            foreach(var line in File.ReadLines(_filename))
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
