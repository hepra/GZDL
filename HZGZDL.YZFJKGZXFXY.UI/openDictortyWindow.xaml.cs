using System;
using System.Collections.Generic;
using System.Linq;
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
	/// openDictortyWindow.xaml 的交互逻辑
	/// </summary>
	public partial class openDictortyWindow : Window {
		public openDictortyWindow() {
			InitializeComponent();
			btnOpen.Click += btnOpen_Click;
		}

		void btnOpen_Click(object sender, RoutedEventArgs e) {
		string path = 	Common.MyFileHelper.OpenFile_getPath(@"F:\dahe\GZDL411CSharp\HZGZDL.YZFJKGZXFXY.UI\GZDL\HZGZDL.YZFJKGZXFXY.UI\bin\Debug\TestData");
		var data = Common.MyFileHelper.OpenFile(path, 964, 0);
			int stringlength = 0;
			int flag =0;
		for (int i = 0; i < data.Length; i++) {
			if (data[i] == 0) {
				flag++;
				stringlength = i;
			}
			else {
				flag = 0;
			}
			if (flag == 6) {
				stringlength -= 4;
				break;
			}
		}
		txtInfo.Text = Encoding.Unicode.GetString(data, 0, stringlength);
		}

		private void Window_Drop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				// Note that you can have more than one file.
				//string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
				MessageBox.Show(fileName);
			}
		}

		private void Window_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Link;
			else e.Effects = DragDropEffects.None;     
		}
		ListBox myBox;
		int count = 0;
		private void myParentsCanvas_MouseEnter(object sender, MouseEventArgs e) {
			if (count == 0) {
				Canvas temp = new Canvas();
				temp.Height = 200;
				temp.Width = 180;
				temp.Background = Brushes.Pink;
				Canvas.SetTop(temp, 40);
				Canvas.SetLeft(temp, 10);
				count++;
				temp.MouseEnter += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
					double  currentTop = Canvas.GetTop(temp);
					if (currentTop <= 0) {
						while (currentTop <=0) {
							Canvas.SetTop(temp, currentTop + 10);
							currentTop += 10;
						}
					}
					
				};
				temp.MouseLeave += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
					double currentTop = Canvas.GetTop(temp);
					if (currentTop <= 10) {
						while (currentTop >= -temp.ActualHeight + 30) {
							Canvas.SetTop(temp, currentTop - 10);
							currentTop -= 10;
						}
					}
				};
				temp.MouseMove += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
					if (mouse.LeftButton == MouseButtonState.Pressed) {
						Canvas.SetTop(temp, mouse.GetPosition(this).Y - temp.ActualHeight / 2);
						Canvas.SetLeft(temp, mouse.GetPosition(this).X - temp.ActualWidth / 2);
					}
				};
				if (count == 1) {
					myBox = new ListBox();
					myBox.Width = temp.Width;
					myBox.Height = temp.Height;
					temp.Children.Add(myBox);
				}
				(sender as Canvas).Children.Add(temp);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			if (myBox != null) {
				myBox.Items.Add("来自我的科科");
			}
		}

	}
}
