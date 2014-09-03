using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using WebApplication1.DAL;
using System.Data.SqlClient;
using System.Data;

namespace WebApplication1
{
    /// <summary>
    /// $codebehindclassname$ 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class SearchSug : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string term = context.Request["term"];

            DataTable table = SQLHelper.ExecuteDataTable("select distinct Keyword from T_SearchLog where Keyword like @word",new SqlParameter("word","%"+term+"%"));
            List<string> list = new List<string>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string word = Convert.ToString(table.Rows[i][0]);
                list.Add(word);
            }

            //string[] strs = {"百度","谷歌","搜狗" };
            JavaScriptSerializer jss = new JavaScriptSerializer();
            context.Response.Write(jss.Serialize(list));//将字符串数组序列化为Json格式发给客户端
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
