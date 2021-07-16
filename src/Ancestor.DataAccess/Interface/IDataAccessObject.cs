using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.ComponentModel;
using Ancestor.DataAccess.Interface;
using Ancestor.Core;

namespace Ancestor.DataAccess.DAO
{
    //  2016-04-05 Andycow0 Added functions SaveChange and CancellChange for transaction.
    //  2016-04-27 WizTonE  Added Bulk Insert
    //  2016-05-23 Andycow0 Commend SaveChange() and CancellChange().
    //  2016-09-09 Andycow0 Added return DBConnection.
    //  2016-09-14 Andycow0 Added DBConnection property by getter;
    //  2016-11-03 WizTonE  Added Query<T1, T2> join table function

    public interface IDataAccessObject : IDisposable
    {
        DBObject DbObject { get; }
        IDbAction GetActionFactory();

        bool IsTransacting { get; }

        #region Function: Query
        #region :Sql string
        AncestorResult Query(string sqlString, object paramsObjects);
        #endregion

        #region :IModel
        AncestorResult Query(IModel objectModel);
        AncestorResult Query<T>(IModel objectModel) where T : class, IModel, new();
        AncestorResult QueryNoRowid(IModel objectModel);
        AncestorResult QueryNoRowid<T>(IModel objectModel) where T : class, IModel, new();
        #endregion

        #region :Expression
        #region ::GenericType
        AncestorResult Query<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        AncestorResult Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new();
        AncestorResult Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition) where T1 : class, new() where T2 : class, new();
        AncestorResult Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new();
        AncestorResult Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new();
        AncestorResult Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new();
        AncestorResult Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new() where T6 : class, new();
        AncestorResult QueryNoRowid<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        #endregion
        #region ::FakeType
        AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Type realType) where FakeType : class, new();
        AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType) where FakeType : class, new();
        AncestorResult Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2 ) where FakeType1 : class, new() where FakeType2 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2 , Type realType3 ) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2 , Type realType3 , Type realType4 ) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2 , Type realType3 , Type realType4 , Type realType5 ) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new();

        AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, string name) where FakeType : class, new();
        AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name) where FakeType : class, new();
        AncestorResult Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2) where FakeType1 : class, new() where FakeType2 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new();
        AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6) where FakeType1 : class, new() where FakeType2 : class, new() where FakeType3 : class, new() where FakeType4 : class, new() where FakeType5 : class, new() where FakeType6 : class, new();
        #endregion
        #endregion
        #endregion



        AncestorResult Insert(IModel objectModel);
        AncestorResult Update(IModel valueObject, object paramsObjects);
        AncestorResult Update(IModel valueObject, IModel whereObject);
        AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new();
        AncestorResult UpdateAll(IModel valueObject, IModel whereObject);
        AncestorResult UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new();
        AncestorResult Delete(IModel whereObject);
        AncestorResult Delete<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        AncestorResult ExecuteNonQuery(string sqlString, object modelObject);
        AncestorResult ExecuteStoredProcedure(string procedureName, bool bindbyName, List<DBParameter> dBParameter);

        // 2019-11-27 Add by nagi, for more suitable at dirty env
        #region Advance Commands
        /// <summary>
        /// 新增資料
        /// </summary>
        /// <param name="model">資料模型</param>
        /// <param name="name">要塞入的資料表名稱</param>
        AncestorResult Insert(IModel model, string name);
        AncestorResult Update(IModel valueObject, object paramsObjects, string name);
        AncestorResult Update(IModel valueObject, IModel whereObject, string name);
        AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new();
        AncestorResult UpdateAll(IModel valueObject, IModel whereObject, string name);
        AncestorResult UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new();
        AncestorResult Delete(IModel whereObject, string name);
        AncestorResult Delete<T>(Expression<Func<T, bool>> predicate, string name) where T : class, new();
        #endregion 

        // 2016-04-05 Add for transaction.
        // 2016-05-23 Commend.
        //void SaveChange();
        //void CancellChange();
        //

        //2016-04-27 Add Bulk Insert
        AncestorResult BulkInsert<T>(List<T> ObjList) where T : class, IModel, new();
        IDbTransaction BeginTransaction();
        IDbTransaction BeginTransaction(IsolationLevel isoLationLevel);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDbConnection DBConnection { get; }

        void Commit();
        void Rollback();

    }
}
