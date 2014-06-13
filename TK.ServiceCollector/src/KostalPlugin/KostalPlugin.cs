﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Quartz;
using Quartz.Impl;

namespace TK.KostalPlugin
{
    public class KostalPlugin : PluginManager.QuartzPluginBase
    {
        private const string c_PluginName = "KostalPlugin";

        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public override Dictionary<string, object> GetParameters()
        {
            var defaultParameter = new Dictionary<string, object>();
            defaultParameter.Add("URL", Url);
            defaultParameter.Add("User", User);
            defaultParameter.Add("Password", Password);
            defaultParameter.Add("CronJobText", m_CronJobText);
            return defaultParameter;
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            Url = initParams.ContainsKey("URL") ? initParams["URL"] : "http://192.168.111.34";
            User = initParams.ContainsKey("User") ? initParams["User"] : "pvserver";
            Password = initParams.ContainsKey("Password") ? initParams["Password"] : "geheim";
            m_CronJobText = initParams.ContainsKey("CronJob") ? initParams["CronJob"] : "0/1 * * * * ?";
            InitializeJob();
        }

        public override string PluginName()
        {
            return c_PluginName;
        }

        public override string PluginVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        protected override void InitializeJob()
        {
            m_JobDetail = JobBuilder.Create<KostalJob>()
                                    //.WithIdentity("KostalReadJob", "ReadGroup")
                                    .Build();
            
            var trigger = TriggerBuilder.Create()
                            //.WithIdentity("KostalTrigger", "ReaderGroup")
                            .WithCronSchedule(m_CronJobText) //"0 23 15,16,17 ? * *")
                            //.ForJob(m_JobDetail)
                            //.ForJob("KostalReadJob", "ReadGroup")
                            .Build();

            m_Scheduler.ScheduleJob(m_JobDetail, trigger);
//            m_Scheduler.ScheduleJob(trigger);
        }
    }
}