using System;
using System.Configuration;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using System.Xml.XPath;
using System.Xml;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Text;
using System.Collections.Generic;

namespace BizTalkComponents.ExtensionObjects.DBLookupHelper
{
    [Serializable]
    public class DBLookupHelper
    {
        private const string asmName = "DBLookupHelper";
        private SqlConnection connection;
        private Dictionary<string, object> retreivedRecord = new Dictionary<string, object>();
        public bool IsConnectionValid { get; private set; }

        /// <summary>
        /// Sets the connection for the current instance, a connectionstring name in the configuration can be provided, or a connection string to the database, if the parameter is not provided or it is null, the default connection will be used instead
        /// </summary>
        public bool SetConnection(string connection)
        {
            string connStr = "";
            if (string.IsNullOrEmpty(connection))
                throw new ArgumentNullException("connection");
            if (ConfigurationManager.ConnectionStrings[connection] != null)
                connStr = ConfigurationManager.ConnectionStrings[connection].ConnectionString;
            else if (Environment.GetEnvironmentVariable(connection, EnvironmentVariableTarget.Machine) != null)
                connStr = Environment.GetEnvironmentVariable(connection, EnvironmentVariableTarget.Machine);
            else
                connStr = connection;
            SqlConnectionStringBuilder cnxnbuilder = new SqlConnectionStringBuilder();
            IsConnectionValid = false;
            try
            {
                cnxnbuilder.ConnectionString = connStr;
                if (string.IsNullOrEmpty(cnxnbuilder.ApplicationName) || cnxnbuilder.ApplicationName == ".Net SqlClient Data Provider")
                    cnxnbuilder.ApplicationName = asmName;
                SqlConnection cnxn = new SqlConnection(cnxnbuilder.ConnectionString);
                cnxn.Open();
                cnxn.Close();
                ConnectionString = cnxnbuilder.ConnectionString;
                IsConnectionValid = true;
            }
            catch (Exception ex)
            {
                LogEvent(ex.Message);
                throw ex;
            }
            return IsConnectionValid;
        }
        /// <summary>
        /// Sets the connection for the current instance, a connectionstring name in the configuration can be provided, or a connection string to the database, if the parameter is not provided or it is null, the default connection will be used instead
        /// </summary>
        public bool SetConnection()
        {
            return SetConnection("DBLookupHelper_DefaultConnection");
        }
        public string ConnectionString { get; private set; }

        private bool CheckConnection()
        {
            if (!IsConnectionValid)
                SetConnection();
            return IsConnectionValid;
        }

        public string GetValue(string tableName, string fieldName, string filter)
        {
            return GetValue(tableName, fieldName, filter, "");
        }
        public string GetValue(string tableName, string fieldName, string filter, string orderBy)
        {
            string val = "";
            if (!CheckConnection()) return string.Empty;
            var reader = GetData(tableName, filter, orderBy, 1);
            if (reader != null && reader.HasRows)
            {
                reader.Read();
                val = reader[fieldName].ToString();
            }
            reader?.Close();
            connection?.Close();
            return val;
        }


        public XmlElement GetRecord(string tableName, string filter, string orderBy)
        {
            return GetDataAsXml(tableName, filter, orderBy, 1);
        }
        public XmlElement GetRecord(string tableName, string filter)
        {
            return GetDataAsXml(tableName, filter, "", 1);
        }
        public XmlElement GetRecord(string tableName)
        {
            return GetDataAsXml(tableName, "", "", 1);
        }
        public bool RetreiveRecord(string tableName, string filter, string orderBy)
        {
            var reader = GetData(tableName, filter, orderBy, 1);
            bool retval = false;
            retreivedRecord.Clear();
            if (reader != null && reader.HasRows)
            {
                reader.Read();
                for (int i = 0; i < reader.FieldCount; i++)
                    retreivedRecord.Add(reader.GetName(i), reader[i]);
                retval = true;
            }
            reader?.Close();
            connection?.Close();
            return retval;
        }
        public string RetreiveField(string fieldName)
        {
            return retreivedRecord.ContainsKey(fieldName) ? retreivedRecord[fieldName].ToString() : null;
        }

        public XmlElement GetRecords(string tableName, string filter, string orderBy, int maxRecords)
        {
            return GetDataAsXml(tableName, filter, orderBy, maxRecords);
        }
        public XmlElement GetRecords(string tableName, string filter, string orderBy)
        {
            return GetDataAsXml(tableName, filter, orderBy, 0);
        }
        public XmlElement GetRecords(string tableName, string filter)
        {
            return GetDataAsXml(tableName, filter, "", 0);
        }
        public XmlElement GetRecords(string tableName)
        {
            return GetDataAsXml(tableName, "", "", 0);
        }
        private XmlElement GetDataAsXml(string tableName, string filter, string orderBy, int maxRecords)
        {
            XmlDocument doc = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);
            writer.WriteStartDocument(true);
            writer.WriteStartElement("LookupResult");
            var reader = GetData(tableName, filter, orderBy, maxRecords);
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    writer.WriteStartElement(tableName);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetValue(i) != DBNull.Value)
                        {
                            writer.WriteStartElement(reader.GetName(i));
                            writer.WriteValue(reader.GetValue(i));
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            reader?.Close();
            connection?.Close();
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(sb.ToString());
            var result = xmldoc.CreateNavigator().Select("/LookupResult");
            return xmldoc.DocumentElement;
        }

        private SqlDataReader GetData(string tableName, string filter, string orderBy, int maxRecords)
        {
            SqlDataReader reader = null;
            if (CheckConnection())
            {
                var sqlstr = BuildQuery(tableName, filter, orderBy, maxRecords);
                connection = new SqlConnection(ConnectionString);
                var cmd = new SqlCommand(sqlstr, connection);
                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    reader = cmd.ExecuteReader();
                }
                catch (Exception ex)
                {
                    if (connection?.State == ConnectionState.Open) connection.Close();
                    LogEvent(ex);
                }
                if (reader != null && !reader.HasRows)
                    LogEvent(string.Format("The criteria applied to table '{0}' returned no records!\n{1}", tableName, filter));
            }
            return reader;
        }
        private string BuildQuery(string tableName, string filter, string orderBy, int maxRecords)
        {
            filter = Regex.Replace(filter, @"\{([^\{\}]*)\}", "\'$1\'");
            string sqlstr = "select";
            if (maxRecords > 0)
                sqlstr += string.Format(" top {0}", maxRecords);
            sqlstr += string.Format(" * from {0}", tableName);
            if (!string.IsNullOrEmpty(filter))
                sqlstr += string.Format(" Where {0}", filter);
            if (!string.IsNullOrEmpty(orderBy))
                sqlstr += string.Format(" order by {0}", orderBy);
            return sqlstr;
        }

        /* Logging Errors
         * Log all errors in the windows event log.
         * and for some cases throw an exception.
         */
        private void LogEvent(Exception ex, bool throwException = false, [CallerMemberName]string memberName = "")
        {
            string errorMessage = "";
            if (ex.InnerException == null)
                errorMessage = string.Format("Procedure: {0}\nException: {1}\nMessage: {2}",
                    memberName, ex.GetType().FullName, ex.Message);
            else
                errorMessage = string.Format("Procedure: {0}\nException: {1}\nMessage: {2}\nInner Exception: {3}\nInner Exception Message: {4}",
                        memberName, ex.GetType().FullName, ex.Message,
                        ex.InnerException.GetType().FullName, ex.InnerException.Message);

            WriteLogEvent(errorMessage, true);
            if (throwException)
                throw new Exception(memberName, ex);
        }

        private void WriteLogEvent(string errorMessage, bool isException)
        {
            EventLog.WriteEntry(asmName, errorMessage, isException ? EventLogEntryType.Error : EventLogEntryType.Warning);
        }

        private void LogEvent(string errorMessage, [CallerMemberName]string memberName = "")
        {
            errorMessage = string.Format("Procedure: {0}\nError Message: {1}",
             memberName, errorMessage);
            WriteLogEvent(errorMessage, false);
        }


    }
}
