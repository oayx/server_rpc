using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace rpc
{
    /// <summary>
    /// rcp
    /// @author hannibal
    /// </summary>
    public static class RPCInvoke
    {
        private static Dictionary<string, List<MessageAwaiter>> _messages = new Dictionary<string, List<MessageAwaiter>>();

        /// <summary>
        /// 增加await消息事件
        /// </summary>
        /// <param name="msg"></param>
        private static void Add(MessageAwaiter msg)
        {
            if (!_messages.TryGetValue(msg.Id, out var list))
            {
                list = new List<MessageAwaiter>();
                _messages.Add(msg.Id, list);
            }
            list.Add(msg);
        }
        /// <summary>
        /// 收到消息
        /// </summary>
        /// <param name="id">消息id</param>
        /// <param name="msg">内容</param>
        public static void RecvMessage(string id, string msg)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (_messages.TryGetValue(id, out var list))
            {
                foreach (var message in list)
                {
                    message.RecvMessage(msg);
                }
                list.Clear();
            }
        }

        public static MessageAwaiter GetAwaiter(this MessageAwaiter msg)
        {
            Add(msg);
            return msg;
        }
    }
    /// <summary>
    /// await消息
    /// </summary>
    public class MessageAwaiter : INotifyCompletion
    {
        /// <summary>
        /// 需要等待的消息id
        /// </summary>
        public string Id;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; private set; }
        
        public bool IsCompleted { get; private set; }

        private Action _continuation;

        public MessageAwaiter(string id)
        {
            Id = id;
        }

        public string GetResult()
        {
            return Message;
        }
        
        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        public void RecvMessage(string msg)
        {
            Message = msg;
            IsCompleted = true;
            _continuation?.Invoke();
        }
    }
}