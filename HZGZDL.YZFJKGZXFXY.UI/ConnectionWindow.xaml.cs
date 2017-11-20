using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace HZGZDL.YZFJKGZXFXY.UI {
	/// <summary>
	/// ConnectionWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ConnectionWindow : Window {
		public ConnectionWindow() {
			InitializeComponent();
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			this.Hide();
			e.Cancel = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			string AddressIP = null;
			foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
				if (ipAddress.AddressFamily.ToString() == "InterNetwork") {
					AddressIP = ipAddress.ToString();
					if (AddressIP.Contains(".1.")) {
						txtLoalIPaddress.Text = AddressIP;
						break;
					}
				}
			}
		}
	}
}
