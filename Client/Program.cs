using rpc;
using System;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Client
    {
        private static Socket _socket;
        private static byte[] _recvBuffer = new byte[4096];   //读缓存

        static void Main(string[] args)
        {
            //获得服务器
            string ip = System.Configuration.ConfigurationManager.AppSettings["server_ip"];
            ushort port = ushort.Parse(System.Configuration.ConfigurationManager.AppSettings["server_port"]);
            
            //设定服务器IP地址  
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(new DnsEndPoint(ip, port)); 
                Console.WriteLine("连接服务器成功!!!");

                BeginReceive();

                while (_socket != null)
                {
                    try
                    {
                        string msg = Console.ReadLine();
                        if (_socket != null && !string.IsNullOrEmpty(msg))
                        {
                            SendMessage(msg);
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("网络错误：" + e.ToString());
                        Close();
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("连接服务器失败:" + ex.ToString());
            }
            Console.ReadLine();
        }

        private static void Close()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            Console.WriteLine("客户端连接已经关闭");
        }
        private static void BeginReceive()
        {
            if (_socket == null) return;
            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), _recvBuffer);
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        private static void OnReceive(IAsyncResult ar)
        {
            if (_socket == null) return;
            try
            {
                ar.AsyncWaitHandle.Close();
                byte[] buf = (byte[])ar.AsyncState;
                int len = _socket.EndReceive(ar);
                if (len > 0)
                {
                    string msg = System.Text.Encoding.UTF8.GetString(buf, 0, len);
                    Console.WriteLine(string.Format("收到服务器数据，长度:{0},内容:{1}", len, msg));

                    RecvMessage(msg);
                    BeginReceive();
                }
                else
                {
                    Close();
                    return;
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10054) Console.WriteLine(e.ToString());
                Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Close();
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">用|分割两部分，第一部分是id，第二部分是内容；如12|test</param>
        private static async void SendMessage(string msg)
        {
            var arr = msg.Split('|');
            if (arr.Length != 2)
            {
                Console.WriteLine("输入消息格式错误，需要以|分割两部分，第一部分是id，第二部分是内容，如12|test");
                return;
            }

            byte[] by = System.Text.Encoding.UTF8.GetBytes(msg);
            int count = _socket.Send(by, 0, by.Length, SocketFlags.None);
            Console.WriteLine("发送字节数：" + count);

            //等待服务器响应，参数是等待的消息id，返回值是消息内容
            var rep = await new MessageAwaiter(arr[0]);
            Console.WriteLine("收到服务器返回内容：" + rep);
        }
        /// <summary>
        /// 收到服务区响应
        /// </summary>
        /// <param name="msg">用|分割两部分，第一部分是id，第二部分是内容</param>
        private static void RecvMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            var arr = msg.Split('|');
            if (arr.Length == 2)
            {
                RPCInvoke.RecvMessage(arr[0], arr[1]);
            }
        }
    }
}