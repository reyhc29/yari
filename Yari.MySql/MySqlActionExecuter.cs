using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;

namespace Yari.MySql
{
    internal class MySqlActionExecuter : DBActionExecuter
    {
        internal MySqlActionExecuter(string connectionString) : base(connectionString)
        {

        }

        internal override JObject Execute(ActionDescriptor actionDescriptor)
        {
            JObject result = new JObject();

            string sqlStatment = createSqlStatement(actionDescriptor);

            IDbConnection storeConnection = new MySqlConnection(this.connectionString);

            using (MySqlCommand command = (MySqlCommand)storeConnection.CreateCommand())
            {
                command.CommandText = sqlStatment;
                command.CommandType = CommandType.Text;

                command.Connection.Open();
                try
                {
                    if (actionDescriptor.ResultType == ResultType.Empty)
                    {
                        command.ExecuteNonQuery();
                    }
                    else if (actionDescriptor.ResultType == ResultType.Scalar)
                    {
                        var scalarResult = command.ExecuteScalar();

                        result.AddProperty("result", scalarResult);
                    }
                    else if (actionDescriptor.ResultType == ResultType.Object)
                    {
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            RowData rd = getResults(reader).FirstOrDefault<RowData>();

                            if (rd != null)
                            {
                                for (int i = 0; i < rd.Columns.Count(); i++)
                                {
                                    result.AddProperty(rd.Columns[i], rd.Values[i]);
                                }
                            }
                        }
                    }
                    else //ResultType.Array, ResultType.MultipleArrays
                    {
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            int resultsCount = 1;
                            string resultName = null;
                            JArray dataArray = null;
                            foreach (RowData rd in getResults(reader))
                            {
                                if (rd.IsFirstRow)
                                {
                                    if (resultName != null)
                                        result.Add(resultName, (JToken)dataArray);

                                    //initialize new array with name
                                    dataArray = new JArray();
                                    if (actionDescriptor.ResultType == ResultType.MultipleArrays)
                                        if (actionDescriptor.ResultNames != null && actionDescriptor.ResultNames.Count >= resultsCount)
                                            resultName = actionDescriptor.ResultNames[resultsCount-1];
                                        else
                                            resultName = $"result{resultsCount}";
                                    else
                                        resultName = "result";

                                    resultsCount++;
                                }

                                JObject row = new JObject();
                                for (int i = 0; i < rd.Columns.Count(); i++)
                                {
                                    row.AddProperty(rd.Columns[i], rd.Values[i]);
                                }

                                dataArray.Add(row);
                            }

                            if (resultName != null)
                                result.Add(resultName, (JToken)dataArray);
                        }
                    }
                }
                finally
                {
                    if (command.Connection.State != ConnectionState.Closed)
                        command.Connection.Close();
                }
            }

            return result; 
        }

        private string createSqlStatement(ActionDescriptor actionDescriptor)
        {
            StringBuilder result = new StringBuilder();

            result
                .Append((actionDescriptor.ActionType == ActionType.StoredProc) ? "call " : "select ")
                .Append(actionDescriptor.ActionName)
                .Append("(");

            if (actionDescriptor.Params != null)
                result
                    .Append("'")
                    .Append(actionDescriptor.Params.ToString())
                    .Append("'");
            
            result.Append(");");

            return result.ToString();
        }

        private class RowData
        {
            public bool IsFirstRow { get; set; }

            public string[] Columns { get; set; }

            public object[] Values { get; set; }

            public RowData(bool isFirstRow, string[] columns, object[] values)
            {
                IsFirstRow = isFirstRow;
                Columns = columns;
                Values = values;
            }
        }

        private IEnumerable<RowData> getResults(IDataReader reader)
        {
            bool firstRow = true;

            do
            {
                firstRow = true;

                string[] columns = (from row in reader.GetSchemaTable().Rows.Cast<DataRow>()
                                          select row[0].ToString()).ToArray();

                while (reader.Read())
                {
                    object[] data = new object[reader.FieldCount];

                    reader.GetValues(data);

                    yield return new RowData(firstRow, columns, data);

                    firstRow = false;
                }

            } while (reader.NextResult());
        }
    }
}
