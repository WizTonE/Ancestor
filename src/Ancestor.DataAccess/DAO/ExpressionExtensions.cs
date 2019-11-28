using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public static class DAOExtensions
    {
        /// <remark>Oracle Only</remark>
        /// <summary>
        /// Left Join 或 Righ Join使用的(+)符號
        /// </summary>        
        /// <example>
        ///     // The result is X.PID = y.PID(+)
        ///     x.PID = y.PID.Plus()
        /// </example>
        public static T Plus<T>(this T t)
        {
            return t;
        }

        /// <summary>
        /// 區間(Between)語句
        /// </summary>        
        /// <param name="begin">起始時間</param>
        /// <param name="end">結束時間</param>
        /// <example>
        ///     var dateEnd = DateTime.Now;
        ///     var dateBgn = dateEnd.AddDays(-3);
        ///     
        ///     //The result is X.BIRTHDATE BETWEEN :dateBgn AND :dateEnd
        ///     x.BIRTHDATE.Between(dateBgn, dateEnd);
        /// </example>
        public static bool Between(this DateTime? dt, DateTime? begin, DateTime? end)
        {
            return dt >= begin && dt <= end;
        }
        /// <summary>
        /// 區間(Between)語句
        /// </summary>        
        /// <param name="begin">起始時間</param>
        /// <param name="end">結束時間</param>
        /// <example>
        ///     var dateEnd = DateTime.Now;
        ///     var dateBgn = dateEnd.AddDays(-3);
        ///     
        ///     //The result is X.BIRTHDATE BETWEEN :dateBgn AND :dateEnd
        ///     x.BIRTHDATE.Between(dateBgn, dateEnd);
        /// </example>
        public static bool Between(this DateTime dt, DateTime? begin, DateTime? end)
        {
            return dt >= begin && dt <= end;
        }
        /// <summary>
        /// 區間(Between)語句(字串)
        /// </summary>        
        /// <param name="begin">起始時間</param>
        /// <param name="end">結束時間</param>
        /// <example>
        ///     var dateEnd = "20170101";
        ///     var dateBgn = "20170201";
        ///     
        ///     //The result is X.BIRTHDATE BETWEEN '20170101' AND '20170201'
        ///     x.BIRTHDATE.Between("20170101", "20170201");
        /// </example>
        public static bool Between(this string dattm, string begin, string end)
        {
            return string.Compare(dattm, begin) >= 0 && string.Compare(dattm, end) <= 0;
        }

        /// <summary>
        /// 選擇全部欄位
        /// </summary>
        /// <example>
        ///     //Will Select X.F1, X.F2, .... , Y.F1
        ///     Query&#60X,Y&#62;((x, y)=>..., (x, y)=>new [] { x.SelectAll(), y.F1  }  ))
        /// </example>
        public static object SelectAll<T>(this T t)
        {
            return t;
        }
        /// <remark>Oracle Only</remark>
        /// <summary>
        /// 過濾Null
        /// </summary>        
        /// <param name="defaultValue">預設值</param>
        /// <example>
        ///     //In Oracle, the result is NVL(X.NAME, :defaultValue) = 'Alice'
        ///     x.NAME.NotNull() == "Alice"
        /// </example>
        public static T NotNull<T>(this T property, T defaultValue)
        {
            return Equals(property, default(T)) ? defaultValue : property;
        }
        /// <remark>Oracle Only</remark>
        /// <summary>
        /// 過濾Null
        /// </summary>        
        /// <param name="defaultValue">預設值</param>
        /// <example>
        ///     //In Oracle, the result is NVL(X.NAME, 'this field is null') = 'Alice'
        ///     x.NAME.NotNull() == "Alice"
        /// </example>
        public static T NotNull<T>(this T property)
        {
            return NotNull(property, default(T));
        }
        /// <summary>
        /// 取得日期部分
        /// </summary>
        public static DateTime? Truncate(this DateTime? value)
        {
            if (value != null)
            {
                var v = value.Value;
                value = new DateTime(v.Year, v.Month, v.Day);
            }
            return value;
        }
        /// <summary>
        /// 取得數值整數部分
        /// </summary>        
        public static decimal? Truncate(this decimal? value)
        {
            if (value != null)
            {
                value = Math.Truncate(value.Value);
            }
            return value;
        }
        /// <summary>
        /// 取得數值整數部分
        /// </summary>        
        public static double? Truncate(this double? value)
        {
            if (value != null)
            {
                value = Math.Truncate(value.Value);
            }
            return value;
        }        
    }
    public class Server
    {
        internal static readonly DateTime SYSDATE = new DateTime(99065829, DateTimeKind.Utc);
        public static DateTime SysDate
        {
            get { return SYSDATE; }
        }
    }
}
