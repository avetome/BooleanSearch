using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanRetrieval.Logic.Models
{
    public struct Notebook
    {
        public Notebook(string brand, string model)
        {
            Brand = brand;
            Model = model;
        }

        public string Brand { get; set; }

        public string Model { get; set; }
    }
}
