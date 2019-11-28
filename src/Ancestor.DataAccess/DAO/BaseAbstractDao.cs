using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ancestor.Core;
using Ancestor.DataAccess.Interface;
using Ancestor.DataAccess.DBAction;
using System.Data.Common;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// Creator : WizTonE 
    /// Date    : 2017/04/17
    /// Subject : BaseAbstractDao
    /// 
    /// History : 
    /// 2017/04/17 WizTonE 建立 : DbDao底層之Abstract Class, 繼承IDataAccessObject, 並提供virtual機制供override使用
    /// </summary>
    public abstract class BaseAbstractDao : DataAccessObject, IDataAccessObject
    {
        #region virtual funtion

        #region Function: Create
        protected virtual AncestorResult Insert(IModel objectModel)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult BulkInsert<T>(List<T> ObjList) where T : class, IModel, new()
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Insert(IModel objectModel, string name)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Function: Read
        #region :Sql string
        protected virtual AncestorResult Query(string sqlString, object paramsObjects)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region :IModel
        protected virtual AncestorResult Query<T>(IModel objectModel) where T : class, new()
        {
            throw new NotImplementedException();
        }


        protected virtual AncestorResult QueryNoRowid<T>(IModel objectModel) where T : class, new()
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Query(IModel objectModel)
        {
            throw new NotImplementedException();
        }

        protected virtual AncestorResult QueryNoRowid(IModel objectModel)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region :Expression
        protected virtual AncestorResult QueryWithJoinCondition(Expression predicate, Expression selectCondition, string[] names, Type[] fakeTypes = null)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult QueryWithJoinCondition(Expression predicate, Expression selectCondition, Type[] types, Type[] fakeTypes = null)
        {
            throw new NotImplementedException();
        }

        #region ::GenericType
        protected virtual AncestorResult Query<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            throw new NotImplementedException();
        }

        protected AncestorResult Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition) where T : class, new()
        {
            return QueryWithJoinCondition(predicate.Body, selectCondition.Body, new Type[] { typeof(T) });
        }

        protected AncestorResult Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition) where T1 : class, new() where T2 : class, new()
        {
            return QueryWithJoinCondition(predicate.Body, selectCondition.Body, new Type[] { typeof(T1), typeof(T2) });
        }

        protected AncestorResult Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new()
        {
            return QueryWithJoinCondition(predicate.Body, selectCondition.Body, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        protected AncestorResult Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new()
        {
            return QueryWithJoinCondition(predicate.Body, selectCondition.Body, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        }

        protected AncestorResult Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new()
        {
            return QueryWithJoinCondition(predicate.Body, selectCondition.Body, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        }

        protected AncestorResult Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition) where T1 : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new() where T5 : class, new() where T6 : class, new()
        {
            return QueryWithJoinCondition(predicate.Body, selectCondition.Body, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
        }
        protected virtual AncestorResult QueryNoRowid<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            throw new NotImplementedException();
        }

        #endregion
        #region ::FakeType
        protected virtual AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Type realType) where FakeType : class, new()
        {
            return Query(predicate, t => new[] { t.SelectAll() }, realType);
        }

        protected AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType) where FakeType : class, new()
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new Type[] { realType },
                new Type[] { typeof(FakeType) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2 = null)
            where FakeType1 : class, new()
            where FakeType2 : class, new()
        {
            return QueryWithJoinCondition(
                 predicate.Body,
                 selectCondition.Body,
                 new Type[] { realType1, realType2 ?? typeof(FakeType2) },
                 new Type[] { typeof(FakeType1), typeof(FakeType2) }
             );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2 = null, Type realType3 = null)
            where FakeType1 : class, new()
            where FakeType2 : class, new()
            where FakeType3 : class, new()
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new Type[] { realType1, realType2 ?? typeof(FakeType2), realType3 ?? typeof(FakeType3) },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2 = null, Type realType3 = null, Type realType4 = null)
            where FakeType1 : class, new()
            where FakeType2 : class, new()
            where FakeType3 : class, new()
            where FakeType4 : class, new()
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new Type[] { realType1, realType2 ?? typeof(FakeType2), realType3 ?? typeof(FakeType3), realType4 ?? typeof(FakeType4) },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3), typeof(FakeType4) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2 = null, Type realType3 = null, Type realType4 = null, Type realType5 = null)
            where FakeType1 : class, new()
            where FakeType2 : class, new()
            where FakeType3 : class, new()
            where FakeType4 : class, new()
            where FakeType5 : class, new()
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new Type[] { realType1, realType2 ?? typeof(FakeType2), realType3 ?? typeof(FakeType3), realType4 ?? typeof(FakeType4), realType5 ?? typeof(FakeType5) },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3), typeof(FakeType4), typeof(FakeType5) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2 = null, Type realType3 = null, Type realType4 = null, Type realType5 = null, Type realType6 = null)
            where FakeType1 : class, new()
            where FakeType2 : class, new()
            where FakeType3 : class, new()
            where FakeType4 : class, new()
            where FakeType5 : class, new()
            where FakeType6 : class, new()
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new Type[] { realType1, realType2 ?? typeof(FakeType2), realType3 ?? typeof(FakeType3), realType4 ?? typeof(FakeType4), realType5 ?? typeof(FakeType5), realType6 ?? typeof(FakeType6) },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3), typeof(FakeType4), typeof(FakeType5), typeof(FakeType6) }
            );
        }
        protected AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, string name)
        {
            return Query(predicate, t => new[] { t.SelectAll() }, name);
        }

        protected AncestorResult Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name)
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new string[] { name ?? typeof(FakeType).Name },
                new Type[] { typeof(FakeType) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2)
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new string[] { name1 ?? typeof(FakeType1).Name, name2 ?? typeof(FakeType2).Name },
                new Type[] { typeof(FakeType1), typeof(FakeType2) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3)
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new string[] { name1 ?? typeof(FakeType1).Name, name2 ?? typeof(FakeType2).Name, name3 ?? typeof(FakeType3).Name },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4)
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new string[] { name1 ?? typeof(FakeType1).Name, name2 ?? typeof(FakeType2).Name, name3 ?? typeof(FakeType3).Name, name4 ?? typeof(FakeType4).Name },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3), typeof(FakeType4) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5)
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new string[] { name1 ?? typeof(FakeType1).Name, name2 ?? typeof(FakeType2).Name, name3 ?? typeof(FakeType3).Name, name4 ?? typeof(FakeType4).Name, name5 ?? typeof(FakeType5).Name },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3), typeof(FakeType4), typeof(FakeType5) }
            );
        }

        protected AncestorResult Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6)
        {
            return QueryWithJoinCondition(
                predicate.Body,
                selectCondition.Body,
                new string[] { name1 ?? typeof(FakeType1).Name, name2 ?? typeof(FakeType2).Name, name3 ?? typeof(FakeType3).Name, name4 ?? typeof(FakeType4).Name, name5 ?? typeof(FakeType5).Name, name6 ?? typeof(FakeType6).Name },
                new Type[] { typeof(FakeType1), typeof(FakeType2), typeof(FakeType3), typeof(FakeType4), typeof(FakeType5), typeof(FakeType6) }
            );
        }

        #endregion
        #endregion

        #endregion

        #region Function: Update
        protected virtual AncestorResult Update(IModel valueObject, object parameterObject)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Update(IModel valueObject, IModel whereObject)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            throw new NotImplementedException();
        }
        protected virtual void UpdateTranslate(IModel valueObject, List<IDataParameter> parameters)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult UpdateAll(IModel valueObject, IModel whereObject)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Update(IModel valueObject, object parameterObject, string name)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Update(IModel valueObject, IModel whereObject, string name)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult UpdateAll(IModel valueObject, IModel whereObject, string name)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name) where T : class, new()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Function: Delete
        protected virtual AncestorResult Delete(IModel whereObject)
        {
            throw new NotImplementedException();
        }

        protected virtual AncestorResult Delete<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult Delete(IModel whereObject, string name)
        {
            throw new NotImplementedException();
        }

        protected virtual AncestorResult Delete<T>(Expression<Func<T, bool>> predicate, string name) where T : class, new()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Function: General
        protected virtual AncestorResult ExecuteNonQuery(string sqlString, object modelObject)
        {
            throw new NotImplementedException();
        }
        protected virtual AncestorResult ExecuteStoredProcedure(string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        {
            throw new NotImplementedException();
        }
        protected virtual IDbTransaction BeginTransaction()
        {
            return DB.BeginTransaction();
        }
        protected virtual IDbTransaction BeginTransaction(IsolationLevel isoLationLevel)
        {
            return DB.BeginTransaction(isoLationLevel);
        }
        protected virtual void Commit()
        {
            DB.DbCommit();
        }
        protected virtual void Rollback()
        {
            DB.DbRollBack();
        }
        #endregion 

        #endregion

        #region Interface Implement
        IDbConnection IDataAccessObject.DBConnection => DB.GetConnectionFactory();



        IDbTransaction IDataAccessObject.BeginTransaction()
        {
            return BeginTransaction();
        }

        IDbTransaction IDataAccessObject.BeginTransaction(IsolationLevel isoLationLevel)
        {
            return BeginTransaction(isoLationLevel);
        }


        AncestorResult IDataAccessObject.BulkInsert<T>(List<T> ObjList)
        {
            return BulkInsert<T>(ObjList);
        }



        void IDataAccessObject.Commit()
        {
            Commit();
        }

        AncestorResult IDataAccessObject.Delete(IModel whereObject)
        {
            return Delete(whereObject);
        }

        AncestorResult IDataAccessObject.Delete<T>(Expression<Func<T, bool>> predicate)
        {
            return Delete<T>(predicate);
        }

        void IDataAccessObject.Dispose()
        {
            Dispose();
        }

        void IDataAccessObject.Dispose(bool disposing)
        {
            Dispose(disposing);
        }

        AncestorResult IDataAccessObject.ExecuteNonQuery(string sqlString, object modelObject)
        {
            return ExecuteNonQuery(sqlString, modelObject);
        }

        AncestorResult IDataAccessObject.ExecuteStoredProcedure(string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        {
            return ExecuteStoredProcedure(procedureName, bindbyName, dBParameter);
        }

        IDbAction IDataAccessObject.GetActionFactory()
        {
            return base.DB = ActionFactory.GetDBAction(DbObject);
        }

        AncestorResult IDataAccessObject.Insert(IModel objectModel)
        {
            return Insert(objectModel);
        }


        AncestorResult IDataAccessObject.Query<T>(IModel objectModel)
        {
            return Query<T>(objectModel);
        }


        AncestorResult IDataAccessObject.Query<T>(Expression<Func<T, bool>> predicate)
        {
            return Query(predicate);
        }

        AncestorResult IDataAccessObject.Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition)
        {
            return Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition)
        {
            return Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition)
        {
            return Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition)
        {
            return Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition)
        {
            return Query(predicate, selectCondition);
        }

        AncestorResult IDataAccessObject.Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition)
        {
            return Query(predicate, selectCondition);
        }
        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Type realType)
        {
            return Query(predicate, realType);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, Type realType)
        {
            return Query(predicate, selectCondition, realType);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, Type realType1, Type realType2)
        {
            return Query(predicate, selectCondition, realType1, realType2);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, Type realType1, Type realType2, Type realType3)
        {
            return Query(predicate, selectCondition, realType1, realType2, realType3);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4)
        {
            return Query(predicate, selectCondition, realType1, realType2, realType3, realType4);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5)
        {
            return Query(predicate, selectCondition, realType1, realType2, realType3, realType4, realType5);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, Type realType1, Type realType2, Type realType3, Type realType4, Type realType5, Type realType6)
        {
            return Query(predicate, selectCondition, realType1, realType2, realType3, realType4, realType5, realType6);
        }
        AncestorResult IDataAccessObject.Query(IModel objectModel)
        {
            return Query(objectModel);
        }

        AncestorResult IDataAccessObject.Query(string sqlString, object paramsObjects)
        {
            return Query(sqlString, paramsObjects);
        }

        AncestorResult IDataAccessObject.QueryNoRowid<T>(IModel objectModel)
        {
            return QueryNoRowid<T>(objectModel);
        }

        AncestorResult IDataAccessObject.QueryNoRowid<T>(Expression<Func<T, bool>> predicate)
        {
            return QueryNoRowid(predicate);
        }

        AncestorResult IDataAccessObject.QueryNoRowid(IModel objectModel)
        {
            return QueryNoRowid(objectModel);
        }

        void IDataAccessObject.Rollback()
        {
            Rollback();
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, object paramsObjects)
        {
            return Update(valueObject, paramsObjects);
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, IModel whereObject)
        {
            return Update(valueObject, whereObject);
        }

        AncestorResult IDataAccessObject.Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            return Update<T>(valueObject, predicate);
        }

        AncestorResult IDataAccessObject.UpdateAll(IModel valueObject, IModel whereObject)
        {
            return UpdateAll(valueObject, whereObject);
        }

        AncestorResult IDataAccessObject.UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            return UpdateAll<T>(valueObject, predicate);
        }

        AncestorResult IDataAccessObject.Insert(IModel model, string name)
        {
            return Insert(model, name);
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, object paramsObjects, string name)
        {
            return Update(valueObject, paramsObjects, name);
        }

        AncestorResult IDataAccessObject.Update(IModel valueObject, IModel whereObject, string name)
        {
            return Update(valueObject, whereObject, name);
        }

        AncestorResult IDataAccessObject.Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name)
        {
            return Update(valueObject, predicate, name);
        }

        AncestorResult IDataAccessObject.UpdateAll(IModel valueObject, IModel whereObject, string name)
        {
            return UpdateAll(valueObject, whereObject, name);
        }

        AncestorResult IDataAccessObject.UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate, string name)
        {
            return UpdateAll(valueObject, predicate, name);
        }

        AncestorResult IDataAccessObject.Delete(IModel whereObject, string name)
        {
            return Delete(whereObject, name);
        }

        AncestorResult IDataAccessObject.Delete<T>(Expression<Func<T, bool>> predicate, string name)
        {
            return Delete(predicate, name);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, string name)
        {
            return Query(predicate, name);
        }

        AncestorResult IDataAccessObject.Query<FakeType>(Expression<Func<FakeType, bool>> predicate, Expression<Func<FakeType, object>> selectCondition, string name)
        {
            return Query(predicate, selectCondition, name);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2>(Expression<Func<FakeType1, FakeType2, bool>> predicate, Expression<Func<FakeType1, FakeType2, object>> selectCondition, string name1, string name2)
        {
            return Query(predicate, selectCondition, name1, name2);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3>(Expression<Func<FakeType1, FakeType2, FakeType3, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, object>> selectCondition, string name1, string name2, string name3)
        {
            return Query(predicate, selectCondition, name1, name2, name3);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, object>> selectCondition, string name1, string name2, string name3, string name4)
        {
            return Query(predicate, selectCondition, name1, name2, name3, name4);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, object>> selectCondition, string name1, string name2, string name3, string name4, string name5)
        {
            return Query(predicate, selectCondition, name1, name2, name3, name4, name5);
        }

        AncestorResult IDataAccessObject.Query<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6>(Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, bool>> predicate, Expression<Func<FakeType1, FakeType2, FakeType3, FakeType4, FakeType5, FakeType6, object>> selectCondition, string name1, string name2, string name3, string name4, string name5, string name6)
        {
            return Query(predicate, selectCondition, name1, name2, name3, name4, name5, name6);
        }




        #endregion

    }
}
