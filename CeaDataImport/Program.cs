using CeaDataImport.ImportHelpers;
using ENVIS.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeaDataImport
{
    class Program
    {
        private struct SeriesName
        {
            public string TableCollumnName;
            public string OriginalName;
            public string TableName;

            public SeriesName(string name, string original, string tableName)
            {
                TableCollumnName = name;
                OriginalName = original;
                TableName = tableName;
            }

            public override string ToString()
            {
                return OriginalName;
            }
        }


        static void Main(string[] args)
        {
            string connectionString = "Server=localhost;Database=SpecianBP;Trusted_Connection=True;MultipleActiveResultSets=true";
            string path = "C:\\Users\\King\\Desktop\\EnvisBackUps\\2018-08.cea";
            MyDataReader ArchiveReader = new MyDataReader();
            ArchiveReader.LoadCea(path);

            List<UniArchiveBase> rows = new List<UniArchiveBase>(100000);

            UniArchiveBase rowActual = null;
            UniArchiveBase rowPreceding = null;
            ulong count = 0;

            do
            {
                if (count == 0)  //first
                {
                    rowActual = ArchiveReader.LoadNext();
                    rowPreceding = rowActual;
                }

                rowPreceding = rowActual;
                rowActual = ArchiveReader.LoadNext();

                if (rowActual != null)
                {
                    rows.Add(rowActual);
                    count++;
                    if (count % 1000 == 0)
                    {
                        Console.WriteLine(count);
                        Console.WriteLine(rowActual.TimeLocal);
                    }
                }

            } while (rowActual.TimeLocal != rowPreceding.TimeLocal);


            var times = rows.OrderBy(i => i.TimeLocal).Select(i => i.TimeLocal).ToList();

            Console.WriteLine(times.First());
            Console.WriteLine(times.Last());
            Console.WriteLine(count);

            string SeriesFile = "C:\\Users\\King\\source\\repos\\CeaDataImport\\CeaDataImport\\bin\\Debug\\Translations.veliciny.txt";
            List<string> seriesNames = System.IO.File.ReadAllLines(SeriesFile).ToList();
            List<SeriesName> pairs = new List<SeriesName>();

            foreach (var pair in seriesNames)
            {
                string[] splitted = pair.Split(';');
                pairs.Add(new SeriesName(splitted[1], splitted[0], splitted[2]));
            }


            List<string> insertRows = new List<string>();

            int iter = 0;
            //foreach each row
            foreach (var row in rows)
            {
                //foreach over collumns
                foreach (var table in pairs.GroupBy(i => i.TableName).ToList())
                {
                    InsertScript ic = new InsertScript("SpecianBP", table.Key);
                    ic.Add("Id", "'" + Guid.NewGuid().ToString() + "'");
                    ic.Add("CreateDate", "'" + DateTimeOffset.Now.ToString() + "'");
                    ic.Add("TimeLocal", "'" + row.TimeLocal.ToString() + "'");

                    if (table.Key != "Comm")
                    {


                        foreach (var pair in table)
                        {
                            //ommit harmonics
                            if (pair.OriginalName.Substring(0, 4) != "Harm")
                            {
                                var value = row.GetMemberValue(pair.OriginalName);

                                if(value.ToString() == "NaN")
                                {
                                    ic.Add(pair.TableCollumnName, "'" + 0 + "'");
                                }
                                else
                                {
                                    ic.Add(pair.TableCollumnName, "'" + value.ToString() + "'");
                                }
                            }
                        }

                        if (table.Key != "")
                        {
                            insertRows.Add(ic.Build());
                        }
                    }
                }

                Console.WriteLine(iter++ + " / " + rows.Count + "  |  " + (iter * 100F) / (float)rows.Count + " %");

                ////develop
                //if (iter % 1000 == 0)
                //{
                //    break;
                //}

            }


            //File.WriteAllLines("insert.sql", insertRows.ToArray());

            // values restricted to one day 1.4.2018
            //var hours = reader.values.Where(i => i.Key.Day == 1 && i.Key.Month == 4).Select(i => double.Parse(i.Key.Hour.ToString())).ToArray(); // 8644 values for each 10 seconds
            //var values = reader.values.Where(i => i.Key.Day == 1 && i.Key.Month == 4).Select(i => double.Parse(i.Value)).ToArray(); // 8644 values for each 10 seconds
            //var values2 = result.Select(i => double.Parse(i.GetMemberValue("I_avg_3I").ToString())).Take(8644).ToArray();
            //var values3 = result.Select(i => double.Parse(i.GetMemberValue("U_avg_U3").ToString())).Take(8644).ToArray();

            
            // Specify the parameter value.
            int paramValue = 5;

            // Create and open the connection in a using block. This
            // ensures that all resources will be closed and disposed
            // when the code exits.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.

                //command.Parameters.AddWithValue("@pricePoint", paramValue);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                try
                {
                    connection.Open();
                    //SqlDataReader reader = command.ExecuteReader();
                    //while (reader.Read())
                    //{
                    //    Console.WriteLine("\t{0}\t{1}\t{2}",
                    //        reader[0], reader[1], reader[2]);
                    //}
                    //reader.Close();

                    iter = 0;
                    int wrong = 0;
                    int success = 0;
                    foreach (var queryString in insertRows)
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        int res = command.ExecuteNonQuery();
                        if (res == 0)
                        {
                            wrong++;
                        }
                        else
                        {
                            success++;
                        }
                        Console.WriteLine(iter++ + " / " + rows.Count + "  |  " + (iter * 100F) / (float)insertRows.Count + " %" + "  |  success: " + success + "  |  wrong: " + wrong);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                //Console.ReadLine();

            }
        }
    }
}
