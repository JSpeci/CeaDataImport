using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateStrings
{
    class Program
    {
        static void Main(string[] args)
        {
            //var lines = System.IO.File.ReadAllLines("C:\\Users\\King\\source\\repos\\CeaDataImport\\CeaDataImport\\bin\\Debug\\veliciny.txt");

            //List<string> newLines = new List<string>();

            //foreach(var line in lines)
            //{
            //    string newline = "public double " + line + " { get; set; }";
            //    newLines.Add(newline);
            //}


            var lines = System.IO.File.ReadAllLines("C:\\Users\\King\\source\\repos\\CeaDataImport\\CeaDataImport\\bin\\Debug\\Translations.veliciny.txt");

            List<string> newLines = new List<string>();

            foreach (var line in lines)
            {
                var names = line.Split(';');
                string newline = "INSERT INTO [dbo].[Units] ([Id],[SeriesName],[UnitName]) VALUES('"+ Guid.NewGuid() + "', '" + names[0] + "' , '" + names[3] + "');";
                newLines.Add(newline);
            }

            System.IO.File.WriteAllLines("insertUnits.sql", newLines.ToArray());
            ;

        }
    }
}
