using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.Connections
{
    public static class LazyPassword
    {
        private static readonly IDictionary<string, string> SchemaPasswords
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> Cache
            = new Dictionary<string, string>();

        public static bool IsAvaliable
        {
            get
            {
                try
                {
                    System.Diagnostics.Trace.WriteLine("avaliable checking...");
                    var connectionStringsSection = ConfigurationManager.GetSection("connectionStrings") as ConnectionStringsSection;
                    // must have ConnectionStringsSection
                    if (connectionStringsSection != null)
                    {
                        System.Diagnostics.Trace.WriteLine("section avaliabled, connection string checking...");
                        // must have protected attribute
                        var @protected = connectionStringsSection.SectionInformation.IsProtected;
                        System.Diagnostics.Trace.WriteLine("section protected status: " + @protected);
                        return @protected;
                    }
                    else
                        System.Diagnostics.Trace.WriteLine("section unavaliabled");

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("avaliable checked fail:" + ex.Message);
                }
                return false;
            }
        }
        //public static string GetPassword(string user, string secret = null, string keyNode = null, Func<IDbConnection> connFactory = null)
        //{
        //    return GetPassword(user, secret, keyNode, null, null, connFactory);
        //}
        public static string GetPassword(string user, string secret = null, string keyNode = null, string dataSource = null, string connectionString = null, Func<IDbConnection> connFactory = null)
        {
            if (connFactory == null)
            {
                // auto detected connection type
                // if 64bit use managed else use legency oracle
                connFactory = new Func<IDbConnection>(() =>
                {
                    var connType = Environment.Is64BitProcess
                        ? Assembly.Load("Oracle.ManagedDataAccess").GetType("Oracle.ManagedDataAccess.Client.OracleConnection", true, true)
                        : Assembly.Load("Oracle.DataAccess").GetType("Oracle.DataAccess.Client.OracleConnection", true, true);
                    var conn = (IDbConnection)Activator.CreateInstance(connType);
                    return conn;
                });
            }
            using (var conn = connFactory())
            {
                return GetPassword(conn, user, secret, keyNode, dataSource, connectionString);
            }
        }
        public static string GetPassword(Core.DBObject dbObject)
        {

            Func<IDbConnection> connFactory = null;
            switch (dbObject.DataBaseType)
            {
                case Core.DBObject.DataBase.Oracle:
                    connFactory = () =>
                    {
                        var connType = Assembly.Load("Oracle.DataAccess").GetType("Oracle.DataAccess.Client.OracleConnection", true, true);
                        var c = (IDbConnection)Activator.CreateInstance(connType);
                        return c;
                    };
                    break;
                case Core.DBObject.DataBase.ManagedOracle:
                    connFactory = () =>
                    {
                        var connType = Assembly.Load("Oracle.ManagedDataAccess").GetType("Oracle.ManagedDataAccess.Client.OracleConnection", true, true);
                        var c = (IDbConnection)Activator.CreateInstance(connType);
                        return c;
                    };
                    break;
                default:
                    throw new NotSupportedException("notsupported database type:" + dbObject.DataBaseType);
            }
            using (var conn = connFactory())
            {
                return GetPassword(conn, dbObject);
            }

        }


        public static string GetPassword(IDbConnection conn, string user, string secret = null, string keyNode = null, string dataSource = null, string connectionString = null)
        {
            if (user == null)
                throw new NullReferenceException("user can not be null");
            var secretKey = secret ?? GetLazyPasswordSecretKey(user);
            return GetPasswordInternal(conn, user, secretKey, keyNode, dataSource, connectionString);
        }
        public static string GetPassword(IDbConnection conn, Core.DBObject dbObject)
        {
            return GetPassword(conn, dbObject.ID, dbObject.LazyPasswordSecretKey, dbObject.LazyPasswordSecretKeyNode, dbObject.LazyPasswordDataSource, dbObject.LazyPasswordConnectionString);
        }
        internal static string GetPasswordInternal(IDbConnection conn, string user, string secretKey, string keyNode = null, string dataSource = null, string connectionString = null)
        {
            if (conn == null)
                throw new NullReferenceException("conn can not be null");
            if (user == null)
                throw new NullReferenceException("user can not be null");
            if (secretKey == null)
                throw new NullReferenceException("secretKey can not be null");
            System.Diagnostics.Trace.WriteLine("schema=" + user);
            System.Diagnostics.Trace.WriteLine("secretKey=" + secretKey);


            string pwd;

            string connStr = null;
            if (connectionString != null)
            {
                System.Diagnostics.Trace.WriteLine("use DBObject connectionString");
                connStr = connectionString;
            }
            else if (Core.AncestorGlobalOptions.GlobalLazyPasswordConnectionString != null)
            {
                System.Diagnostics.Trace.WriteLine("use GlobalLazyPasswordConnectionString");
                connStr = Core.AncestorGlobalOptions.GlobalLazyPasswordConnectionString;
            }
            else
            {
                if (keyNode == null)
                    keyNode = GetLazyPasswordSecretKeyNode(user);
                System.Diagnostics.Trace.WriteLine("keyNode=" + keyNode);

                connStr = ConfigurationManager.ConnectionStrings[keyNode].ConnectionString;
                if (dataSource != null)
                {
                    System.Diagnostics.Trace.WriteLine("use DBObject datasource: " + dataSource);
                    connStr = ReplaceDataSource(conn, connStr, dataSource);
                }
                else if (Core.AncestorGlobalOptions.GlobalLazyPasswordDataSource != null)
                {
                    System.Diagnostics.Trace.WriteLine("use GlobalLazyPasswordDataSource: " + Core.AncestorGlobalOptions.GlobalLazyPasswordDataSource);
                    connStr = ReplaceDataSource(conn, connStr, Core.AncestorGlobalOptions.GlobalLazyPasswordDataSource);
                }
            }

            //TODO: Replace MaxPoolSize
            connStr = ReplaceConnectionProperty(conn, connStr);

            System.Diagnostics.Trace.WriteLine("connStr=" + connStr);
            conn.ConnectionString = connStr;

            var cacheKey = string.Format("{0}^{1}", user, connStr);
            if (!SchemaPasswords.TryGetValue(cacheKey, out pwd))
            {
                var opened = !conn.State.HasFlag(ConnectionState.Open);
                try
                {
                    if (opened)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "FGET_USER_PASSWORD";
                        cmd.CommandType = CommandType.StoredProcedure;

                        var p0 = cmd.CreateParameter();
                        p0.ParameterName = "RtnVal";
                        p0.DbType = DbType.String;
                        p0.Direction = ParameterDirection.ReturnValue;
                        p0.Size = 200;
                        cmd.Parameters.Add(p0);

                        var p1 = cmd.CreateParameter();
                        p1.ParameterName = "V_SCHEMAUSER";
                        p1.DbType = DbType.String;
                        p1.Value = user;
                        p1.Direction = ParameterDirection.Input;
                        p1.Size = 100;
                        cmd.Parameters.Add(p1);

                        var p2 = cmd.CreateParameter();
                        p2.ParameterName = "V_KEY";
                        p2.DbType = DbType.String;
                        p2.Value = secretKey;
                        p2.Direction = ParameterDirection.Input;
                        p2.Size = 200;
                        cmd.Parameters.Add(p2);

                        cmd.ExecuteNonQuery();

                        var value = p0.Value.ToString();
                        if (value != "null")
                        {
                            pwd = value;
                            System.Diagnostics.Trace.WriteLine("pwd=" + pwd);
                            SchemaPasswords.Add(cacheKey, pwd);
                        }
                        else
                        {
                            SchemaPasswords.Add(cacheKey, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
                finally
                {
                    if (conn.State.HasFlag(ConnectionState.Open) && opened)
                    {

                        System.Diagnostics.Trace.WriteLine("close conection");
                        conn.Close();


                        // clear pool
                        var mClearPool = conn.GetType().GetMethod("ClearPool", BindingFlags.Public | BindingFlags.Static);
                        if(mClearPool != null)
                        {
                            System.Diagnostics.Trace.WriteLine("clear pool");
                            mClearPool.Invoke(null, new object[] { conn });
                        }
                    }
                }
            }
            return pwd;
        }
        private static string ReplaceDataSource(IDbConnection conn, string connStr, string dataSource)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "connection cant be null");
            string connStrBuilderTypeName = null;
            if (conn.GetType().FullName == "Oracle.DataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.DataAccess.Client.OracleConnectionStringBuilder";
            else if (conn.GetType().FullName == "Oracle.ManagedDataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder";
            System.Diagnostics.Trace.WriteLine("DbConnectionStringBuilder type: " + connStrBuilderTypeName);
            var connStrBuilderType = conn.GetType().Assembly.GetType(connStrBuilderTypeName, true, true);
            if (connStrBuilderType == null)
                throw new NullReferenceException("can not found type: " + connStrBuilderTypeName);

            var connStrBuilder = (DbConnectionStringBuilder)Activator.CreateInstance(connStrBuilderType, connStr);
            PropertyInfo property = null;
            switch (connStrBuilderTypeName)
            {
                case "Oracle.DataAccess.Client.OracleConnectionStringBuilder":
                case "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder":
                    property = connStrBuilderType.GetProperty("DataSource");
                    if (property == null)
                        throw new InvalidOperationException("can not find DataSource property");
                    property.SetValue(connStrBuilder, dataSource, null);
                    break;
            }
            return connStrBuilder.ConnectionString;
        }
        private static string ReplaceConnectionProperty(IDbConnection conn, string connStr)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "connection cant be null");
            string connStrBuilderTypeName = null;
            if (conn.GetType().FullName == "Oracle.DataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.DataAccess.Client.OracleConnectionStringBuilder";
            else if (conn.GetType().FullName == "Oracle.ManagedDataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder";
            System.Diagnostics.Trace.WriteLine("DbConnectionStringBuilder type: " + connStrBuilderTypeName);
            var connStrBuilderType = conn.GetType().Assembly.GetType(connStrBuilderTypeName, true, true);
            if (connStrBuilderType == null)
                throw new NullReferenceException("can not found type: " + connStrBuilderTypeName);
            var connStrBuilder = (DbConnectionStringBuilder)Activator.CreateInstance(connStrBuilderType, connStr);
            //PropertyInfo propertyMinPool = null;
            PropertyInfo propertyPooling = null;

            switch (connStrBuilderTypeName)
            {
                case "Oracle.DataAccess.Client.OracleConnectionStringBuilder":
                case "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder":
                    //propertyMinPool = connStrBuilderType.GetProperty("MinPoolSize");
                    //if (propertyMinPool != null)
                    //    propertyMinPool.SetValue(connStrBuilder, 0, null);
                    propertyPooling = connStrBuilderType.GetProperty("Pooling");
                    if (propertyPooling != null)
                        propertyPooling.SetValue(connStrBuilder, false, null);
                    break;
            }
            return connStrBuilder.ConnectionString;
        }
        private static string GetLazyPasswordSecretKey(string user)
        {
            var secretKeyName = Core.AncestorGlobalOptions.LazyPasswordSecretKeyPrefix + user.ToUpper();
            var cacheKey = "K:" + secretKeyName;
            string secretKey;
            if (!Cache.TryGetValue(cacheKey, out secretKey))
            {
                secretKey = ConfigurationManager.AppSettings[secretKeyName];
                Cache.Add(cacheKey, secretKey);
            }
            return secretKey;
        }
        private static string GetLazyPasswordSecretKeyNode(string user)
        {
            var secretKeyNodeName = Core.AncestorGlobalOptions.LazyPasswordSecretKeyNodePrefix + user.ToUpper();
            var cacheKey = "KN:" + secretKeyNodeName;
            string secretKeyNode;
            if (!Cache.TryGetValue(cacheKey, out secretKeyNode))
            {
                secretKeyNode = ConfigurationManager.AppSettings[secretKeyNodeName] ?? Core.AncestorGlobalOptions.LazyPasswordSecretKeyNode;
                Cache.Add(cacheKey, secretKeyNode);
            }
            return secretKeyNode;
        }

    }
}
