using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;

namespace QuartzTest
{
    class Program
    {
        static void Main(string[] args)
        {            
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            JobDetail job = new JobDetail("job1", "group1", typeof(MyJob));//IndexJob为实现了IJob接口的类
            DateTime ts = TriggerUtils.GetNextGivenSecondDate(null, 1);//1秒后开始第一次运行
            TimeSpan interval = TimeSpan.FromSeconds(5);//每隔5秒执行一次
            Trigger trigger = new SimpleTrigger("trigger1", "group1", "job1", "group1", ts, null,
                                                    SimpleTrigger.RepeatIndefinitely, interval);//每若干小时运行一次，小时间隔由appsettings中的IndexIntervalHour参数指定

            sched.AddJob(job, true);
            sched.ScheduleJob(trigger);
            sched.Start();//只有调用Start以后任务才会开始计划

            Console.ReadKey();
        }
    }
}
