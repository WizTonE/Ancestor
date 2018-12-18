using Ancestor.Core;
using Ancestor.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Connections
{
    /// <summary>
    /// Author  : poohlin 
    /// Date    : 2018/11/05 10:00
    /// Subject : OleAction
    /// 
    /// History : 
    /// 2018/11/05 poohlin 建立
    /// 注意 ConnectionFactory 要加入 Access
    /// _connections.Add(new ConnectionResource(DBObject.DataBase.Access, new Lazy<IConnection>(() => new OleDBConnection(dbOBject))));
    /// </summary>
    public class OleDBConnection : IConnection
    {
        OleDbConnection DBConnection { get; set; }
        string ConnectionString { get; set; }

        public OleDBConnection(DBObject dbObject)
        {
            DBConnection = new OleDbConnection();
            this.SetConnectionObject(dbObject);
        }

        /// <summary>
        /// 設定連線
        /// 1. DBObject.Mode.Direct為直接連線, 直接連線至ping的到的資料庫位置
        /// 2. 其餘一律參照tnsname.ora中之連線位置
        /// featrue 加入.accdb的使用 目前都為x86
        /// </summary>
        /// <param name="dbObject"></param>
        public void SetConnectionObject(DBObject dbObject)
        {
            dBObject = dbObject;

            string FileName = dBObject.Schema;//檔案位置 //連線資料庫Schema
            string ProviderName = "Microsoft.Jet.OLEDB.4.0;";//注意要用x86編譯!!!
            //string ProviderName = "Microsoft.ACE.Oledb.12.0;";//可以用x64 注意檔案需為x64
            //Access2003(含)以前版本使用(.mdb)：Provider=Microsoft.Jet.OLEDB.4.0
            //Access2007(含)以後版本使用(.accdb)：Provider=Microsoft.ACE.Oledb.12.0
            //https://www.connectionstrings.com/access/
            
            string UserId = dbObject.ID + ";";
            string Password = dbObject.Password + ";";
            string DataSource = FileName;
            if (!File.Exists(DataSource))
            {
                Debug.WriteLine("file is not 存在!!");
                return;
            }

            string ConnectionString =
                    "Provider=" + ProviderName +
                    "Data Source=" + DataSource + ";" +
                    "User Id=" + UserId +
                    "Password=" + Password;

            //Provider = Microsoft.ACE.OLEDB.12.0; Data Source = C:\myFolder\myAccessFile.accdb;
            //Persist Security Info = False;
            //Provider = Microsoft.ACE.OLEDB.12.0; Data Source = C:\myFolder\myAccessFile.mdb;
            //Persist Security Info = False;
            //Jet OLEDB:Database Password=MyDbPassword;
            //if (dbObject.Password == null)
            //{
            //    ConnectionString =
            //        "Provider=" + ProviderName +
            //        "Data Source=" + DataSource + ";" +
            //        "Persist Security Info = False;";
            //}
            DBConnection.ConnectionString = ConnectionString;
        }

        /// <summary>
        /// 傳回連線物件
        /// </summary>
        /// <returns></returns>
        public DbConnection GetConnectionObject()
        {
            return DBConnection;
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
            }
        }

        public DBObject dBObject { get; set; }
    }
}
