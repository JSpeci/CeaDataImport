using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeaDataImport.ImportHelpers
{
    public interface ScriptBuilder
    {
        string Build();
    }

    public class InsertScript : ScriptBuilder
    {
        public List<string> Keys { get; set; }
        public List<string> Values { get; set; }

        private string _DbName;
        private string _TableName;

        public string DbName { get { return "[" + _DbName + "]"; } }
        public string TableName { get { return "[" + _TableName + "]"; } }

        public string BuildedLastly { get; internal set; }

        public InsertScript(string dbName, string tableName)
        {
            _DbName = dbName;
            _TableName = tableName;
            Keys = new List<string>();
            Values = new List<string>();
        }

        public void Add(string key, string value)
        {
            Keys.Add(key);
            Values.Add(value);
        }

        public string Build()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(DbName + ".[dbo]." + TableName);

            sb.Append(" ( ");

            for (int i = 0; i < Keys.Count - 1; i++)
            {
                sb.Append("[");
                sb.Append(Keys[i]);
                sb.Append("],");
            }

            sb.Append("[");
            sb.Append(Keys[Keys.Count - 1]);
            sb.Append("]");

            sb.Append(" ) VALUES ( ");
            for (int i = 0; i < Values.Count -1; i++)
            {
                sb.Append(Values[i] + ",");
            }

            sb.Append(Values[Values.Count -1]);
            sb.AppendLine(" );");

            //debug only
            //Console.WriteLine(sb.ToString());

            BuildedLastly = sb.ToString();
            return sb.ToString();
        }


    }
}
