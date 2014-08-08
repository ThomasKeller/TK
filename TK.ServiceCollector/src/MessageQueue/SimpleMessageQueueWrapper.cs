using System;
using System.Messaging;
using Newtonsoft.Json;

namespace TK.SimpleMessageQueue
{
    public class SimpleMessageQueueWrapper<T>
    {
        private MessageQueue _Queue;

        public string MessageLabel
        {
            get;
            set;
        }

        public void Initialize(string messageQueuePath)
        {
            if (!MessageQueue.Exists(messageQueuePath))
            {
                this._Queue = MessageQueue.Create(messageQueuePath);
            }
            else
            {
                this._Queue = new MessageQueue(messageQueuePath);
            }
            this._Queue.Formatter = new XmlMessageFormatter(new Type[]
			{
				typeof(string)
			});
        }

        public void Send(T objectInstance)
        {
            string pushJson = JsonConvert.SerializeObject(objectInstance);
            this._Queue.Send(pushJson, string.Format("{0}: {1:yyyy-MM-dd HH:mm:ss}", this.MessageLabel, DateTime.Now));
        }

        public T Peek()
        {
            MessageEnumerator enumerator = this._Queue.GetMessageEnumerator2();
            if (enumerator.MoveNext())
            {
                Message message = enumerator.Current;
                if (message != null)
                {
                    string messageJson = message.Body.ToString();
                    return JsonConvert.DeserializeObject<T>(messageJson);
                }
            }
            return default(T);
        }

        public T Receive()
        {
            Message message = this._Queue.Receive();
            if (message.Body != null)
            {
                string messageJson = message.Body.ToString();
                return JsonConvert.DeserializeObject<T>(messageJson);
            }
            return default(T);
        }
    }
}