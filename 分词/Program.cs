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
    using Lucene.Net.QueryParsers;

    class Program
    {
        static string indexPath = AppDomain.CurrentDomain.BaseDirectory + "/Lucene.Net";

        static void Main(string[] args)
        {
            BuildIndex();

            FSDirectory directiory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
            IndexReader reader = IndexReader.Open(directiory, true);
            IndexSearcher searcher = new IndexSearcher(reader);//搜索者
            PhraseQuery query = new PhraseQuery();//查询条件

            Analyzer analyzer = new PanGuAnalyzer();//盘古分词检索
            TokenStream tokenStream = analyzer.TokenStream("", new StringReader("开发的程序"));
            Lucene.Net.Analysis.Token token = null;
            while ((token = tokenStream.Next()) != null)
            {
                query.Add(new Term("content", token.Term()));
                Console.WriteLine("分词:" + token.Term());
            }

            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);//创建结果收集器（容器），最多放1000条数据
            searcher.Search(query, collector);
            Console.WriteLine(collector.GetTotalHits());//结果条数

            TopDocs docs = collector.TopDocs();

            foreach (var doc in docs.scoreDocs)
            {
                Document document = searcher.Doc(doc.doc);//根据docId拿到document
                Console.WriteLine("{0}:{1}", document.Get("number"), document.Get("content"));
            }

            Console.ReadKey();
        }

        private static void BuildIndex()
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

            Document document1 = new Document();
            document1.Add(new Field("number", "1", Field.Store.YES, Field.Index.ANALYZED));
            document1.Add(
                new Field(
                    "content",
                    "NLuke是参照Luke(lukeall)的功能开发的Lucene索引管理工具",
                    Field.Store.YES,
                    Field.Index.ANALYZED,
                    Field.TermVector.WITH_POSITIONS_OFFSETS));

            writer.AddDocument(document1); //Insert into Index(number,body) values(@number,@body)


            Document document2 = new Document();
            document2.Add(new Field("number", "2", Field.Store.YES, Field.Index.ANALYZED));
            document2.Add(
                new Field(
                    "content",
                    "可以根据分词器和选择的字段解析为表达式显示在2区,开发的程序",
                    Field.Store.YES,
                    Field.Index.ANALYZED,
                    Field.TermVector.WITH_POSITIONS_OFFSETS));

            writer.AddDocument(document2); //Insert into Index(number,body) values(@number,@body)
            writer.Close();
            directory.Close();
        }
    }
}
