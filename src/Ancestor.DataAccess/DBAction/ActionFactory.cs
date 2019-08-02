using Ancestor.Core;
using Ancestor.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    /// <summary>
    /// Author  : Andycow0 
    /// Date    : 2015/07/31 14:00
    /// Subject : ActionFactory 資料庫 Action 工廠模式.
    ///           
    /// History : 
    /// 2015/07/31 Andycow0 建立
    /// </summary>
    public static class ActionFactory
    {
        public static IDbAction GetDBAction(DBObject dbObj)
        {
            DbActionResource resource = new DbActionResource(dbObj);

            // 依據 DBObject 的 DBType 取得對應的 DBAction 物件
            var DBResource = resource.Actions.Where(x => x.DataBase == dbObj.DataBaseType).FirstOrDefault();

            if (resource != null && DBResource.DbAction != null)
            {
                DBResource.DbAction.Value.GetConnectionFactory();
                return DBResource.DbAction.Value;
            }

            throw new NullReferenceException("找不到相對應的 DBAction");
        }
    }

    internal class DbActionResource
    {
        private List<DbActionResource> _actions;
        internal Lazy<IDbAction> DbAction { get; private set; }
        // 屬性注入 DBObject
        internal DBObject DbObject { get; set; }
        internal DBObject.DataBase DataBase { get; private set; }
        internal List<DbActionResource> Actions
        {
            get
            {
                if (_actions == null)
                {
                    // 注入 DBObject 並取得 DBAction 清單
                    GetActions(DbObject);
                }
                return _actions;
            }
        }

        internal DbActionResource(DBObject dbObject)
        {
            DbObject = dbObject;
        }
        // 組合 DBAction 的總清單

        private DbActionResource(DBObject.DataBase dataBase, Lazy<IDbAction> dbAction)
        {
            DataBase = dataBase;
            DbAction = dbAction;
        }
        private void GetActions(DBObject dbObject)
        {
            _actions = new List<DbActionResource>();
            _actions.Add(new DbActionResource(DBObject.DataBase.MSSQL, new Lazy<IDbAction>(() => new MSSqlAction(dbObject))));
            _actions.Add(new DbActionResource(DBObject.DataBase.MySQL, new Lazy<IDbAction>(() => new MySqlAction(dbObject))));
            _actions.Add((new DbActionResource(DBObject.DataBase.Oracle, new Lazy<IDbAction>(() => new OracleAction(dbObject)))));
            _actions.Add((new DbActionResource(DBObject.DataBase.Access, new Lazy<IDbAction>(() => new OleAction(dbObject)))));
            _actions.Add((new DbActionResource(DBObject.DataBase.ManagedOracle, new Lazy<IDbAction>(() => new ManagedOracleAction(dbObject)))));
        }
    }
}
