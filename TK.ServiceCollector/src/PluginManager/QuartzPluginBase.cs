using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using System;

namespace TK.PluginManager
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class QuartzPluginBase : PluginBase
    {
        protected readonly IScheduler _Scheduler = new StdSchedulerFactory().GetScheduler();
        protected string _CronJobText = string.Empty;
        protected IJobDetail _JobDetail = null;

        /// <summary>
        /// Create this methods in the derived class
        /// </summary>
        /// <example>
        /// IJobDetail job = JobBuilder.Create<T>()
        ///                            .WithIdentity("job1", "group1")
        ///                            .Build();
        /// </example>
        protected abstract void InitializeJob();

        public override void Start()
        {
            if (_Scheduler.IsStarted)
            {
                return;
            }
            if (_JobDetail == null)
            {
                throw new Exception("Call InitializeJob first");
            }
            if (string.IsNullOrEmpty(_CronJobText))
            {
                throw new Exception("Set a valid CronJobText");
            }
            _Scheduler.Start();
        }

        public override void Pause()
        {
            _Scheduler.PauseAll();
        }

        public override void Resume()
        {
            _Scheduler.ResumeAll();
        }

        public override void Stop()
        {
            _Scheduler.Shutdown(true);
        }

        public void ForceStop()
        {
            _Scheduler.Shutdown(false);
        }
    }
}
