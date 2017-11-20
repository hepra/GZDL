using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class TCPHelper {
		public class TCPConnectState {
			public Queue<string> Message = new Queue<string>();
			public bool Is_Health = true;
		}
		#region   异步TCP服务
		public class asyncTcpSever {
			
			public Socket socket_Sever { get; set; }
			public delegate void recieve_message(byte[] data);
			public recieve_message Message_receive { get; set; }
			public TCPConnectState tcpState = new TCPConnectState();
			public Socket client;
			public bool is_data_recvive_end = false;
			byte[] buffer;
			public int Recieve_Message_BufferSize { get; set; }
			public asyncTcpSever(string ip, int port) {
				try {
					//创建一个新的Socket,这里我们使用最常用的基于TCP的Stream Socket（流式套接字）
					socket_Sever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					//将该socket绑定到主机上面的某个端口
					socket_Sever.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
					//启动监听，并且设置一个最大的队列长度
					socket_Sever.Listen(4);
					//开始接受客户端连接请求
					socket_Sever.BeginAccept(new AsyncCallback(ClientAccepted), socket_Sever);
				}
				catch (Exception error) {
					tcpState.Message.Enqueue("服务器开启失败:" + error.Message);
					tcpState.Is_Health = false;
				}
			}

			public void Send(byte[] msg, EndPoint remotePoint) {
				try {
					socket_Sever.SendTo(msg, remotePoint);
				}
				catch (Exception e) {
					tcpState.Message.Enqueue("消息发送失败:" + e.Message);
					tcpState.Is_Health = false;
				}
			}

			private void ReceiveMessage(IAsyncResult ar) {
				var socket = ar.AsyncState as Socket;
				var length = socket.EndReceive(ar);
				if (!is_data_recvive_end) {
					Message_receive(buffer);
					//接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
					socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
				}
				else {
					client.Disconnect(false);
					socket.Close();
					tcpState.Message.Enqueue("传输已终止");
					tcpState.Is_Health = true;
				}
			}
			private void ClientAccepted(IAsyncResult ar) {
				var socket = ar.AsyncState as Socket;
				client = socket.EndAccept(ar);
				is_data_recvive_end = false;
				buffer = new byte[Recieve_Message_BufferSize];
				client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
				socket.BeginAccept(new AsyncCallback(ClientAccepted), socket_Sever);
			}

		}
		#endregion
		#region 异步TCP客户端
		public class asyncTcpClient {
			public Socket client{ get; set; }
			public delegate void recieve_message(byte[] data);
			public recieve_message Message_receive { get; set; }
			public TCPConnectState tcpState = new TCPConnectState();
			public bool IsDataReceive = true;
			byte[] buffer;
			public int RecieveBufferSize { get; set; }
			public void 连接服务器(EndPoint remote) {
				try {
					if (client == null) {
						client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					}
					client.ReceiveTimeout = 10;
					client.BeginConnect(remote, new AsyncCallback(ConnectCallback), this.client);
				}
				catch (Exception e) {
					client.Close();
					client = null;
					tcpState.Message.Enqueue("连接失败"+remote+"请重新连接! 错误消息:"+e.Message);
					tcpState.Is_Health = false;
				}
			}
			public asyncTcpClient(recieve_message receive_delegate, int buffer_size) {
				this.Message_receive = receive_delegate;
				this.RecieveBufferSize = buffer_size;
				client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}
			public void 连接服务器(string 服务器IP, int 服务器端口) {
				try {
					if (client == null) {
						client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					}
					client.ReceiveTimeout = 10;
					IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(服务器IP), 服务器端口);
					client.BeginConnect(remoteIP, new AsyncCallback(ConnectCallback), this.client);
				}
				catch(Exception e) {
					client.Close();
					client = null;
					tcpState.Message.Enqueue("连接失败" + 服务器IP + ":" + 服务器端口 + "请重新连接! 错误消息:" + e.Message);
					tcpState.Is_Health = false;
				}
				
			}
			void ConnectCallback(IAsyncResult ar) {
				try {
					var socket = ar.AsyncState as Socket;
					socket.EndConnect(ar);
					tcpState.Message.Enqueue("连接"+socket.RemoteEndPoint+"成功");
					tcpState.Is_Health = true;
					buffer = new byte[RecieveBufferSize];
					IsDataReceive = true;
					socket.BeginReceive(buffer, 0, RecieveBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), this.client);
				}
				catch (Exception e) {
					client.Close();
					client = null;
					tcpState.Message.Enqueue("接受数据失败,请重新连接:" + e.Message);
					tcpState.Is_Health = false;
				}
			}
			void ReceiveCallback(IAsyncResult ar) {
				try {
					var socket = ar.AsyncState as Socket;
					int length = socket.EndReceive(ar);
					if (IsDataReceive) {
						Message_receive(buffer);
					}
					socket.BeginReceive(buffer, 0, RecieveBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), this.client);
				}
				catch (Exception e) {
					client.Close();
					client = null;
					tcpState.Message.Enqueue("与下位机连接中断,请重新连接:" + e.Message);
					tcpState.Is_Health = false;
				}
			}
			public void Send(string data) {
				byte[] temp = Encoding.UTF8.GetBytes(data);
				if (client == null) {
					tcpState.Message.Enqueue("连接中断,命令无法送达,请重新连接");
					tcpState.Is_Health = false;
					return;
				}
				if (client.Connected) {
					client.BeginSend(temp, 0, temp.Length, SocketFlags.None, new AsyncCallback(SendCallback), this.client);
				}
				else {
					tcpState.Message.Enqueue("正在连接中.....");
				}
			}
			public void Send(byte[] data) {
				if (client == null) {
					tcpState.Message.Enqueue("连接中断,命令无法送达,请重新连接");
					tcpState.Is_Health = false;
					return;
				}
				if (client.Connected) {
					try {
						client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), this.client);
					}
					catch {
						tcpState.Message.Enqueue("连接已关闭,请重新连接");
						tcpState.Is_Health = false;
						return;
					}
				}
				else {
					tcpState.Message.Enqueue("正在连接中.....");
					tcpState.Is_Health = true;
				}
			}
			void SendCallback(IAsyncResult ar) {
				var socket = ar.AsyncState as Socket;
				int length = socket.EndSend(ar);
				tcpState.Message.Enqueue("发送成功数据长度:" + length);
				tcpState.Is_Health = true;
			}
		}
		#endregion
	}
}
