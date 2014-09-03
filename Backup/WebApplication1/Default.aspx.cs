using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;
using log4net;
using PanGu;
using Lucene.Net.Store;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using WebApplication1.DAL.DataSetSearchLogTableAdapters;

namespace WebApplication1
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request["kw"]))
            {
                //Response.Write("新来的");
                
            }
            else
            {
                //Response.Write("回头客");
                string kw = Request["kw"];

                new T_SearchLogTableAdapter().Insert(Guid.NewGuid(), DateTime.Now, Request.UserHostAddress, kw);

                string indexPath = Server.MapPath("~/Index");
                FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
                IndexReader indexReader = IndexReader.Open(directory, true);
                IndexSearcher searcher = new IndexSearcher(indexReader);//搜索者
                PhraseQuery query = new PhraseQuery();//查询条件
                foreach (string word in segString(kw))
                {
                    query.Add(new Term("body", word));
                }
                query.SetSlop(1000);
                TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);//创建结果收集器（容器），最多放1000条数据
                searcher.Search(query, null, collector);//开始使用query条件搜索，搜索结果放入collector中
                Console.WriteLine(collector.GetTotalHits());//结果条数
                TopDocs topDocs = collector.TopDocs();//得到结果
                List<SearchResult> list = new List<SearchResult>();
                foreach (ScoreDoc scoreDoc in topDocs.scoreDocs)
                {
                    int docId = scoreDoc.doc;//拿到搜到的文档的id（Lucene内部分配的id），结果中不是保存着Document，为的是提高效率、降低资源占用
                    Document document = searcher.Doc(docId);//根据docId拿到document
                    string url = document.Get("url");
                    string title = document.Get("title");
                    string body = document.Get("body");

                    PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter =
           new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
                    //创建 Highlighter ，输入HTMLFormatter 和 盘古分词对象Semgent 
                    PanGu.HighLight.Highlighter highlighter =
                                    new PanGu.HighLight.Highlighter(simpleHTMLFormatter,
                                    new Segment());
                    //设置每个摘要段的字符数 
                    highlighter.FragmentSize = 200;
                    //获取最匹配的摘要段 
                    body = highlighter.GetBestFragment(kw, body);

                    SearchResult result = new SearchResult() { URL = url, Title = title, Body = body };
                    list.Add(result);
                }
                searcher.Close();
                indexReader.Close();
                directory.Close();
                RepeaterResult.DataSource = list;
                RepeaterResult.DataBind();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
       //     string indexPath = Server.MapPath("~/Index");
       //     FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
       //     IndexReader indexReader = IndexReader.Open(directory, true);
       //     IndexSearcher searcher = new IndexSearcher(indexReader);//搜索者
       //     PhraseQuery query = new PhraseQuery();//查询条件
       //     foreach (string word in segString(txtKW.Text))
       //     {
       //         query.Add(new Term("body", word));
       //     }
       //     query.SetSlop(1000);
       //     TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);//创建结果收集器（容器），最多放1000条数据
       //     searcher.Search(query, null, collector);//开始使用query条件搜索，搜索结果放入collector中
       //     Console.WriteLine(collector.GetTotalHits());//结果条数
       //     TopDocs topDocs = collector.TopDocs();//得到结果
       //     List<SearchResult> list = new List<SearchResult>();
       //     foreach (ScoreDoc scoreDoc in topDocs.scoreDocs)
       //     {
       //         int docId = scoreDoc.doc;//拿到搜到的文档的id（Lucene内部分配的id），结果中不是保存着Document，为的是提高效率、降低资源占用
       //         Document document = searcher.Doc(docId);//根据docId拿到document
       //         string url = document.Get("url");
       //         string title = document.Get("title");
       //         string body = document.Get("body");

       //         PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter =
       //new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
       //         //创建 Highlighter ，输入HTMLFormatter 和 盘古分词对象Semgent 
       //         PanGu.HighLight.Highlighter highlighter =
       //                         new PanGu.HighLight.Highlighter(simpleHTMLFormatter,
       //                         new Segment());
       //         //设置每个摘要段的字符数 
       //         highlighter.FragmentSize = 200;
       //         //获取最匹配的摘要段 
       //         body = highlighter.GetBestFragment(txtKW.Text, body);

       //         SearchResult result = new SearchResult() { URL=url,Title=title,Body=body};
       //         list.Add(result);
       //     }
       //     searcher.Close();
       //     indexReader.Close();
       //     directory.Close();
       //     RepeaterResult.DataSource = list;
       //     RepeaterResult.DataBind();
        }

        private static string[] segString(string s)
        {
            Segment segment = new Segment();
            return (from wordInfo in segment.DoSegment(s)
                    select wordInfo.Word).ToArray();
        }

    }

    public class SearchResult
    {
        public string URL { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
