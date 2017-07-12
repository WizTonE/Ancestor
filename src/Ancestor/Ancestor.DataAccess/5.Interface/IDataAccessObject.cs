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

    public interface IDataAccessObject
    {
        IDbAction GetActionFactory();
        AncestorResult Query<T>(IModel objectModel) where T : class, IModel, new();
        AncestorResult Query<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        AncestorResult Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new();
        AncestorResult Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition) where T1 : class, new() where T2 : class, new();
        AncestorResult Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new();
        AncestorResult Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new();
        AncestorResult Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new();
        AncestorResult Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new() where T6 : class, new();
        AncestorResult Query(IModel objectModel);
        AncestorResult Query(string sqlString, object paramsObjects);
        AncestorResult QueryNoRowid<T>(IModel objectModel) where T : class, IModel, new();
        AncestorResult QueryNoRowid<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        AncestorResult QueryNoRowid(IModel objectModel);
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
        // 2016-04-05 Add for transaction.
        // 2016-05-23 Commend.
        //void SaveChange();
        //void CancellChange();
        //
        void Dispose();
        void Dispose(bool disposing);

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
