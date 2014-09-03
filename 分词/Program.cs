using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using NSharp.SearchEngine.Lucene.Analysis.Cjk;
using Lucene.Net.Analysis.PanGu;
using PanGu;
using Lucene.Net.Store;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Net;
using mshtml;

namespace 分词
{
    class Program
    {
        static void Main(string[] args)
        {
            // IndexWrite.Write();

            //Console.ReadKey();

            Searcher();

            // HighLight();

            Console.ReadKey();
        }

        private static void HighLight()
        {
            //创建HTMLFormatter,参数为高亮单词的前后缀 
            PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter =
                   new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            //创建 Highlighter ，输入HTMLFormatter 和 盘古分词对象Semgent 
            PanGu.HighLight.Highlighter highlighter =
                            new PanGu.HighLight.Highlighter(simpleHTMLFormatter,
                            new Segment());
            //设置每个摘要段的字符数 
            highlighter.FragmentSize = 50;
            //获取最匹配的摘要段 
            String s = highlighter.GetBestFragment("大学生就业", "我是大学生，我要就业啦");
            Console.WriteLine(s);
        }

        private static void Searcher()
        {
            string indexPath = "d:/Lucene.Net";
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
            IndexReader indexReader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(indexReader);//搜索者
            PhraseQuery query = new PhraseQuery();//查询条件

            // 分词
            Analyzer analyzer = new PanGuAnalyzer();//盘古分词检索
            TokenStream tokenStream = analyzer.TokenStream("", new StringReader("快店送红包"));
            Token token = null;
            while ((token = tokenStream.Next()) != null)
            {
                query.Add(new Term("body", token.Term()));
                Console.WriteLine("分词" + token.Term());
            }

            query.SetSlop(100);//词之间距离不超过100个单词就可以匹配
            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);//创建结果收集器（容器），最多放1000条数据
            searcher.Search(query, null, collector);//开始使用query条件搜索，搜索结果放入collector中
            Console.WriteLine(collector.GetTotalHits());//结果条数

            TopDocs topDocs = collector.TopDocs();//得到结果

            foreach (ScoreDoc scoreDoc in topDocs.scoreDocs)
            {
                int docId = scoreDoc.doc;//拿到搜到的文档的id（Lucene内部分配的id），结果中不是保存着Document，为的是提高效率、降低资源占用
                Document document = searcher.Doc(docId);//根据docId拿到document
                string num = document.Get("number");
                string body = document.Get("body");
                Console.WriteLine("文件名：" + num + ".txt，内容：" + body);
                Console.WriteLine("------------------------------------------------");
            }
            searcher.Close();
            indexReader.Close();
            directory.Close();
        }
    }
}
