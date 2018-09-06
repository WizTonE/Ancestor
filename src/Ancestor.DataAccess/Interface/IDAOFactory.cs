using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ancestor.DataAccess.DAO;

namespace Ancestor.DataAccess.Factory
{
    public interface IDAOFactory
    {
        IDataAccessObject GetDataAccessObjectFactory();
    }
}
