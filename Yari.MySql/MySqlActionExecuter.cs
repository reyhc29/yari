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
    public class MySqlActionExecuter : DBActionExecuter
    {
        public MySqlActionExecuter(string connectionString) : base(connectionString)
        {

        }

        internal override JObject Execute(ActionDescriptor actionDescriptor)
        {
            try
            {
                JObject result = new JObject();

                MySqlConnection storeConnection = new MySqlConnection(this.connectionString);

                using (MySqlCommand command = storeConnection.CreateCommand())
                {
                    try
                    {
                        string commandText = generateCommandText(actionDescriptor);

                        if (onLog != null)
                            onLog(actionDescriptor.LogLevel, $"Executing query: {getTraceCommandText(commandText, actionDescriptor.Params)}", null);

                        command.CommandType = CommandType.Text;
                        command.CommandText = commandText;

                        if (actionDescriptor.Params != null && actionDescriptor.Params.Count() > 0)
                            setCommandParameters(command, actionDescriptor.Params);

                        command.Connection.Open();

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
                                    if (rd.Type == RowType.FirstRow || rd.Type == RowType.EmptyRow)
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

                                    if (rd.Type != RowType.EmptyRow)
                                    {
                                        JObject row = new JObject();
                                        for (int i = 0; i < rd.Columns.Count(); i++)
                                        {
                                            row.AddProperty(rd.Columns[i], rd.Values[i]);
                                        }

                                        dataArray.Add(row);
                                    }
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
            catch (Exception exp)
            {
                if (onLog != null)
                    onLog(LogLevel.Error, "Action Execution Failed!", exp);

                throw;
            }
        }

        private static void setCommandParameters(MySqlCommand command, IEnumerable<dynamic> parameters)
        {
            foreach (var param in parameters.Select((value, index) => new { Name = $"@p{index}", Value = value }))
            {
                //treat some especial cases firts
                if (param.Value is JObject)
                {
                    command.Parameters.Add(param.Name, MySqlDbType.JSON).Value = param.Value;
                }
                else //default assigment
                {
                    command.Parameters.AddWithValue(param.Name, param.Value);
                }
            }
        }

        private string generateCommandText(ActionDescriptor actionDescriptor)
        {
            string commandParameters = generateCommandParameters(actionDescriptor.Params);

            string executionStmtSection = (actionDescriptor.ActionType == ActionType.StoredProcedure) ? "call" : "select";

            string statement = $"{executionStmtSection} {actionDescriptor.ActionName}({commandParameters});";

            return statement;
        }

        private string getTraceCommandText(string statement, IEnumerable<dynamic> parameters)
        {
            string traceCommandText = statement;

            if (parameters != null && parameters.Count() > 0)
            {
                foreach (var param in parameters.Select((value, index) => new { value, index }))
                {
                    string isQuoted = (param.value is string) ? "'" : "";
                    string fixedParam = isQuoted + param.value.ToString() + isQuoted;

                    traceCommandText = traceCommandText.Replace($"@p{param.index}", fixedParam);
                }
            }

            return traceCommandText;
        }

        private string generateCommandParameters(IEnumerable<dynamic> parameters)
        {
            string commandParameters = String.Empty;

            if (parameters != null && parameters.Count() > 0)
                commandParameters = string.Join(',', parameters.Select((p, i) => $"@p{i}"));

            return commandParameters;
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

        private enum RowType { FirstRow, DataRow, EmptyRow }

        private class RowData
        {
            public RowType Type { get; set; }

            public string[] Columns { get; set; }

            public object[] Values { get; set; }

            public RowData(RowType type, string[] columns, object[] values)
            {
                Type = type;
                Columns = columns;
                Values = values;
            }
        }

        private IEnumerable<RowData> getResults(IDataReader reader)
        {
            bool readingFirstRow = true;
            bool moreRows = false;

            do
            {
                readingFirstRow = true;

                string[] columns = (from row in reader.GetSchemaTable().Rows.Cast<DataRow>()
                                          select row[0].ToString()).ToArray();

                while (true)
                {
                    moreRows = reader.Read();

                    if (readingFirstRow && !moreRows)
                    {
                        yield return new RowData(RowType.EmptyRow, columns, null);

                        break;
                    }

                    if (moreRows)
                    {
                        object[] data = new object[reader.FieldCount];

                        reader.GetValues(data);

                        yield return new RowData(readingFirstRow ? RowType.FirstRow : RowType.DataRow, columns, data);

                        readingFirstRow = false;
                    }
                    else //last row of a result with at least one row
                    {
                        break;
                    }
                }

            } while (reader.NextResult());
        }
    }
}
