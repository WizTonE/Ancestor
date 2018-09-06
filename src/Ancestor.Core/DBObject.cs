using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Author  : WizTonE 
    /// Date    : 2015/07/27
    /// Subject : DBObject 設定資料庫參數物件
    /// 
    /// History : 
    /// 2015/07/27 WizTonE建立
    /// 
    /// </summary>
    public class DBObject
    {
        //列舉資料庫型態
        public enum DataBase { 
            Oracle, 
            MSSQL, 
            SQLlite, 
            Access, 
            MySQL,
            Sybase
        };
        //列舉連線方式
        public enum Mode { Direct, DSN, TNSNAME };
        //連線資料庫IP
        public string IP { get; set; }
        //連線資料庫DNS Name
        public string Hostname { get; set; }
        //資料庫連線節點
        public string Node { get; set; }
        //連線資料庫Schema
        public string Schema { get; set; }
        //資料庫帳號
        public string ID { get; set; }
        //資料庫密碼
        public string Password { get; set; }
        //資料庫通訊埠
        public string Port { get; set; }
        //資料庫型態
        public DataBase DataBaseType { get; set; }
        //資料庫連線方式
        public Mode ConnectedMode { get; set; }
        //public int? IncreasePoolSize { get; set; }
        public IConnectionString ConnectionString { get; set; }
    }
}
