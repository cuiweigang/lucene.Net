using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Log4NetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            ILog log = LogManager.GetLogger(typeof(Program));
            log.Debug("我的第一条日志");
            log.Warn("程序连接时间过长");
            log.Warn("用户尝试进行XSS攻击");
            string s = Console.ReadLine();
            try
            {
                int i = Convert.ToInt32(s);
            }
            catch (Exception ex)
            {
                log.Error("数据转换出错，用户输入："+s);
                log.Error("数据转换出错，用户输入：" + s,ex);
            }
        }
    }
}
