using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace QuartzTest
{
    class MyJob:IJob
    {
        #region IJob 成员

        public void Execute(JobExecutionContext context)
        {
            Console.WriteLine(DateTime.Now);
        }

        #endregion
    }
}
