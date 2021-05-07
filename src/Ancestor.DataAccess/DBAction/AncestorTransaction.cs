using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public class AncestorTransaction : IDbTransaction
    {
        private readonly IDataAccessObject _dao;
        private readonly IDbTransaction _transcation;

        public AncestorTransaction(IDataAccessObject dao, IDbTransaction transcation)
        {
            _dao = dao;
            _transcation = transcation;
        }

        public IDbConnection Connection
        {
            get { return _transcation.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return _transcation.IsolationLevel; }
        }

        public void Commit()
        {
            _dao.Commit();
            //_transcation.Commit();
        }

        public void Dispose()
        {
            _dao.Rollback();
            //_transcation.Dispose();
        }

        public void Rollback()
        {
            _dao.Rollback();
            //_transcation.Rollback();
        }
    }
}
