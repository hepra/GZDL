
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace HZGZDL.YZFJKGZXFXY.Common {
	public class UDPHelper {

		public class UDPConnectState {
			public Queue<string> Message = new Queue<string>();
			public bool Is_Health = true;
		}
		public class asycUDPSever {
			public Socket UDPSever { get; set; }
			public delegate void recieve_message(byte[] data);
			public recieve_message Message_receive { get; set; }
			public UDPConnectState State = new UDPConnectState();
			public bool IsDataReceive = true;
			byte[] buffer;
			public EndPoint remote;
			public int RecieveBufferSize { get; set; }
			public void 开启UDP服务(EndPoint local, EndPoint remote) {

				try {
					if (UDPSever == null) {
						UDPSever = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
						UDPSever.Bind(local);
					}
					this.remote = remote;
				}
				catch (Exception e) {
					//UDPSever.Close();
					//UDPSever = null;
					State.Message.Enqueue("UDP服务开启失败" + e.Message);
					State.Is_Health = false;
				}
			}
			public void Receive() {
				buffer = new byte[RecieveBufferSize];
				UDPSever.BeginReceive(buffer, 0, RecieveBufferSize, SocketFlags.None,  new AsyncCallback(ReceiveCallback), UDPSever);
			}
			void ReceiveCallback(IAsyncResult ar) {
				try {
					var socket = ar.AsyncState as Socket;
					int length = socket.EndReceive(ar);
					if (IsDataReceive) {
						Message_receive(buffer);
					}
					if (length > 0) {
						socket.BeginReceive(buffer, 0, RecieveBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), UDPSever);
					}
				}
				catch (Exception e) {
					//UDPSever.Close();
					//UDPSever = null;
					State.Message.Enqueue("接受数据失败:" + e.Message);
					State.Is_Health = false;
				}
			}
			public void Send(byte[] data) {
				try {
					UDPSever.BeginSendTo(data, 0, data.Length, SocketFlags.None, this.remote, new AsyncCallback(SendCallback), UDPSever);
				}
				catch (Exception e) {
					//UDPSever.Close();
					//	UDPSever = null;
					State.Message.Enqueue("UDP发送回调失败:" + e.Message);
					State.Is_Health = false;
				}
			}
			void SendCallback(IAsyncResult ar) {
				try {
					UDPSever.EndSend(ar);
				}
				catch (Exception e) {
					//	UDPSever.Close();
					//UDPSever = null;
					State.Message.Enqueue("UDP发送回调失败:" + e.Message);
					State.Is_Health = false;
				}
			}
		}
	}
}