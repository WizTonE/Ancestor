using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Author  : WizTone 
    /// Date    : 2016/06/14
    /// Subject : Ancestor Result Object
    /// </summary>
    public class AncestorResult : IAncestorResult
    {
        public bool IsSuccess { get; set; }
        public IList DataList { get; set; } 
        public DataTable ReturnDataTable { get; set; }
        public int EffectRows { get; set; }
        public string Message { get; set; }
        public List<T> ResultList<T>() where T : class, new()
        {
            var returnList = new List<T>();
            try
            {
                

                if (DataList == null)
                    returnList = ReturnDataTable.ToList<T>();
                else
                    returnList = DataList as List<T> ?? DataList.MapTo<T>();

                var HardWordList = new T().GetType().GetProperties().ToList().FindAll(x => x.GetCustomAttributes(typeof(HardWordAttribute), false).Count() > 0);
                if(HardWordList.Count() > 0)
                {
                    HardWordList.ForEach(hw => {
                        returnList.ForEach(item=>
                        {
                            item.GetType().GetProperties().ToList().ForEach(prop => {
                                if (prop.Name == hw.Name)
                                {
                                    int codepage = 950;
                                    var attr = (HardWordAttribute)(hw.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault());
                                    if (attr.CodePage != 0)
                                        codepage = attr.CodePage;
                                    if(!string.IsNullOrEmpty(attr.CodeName))
                                        codepage = Encoding.GetEncoding(attr.CodeName).CodePage;
                                    var value = prop.GetValue(item, null);
                                    if (value != null)
                                    {
                                        prop.SetValue(item,
                                        Encoding.GetEncoding(codepage).GetString(DataTools.StringToByteArray(value.ToString())),
                                        null);
                                    }
                                }
                            });
                        });
                    });
                }

                return returnList;
            }
            catch (System.Exception)
            {
                Message = "轉換型態失敗, 請確認型態是否正確";
                return null;
            }


        }
    }
}
