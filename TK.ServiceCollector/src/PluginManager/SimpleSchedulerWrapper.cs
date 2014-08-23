using System;
using System.Linq;
using System.Collections.Generic;

namespace TK.PluginManager
{
    using Quartz;
    using TK.Logging;
    
    public delegate void MethodToExecute(IDictionary<string, object> paramters);
    
    public class SimpleSchedulerWrapper
    {
        internal class QuartzJob : Quartz.IJob
        {
            private static ILogger _Logger = LoggerFactory.GetCurrentClassLogger();

            void IJob.Execute(IJobExecutionContext context)
            {
                _Logger.DebugFormat("entering... {0}", System.Reflection.MethodBase.GetCurrentMethod());
                // we are looking for the first parameter
                var keyValuePair = context.JobDetail.JobDataMap.FirstOrDefault();
                if (keyValuePair.Value != null)
                {
                    var jobParameters = keyValuePair.Value as Tuple<MethodToExecute, Dictionary<string, object>>;
                    _Logger.DebugFormat("invoke {0}", jobParameters.Item1.Method.Name);
                    jobParameters.Item1.Invoke(jobParameters.Item2);
                }
            }
        }

        [DisallowConcurrentExecution]
        internal class QuartzJobConcurrentExecution : Quartz.IJob
        {
            void IJob.Execute(IJobExecutionContext context)
            {
                _Logger.DebugFormat("entering... {0}", System.Reflection.MethodBase.GetCurrentMethod());
                // we are looking for the first parameter
                var keyValuePair = context.JobDetail.JobDataMap.FirstOrDefault();
                if (keyValuePair.Value != null)
                {
                    var jobParameters = keyValuePair.Value as Tuple<MethodToExecute, Dictionary<string, object>>;
                    _Logger.DebugFormat("invoke {0}", jobParameters.Item1.Method.Name);
                    jobParameters.Item1.Invoke(jobParameters.Item2);
                }
            }
        }

        private static ILogger _Logger = LoggerFactory.GetCurrentClassLogger();
        private readonly IScheduler _Scheduler = new Quartz.Impl.StdSchedulerFactory().GetScheduler();

        /// <summary>
        /// Add a job to the Scheduler.
        /// </summary>
        /// <param name="cronJobText">define the cron job: e.g. 0/5 * * * * ? for every 5 second </param>
        /// <param name="methodToExecute">define which method should be executed</param>
        /// <param name="parameters">the parameters that should be used in the methods (thread safe)</param>
        /// <param name="AllowConcurrentExecution">define if the scheduler allows a second call even if the first is not finished</param>
        public void AddJob(string cronJobText, MethodToExecute methodToExecute, Dictionary<string, object> parameters, bool AllowConcurrentExecution)
        {
            IJobDetail jobDetail = AllowConcurrentExecution ? 
                JobBuilder.Create<QuartzJob>().Build() :
                JobBuilder.Create<QuartzJobConcurrentExecution>().Build();

            jobDetail.JobDataMap.Add(
                "JobParameters",
                new Tuple<MethodToExecute, Dictionary<string, object>>(methodToExecute, parameters));
            var trigger = TriggerBuilder.Create()
                .WithCronSchedule(cronJobText) //"0 23 15,16,17 ? * *"
                .Build();
            _Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void Start()
        {
            _Scheduler.Start();
        }

        public void StartDelayed(TimeSpan delay)
        {
            _Scheduler.StartDelayed(delay);
        }

        public void Standby()
        {
            _Scheduler.Standby();
        }

        public void Resume()
        {
            _Scheduler.ResumeAll();
        }

        public void Shutdown(bool waitForJobsToComplete)
        {
            _Scheduler.Shutdown(waitForJobsToComplete);
        }
    }
}