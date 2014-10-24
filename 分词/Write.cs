using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 分词
{
    using System.IO;
    using System.Net;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.PanGu;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Store;

    using mshtml;

    using PanGu;

    public static class Write
    {
        static string indexPath = AppDomain.CurrentDomain.BaseDirectory + "/Lucene.Net";

        public static void BeginWrite()
        {
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
            bool isUpdate = IndexReader.IndexExists(directory);
            if (isUpdate)
            {
                //如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁
                if (IndexWriter.IsLocked(directory))
                {
                    IndexWriter.Unlock(directory);
                }
            }

            IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);
            var path = AppDomain.CurrentDomain.BaseDirectory + "/redbag.txt";
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path, Encoding.Default);

                for (int i = 0; i < lines.Count(); i++)
                {
                    var text = lines[i];
                    Console.WriteLine(text);

                    try
                    {
                        Document document = new Document();
                        document.Add(new Field("number", i.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                        document.Add(
                            new Field(
                                "body",
                                text,
                                Field.Store.YES,
                                Field.Index.ANALYZED,
                                Field.TermVector.WITH_POSITIONS_OFFSETS));
                        writer.AddDocument(document); //Insert into Index(number,body) values(@number,@body)
                        Console.WriteLine("索引" + i + "完毕");
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
            writer.Close();
            directory.Close();
        }

        /*
            Analyzer analyzer = new PanGuAnalyzer();//new StandardAnalyzer();
            TokenStream tokenStream = analyzer.TokenStream("", new StringReader("目录下的文件“复制到输出目录”设定为“如果较新则复制。欢迎来到如鹏网学习技术，传智播客首席执行官王洪义，你好我好王敏，C#编程基础C++技术，挪鸡鸭手机，能偶及手机"));
            Lucene.Net.Analysis.Token token = null;
            while ((token = tokenStream.Next()) != null)
            {
                Console.WriteLine(token.TermText());
            }*/

        /*
        Segment segment = new Segment();
        var result = segment.DoSegment("目录下的文件“复制到输出目录”设定为“如果较新则复制。欢迎来到如鹏网学习技术，传智播客首席执行官王洪义，你好我好王敏，C#编程基础C++技术，挪鸡鸭手机，能偶及手机");
        foreach (WordInfo wordInfo in result)
        {
            Console.WriteLine(wordInfo.Word);
        }*/

        /*
        FSDirectory directory = FSDirectory.Open(new DirectoryInfo("c:/index"), new NativeFSLockFactory());//只是定义，没有真正的创建
        Console.WriteLine(IndexReader.IndexExists(directory));
        IndexWriter indexWriter = new IndexWriter(directory, new PanGuAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
        Document doc = new Document();
        doc.Add(new Field("Id","1111",Field.Store.YES,Field.Index.NOT_ANALYZED));
        doc.Add(new Field("URL", "http://www.baidu.com/n.htm", Field.Store.YES, Field.Index.ANALYZED));
        doc.Add(new Field("Title", "最新消息", Field.Store.YES, Field.Index.ANALYZED));
        doc.Add(new Field("Body", "最新消息，特价商品！", Field.Store.YES, Field.Index.ANALYZED));
        doc.Add(new Field("年龄", "888888！", Field.Store.YES, Field.Index.ANALYZED));
        indexWriter.AddDocument(doc);
        indexWriter.Close();
         */

        //创建索引
        /*
        string indexPath = "d:/index";
        FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
        bool isUpdate = IndexReader.IndexExists(directory);
        if (isUpdate)
        {
            //如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁
            if (IndexWriter.IsLocked(directory))
            {
                IndexWriter.Unlock(directory);
            }
        }
        IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);
        for (int i = 1000; i < 1100; i++)
        {
            string txt = string.Format("Index:{0}", i);
            Document document = new Document();
            document.Add(new Field("number", i.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("body", txt, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));
            writer.AddDocument(document);//Insert into Index(number,body) values(@number,@body)
            Console.WriteLine("索引" + i + "完毕");
        }
        writer.Close();
        directory.Close();

        */

        public static void Searcher()
        {
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
    }
}
