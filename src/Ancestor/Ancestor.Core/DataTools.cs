using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ancestor.Core
{
    public static class DataTools
    {
        public static DataTable ToDataTable<TResult>(this IEnumerable<TResult> ListValue, bool useDisplayName = false) where TResult : class, new()
        {
            //建立一個回傳用的 DataTable
            DataTable dt = new DataTable();

            //取得映射型別
            Type type = typeof(TResult);

            //宣告一個 PropertyInfo 陣列，來接取 Type 所有的共用屬性
            PropertyInfo[] PI_List = null;
            System.ComponentModel.DisplayNameAttribute displayName = null;
            foreach (var item in ListValue)
            {                
                //判斷 DataTable 是否已經定義欄位名稱與型態
                if (dt.Columns.Count == 0)
                {
                    //取得 Type 所有的共用屬性
                    PI_List = item.GetType().GetProperties();
                    
                    //將 List 中的 名稱 與 型別，定義 DataTable 中的欄位 名稱 與 型別
                    foreach (var item1 in PI_List)
                    {
                        Type t = Nullable.GetUnderlyingType(item1.PropertyType)
                            ?? item1.PropertyType;
                        if(useDisplayName)
                            displayName = item1.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true).FirstOrDefault() as System.ComponentModel.DisplayNameAttribute;
                        dt.Columns.Add(displayName != null ? displayName.DisplayName : item1.Name, t);
                    }
                }

                //在 DataTable 中建立一個新的列
                DataRow dr = dt.NewRow();

                //將資料足筆新增到 DataTable 中
                foreach (var item2 in PI_List)
                {
                    if (useDisplayName)
                        displayName = item2.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true).FirstOrDefault() as System.ComponentModel.DisplayNameAttribute;
                    dr[displayName != null ? displayName.DisplayName : item2.Name] = item2.GetValue(item, null) == null ? DBNull.Value : item2.GetValue(item, null);
                }

                dt.Rows.Add(dr);
            }

            dt.AcceptChanges();

            return dt;
        }

        public static List<TResult> ToList<TResult>(this DataTable DataTableValue) where TResult : class, new()
        {
            //建立一個回傳用的 List<TResult>
            List<TResult> Result_List = new List<TResult>();

            //取得映射型別
            Type type = typeof(TResult);

            //儲存 DataTable 的欄位名稱
            List<PropertyInfo> pr_List = new List<PropertyInfo>();

            foreach (PropertyInfo item in type.GetProperties(BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.Public))
            {
                if (DataTableValue.Columns.IndexOf(item.Name) != -1)
                {
                    pr_List.Add(item);
                }
            }

            //足筆將 DataTable 的值新增到 List<TResult> 中
            foreach (DataRow item in DataTableValue.Rows)
            {
                TResult tr = new TResult();

                foreach (PropertyInfo item1 in pr_List)
                {
                    if (item[item1.Name] != DBNull.Value)
                    {
                        Type t = Nullable.GetUnderlyingType(item1.PropertyType)
                            ?? item1.PropertyType;
                        object safeValue = null;

                        //if (item1.PropertyType.IsArray)
                        //    safeValue = (item[item1.Name] == null) ? null : Convert.ChangeType(item[item1.Name].ToString().ToCharArray(), t);
                        //else
                            safeValue = (item[item1.Name] == null) ? null : Convert.ChangeType(item[item1.Name], t);

                        item1.SetValue(tr, safeValue, null);
                    }
                }

                Result_List.Add(tr);
            }

            return Result_List;
        }
        public static List<T> MapTo<T>(this IList source) where T : class, new()
        {
            var result = new List<T>();
            if (source.Count == 0)
                return result;


            var enumerableType = source[0].GetType();
            var properties = from sp in enumerableType.GetProperties()
                             from dp in typeof(T).GetProperties()
                             where sp.Name.Equals(dp.Name, StringComparison.OrdinalIgnoreCase) && dp.CanWrite && sp.CanRead && dp.PropertyType.IsAssignableFrom(sp.PropertyType)
                             select new { Source = sp, Destination = dp };
            T t;
            foreach (var src in source) {
                t = new T();
                properties.ToList().ForEach(p =>
                {
                    p.Destination.SetValue(t, p.Source.GetValue(src, null), null);
                });
                result.Add(t);
            }
            return result;
        }
        public static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                //若此屬性存在於DataRow中才將值取出
                //否則只是用來存取其他用途用的Property
                if (row.Table.Columns.Contains(property.Name))
                    property.SetValue(item, row[property.Name] != System.DBNull.Value ? row[property.Name] : "", null);
            }
            return item;
        }

        public static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties, Dictionary<string, string> mappings) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (mappings.ContainsKey(property.Name))
                    property.SetValue(item, row[mappings[property.Name]], null);
            }
            return item;
        }

        public static byte[] StringToByteArray(string hex)
        {
            if (!hex.All(c => char.IsLetterOrDigit(c)))
                return Encoding.Default.GetBytes(hex);
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
