using System.Collections.Generic;

namespace TK.PluginManager
{
    using TK.SimpleMessageQueue;
    
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class SchedulePluginBase : PluginBase
    {
        protected readonly SimpleSchedulerWrapper _Scheduler = new SimpleSchedulerWrapper();
        private readonly SimpleMessageQueueWrapper<MeasureValueBox> _Queue = new SimpleMessageQueueWrapper<MeasureValueBox>();
        protected string _CronJobText = string.Empty;

        protected void InitQueue(string queuePath, string pluginName)
        {
            _Queue.Initialize(queuePath);
            _Queue.MessageLabel = pluginName;
        }

        protected void SendToQueue(MeasureValueBox measureValueBox)
        {
            _Queue.Send(measureValueBox);
        }

        protected MeasureValueBox ReceiveFromQueue()
        {
            return _Queue.Receive();
        }



        public override void Start()
        {
            _Scheduler.Start();
        }

        public override void Pause()
        {
            _Scheduler.Standby();
        }

        public override void Resume()
        {
            _Scheduler.Resume();
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