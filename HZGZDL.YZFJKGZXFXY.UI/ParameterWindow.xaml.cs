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
	/// ParameterWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ParameterWindow : Window {
		public Model.ParameterInfo ParameterInfo {get;set;}
		BLL.TestSettingService service = new BLL.TestSettingService();
		private Model.ParameterInfo _defaultPara = new Model.ParameterInfo();
		public ParameterWindow( Model.ParameterInfo para) {
			InitializeComponent();
			btnCanel.Click += btnCanel_Click;
			btnDefault.Click += btnDefault_Click;
			btnConfirmSet.Click += btnConfirmSet_Click;
			this.Activated += ParameterWindow_Activated;
			ParameterInfo = para;
			_defaultPara.ID = ParameterInfo.ID;
			initDefaultPara(_defaultPara);
			//电流输出电压
			addComboxItem(cbxCurrentOutVoltForChannel_1, 1);
			addComboxItem(cbxCurrentOutVoltForChannel_1, -1);
			addComboxItem(cbxCurrentOutVoltForChannel_2, 1);
			addComboxItem(cbxCurrentOutVoltForChannel_2, -1);
			addComboxItem(cbxCurrentOutVoltForChannel_3, 1);
			addComboxItem(cbxCurrentOutVoltForChannel_3, -1);
			//电流量程
			addComboxItem(cbxCurrentRangeForChannel_1, 10);
			addComboxItem(cbxCurrentRangeForChannel_1, 100);
			addComboxItem(cbxCurrentRangeForChannel_2, 10);
			addComboxItem(cbxCurrentRangeForChannel_2, 100);
			addComboxItem(cbxCurrentRangeForChannel_3, 10);
			addComboxItem(cbxCurrentRangeForChannel_3, 100);
			//振动输出电压
			addComboxItem(cbxWaveOutVoltForChannel_1, 10);
			addComboxItem(cbxWaveOutVoltForChannel_1, -10);
			addComboxItem(cbxWaveOutVoltForChannel_2, 10);
			addComboxItem(cbxWaveOutVoltForChannel_2, -10);
			addComboxItem(cbxWaveOutVoltForChannel_3, 10);
			addComboxItem(cbxWaveOutVoltForChannel_3, -10);
			//振动量程
			addComboxItem(cbxWaveRangeForChannel_1, 50);
			addComboxItem(cbxWaveRangeForChannel_2, 50);
			addComboxItem(cbxWaveRangeForChannel_3, 50);
		}

		void ParameterWindow_Activated(object sender, EventArgs e) {
			setUIElement(service.LoadEntities(p => true).OrderByDescending(p=>p.Date).FirstOrDefault());
		}
		private void addComboxItem(ComboBox cbx, object value) {
			cbx.Items.Add(value);
		}
		private double stringToDouble(ComboBox cbx) {

			double flag = 0;
			if (double.TryParse(cbx.SelectedItem.ToString(), out flag)) {
				return flag;
			}
			else {
				return 0;
			}
		}

		void addItemToCmb(ComboBox p, string value) {
			if (p.Items.Count <= 0) {
				return;
			}
			foreach (var item in p.Items) {
				if (item.ToString() == value) {
					p.Text = value;
					return;
				}
			}
			p.Items.Add(value);
			p.SelectedIndex = 0;
			p.Text = value;
		}
		private void setUIElement(Model.ParameterInfo para) {
			if (para == null) {
				return;
			}
			//电流量程
			addItemToCmb(cbxCurrentRangeForChannel_1, para.currentChannel_1_Range + "");
			addItemToCmb(cbxCurrentRangeForChannel_2, para.currentChannel_2_Range + "");
			addItemToCmb(cbxCurrentRangeForChannel_3, para.currentChannel_3_Range + "");
			//电流输出电压
			addItemToCmb(cbxCurrentOutVoltForChannel_1, para.currentChannel_1_OutVolt + "");
			addItemToCmb(cbxCurrentOutVoltForChannel_2, para.currentChannel_2_OutVolt + "");
			addItemToCmb(cbxCurrentOutVoltForChannel_3, para.currentChannel_3_OutVolt + "");
			//振动量程
			addItemToCmb(cbxWaveRangeForChannel_1, para.shakeChannel_1_Range + "");
			addItemToCmb(cbxWaveRangeForChannel_2, para.shakeChannel_2_Range + "");
			addItemToCmb(cbxWaveRangeForChannel_3, para.shakeChannel_3_Range + "");

			//振动输出电压
			addItemToCmb(cbxWaveOutVoltForChannel_1, para.shakeChannel_1_OutVolt + "");
			addItemToCmb(cbxWaveOutVoltForChannel_2, para.shakeChannel_2_OutVolt + "");
			addItemToCmb(cbxWaveOutVoltForChannel_3, para.shakeChannel_3_OutVolt + "");
			//阀值
			txtCurrentFlagValue.Text = para.CurrentFlag+"";
			txtCurrentFrequency.Text = para.CurrentFrequency + "";
		}
		private void initDefaultPara(Model.ParameterInfo _defaultPara) {
			_defaultPara.currentChannel_1_Range =10;
			_defaultPara.currentChannel_1_OutVolt =1;
			_defaultPara.currentChannel_2_Range = 10;
			_defaultPara.currentChannel_2_OutVolt = 1;
			_defaultPara.currentChannel_3_Range = 10;
			_defaultPara.currentChannel_3_OutVolt =1;

			_defaultPara.shakeChannel_1_Range =50;
			_defaultPara.shakeChannel_1_OutVolt = 10;
			_defaultPara.shakeChannel_2_Range = 50;
			_defaultPara.shakeChannel_2_OutVolt = 10;
			_defaultPara.shakeChannel_3_Range =50;
			_defaultPara.shakeChannel_3_OutVolt =10;

			_defaultPara.currentChannel_1_CheckPara = 1.0;
			_defaultPara.currentChannel_2_CheckPara = 1.0;
			_defaultPara.currentChannel_3_CheckPara = 1.0;
			_defaultPara.shakeChannel_1_CheckPara = 1.0;
			_defaultPara.shakeChannel_2_CheckPara = 1.0;
			_defaultPara.shakeChannel_3_CheckPara = 1.0;

			_defaultPara.CurrentFrequency = 50;
			_defaultPara.CurrentFlag = 0.8;
			_defaultPara.Date = DateTime.Now;
			ParameterInfo = _defaultPara;
		}

		void btnConfirmSet_Click(object sender, RoutedEventArgs e) {
			double flag = 0;
			if (double.TryParse(txtCurrentFlagValue.Text,out flag)) {
				ParameterInfo.CurrentFlag = flag;
			}
			else {
				MessageBox.Show("请输入正确阀值");
				txtCurrentFlagValue.Focus();
				return;
			}
			ParameterInfo.currentChannel_1_Range =stringToDouble(cbxCurrentRangeForChannel_1);
			ParameterInfo.currentChannel_1_OutVolt = stringToDouble(cbxCurrentOutVoltForChannel_1);
			ParameterInfo.currentChannel_2_Range = stringToDouble(cbxCurrentRangeForChannel_2);
			ParameterInfo.currentChannel_2_OutVolt = stringToDouble(cbxCurrentOutVoltForChannel_2);
			ParameterInfo.currentChannel_3_Range = stringToDouble(cbxCurrentRangeForChannel_3);
			ParameterInfo.currentChannel_3_OutVolt = stringToDouble(cbxCurrentOutVoltForChannel_3);

			ParameterInfo.shakeChannel_1_Range = stringToDouble(cbxWaveRangeForChannel_1);
			ParameterInfo.shakeChannel_1_OutVolt = stringToDouble(cbxWaveOutVoltForChannel_1);
			ParameterInfo.shakeChannel_2_Range = stringToDouble(cbxWaveRangeForChannel_2);
			ParameterInfo.shakeChannel_2_OutVolt = stringToDouble(cbxWaveOutVoltForChannel_2);
			ParameterInfo.shakeChannel_3_Range = stringToDouble(cbxWaveRangeForChannel_3);
			ParameterInfo.shakeChannel_3_OutVolt = stringToDouble(cbxWaveOutVoltForChannel_3);

			ParameterInfo.currentChannel_1_CheckPara = 1.0;
			ParameterInfo.currentChannel_2_CheckPara = 1.0;
			ParameterInfo.currentChannel_3_CheckPara = 1.0;
			ParameterInfo.shakeChannel_1_CheckPara = 1.0;
			ParameterInfo.shakeChannel_2_CheckPara = 1.0;
			ParameterInfo.shakeChannel_3_CheckPara = 1.0;

			ParameterInfo.CurrentFrequency = 50;
			ParameterInfo.Date = DateTime.Now;
			if (ParameterInfo.currentChannel_1_OutVolt == 0 && ParameterInfo.currentChannel_1_Range == 0) {
				if (MessageBox.Show("参数设置出错!点击[确认]使用默认值,否则请重新设置!") == MessageBoxResult.OK) {
					ParameterInfo = _defaultPara;
				}
				else {
					return;
				}
			}
			var temp = 	service.LoadEntities(p => p.ID == ParameterInfo.ID).FirstOrDefault();
			if (temp == null) {
				service.AddEntity(ParameterInfo);
			}
			else {
				service.Modfiy(ParameterInfo);
			}
			
		 this.Hide();
		}
	
		void btnDefault_Click(object sender, RoutedEventArgs e) {
			ParameterInfo = _defaultPara;
			setUIElement(_defaultPara);
		}

		void btnCanel_Click(object sender, RoutedEventArgs e) {
			this.Hide();
		}
	}
}
