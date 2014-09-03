using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using log4net;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Store;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Analysis.PanGu;
using mshtml;
using Lucene.Net.Documents;
using System.Web.Hosting;

namespace WebApplication1
{
    public class IndexJob:IJob
    {
        private static ILog log = LogManager.GetLogger(typeof(IndexJob));

        //获得最大帖子编号。不要把所有的代码都写到一个方法中，抽象独立的方法，代码更清晰
        private static int GetMaxThreadId()
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string html = wc.DownloadString("http://localhost:8080/tools/rss.aspx");
            XDocument doc = XDocument.Parse(html);
            string link = doc.Descendants("item").First().Element("link").Value;
            Regex regex = new Regex(@"showtopic-(\d+)");
            Match match = regex.Match(link);
            string id = match.Groups[1].Value;
            return Convert.ToInt32(id);
        }

        #region IJob 成员

        public void Execute(JobExecutionContext context)
        {

           // string indexPath = HttpContext.Current.Server.MapPath("~/Index");
            string indexPath = HostingEnvironment.MapPath("~/Index");
            log.Debug("开始创建索引，索引目录："+indexPath);
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
            bool isUpdate = IndexReader.IndexExists(directory);
            log.Debug("索引目录存在状态："+isUpdate);
            if (isUpdate)
            {
                //如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁
                if (IndexWriter.IsLocked(directory))
                {
                    log.Debug("解锁索引库");
                    IndexWriter.Unlock(directory);
                }
            }
            log.Debug("开始爬文章");//能把零件拼起来！
            IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

            for (int i =900; i <= 1000; i++)
            //for (int i = 1; i <= GetMaxThreadId(); i++)
            {
                log.Debug("开始爬编号为"+i+"的帖子");
                try
                {
                    WebClient wc = new WebClient();
                    wc.Encoding = Encoding.UTF8;
                    string url = "http://localhost:8080/showtopic-" + i + ".aspx";
                    string html = wc.DownloadString(url);
                    HTMLDocumentClass htmlDoc = new HTMLDocumentClass();
                    htmlDoc.designMode = "on"; //不让解析引擎去尝试运行javascript
                    htmlDoc.IHTMLDocument2_write(html);
                    string title = htmlDoc.title;
                    string bodyText = htmlDoc.body.innerText;

                    writer.DeleteDocuments(new Term("url",url));//删除旧的数据，以url做“主键”。避免重复索引

                    Document document = new Document();
                    document.Add(new Field("url", url, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(new Field("title", title, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(new Field("body", bodyText, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));
                    writer.AddDocument(document);
                    log.Debug("爬编号为" + i + "的帖子结束");
                }
                catch (Exception ex)//也避免一个帖子爬取失败，后续不再执行，因为很难确保1W个帖子都很没有异常的download
                {
                    log.Error("爬编号为"+i+"的帖子发生异常",ex);
                }
            }
            log.Debug("结束索引，开始关闭Writer和Directory");
            writer.Close();
            directory.Close();
            log.Debug("关闭Writer和Directory完成");
        }

        #endregion
    }
}
