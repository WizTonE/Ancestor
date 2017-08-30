using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public static class DAOExtensions
    {
        public static T Plus<T>(this T t)
        {
            return t;
        }

        public static bool Between(this DateTime? dt, DateTime begin, DateTime end)
        {
            return true;
        }
        public static bool Between(this string dattm, string begin, string end)
        {
            return true;
        }
        public static object SelectAll<T>(this T t)
        {
            return t;
        }
        public static T NotNull<T>(this T property, string defaultValue)
        {
            return property;
        }
        public static T NotNull<T>(this T property)
        {
            return property;
        }
    }
}
