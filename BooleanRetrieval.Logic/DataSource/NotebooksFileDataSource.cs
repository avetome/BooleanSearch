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
            LoadDataFromSource();
        }

        public Dictionary<int, Notebook> Notebooks => _notebooks;

        public List<int> GetAllIds()
        {
            return _notebooks.Keys.ToList();
        }

        public Dictionary<int, Notebook> GetAllNotebook()
        {
            return _notebooks;
        }

        private void LoadDataFromSource()
        {
            _notebooks = new Dictionary<int, Notebook>();

            int counter = 0;
            string line;
            string idStr = string.Empty;

            using (StreamReader file = new StreamReader(_filename))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var i = 0;
                    while (line[i] != ',' && i < line.Length)
                    {
                        i++;
                    }

                    idStr = line.Substring(0, i++);

                    // Ignore items without id. 
                    if (!int.TryParse(idStr, out var id))
                    {
                        continue;
                    }

                    var notebook = new Notebook() { };
                    var descriptionStart = i;

                    // We look on brand and model same way, just because we don't need any ranging yet
                    while (true)
                    {
                        if (line[i] == ',')
                        {
                            notebook.Brand = line.Substring(descriptionStart, i - descriptionStart);
                            descriptionStart = i + 1;
                        }

                        if (++i == line.Length)
                        {
                            notebook.Model = line.Substring(descriptionStart, i - descriptionStart);
                            _notebooks.Add(id, notebook);

                            break;
                        }
                    }

                    if (counter > 0 && counter % 5000 == 0)
                    {
                        // Console.WriteLine($"Indexing {counter} lines...");
                    }

                    counter++;
                }
            }
        }
    }
}
