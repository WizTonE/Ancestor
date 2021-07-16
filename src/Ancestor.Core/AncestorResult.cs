using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System;

namespace Ancestor.Core
{
    /// <summary>
    /// Author  : WizTone 
    /// Date    : 2016/06/14
    /// Subject : Ancestor Result Object
    /// </summary>
    public class AncestorResult : IAncestorResult
    {
        private bool _hardword = true;
        public bool IsSuccess { get; set; }
        public IList DataList { get; set; }
        public DataTable ReturnDataTable { get; set; }
        public int EffectRows { get; set; }
        public string Message { get; set; }
        private Exception _exception;
        internal bool HardwordFlag
        {
            get { return _hardword; }
            set { _hardword = value; }
        }
        public List<T> ResultList<T>() where T : class, new()
        {
            var returnList = new List<T>();
            List<T> tempList = null;
            try
            {
                if (DataList == null)
                    tempList = ReturnDataTable.ToList<T>();
                else
                    tempList = DataList as List<T> ?? DataList.MapTo<T>();

                // 如果可能的話就將項目複製出來，避免影響到原始資料
                if (typeof(ICloneable).IsAssignableFrom(typeof(T)))
                    returnList.AddRange(tempList.Select(r => (T)(r as ICloneable).Clone()));
                else
                    returnList.AddRange(tempList);


                if (HardwordFlag)
                {
                    var HardWordList = new T().GetType().GetProperties().ToList().FindAll(x => x.GetCustomAttributes(typeof(HardWordAttribute), false).Count() > 0);
                    if (HardWordList.Count() > 0)
                    {
                        HardWordList.ForEach(hw =>
                        {
                            returnList.ForEach(item =>
                            {
                                item.GetType().GetProperties().ToList().ForEach(prop =>
                                {
                                    if (prop.Name == hw.Name)
                                    {
                                        var attr = hw.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;

                                        var encoding = attr != null ? attr.Encoding : Encoding.GetEncoding(950);
                                        var value = prop.GetValue(item, null);
                                        if (value != null)
                                        {
                                            prop.SetValue(item,
                                            encoding.GetString(DataTools.StringToByteArray(value.ToString())),
                                            null);
                                        }
                                    }
                                });
                            });
                        });

                    }
                }

                return returnList;
            }
            catch (System.Exception ex)
            {
                _exception = ex;
                Message = ex.Message;
                return null;
            }


        }
    }
}
