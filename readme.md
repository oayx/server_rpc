## 主要功能  
用C#实现的rpc调用，使的通信发送和接收编码以类似同步的方式进行。相对于原本的rpc或grpc，使用上简化很多；  
而且不需要客户端和服务器使用同一套技术，可以完全独立运行  
## 核心介绍
主要用到了C#提供的await，实现了GetAwaiter接口(参考RPCInvoke类)；另外是在主线程跑，额外性能开销极少，不过每次调用会new一个MessageAwaiter
## 附例说明
Client：客户端，可以通过cmd窗口输入信息发送给服务器。测试使用的消息格式是(id|内容)，发送消息后，会用await等待服务器返回  
Server：服务器，目前收到消息后，会原翻不动的发给客户端  
## 待优化
后续可以考虑把MessageAwaiter做出对象池，这样就不需要new一个对象
