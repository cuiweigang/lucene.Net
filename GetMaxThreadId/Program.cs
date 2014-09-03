using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace GetMaxThreadId
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string html = wc.DownloadString("http://localhost:8080/tools/rss.aspx");
            XDocument doc = XDocument.Parse(html);
            string link = doc.Descendants("item").First().Element("link").Value;


            Console.WriteLine(link);
            Regex regex = new Regex(@"showtopic-(\d+)");
            Match match =  regex.Match(link);
            string id = match.Groups[1].Value;
            Console.WriteLine(id);
            Console.ReadKey();
        }
    }
}
