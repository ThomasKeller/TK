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
        protected readonly IScheduler m_Scheduler = new StdSchedulerFactory().GetScheduler();
        protected string m_CronJobText = string.Empty;
        protected IJobDetail m_JobDetail = null;

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
            if (m_Scheduler.IsStarted)
            {
                return;
            }
            if (m_JobDetail == null)
            {
                throw new Exception("Call InitializeJob first");
            }
            if (string.IsNullOrEmpty(m_CronJobText))
            {
                throw new Exception("Set a valid CronJobText");
            }
            m_Scheduler.Start();
        }

        public override void Pause()
        {
            m_Scheduler.PauseAll();
        }

        public override void Resume()
        {
            m_Scheduler.ResumeAll();
        }

        public override void Stop()
        {
            m_Scheduler.Shutdown(true);
        }
    }
}
