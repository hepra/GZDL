using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;


namespace HZGZDL.YZFJKGZXFXY.UI {
	/// <summary>
	/// ConnectionWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ConnectionWindow : Window {

		private DispatcherTimer timer_state = new DispatcherTimer();
		static string root_path = (System.AppDomain.CurrentDomain.BaseDirectory) + "Resources\\";
		public ConnectionWindow() {
			InitializeComponent();
			btnOneKeyChangeLocalIPadress.Click += btnOneKeyChangeLocalIPadress_Click;
			lab_ShowTime.Visibility = System.Windows.Visibility.Hidden;

			timer_state.Interval = TimeSpan.FromMilliseconds(1000);
			timer_state.Tag = 5;
			timer_state.Tick += (EventHandler)delegate {
				int flag = (int)timer_state.Tag;
				lab_ShowTime.Content = "请等待 [ " + flag + " ]";
				timer_state.Tag = flag - 1;
				if ((int)timer_state.Tag == -1) {
					timer_state.Stop();
					timer_state.Tag = 5;
					this.Dispatcher.Invoke(new Action(delegate {
						btnConfirmConnection.IsEnabled = false;
						txtLoalIPaddress.FontSize = 12;
						txtLoalIPaddress.Foreground = Brushes.Red;
						txtLoalIPaddress.Text = "请点击[一键配置本地IP]配置本地IP";
						string AddressIP = null;
						foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
							if (ipAddress.AddressFamily.ToString() == "InterNetwork") {
								AddressIP = ipAddress.ToString();
								if (AddressIP.Contains(".1.")) {
									txtLoalIPaddress.Text = AddressIP;
									txtLoalIPaddress.FontSize = 30;
									txtLoalIPaddress.Foreground = Brushes.Black;
									btnConfirmConnection.IsEnabled = true;
									break;
								}
							}
						}
						lab_ShowTime.Visibility = System.Windows.Visibility.Hidden;
						btnConfirmConnection.IsEnabled = true;
					}));
					
				}
			};
		}

		void btnOneKeyChangeLocalIPadress_Click(object sender, RoutedEventArgs e) {
			if (RunCmd2("1")) {
				btnOneKeyChangeLocalIPadress.IsEnabled = false;
				btnConfirmConnection.IsEnabled = false;
				lab_ShowTime.Visibility = System.Windows.Visibility.Visible;
				timer_state.Start();
			}
		}

		   
      public  bool RunCmd2(string cmdStr)
        {
            Process proc = null;
            try
            {
                proc = new Process();
				proc.StartInfo.WorkingDirectory = root_path;
                proc.StartInfo.FileName = "修改IP1网段.bat";
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                proc.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			this.Hide();
			e.Cancel = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			string AddressIP = null;
			btnConfirmConnection.IsEnabled = false;
			txtLoalIPaddress.FontSize = 12;
			txtLoalIPaddress.Foreground = Brushes.Red;
			txtLoalIPaddress.Text = "请点击[一键配置本地IP]配置本地IP";
			foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
				if (ipAddress.AddressFamily.ToString() == "InterNetwork") {
					AddressIP = ipAddress.ToString();
					if (AddressIP.Contains(".1.")) {
						txtLoalIPaddress.Text = AddressIP;
						txtLoalIPaddress.FontSize = 30;
						txtLoalIPaddress.Foreground = Brushes.Black;
						btnConfirmConnection.IsEnabled = true;
						break;
					}
				}
			}
		}

		private void Window_Activated(object sender, EventArgs e) {
			string AddressIP = null;
			txtLoalIPaddress.FontSize = 12;
			txtLoalIPaddress.Foreground = Brushes.Red;
			txtLoalIPaddress.Text = "请点击[一键配置本地IP]配置本地IP";
			btnConfirmConnection.IsEnabled = false;
			foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
				if (ipAddress.AddressFamily.ToString() == "InterNetwork") {
					AddressIP = ipAddress.ToString();
					if (AddressIP.Contains(".1.")) {
						txtLoalIPaddress.Text = AddressIP;
						txtLoalIPaddress.FontSize = 30;
						txtLoalIPaddress.Foreground = Brushes.Black;
						btnConfirmConnection.IsEnabled = true;
						break;
					}
				}
			}
		}
	}
}
