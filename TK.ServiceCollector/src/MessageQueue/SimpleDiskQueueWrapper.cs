using DiskQueue;
using System;
using System.Messaging;

namespace TK.TimeSeriesQueue
{
    /// <summary>
    /// Simple Wrapper for DiskQueue
    /// https://github.com/i-e-b/DiskQueue
    /// </summary>
    /// <typeparam name="T">Type to Wrap</typeparam>
    public class SimpleDiskQueueWrapper<T> 
    {
        private string _QueuePath;

        /// <summary>
        /// Initialize the Message Queue
        /// with the give Name
        /// @".\Private$\Customers"
        /// </summary>
        /// <param name="messageQueuePath">e.g. .\Private$\Customers</param>
        public void Initialize(string messageQueuePath)
        {
            _QueuePath = messageQueuePath;
        }

        /// <summary>
        /// Send instance of type T to Queue
        /// </summary>
        /// <param name="objectInstance">Instance of type T to store on Queue</param>
        public void Send(T objectInstance)
        {
            string pushJson = Newtonsoft.Json.JsonConvert.SerializeObject(objectInstance);
            using (var queue = new PersistentQueue(_QueuePath))
            {
                using (var session = queue.OpenSession())
                {
                    session.Enqueue(System.Text.Encoding.GetEncoding(0).GetBytes(pushJson));
                    session.Flush();
                }
            }
        }

        /// <summary>
        /// Peek the oldest instance from the Queue, but don't remove the instance
        /// To read the instance and remove it use Receive()
        /// </summary>
        /// <returns>Instance of type T</returns>
        public T Peek()
        {
            using (var queue = new PersistentQueue(_QueuePath)) {
                using (var session = queue.OpenSession()) {
                    byte[] queueItem = session.Dequeue();
                    if (queueItem != null) {
                        string value = System.Text.Encoding.GetEncoding(0).GetString(queueItem);
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
                    }
                    // session.Flush();
                }
            }
            return default(T);
        }

        /// <summary>
        /// Receive the oldest instance from the Queue and remove it.
        /// </summary>
        /// <returns>Instance of type T</returns>
        public T Receive()
        {
            using (var queue = new PersistentQueue(_QueuePath)) {
                using (var session = queue.OpenSession()) {
                    byte[] queueItem = session.Dequeue();
                    if (queueItem != null) {
                        string value = System.Text.Encoding.GetEncoding(0).GetString(queueItem);
                        T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
                        session.Flush();
                        return result;
                    }
                }
            }
            return default(T);
        }
    }
}
