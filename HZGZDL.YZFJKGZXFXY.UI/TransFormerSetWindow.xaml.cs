#define Access
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
	/// TransFormerSetWindow.xaml 的交互逻辑
	/// </summary>
	public partial class TransFormerSetWindow : Window {
		public Model.TransformerInfo transformerInfo { get; set; }
		private Model.Switch_Manufactory_Model switchMMInfo = new Model.Switch_Manufactory_Model();
		private Model.Trans_Manufactory_Model transMMInfo =new Model.Trans_Manufactory_Model();
		bool IsInit = true;
		public delegate void SetTreeView();
		public SetTreeView set;

		/*********************ACCESS*************************/
		public BLL.TransAccessService AccessService = new BLL.TransAccessService();
		public BLL.SwitchMMAccessSercvice Access_switchMMservice = new BLL.SwitchMMAccessSercvice();
		public BLL.TransMMAccessService Access_transMMservice = new BLL.TransMMAccessService();

		/*********************SqlSever*************************/
		public BLL.TransformerService service = new BLL.TransformerService();
		public BLL.SwitchMMService _switchMMservice = new BLL.SwitchMMService();
		public BLL.TransMMService _transMMservice = new BLL.TransMMService();


		List<Model.Switch_Manufactory_Model> smm = new List<Model.Switch_Manufactory_Model>();
		List<Model.Trans_Manufactory_Model> tmm = new List<Model.Trans_Manufactory_Model>();
		public List<Model.TransformerInfo> trans = new List<Model.TransformerInfo>();
		#region 构造函数
		public TransFormerSetWindow(Model.TransformerInfo transInfo, SetTreeView setTree) {
			InitializeComponent();
			transformerInfo = transInfo;
			set = setTree;
			//更新UI
			//公司
			IsInit = true;
#if Access
			AccessService.Service.CreateDataBase(transformerInfo);
			Access_switchMMservice.Service.CreateDataBase(switchMMInfo);
			Access_transMMservice.Service.CreateDataBase(transMMInfo);
#endif
			cmbTransformerCompany.DropDownClosed += cmbTransformerCompany_DropDownClosed;
			cmbTransformerCompany.TextInput += TextInput;

			//变压器
			cmbTransFormers.SelectionChanged += cmbTransFormers_SelectionChanged;
			cmbTransFormers.TextInput += TextInput;

			//开关
			cmbSwitchManufactory.DropDownClosed += cmbSwitchManufactory_DropDownClosed;
			cmbSwitchManufactory.TextInput += TextInput;

			//变压器型号
			cmbTransFormersManufactory.DropDownClosed += cmbTransFormersManufactory_DropDownClosed;
			cmbTransFormersManufactory.TextInput += TextInput;
			btnUpdateData_Click(null, null);
		}

		
		#endregion
	
		#region 公司 Cmb事件

		void cmbTransformerCompany_DropDownClosed(object sender, EventArgs e) {
			var temp = cmbTransformerCompany.Text;
			if (String.IsNullOrEmpty(temp)) {
				return;
			}
			//根据所选择的公司  更新公司属下的变压器
			transformerInfo.CompanyName = temp.ToString();
		#if Access
			trans = AccessService.SelectList(t => true);
		#else
			trans = service.LoadEntities(t => true).OrderByDescending(u => u.Date).ToList();
		#endif

			var company = from c in trans
						  where c.CompanyName == transformerInfo.CompanyName
						  select c;
			//原有子项清空
			//添加新的变压器名称
			cmbTransFormers.Items.Clear();
			foreach (var item in company) {
				cmbTransFormers.Items.Add(item.TransformerName);
			}
			cmbTransFormers.SelectedIndex = 0;
		}
		#endregion

		#region 变压器Cmb事件
		void cmbTransFormers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!IsInit) {
				if (cmbTransFormers.SelectedItem == null) {
					return;
				}
				var temp = cmbTransFormers.SelectedItem.ToString();
				if (string.IsNullOrEmpty(temp)) {
					return;
				}
				//根据公司和变压器更新其他信息
				#if Access
								trans = AccessService.SelectList(t => true);
				#else
							trans = service.LoadEntities(t => true).OrderByDescending(u => u.Date).ToList();
				#endif
				var __temp = from t in trans
							 where t.CompanyName == transformerInfo.CompanyName && t.TransformerName == temp
							 select t;
				if (__temp == null) {
					return;
				}
				updateUI(__temp.FirstOrDefault());
			}
		}
		
		#endregion

		#region 变压器制造商事件
		void cmbTransFormersManufactory_DropDownClosed(object sender, EventArgs e) {
			if (cmbTransFormersManufactory.SelectedItem == null) {
				return;
			}
			var temp = from t in tmm
					   where t.Manufactory == cmbTransFormersManufactory.SelectedItem.ToString()
					   select t;
			cmbTransFormersModel.Items.Clear();
			foreach (var item in temp) {
				cmbTransFormersModel.Items.Add(item.Model);
			}
		}
		#endregion

		#region 开关制造商下拉框事件
		void cmbSwitchManufactory_DropDownClosed(object sender, EventArgs e) {
			if (cmbSwitchManufactory.SelectedItem == null) {
				return;
			}
			var temp = from s in smm
					   where s.Manufactory == cmbSwitchManufactory.SelectedItem.ToString()
					   select s;
			cmbSwitchModel.Items.Clear();
			foreach (var item in temp) {
				cmbSwitchModel.Items.Add(item.Model);
			}
		}
		#endregion

		#region  确认
		private void btnTransformerParaConfirm_Click(object sender, RoutedEventArgs e) {
			if (!checkNullInput()) {
				return;
			}
			transformerInfo.Date = System.DateTime.Now;
			transformerInfo.CompanyName = cmbTransformerCompany.Text;
			transformerInfo.TransformerName = cmbTransFormers.Text;
			transformerInfo.TransformerManufactory = cmbTransFormersManufactory.Text;
			transformerInfo.TransformerModel = cmbTransFormersModel.Text;
			transformerInfo.Phase = cb1P.IsChecked == true ? 1 : 3;
			transformerInfo.Winding = cb2RZ.IsChecked == true ? 2 : 3;
			if (cbOne.IsChecked == true) {
				transformerInfo.SwitchColumnCount = 1;
			}
			if (cbTwo.IsChecked == true) {
				transformerInfo.SwitchColumnCount = 2;
			}
			if (cbThrid.IsChecked == true) {
				transformerInfo.SwitchColumnCount = 3;
			}
			transformerInfo.SwitchManufactory = cmbSwitchManufactory.Text;
			transformerInfo.SwitchModel = cmbSwitchModel.Text;
			transformerInfo.StartPos = int.Parse(tbStartWorkingPosition.Text);
			transformerInfo.EndPos = int.Parse(tbEndWorkingPosition.Text);
			//加入数据库 如果已存在 则更新 否则 添加
			//验证变电站所属公司是否存在  存在 true  不存在为 false
			#region 数据库添加变压器所属单位和变压器时 判断是否已存在,若已存在则更新
			
#if Access
						trans = AccessService.SelectList(t => true);
						var _tempTrans = from t in trans
										 where t.CompanyName == transformerInfo.CompanyName
										 select t;

						if (_tempTrans.Count() > 0) {
							var ___temptrans = from t in _tempTrans
											   where t.TransformerName == transformerInfo.TransformerName
											   select t;
							int len = ___temptrans.Count();
							//int ID = ___temptrans.FirstOrDefault().ID;
							int ID11 = transformerInfo.ID;
							if (len > 0) {
								AccessService.Update(transformerInfo, t => t.ID == ID11);
							}
							else {
								AccessService.Insert(transformerInfo);
							}
						}
						else {
							AccessService.Insert(transformerInfo);
						}
			
#else
										trans = service.LoadEntities(t => true).OrderByDescending(u => u.Date).ToList();
										var _tempTrans = from t in trans
														 where t.CompanyName == transformerInfo.CompanyName
														 select t;

										if (_tempTrans.Count() > 0) {
											var ___temptrans = from t in _tempTrans
															   where t.TransformerName == transformerInfo.TransformerName
															   select t;
											int len = ___temptrans.Count();
											//int ID = ___temptrans.FirstOrDefault().ID;
											int ID11 = transformerInfo.ID;
											if (len >0) {
												if (!service.Modfiy(transformerInfo)) {
													MessageBox.Show("修改失败");
												}
											}
											else {
												service.AddEntity(transformerInfo);
											}
										}
										else {
											service.AddEntity(transformerInfo);
										}
										service.SaveChange();
			
#endif

			#endregion

			#region 数据库添加变压器型号和变压器制造商时 判断是否已存在,若已存在则更新

#if Access
						Model.Trans_Manufactory_Model _trans_MM = new Model.Trans_Manufactory_Model();
						_trans_MM.Model = cmbTransFormersModel.Text;
						_trans_MM.Manufactory = cmbTransFormersManufactory.Text;

						if (Access_transMMservice.SelectList(t => t.Manufactory == _trans_MM.Manufactory).Count > 0) {
							if (Access_transMMservice.SelectList(t => t.Model == _trans_MM.Model).Count <= 0) {
								Access_transMMservice.Insert(_trans_MM);
							}
						}
						else {
							Access_transMMservice.Insert(_trans_MM);
						}
#else
					if (_transMMservice.IsManufactoryExit(cmbTransFormersManufactory.Text)) {
						if (!_transMMservice.IsModelExit(cmbTransFormersModel.Text)) {
							Model.Trans_Manufactory_Model _trans_MM = new Model.Trans_Manufactory_Model();
							_trans_MM.Model = cmbTransFormersModel.Text;
							_trans_MM.Manufactory = cmbTransFormersManufactory.Text;
							_transMMservice.AddEntity(_trans_MM);
						}
					}
					else {
						Model.Trans_Manufactory_Model _trans_MM = new Model.Trans_Manufactory_Model();
						_trans_MM.Model = cmbTransFormersModel.Text;
						_trans_MM.Manufactory = cmbTransFormersManufactory.Text;
						_transMMservice.AddEntity(_trans_MM);
					}		
#endif


			#endregion

			#region 数据库添加开关型号和开关制造商时 判断是否已存在,若已存在则更新

#if Access
						Model.Switch_Manufactory_Model _switch_MM = new Model.Switch_Manufactory_Model();
						_switch_MM.Model = cmbSwitchModel.Text;
						_switch_MM.Manufactory = cmbSwitchManufactory.Text;

						if (Access_switchMMservice.SelectList(t => t.Manufactory == _switch_MM.Manufactory).Count > 0) {
							if (Access_transMMservice.SelectList(t => t.Model == _switch_MM.Model).Count <= 0) {
								Access_switchMMservice.Insert(_switch_MM);
							}
						}
						else {
							Access_switchMMservice.Insert(_switch_MM);
						}
#else
					if (_switchMMservice.IsManufactoryExit(cmbSwitchManufactory.Text)) {
				if (!_switchMMservice.IsModelExit(cmbSwitchModel.Text)) {
					Model.Switch_Manufactory_Model _trans_MM = new Model.Switch_Manufactory_Model();
					_trans_MM.Model = cmbSwitchModel.Text;
					_trans_MM.Manufactory = cmbSwitchManufactory.Text;
					_switchMMservice.AddEntity(_trans_MM);
				}
			}
			else {
				Model.Switch_Manufactory_Model _trans_MM = new Model.Switch_Manufactory_Model();
				_trans_MM.Model = cmbSwitchModel.Text;
				_trans_MM.Manufactory = cmbSwitchManufactory.Text;
				_switchMMservice.AddEntity(_trans_MM);
			}
#endif

						#endregion



			set();
			this.Hide();
		}
		#endregion

		#region 取消
		private void btnTransformerParaCancel_Click(object sender, RoutedEventArgs e) {
			this.Hide();
		}
		#endregion

		#region 文字输入

		void TextInput(object sender, TextCompositionEventArgs e) {
			var temp = sender as ComboBox;
			foreach (var item in temp.Items) {
				if (temp.Text == item.ToString()) {
					temp.SelectedItem = item;
					temp.Text = item.ToString();
				}
			}
		}
		
		#endregion

		#region UI更新
		void updateUI(Model.TransformerInfo trans) {
			if (trans == null) {
				return;
			}
			selectCmb(cmbTransFormersManufactory, trans.TransformerManufactory);
			selectCmb(cmbTransFormersModel, trans.TransformerModel);
			selectCmb(cmbSwitchManufactory, trans.SwitchManufactory);
			selectCmb(cmbSwitchModel, trans.SwitchModel);
			switch (trans.Winding) {
				case (2): cb2RZ.IsChecked = true; break;
				case (3): cb3RZ.IsChecked = true; break;
				default: break;
			}
			switch (trans.Phase) {
				case (1): cb1P.IsChecked = true; break;
				case (3): cb3P.IsChecked = true; break;
				default: break;
			}
			switch (trans.SwitchColumnCount) {
				case (1): cbOne.IsChecked = true; break;
				case (2): cbTwo.IsChecked = true; break;
				case (3): cbThrid.IsChecked = true; break;
				default: break;
			}

			tbEndWorkingPosition.Text = trans.EndPos + "";
			tbStartWorkingPosition.Text = trans.StartPos + "";

			transformerInfo = trans;
		}
		#endregion

		#region Null值检查
		bool checkNullInput() {
			if (string.IsNullOrEmpty(cmbSwitchManufactory.Text)) {
				MessageBox.Show("请设置开关制造商信息");
				cmbSwitchManufactory.IsDropDownOpen = true;
				return false;
			}
			if (string.IsNullOrEmpty(cmbSwitchModel.Text)) {
				MessageBox.Show("请设置开关型号信息");
				cmbSwitchModel.IsDropDownOpen = true;
				return false;
			}
			if (string.IsNullOrEmpty(cmbTransformerCompany.Text)) {
				MessageBox.Show("请设置测试单位信息");
				cmbTransformerCompany.IsDropDownOpen = true;
				return false;
			}
			if (string.IsNullOrEmpty(cmbTransFormers.Text)) {
				MessageBox.Show("请设置测试变压器名称");
				cmbTransFormers.IsDropDownOpen = true;
				return false;
			}

			if (string.IsNullOrEmpty(cmbTransFormersManufactory.Text)) {
				MessageBox.Show("请设置测试变压器制造商信息");
				cmbTransFormersManufactory.IsDropDownOpen = true;
				return false;
			}
			if (string.IsNullOrEmpty(cmbTransFormersModel.Text)) {
				MessageBox.Show("请设置测试变压器型号商信息");
				cmbTransFormersModel.IsDropDownOpen = true;
				return false;
			}
			if (string.IsNullOrEmpty(tbEndWorkingPosition.Text)) {
				MessageBox.Show("请设置开关最大工作位置");
				tbEndWorkingPosition.Focus();
				return false;
			}
			if (string.IsNullOrEmpty(tbStartWorkingPosition.Text)) {
				MessageBox.Show("请设置开关最小工作位置");
				tbStartWorkingPosition.Focus();
				return false;
			}
			return true;
		}
		#endregion

		void addItemToCmb(ComboBox p, string value) {
			foreach (var item in p.Items) {
				if (item.ToString() == value) {
					return;
				}
			}
			p.Items.Add(value);
		}

		void selectCmb(ComboBox cbx, string value) {
			foreach (var item in cbx.Items) {
				if (item.ToString() == value) {
					cbx.SelectedItem = item;
					return;
				}
			}
			cbx.Items.Add(value);
			cbx.SelectedIndex = cbx.Items.Count - 1;
		}

		 private void Window_Activated(object sender, EventArgs e) {
		 }
		 private void LoadAllEntities() {
			 trans = service.LoadEntities(t => true).OrderByDescending(u => u.Date).ToList();
			 smm = _switchMMservice.LoadEntities(u => true).OrderByDescending(u => u.Manufactory).ToList();
			 tmm = _transMMservice.LoadEntities(u => true).OrderByDescending(u => u.Manufactory).ToList();
		 }
		 private void LoadAllAccessEntities() {
			 trans = AccessService.SelectList(t => true).OrderByDescending(t=>t.Date).ToList();
			 smm = Access_switchMMservice.SelectList(s => true).OrderByDescending(u=>u.Manufactory).ToList();
			 tmm = Access_transMMservice.SelectList(s => true).OrderByDescending(u => u.Manufactory).ToList();
		 }
		 private void btnUpdateData_Click(object sender, RoutedEventArgs e) {
			 //数据刷新
#if Access
			 LoadAllAccessEntities();
#else
			 LoadAllEntities();
#endif

			 //cmb赋值
			 cmbTransformerCompany.Items.Clear();
			 cmbTransFormers.Items.Clear();
			 cmbSwitchManufactory.Items.Clear();
			 cmbTransFormersManufactory.Items.Clear();
			 foreach (var item in trans) {
				 addItemToCmb(cmbTransformerCompany, item.CompanyName);
				 addItemToCmb(cmbTransFormers, item.TransformerName);
			 }
			 foreach (var item in tmm) {
				 addItemToCmb(cmbTransFormersManufactory, item.Manufactory);
			 }
			 foreach (var item in smm) {
				 addItemToCmb(cmbSwitchManufactory, item.Manufactory);
			 }
			 cmbTransformerCompany.SelectedIndex = 0;
			 transformerInfo.CompanyName = cmbTransformerCompany.SelectedItem.ToString();
			 IsInit = false;
			 //cmbTransformerCompany_SelectionChanged(null, null);
			 return;
		 }



	}
}
