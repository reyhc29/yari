using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections;
using System;
using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.ComponentModel;

[assembly: InternalsVisibleTo("Yari.Test")]
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

            MySqlConnection storeConnection = new MySqlConnection(this.connectionString);

            using (MySqlCommand command = new MySqlCommand(actionDescriptor.ActionName, storeConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Connection.Open();
                try
                {
                    if (actionDescriptor.Params != null)
                    {
                        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(actionDescriptor.Params))
                        {
                            MySqlParameter parameter = new MySqlParameter(propertyDescriptor.Name, propertyDescriptor.GetValue(actionDescriptor.Params));

                            if (actionDescriptor.Params is JObject)
                                parameter.MySqlDbType = MySqlDbType.JSON;

                            command.Parameters.Add(parameter);
                        }
                    }

                    if (logger != null)
                        logger.LogDebug("Executing query: ", getGeneratedSql(command));

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
                                        if (actionDescriptor.ResultNames != null && actionDescriptor.ResultNames.Count() >= resultsCount)
                                            resultName = actionDescriptor.ResultNames.ElementAt(resultsCount - 1);
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
                catch (Exception exp)
                {
                    if (logger != null)
                        logger.LogError(exp, "Action Execution Failed!");

                    throw;
                }
                finally
                {
                    if (command.Connection.State != ConnectionState.Closed)
                        command.Connection.Close();
                }
            }

            return result; 
        }

        private string getGeneratedSql(MySqlCommand cmd)
        {
            string result;

            List<string> parameters = new List<string>();
            foreach (MySqlParameter p in cmd.Parameters)
            {
                string isQuted = (p.Value is string) ? "'" : "";
                parameters.Add(isQuted + p.Value.ToString() + isQuted);
            }

            if (parameters.Count == 0)
                result = $"{cmd.CommandText.ToString()}()";
            else
                result = $"{cmd.CommandText.ToString()}({string.Join(',', parameters)})";

            return result;
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
