#define Access
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HZGZDL.YZFJKGZXFXY.Common;
using Steema.TeeChart.WPF;
using Steema.TeeChart.WPF.Styles;

namespace HZGZDL.YZFJKGZXFXY.UI {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			//代码优先自动更新数据库
			System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<Model.myEFTransFormerInfo>());
			//解决数据库初始化耗时
			using (var dbcontext = new Model.myEFTransFormerInfo()) {
				var objectContext = ((IObjectContextAdapter)dbcontext).ObjectContext;
				var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
				mappingCollection.GenerateViews(new List<EdmSchemaError>());
			}
			btnStartTest.IsEnabled = false;
			btnStop.IsEnabled = false;
			btnShowAD.IsChecked = true;
		}

		#region 系统变量声明
		/// <summary>
		/// 查看运行时间
		/// </summary>
		Stopwatch getWorkingTime = new Stopwatch();

		/// <summary>
		/// 函数行号获取
		/// </summary>
		System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);

		/// <summary>
		/// 自定义坐标的起点们
		/// </summary>
		List<double> offset;

		/// <summary>
		/// 1
		/// </summary>
		double MUT = 1;

		/// <summary>
		/// 坐标轴分区个数 2
		/// </summary>
		static int AxisCount =2;

		/// <summary>
		/// 测试是否开始
		/// </summary>
		bool is_Measuring = false;

		/// <summary>
		/// 接受的消息Buffer长度
		/// </summary>
		static int dataBuffSzie = 964;

		/// <summary>
		/// 显示时转换成int 的偷点数 10
		/// </summary>
		static int __偷点数 = 1;

		/// <summary>
		/// 0.12
		/// </summary>
		static int ___每次刷新时间s = 1;
		/// <summary>
		/// 120
		/// </summary>
		static int __平移每次 = __偷点数 * 12;//120
		/// <summary>
		/// 20
		/// </summary>
		static int  ___刷新时取平均值的点Count = 100;
		/// <summary>
		/// 20
		/// </summary>
		/// <summary>
		/// 0.002
		/// </summary>
		static double ___X轴最大值 = 0.6;
		static int ConcurrentQueueDrawCount = 15/ ___每次刷新时间s;
		static int _current10sCount = 500;
		
		static int _wave10sCount = 1000000 / __偷点数 / ___刷新时取平均值的点Count;
		/// <summary>
		/// 将数据保存到文件 false
		/// </summary>
		bool Is_TimeToSaveFile = false;

		/// <summary>
		/// 刷新的时间  (秒) 0
		/// </summary>
		double Time = 0;

		public Model.SystemMsgLevel LogShowLevel = Model.SystemMsgLevel.INFO;
		
		/// <summary>
		/// 系统消息监听线程
		/// </summary>
		Thread showSysMsg;

		/// <summary>
		/// 下位机IP
		/// </summary>
		EndPoint remote = null;

		EndPoint loacl = null;

		/// <summary>
		/// 连接窗口实例
		/// </summary>
		ConnectionWindow connectionWindow;

		Common.UDPHelper.asycUDPSever UDPSever;

		/// <summary>
		/// 自定义消息对列
		/// </summary>
	     public	ConcurrentQueue<string> myMessageConcurrentQueue = new ConcurrentQueue<string>();
		/// <summary>
		/// 是否继续处理数据
		/// </summary>
		bool is_ContinueProcessData = true;
		bool IsMesureIng = false;

		/// <summary>
		/// 接收队列
		/// </summary>
		ConcurrentQueue<byte[]> RecedData = new ConcurrentQueue<byte[]>();
		ConcurrentQueue<byte[]> SavetoFileConcurrentQueue = new ConcurrentQueue<byte[]>();

		ConcurrentQueue<byte[]> _waveByteConcurrentQueue1 = new ConcurrentQueue<byte[]>();
		ConcurrentQueue<byte[]> _waveByteConcurrentQueue2 = new ConcurrentQueue<byte[]>();
		ConcurrentQueue<byte[]> _waveByteConcurrentQueue3 = new ConcurrentQueue<byte[]>();
		ConcurrentQueue<byte[]> _currentByteConcurrentQueue1 = new ConcurrentQueue<byte[]>();
		ConcurrentQueue<byte[]> _currentByteConcurrentQueue2= new ConcurrentQueue<byte[]>();
		ConcurrentQueue<byte[]> _currentByteConcurrentQueue3 = new ConcurrentQueue<byte[]>();
		ConcurrentQueue<double[]> _waveDataConcurrentQueue1_Array = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> _waveDataConcurrentQueue2_Array = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> _waveDataConcurrentQueue3_Array = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> _currentDataConcurrentQueue1_Array = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> _currentDataConcurrentQueue2_Array = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> _currentDataConcurrentQueue3_Array = new ConcurrentQueue<double[]>();
		ConcurrentQueue<string> voltConcurrentQueue = new ConcurrentQueue<string>();
		/// <summary>
		/// 收到的包数
		/// </summary>
		int PageCount = 0;

		/// <summary>
		/// 根目录
		/// </summary>
		string RootPath = (System.AppDomain.CurrentDomain.BaseDirectory);

		Model.TransformerInfo myTransformerInfo = new Model.TransformerInfo();
		Model.ParameterInfo myParameterInfo = new Model.ParameterInfo();
		#endregion

		#region Chart初始化设置
		List<Axis> AxisList = new List<Axis>();
		List<FastLine> LineList = new List<FastLine>();
		List<Color> ColorList = new List<Color>();
		List<Brush> BrushList = new List<Brush>();
		List<Steema.TeeChart.WPF.Tools.CursorTool> cursorList = new List<Steema.TeeChart.WPF.Tools.CursorTool>();
		Steema.TeeChart.WPF.Tools.CursorTool CurorV;
		Random X = new Random();
		private void TChart_Loaded(object sender, RoutedEventArgs e) {

		}
		private void initTeeChart(Steema.TeeChart.WPF.TChart chart) {

			chart.Aspect.View3D = false;
			chart.Legend.Visible = false;
			chart.Header.Visible = false;
			chart.Legend.LegendStyle = LegendStyles.Series;
			chart.Axes.Left.Inverted = false;
			chart.Axes.Left.SetMinMax(0, 100);
			chart.Walls.Visible = false;
			chart.Axes.Top.Visible = false;
			chart.Axes.Right.Visible = false;
			Steema.TeeChart.WPF.Axis.AxisLinePen pen = new Steema.TeeChart.WPF.Axis.AxisLinePen();
			pen.Color = Colors.OrangeRed;
			chart.Axes.Left.PositionUnits = PositionUnits.Percent;
			chart.Axes.Bottom.AxisPen = pen;
			chart.Axes.Left.AxisPen = pen;
			//Chart.Axes.Bottom.Grid.Visible = true;
			//Chart.Axes.Left.Grid.Visible = true;
			chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			chart.Axes.Bottom.AutomaticMinimum = true;
			chart.MouseMove += Chart_MouseMove;
			chart.MouseDoubleClick += Chart_MouseDoubleClick;
			if (chart.Name == Chart.Name) {
				InitAxes();
			}
			Steema.TeeChart.WPF.Themes.BlackIsBackTheme black_theme =
			new Steema.TeeChart.WPF.Themes.BlackIsBackTheme(chart.Chart);
			black_theme.Apply(chart.Chart);
			InitLine(chart);
			if (chart.Name == Chart.Name) {
				InitCursor();
			}
			
		}
		class BlackTheme : Steema.TeeChart.WPF.Themes.OperaTheme {
			protected override void SetDefaultValues() {
				base.SetDefaultValues();
			}
		}
		void Chart_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			Chart.Axes.Left.SetMinMax(0, 100);
			Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
		}

		void Chart_MouseMove(object sender, MouseEventArgs e) {
			//if (e.RightButton == MouseButtonState.Pressed) {
			//	Chart.Axes.Left.SetMinMax(0, 100);
			//	Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			//}
			//if (e.LeftButton == MouseButtonState.Pressed) {
			//	Chart.Axes.Left.SetMinMax(0, 100);
			//	Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			//}
		}

		private void InitLine(Steema.TeeChart.WPF.TChart chart) {
			for (int i = 0; i < AxisCount*3; i++) {
				FastLine line = new FastLine(chart.Chart);
				Steema.TeeChart.WPF.Drawing.ChartPen pen = new Steema.TeeChart.WPF.Drawing.ChartPen();
				pen.Color = ColorList[i%6];
				pen.Width = 2;
				line.LinePen = pen;
				LineList.Add(line);
			}
		}

		private void InitAxes() {
			offset = Common.ChartInit.GetAxisPos(AxisCount);
			for (int i = 0; i < AxisCount; i++) {
				Axis temp = new Axis(Chart.Chart);
				Axis.AxisLinePen pen = new Axis.AxisLinePen();
				pen.Color = ColorList[i%6];
				pen.Width = 1;
				temp.Labels.Color = ColorList[i % 6];
				temp.AxisPen = pen;
				temp.Automatic = true;
				temp.SetMinMax(0, 200);
				temp.Grid.Visible = true;
				temp.StartEndPositionUnits = PositionUnits.Percent;
				temp.StartPosition = offset[i] - offset[0];
				temp.EndPosition = offset[i];
				Chart.Axes.Custom.Add(temp);
				AxisList.Add(temp);
			}
		}

		private void InitCursor(List<Steema.TeeChart.WPF.Tools.CursorTool> cursorList, Color color, int width, Steema.TeeChart.WPF.Tools.CursorToolStyles style, bool IsFollowMouse) {
			Steema.TeeChart.WPF.Tools.CursorTool Cursor = new Steema.TeeChart.WPF.Tools.CursorTool(Chart.Chart);
			Cursor.Style = style;
			Steema.TeeChart.WPF.Drawing.ChartPen pen = new Steema.TeeChart.WPF.Drawing.ChartPen();
			pen.Color = color;
			pen.Width = 0.5;
			Cursor.Pen = pen;
			Cursor.FollowMouse = IsFollowMouse;
			cursorList.Add(Cursor);
		}

		void InitCursor() {
			CurorV = new Steema.TeeChart.WPF.Tools.CursorTool(Chart.Chart);
			Steema.TeeChart.WPF.Drawing.ChartPen pen = new Steema.TeeChart.WPF.Drawing.ChartPen();
			CurorV.Style = Steema.TeeChart.WPF.Tools.CursorToolStyles.Vertical;
			pen.Color = Color.FromRgb(0x4b,0x5c,0xc4);
			pen.Width = 2;
			CurorV.Pen = pen;
			CurorV.FollowMouse = false;
			CurorV.Active = false;
		}

		private void AddDataToTheFirstAxes(FastLine line, Axis axis) {
			line.CustomVertAxis = axis;
		}
		#endregion

		#region 数据接受
		object synObj = new object();
		bool Is_ConnectSuccess = false;
		private void tcpMsgRecvive(byte[] data_buffer) {
			Is_ConnectSuccess = true;
			//连接响应
			if ((data_buffer[0] == 0 && data_buffer[1] == 0xff) || (data_buffer[0] == 0 && data_buffer[1] == 0x0)) {
				this.Dispatcher.Invoke(new Action(delegate {
				if (btnConnectDevice.IsEnabled) {
						btnStartTest.IsEnabled = true;
						btnConnectDevice.IsEnabled = false;
						connectionWindow.Hide();
						myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + "下位机连接成功收到返回码:0X00FF00FF");
				}
				}));
			}
			//停止测试响应
			if (data_buffer[0] == 0x05 && data_buffer[1] == 0x55) {
				this.Dispatcher.Invoke(new Action(delegate {
					MeasureReSet();
					myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.INFO +"|时间="+DateTime.Now+"::行号=" + GetLineNum() + "::Msg="+ "测试已停止收到返回码:0X05550555");
				}));
				_currentByteConcurrentQueue1 = new ConcurrentQueue<byte[]>();
				_currentByteConcurrentQueue2 =new ConcurrentQueue<byte[]>();
				_currentByteConcurrentQueue3 =new ConcurrentQueue<byte[]>();
				_currentDataConcurrentQueue1_Array=new ConcurrentQueue<double[]>();
				_currentDataConcurrentQueue2_Array=new ConcurrentQueue<double[]>();
				_currentDataConcurrentQueue3_Array=new ConcurrentQueue<double[]>();
				_waveByteConcurrentQueue1 = new ConcurrentQueue<byte[]>();
				_waveByteConcurrentQueue2 = new ConcurrentQueue<byte[]>();
				_waveByteConcurrentQueue3 = new ConcurrentQueue<byte[]>();
				_waveDataConcurrentQueue1_Array=new ConcurrentQueue<double[]>();
				_waveDataConcurrentQueue2_Array=new ConcurrentQueue<double[]>();
				_waveDataConcurrentQueue3_Array=new ConcurrentQueue<double[]>();
			}
			//保存数据
			if (data_buffer[0] == 0x09 && data_buffer[1] == 0x09) {
					//保存到缓存 数据处理
						IsMesureIng = true;
						byte[] temp = new byte[dataBuffSzie];
						data_buffer.CopyTo(temp, 0);
						RecedData.Enqueue(temp);
						//保存到队列  写入文件
						SavetoFileConcurrentQueue.Enqueue(temp);
					if (SavetoFileConcurrentQueue.Count > 1250) {
						byte[] s;
						SavetoFileConcurrentQueue.TryDequeue(out s);
					}
					//获取下一包数据
					UDPSever.Send(Model.Commander.GetDataCode);
					if (Is_TimeToSaveFile) {
						PageCount++;
						if (PageCount >= 12500) {
							is_ContinueProcessData = false;
							UDPSever.Send(Model.Commander.StopCode);
						}
					}
			}
			if (data_buffer[0] == 0x50 && data_buffer[1] == 0x50) {
				double voltage = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, 2))/10;
				voltConcurrentQueue.Enqueue(voltage.ToString("0.##"));
				if (voltConcurrentQueue.Count > 1) {
					string s;
					voltConcurrentQueue.TryDequeue(out s);
				}
			}
		}
		#endregion

		#region  数据分组
		void groupData() {
			// 50包byte 为 40ms 数据   4000个 int 点
 			//偷 10个点 转换 成 int   40ms 为 400 点
			int _countConvertCount = 4000/__偷点数;
			int _conuntPageConvert = 50;
			int _countEachConcurrentQueue = 80 / __偷点数;
			double[] temp__Array1 = new double[_countConvertCount];
			double[] temp__Array2 = new double[_countConvertCount];
			double[] temp__Array3 = new double[_countConvertCount];
			double[] temp__Array4 = new double[_countConvertCount];
			double[] temp__Array5 = new double[_countConvertCount];
			double[] temp__Array6 = new double[_countConvertCount];
				while (true) {
					if (!is_ContinueProcessData) {
						break;
						//return;
					}
					//分组一次 分 4ms  数据  偷点数
					if (RecedData.Count > _conuntPageConvert) {
						for (int pageindex = 0; pageindex < _conuntPageConvert; pageindex++) {
							byte[] data_buffer ;
							RecedData.TryDequeue(out data_buffer);
							if (data_buffer == null) {
								continue;
							}
							for (int i = 4; i < data_buffer.Length; i += __平移每次) {
								temp__Array1[_countEachConcurrentQueue * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i))) / MUT;
								temp__Array2[_countEachConcurrentQueue * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 2))) / MUT;
								temp__Array3[_countEachConcurrentQueue * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 4))) / MUT;
								temp__Array4[_countEachConcurrentQueue * pageindex + i / __平移每次] = -(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 6))) / MUT;
								temp__Array5[_countEachConcurrentQueue * pageindex + i / __平移每次] = -(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 8))) / MUT;
								temp__Array6[_countEachConcurrentQueue * pageindex + i / __平移每次] = -(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 10))) / MUT;
							}
						}
						//50包 偷 200个点   40ms
						_currentDataConcurrentQueue1_Array.Enqueue(temp__Array1);
						_currentDataConcurrentQueue2_Array.Enqueue(temp__Array2);
						_currentDataConcurrentQueue3_Array.Enqueue(temp__Array3);
						_waveDataConcurrentQueue1_Array.Enqueue(temp__Array4);
						_waveDataConcurrentQueue2_Array.Enqueue(temp__Array5);
						_waveDataConcurrentQueue3_Array.Enqueue(temp__Array6);
						if (_waveDataConcurrentQueue3_Array.Count > 20) {
							calculate_电流有效值Draw(_currentDataConcurrentQueue1_Array, currentDrawConcurrentQueue_channel_1);
							calculate_电流有效值Draw(_currentDataConcurrentQueue2_Array, currentDrawConcurrentQueue_channel_2);
							calculate_电流有效值Draw(_currentDataConcurrentQueue3_Array, currentDrawConcurrentQueue_channel_3);
							WaveProgress(_waveDataConcurrentQueue1_Array, waveDrawConcurrentQueue_channel_1);
							WaveProgress(_waveDataConcurrentQueue2_Array, waveDrawConcurrentQueue_channel_2);
							WaveProgress(_waveDataConcurrentQueue3_Array, waveDrawConcurrentQueue_channel_3);
						}
					}
					else {
						Thread.Sleep(10);
					}
				}

		}
		object prepareobj = new object();

		#endregion

		#region 数据保存
		void save_data(string fileName, byte[] data) {
			MyFileHelper.SaveFile_Append(fileName, data, dataBuffSzie);
		}
		byte[] createFileHeaderInfo(Model.TransformerInfo transInfo) {
			byte[] temp = new byte[dataBuffSzie];
			var head = Encoding.Unicode.GetBytes(transInfo.CompanyName + "|" + transInfo.TransformerName + "|" + transInfo.Date + "|" + transInfo.CurrentPos + transInfo.IsForwardHandoff);
			for (int i = 0; i < head.Length; i++) {
				temp[i] = head[i];
			}
				return temp;
		}
		object saveobj = new object();
		void dataSaveForThread() {
		
			while (true) {
				if (!is_ContinueProcessData && SavetoFileConcurrentQueue.Count == 0) {
					myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + "数据保存成功!");
					this.Dispatcher.Invoke(new Action(delegate {
						Show原始数据(_testDataPath);
					}));
					return;
				}
				try {
					if (SavetoFileConcurrentQueue.Count > 0 && Is_TimeToSaveFile) {
							byte[] temp;
							SavetoFileConcurrentQueue.TryDequeue(out temp);
							if (temp == null) {
								continue;
							}
							save_data(_testDataPath, temp);
					}
					else {
						Thread.Sleep(10);
					}
				}
				catch (Exception e) {
					myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + e.Message);
				}

			}
		}
		#endregion

		#region  数据显示

		ConcurrentQueue<double[]> currentDrawConcurrentQueue_channel_1 = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> currentDrawConcurrentQueue_channel_2 = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> currentDrawConcurrentQueue_channel_3 = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> waveDrawConcurrentQueue_channel_1 = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> waveDrawConcurrentQueue_channel_2 = new ConcurrentQueue<double[]>();
		ConcurrentQueue<double[]> waveDrawConcurrentQueue_channel_3 = new ConcurrentQueue<double[]>();


		object drawobj = new object();

		void dataShow_MyWay() {
			this.Chart.Dispatcher.Invoke(new Action(delegate {
					AxisList[1].SetMinMax(0, AxisMaxValue);
					DrawCurrentLine(currentDrawConcurrentQueue_channel_1, LineList[0]);
					DrawCurrentLine(currentDrawConcurrentQueue_channel_2, LineList[1]);
					DrawCurrentLine(currentDrawConcurrentQueue_channel_3, LineList[2]);
					DrawWaveLine(waveDrawConcurrentQueue_channel_1, LineList[3]);
					DrawWaveLine(waveDrawConcurrentQueue_channel_2, LineList[4]);
					DrawWaveLine(waveDrawConcurrentQueue_channel_3, LineList[5]);
			}));
		}


		void dataProcessForThread() {
			while (true) {
				if (!is_ContinueProcessData) {
					break;
				}
				try {
					if (waveDrawConcurrentQueue_channel_3.Count >= ConcurrentQueueDrawCount) {
						dataShow_MyWay();
					}
					else {
						Thread.Sleep(20);
					}
				}
				catch (Exception e) {
					myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + e.Message);
				}
			}
		}


		static int flag = 4000 / ___刷新时取平均值的点Count;

		void DrawWaveLine(ConcurrentQueue<double[]> data, FastLine line) {
			try {
				int col = ConcurrentQueueDrawCount;
				int row = flag*___每次刷新时间s;
				double[][] temp = new double[col][];
				temp= data.ToArray();
				double[] pointXBuffer = new double[col * row];
				double[] pointBuffer = new double[col * row];
				for (int i = 0; i < col; i++) {
					for (int j = 0; j < row; j++) {
						if (temp[i] == null) {
							return;
						}
						pointBuffer[i * row + j] = temp[i][j];
						pointXBuffer[i * row + j] = (i * row + j) * 0.04 * ___每次刷新时间s/row;
					}
				}
				line.Add(pointXBuffer, pointBuffer);
				for (int i = 0; i < ___每次刷新时间s; i++) {
					if (data.Count == 0) {
						break;
					}
					double[] s;
					data.TryDequeue(out s);
				}
			}
			catch (Exception e) {
				myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR +"|时间="+DateTime.Now+"::行号=" + GetLineNum() + "::Msg="+ e.Message);
			}
		}
		void DrawCurrentLine(ConcurrentQueue<double[]> data, FastLine line) {
			try {
				int col = ConcurrentQueueDrawCount;
				int row =2*___每次刷新时间s;
				double[][] temp = new double[col][];
				temp = data.ToArray();
				if (temp == null) { return; }
				double[] pointXBuffer = new double[col * row];
				double[] pointBuffer = new double[col * row];
				for (int i = 0; i < col; i++) {
					for (int j = 0; j < row; j++) {
						if (temp[i] == null) {
							return;
						}

						pointBuffer[i * row + j] = temp[i][j];
						pointXBuffer[i * row + j] = (i * row + j) * 0.04 * ___每次刷新时间s / row;
					}
				}
				
				line.Add(pointXBuffer, pointBuffer);
				for (int i = 0; i < ___每次刷新时间s; i++) {
					if (data.Count == 0) {
						break;
					}
					double[] de;
					data.TryDequeue(out de);
				}
			}
			catch (Exception e) {
				myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + e.Message);
			}
		}
		#endregion

		#region 计算电流有效值
		bool IsNeedSetMax_1 = true;
		bool IsNeedSetMax_10 = true;
		bool IsNeedSetMax_100 = true;
		double AxisMaxValue = 0.1;
		void calculate_电流有效值Draw(ConcurrentQueue<double[]> source_data, ConcurrentQueue<double[]> result_storage) {
			//source_data  每个成员 40ms   如果电流是 50hz  那么 就是2个周期
			int 每个队列成员包含的电流有效值个数 = 2*___每次刷新时间s;//2
			int 计算有效点需要的数据点数 = 2000/__偷点数;//6
			double sum = 0;
			double[] 有效值存放 = new double[每个队列成员包含的电流有效值个数];

			for (int i = 0; i < ___每次刷新时间s; i++) {
					double[] temp ;
					source_data.TryDequeue(out temp);
					for (int j = 0; j < 2; j++) {
						sum = 0;
						for (int k = 0; k < 计算有效点需要的数据点数; k++) {
							double _currentConvetValue = (temp[j * 计算有效点需要的数据点数 + k] * 2.5 * 10.0) / 32768.0;
							sum += _currentConvetValue * _currentConvetValue;
						}
						double __currentFffectValue = Math.Sqrt(sum / 计算有效点需要的数据点数) * 2;
						if (__currentFffectValue > 0 && __currentFffectValue < 1 && IsNeedSetMax_1) {
							AxisMaxValue = 1;
							IsNeedSetMax_1 = false;
						}
						if (__currentFffectValue >= 0.9 && __currentFffectValue < 10 && IsNeedSetMax_10) {
							AxisMaxValue = 10;
							IsNeedSetMax_10 = false;
						}
						if (__currentFffectValue > 9 && __currentFffectValue < 100 && IsNeedSetMax_100) {
							AxisMaxValue = 100;
							IsNeedSetMax_100 = false;
						}
						有效值存放[i * 2 + j] = __currentFffectValue;
						//如果有效值超过阀值  可以开始保存数据
						if (__currentFffectValue >= myParameterInfo.CurrentFlag) {
							Is_TimeToSaveFile = true;
						}
					}
			}
			result_storage.Enqueue(有效值存放);
			if (result_storage.Count > ConcurrentQueueDrawCount) {
				double[] s;
				result_storage.TryDequeue(out s);
			}
		}
		#endregion

		#region 计算振动AD值
		void __绘图数据处理程序(ConcurrentQueue<double[]> data, ConcurrentQueue<double[]> desData) {
			double[] tempArray = new double[___刷新时取平均值的点Count];//20
			double[] Y = new double[flag * ___每次刷新时间s];
			for (int index = 0; index < ___每次刷新时间s; index++) {//2
					double[] temp; 
					data.TryDequeue(out temp);
					Array.Sort(temp);
					for (int j = 0; j < flag; j++) {//50
						Y[index * flag + j] = temp[j * (___刷新时取平均值的点Count - 1)] * 625 / 32768;//index 0,1;
					}
			}
			desData.Enqueue(Y);
			if (desData.Count > ConcurrentQueueDrawCount) {
				double[] s; 
				desData.TryDequeue(out s);
			}
		}
		void WaveProgress(ConcurrentQueue<double[]> data, ConcurrentQueue<double[]> desData) {
			double[] Y = new double[flag * ___每次刷新时间s];
			for (int index = 0; index < ___每次刷新时间s; index++) {//2
				double[] temp;
				data.TryDequeue(out temp);
					for (int j = 0; j < flag; j++) {//50
						Y[index * flag + j] = temp[j * (___刷新时取平均值的点Count - 1)] * 625 / 32768;//index 0,1;
					}
			}
			desData.Enqueue(Y);
			if (desData.Count > ConcurrentQueueDrawCount) {
				double[] s;
				desData.TryDequeue(out s);
			}
		}
		#endregion

		#region 测试数据保存完成之后打开数据
		string current_DataPath;
		void Show原始数据(string path) {

			Time = 0;
			for (int i = 0; i < AxisCount; i++) {
				LineList[i].XValues.Clear();
				LineList[i].YValues.Clear();
			}
			Chart.Axes.Bottom.SetMinMax(0, 10);
			ChartAD.Axes.Left.SetMinMax(-10, 10);
			AxisMaxValue = 0.1;
			AxisList[0].SetMinMax(-10, 10);
			AxisList[1].SetMinMax(-10, 10);
			Common.DataFromFile.CreateDataFromFile(path);
			myProgressBar.Maximum = 964*11250;
			ThreadPool.QueueUserWorkItem(new WaitCallback(Draw));
			current_DataPath = path;
		}

		private void Draw(object state) {
			while (true) {
				if (Common.DataFromFile.IsReadComplete) {
					if (Common.DataFromFile.Wave_Channel_1_Data.Count < 1000000) {
						myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + "数据无效或者已损坏!!");
						string[] item = current_DataPath.Split('\\');
						current_DataPath = "";
						for (int i = 0; i < item.Length - 1; i++) {
							current_DataPath += item[i] + "\\";
						}
						Directory.Delete(current_DataPath, true);
						this.Chart.Dispatcher.Invoke(new Action(delegate {
							if (currentItem == null || currentSelectItem == null) {
								return;
							}
							currentItem.Items.Remove(currentSelectItem);
						}));
						
						break;
					}
					for(int i=0;i<1000000;i++)
					{
						Wave_1_Y[i] = Common.DataFromFile.Wave_Channel_1_Data[i];
						Wave_2_Y[i] = Common.DataFromFile.Wave_Channel_2_Data[i];
						Wave_3_Y[i] = Common.DataFromFile.Wave_Channel_3_Data[i];
						CurrentAD_1_Y[i] = Common.DataFromFile.Current_Channel_1_Data[i];
						CurrentAD_2_Y[i] = Common.DataFromFile.Current_Channel_2_Data[i];
						CurrentAD_3_Y[i] = Common.DataFromFile.Current_Channel_3_Data[i];
					}
					Common.DataFromFile.Wave_Channel_1_Data.Clear();
					Common.DataFromFile.Wave_Channel_2_Data.Clear();
					Common.DataFromFile.Wave_Channel_3_Data.Clear();
					Current_1_Y = Common.DataFromFile.CulcateCurrentEffect(Common.DataFromFile.Current_Channel_1_Data);
					Current_2_Y = Common.DataFromFile.CulcateCurrentEffect(Common.DataFromFile.Current_Channel_2_Data);
					Current_3_Y = Common.DataFromFile.CulcateCurrentEffect(Common.DataFromFile.Current_Channel_3_Data);
					double[] temp1 = new double[2048];
					double[] temp2 = new double[2048];
					double[] temp3 = new double[2048];
					double[] temp1_current = new double[2048];
					double[] temp2_current = new double[2048];
					double[] temp3_current = new double[2048];
					Common.myComplex.Complex[] temp4 = new Common.myComplex.Complex[2048];
					Common.myComplex.Complex[] temp5 = new Common.myComplex.Complex[2048];
					Common.myComplex.Complex[] temp6 = new Common.myComplex.Complex[2048];
					Common.myComplex.Complex[] temp4_current = new Common.myComplex.Complex[2048];
					Common.myComplex.Complex[] temp5_current = new Common.myComplex.Complex[2048];
					Common.myComplex.Complex[] temp6_current = new Common.myComplex.Complex[2048];
					for (int i = 0; i < 488; i++) {

						for (int j = 0; j < 2048; j++) {
							temp1[j] = Wave_1_Y[i * 2048 + j];
							temp2[j] = Wave_2_Y[i * 2048 + j];
							temp3[j] = Wave_3_Y[i * 2048 + j];
							temp1_current[j] = CurrentAD_1_Y[i * 2048 + j];
							temp2_current[j] = CurrentAD_2_Y[i * 2048 + j];
							temp3_current[j] = CurrentAD_3_Y[i * 2048 + j];
						}
						temp4 = Common.FreqAnalyzer.FFT(temp1, false);
						temp5 = Common.FreqAnalyzer.FFT(temp2, false);
						temp6 = Common.FreqAnalyzer.FFT(temp3, false);
						temp4_current = Common.FreqAnalyzer.FFT(temp1_current, false);
						temp5_current = Common.FreqAnalyzer.FFT(temp2_current, false);
						temp6_current = Common.FreqAnalyzer.FFT(temp3_current, false);
						for (int j = 0; j < 2048; j++) {
							WaveComplex_channel_1[i * 2048 + j] = temp4[j].ToModul();
							WaveComplex_channel_2[i * 2048 + j] = temp5[j].ToModul();
							WaveComplex_channel_3[i * 2048 + j] = temp6[j].ToModul();
							CurrentComplex_channel_1[i * 2048 + j] = temp4_current[j].ToModul();
							CurrentComplex_channel_2[i * 2048 + j] = temp5_current[j].ToModul();
							CurrentComplex_channel_3[i * 2048 + j] = temp6_current[j].ToModul();
						}
					}
					for (int i = 1000000 - 576; i < 1000000; i++) {
						WaveComplex_channel_1[i] = 0;
						WaveComplex_channel_2[i] = 0;
						WaveComplex_channel_3[i] = 0;
						CurrentComplex_channel_1[i] = 0;
						CurrentComplex_channel_2[i] = 0;
						CurrentComplex_channel_3[i] = 0;
					}
					this.Chart.Dispatcher.Invoke(new Action(delegate {
						setADline(10,0);
					}));
					break;
				}
				else {
					myProgressBar.Dispatcher.Invoke(new Action(delegate {
						myProgressBar.Visibility = System.Windows.Visibility.Visible;
						myProgressBar.Value = Common.DataFromFile.currentPos;
					}));
					Thread.Sleep(50);
				}
			}
		}
		
		#endregion
	
		#region 增量刷振动数据
		double[] Wave_X = new double[1000000];
		double[] Wave_1_Y = new double[1000000];
		double[] Wave_2_Y = new double[1000000];
		double[] Wave_3_Y = new double[1000000];
		double[] Current_X = new double[500];
		double[] Current_1_Y = new double[500];
		double[] Current_2_Y = new double[500];
		double[] Current_3_Y = new double[500];
		double[] CurrentAD_1_Y = new double[1000000];
		double[] CurrentAD_2_Y = new double[1000000];
		double[] CurrentAD_3_Y = new double[1000000];
		void reflushChart_wave(double Max, double Min, double[] x, double[] y, FastLine line) {
			AxisList[0].SetMinMax(-10, 10);
			line.XValues.Clear();
			line.YValues.Clear();
			int arrayCount = (int)(Math.Round(Max - Min, 4) * 100000);
			int _取点求平均Count = 1;
			int drawCount = arrayCount;
			while (drawCount >= 1000) {
				drawCount = drawCount / 2;
				_取点求平均Count *=2;
			}
			int offset = arrayCount / drawCount;
			double[] tempx = new double[drawCount];
			double[] tempy = new double[drawCount];
			for (int i = 0; i < drawCount; i++) {
				tempx[i] = x[(int)(Min * 100000 + (i) * offset)];
			}
			double sum = 0;
			for (int i = 0; i < drawCount; i++) {
				sum = 0;
				for (int j = 0; j < _取点求平均Count; j++) {
					sum += y[(int)(Min * 100000) + i * _取点求平均Count + j];
				}
				tempy[i] = sum / _取点求平均Count;
			}
			line.Add(tempx, tempy);
		}

		void reflushChart_FFTwave(double Max, double Min,double[] y, FastLine line) {
			myParameterInfo = systemSettingWindow.ParameterInfo;
			ChartAD.Axes.Left.SetMinMax(-myParameterInfo.shakeChannel_1_Range, myParameterInfo.shakeChannel_1_Range);
			ChartFFT.Axes.Left.SetMinMax(0, 5000);
			line.XValues.Clear();
			line.YValues.Clear();

			int arrayCount = (int)(Math.Round(Max - Min, 4) * 100000);
			double[] x = new double[arrayCount];
			for (int i = 0; i < arrayCount; i++) {
				x[i] = i;
			}
			int _取样Count = 1;
			int drawCount = arrayCount;
			while (drawCount >= 5000) {
				drawCount = drawCount / 2;
				_取样Count *= 2;
			}
			int offset = arrayCount / drawCount;
			double[] tempx = new double[drawCount];
			double[] tempy = new double[drawCount];
			for (int i = 0; i < drawCount; i++) {
				tempx[i] = x[(i) * offset];
				tempy[i] = y[(int)(Min * 100000 + (i) * offset)];
			}
			line.Add(tempx, tempy);
		}
		void reflushChart_current(double Max, double Min, double[] x, double[] y, FastLine line) {
			AxisList[1].SetMinMax(-1, 1);
			line.XValues.Clear();
			line.YValues.Clear();
			int arrayCount = (int)(Math.Round(Max - Min, 4) * 100000);
			int _取样Count = 1;
			int drawCount = arrayCount;
			while (drawCount >= 5000) {
				drawCount = drawCount / 2;
				_取样Count *= 2;
			}
			int offset = arrayCount / drawCount;
			double[] tempx = new double[drawCount];
			double[] tempy = new double[drawCount];
			for (int i = 0; i < drawCount; i++) {
				tempx[i] = x[(int)(Min * 100000 + (i) * offset)];
				tempy[i] = y[(int)(Min * 100000 + (i) * offset)];
			}
			line.Add(tempx, tempy);
		}
		void reflushChart_FFTcurrent(double Max, double Min, double[] y, FastLine line) {
			ChartAD.Axes.Left.SetMinMax(-myParameterInfo.shakeChannel_1_Range, myParameterInfo.shakeChannel_1_Range);
			ChartFFT.Axes.Left.SetMinMax(0, 5000);
			line.XValues.Clear();
			line.YValues.Clear();
			
			int arrayCount = (int)(Math.Round(Max - Min, 4) * 100000);
			double[] x = new double[arrayCount];
			for (int i = 0; i < arrayCount; i++) {
				x[i] = i;
			}
			int _取样Count = 1;
			int drawCount = arrayCount;
			while (drawCount >= 5000) {
				drawCount = drawCount / 2;
				_取样Count *= 2;
			}
			int offset = arrayCount / drawCount;
			double[] tempx = new double[drawCount];
			double[] tempy = new double[drawCount];
			for (int i = 0; i < drawCount; i++) {
				tempx[i] = x[(i) * offset];
				tempy[i] = y[(int)(Min * 100000 + (i) * offset)];
			}
			line.Add(tempx, tempy);
		}
		#endregion

		#region 启动时扫描 TestData中的数据并更新TreeView
		void AutoUpdateViewTree() {
			GetDirectoryInfoRecursion(RootPath + "TestData");
		}
		void GetDirectoryInfoRecursion(string directory) {
			if (directory == "") {
				return;
			}
			try {
				DirectoryInfo di = new DirectoryInfo(directory);
				DirectoryInfo[] directory_list = di.GetDirectories();
				StringBuilder sb = new StringBuilder();
				if (directory_list.Length <= 0) {
					FileInfo[] file_name_list = di.GetFiles();
					//获取原始数据
					if (file_name_list.Length > 0) {
						//单相
						for (int i = 0; i < file_name_list.Length; i++) {
							string[] _transInfoes = directory.Split('\\');
							string[] _trans = new string[5];
							for (int j = 0; j < _trans.Length; j++) {
								_trans[j] = _transInfoes[_transInfoes.Length - 5 + j];
							}
							Common.TreeViewHelper.TreeViewUpdateLocal(myParentTree, 6, _trans);
						}
					}
				}
				else {
					for (int i = 0; i < directory_list.Length; i++) {
						string path = directory_list[i].ToString();
						GetDirectoryInfoRecursion(directory + "\\" + path);
					}
				}
			}
			catch {

			}
		


		}
		#endregion

		#region 连接仪器

		#region 创建TCP连接
		private void createConnection() {


			if (UDPSever == null) {
				UDPSever = new Common.UDPHelper.asycUDPSever();
				UDPSever.RecieveBufferSize = dataBuffSzie;
				UDPSever.Message_receive = tcpMsgRecvive;
			}
			UDPSever.IsDataReceive = true;
			UDPSever.开启UDP服务(loacl, remote);
			UDPSever.Receive();
			Thread.Sleep(200);
			UDPSever.Send(Model.Commander.ConnectCode);
			//TcpClient.Send(Model.Commander.ConnectCode);
			//if (TcpClient == null) {
			//	TcpClient = new Common.TCPHelper.asyncTcpClient(tcpMsgRecvive, dataBuffSzie);
			//	TcpClient.连接服务器(remote);
			//}
			//else if (TcpClient.client == null) {
			//	TcpClient.连接服务器(remote);
			//}
			//TcpClient.Send(Model.Commander.ConnectCode);
		}

		void connectionConfim(object sender, RoutedEventArgs e) {
			remote = new IPEndPoint(IPAddress.Parse(connectionWindow.txtIPaddress.Text), int.Parse(connectionWindow.txtPort.Text));
			loacl = new IPEndPoint(IPAddress.Parse(connectionWindow.txtLoalIPaddress.Text), 12138);
			createConnection();
		}
		#endregion
		private void btnConnectDevice_Click(object sender, RoutedEventArgs e) {

			if (connectionWindow == null) {
				connectionWindow = new ConnectionWindow();
				connectionWindow.Owner = this;
				connectionWindow.btnConfirmConnection.Click += connectionConfim;
				connectionWindow.Show();
			}
			else {
				connectionWindow.Activate();
				connectionWindow.Show();
			}
		}
		#endregion

		#region 开始测试
		string _testDataPath;
		void DoSomethingForStart() {
			btnShowAD.IsChecked = true;
			AddDataToTheFirstAxes(LineList[0], AxisList[1]);
			AddDataToTheFirstAxes(LineList[1], AxisList[1]);
			AddDataToTheFirstAxes(LineList[2], AxisList[1]);
			AddDataToTheFirstAxes(LineList[3], AxisList[0]);
			AddDataToTheFirstAxes(LineList[4], AxisList[0]);
			AddDataToTheFirstAxes(LineList[5], AxisList[0]);
			Time = 0;
			for (int i = 0; i < AxisCount; i++) {
				LineList[i].XValues.Clear();
				LineList[i].YValues.Clear();
			}
			Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			AxisMaxValue = 0.1;
			AxisList[0].SetMinMax(-10, 10);
			AxisList[1].SetMinMax(0, AxisMaxValue);
			myParameterInfo.CurrentFrequency = 50;
			PageCount = 0;
			btnStop.IsEnabled = true;
			btnStartTest.IsEnabled = false;
			is_Measuring = true;
			is_ContinueProcessData = true;
			UDPSever.IsDataReceive = true;
			Is_TimeToSaveFile = false;
			SavetoFileConcurrentQueue= new ConcurrentQueue<byte[]>();
			RecedData= new ConcurrentQueue<byte[]>();
			IsNeedSetMax_1 = true;
			IsNeedSetMax_10 = true;
			IsNeedSetMax_100 = true;
			double[] temp1 = new double[flag*___每次刷新时间s];
			double[] temp2 = new double[2*___每次刷新时间s];
			#region 数据容器初始化
			temp1.Initialize();
			temp2.Initialize();
			waveDrawConcurrentQueue_channel_1= new ConcurrentQueue<double[]>();
			waveDrawConcurrentQueue_channel_2 = new ConcurrentQueue<double[]>();
			waveDrawConcurrentQueue_channel_3 = new ConcurrentQueue<double[]>();
			currentDrawConcurrentQueue_channel_1 = new ConcurrentQueue<double[]>();
			currentDrawConcurrentQueue_channel_2 = new ConcurrentQueue<double[]>();
			currentDrawConcurrentQueue_channel_3 = new ConcurrentQueue<double[]>();
			for (int i = 0; i < ConcurrentQueueDrawCount; i++) {
				waveDrawConcurrentQueue_channel_1.Enqueue(temp1);
				waveDrawConcurrentQueue_channel_2.Enqueue(temp1);
				waveDrawConcurrentQueue_channel_3.Enqueue(temp1);
				currentDrawConcurrentQueue_channel_1.Enqueue(temp2);
				currentDrawConcurrentQueue_channel_2.Enqueue(temp2);
				currentDrawConcurrentQueue_channel_3.Enqueue(temp2);
			}
			#endregion
		}
		private void btnStartTest_Click(object sender, RoutedEventArgs e) {
			if (TransformerInfoSetting == null) {
				TransformerInfoSetting = new TransFormerSetWindow(myTransformerInfo,setTreeView);
				TransformerInfoSetting.Owner = this;
				TransformerInfoSetting.Show();
				return;
			}
			_canstartTest.Visibility = System.Windows.Visibility.Visible;
			myTransformerInfo = TransformerInfoSetting.transformerInfo;
			cmbCurrentPos.Items.Clear();
			for (int i = myTransformerInfo.StartPos; i <= myTransformerInfo.EndPos; i++) {
				cmbCurrentPos.Items.Add(i + "");
			}
			foreach (var item in cmbCurrentPos.Items) {
				if (item.ToString() == myTransformerInfo.CurrentPos + "") {
					cmbCurrentPos.SelectedItem = item;
					tbCompanyName.Content = myTransformerInfo.CompanyName;
					tbTransName.Content = myTransformerInfo.TransformerName;
					if (myTransformerInfo.IsForwardHandoff) {
						rbBackward.IsChecked = true;
					}
					return;
				}
			}
			cmbCurrentPos.SelectedIndex = 0;
			if (myTransformerInfo.IsForwardHandoff) {
				rbBackward.IsChecked = true;
			}
			tbCompanyName.Content = myTransformerInfo.CompanyName;
			tbTransName.Content = myTransformerInfo.TransformerName;
		}
		void btnConfrimStart_Click(object sender, RoutedEventArgs e) {
			#region 测试开始更新一些数据库条目
			//更新数据库

			myTransformerInfo.Date = DateTime.Now;
			myTransformerInfo.CurrentPos = int.Parse(cmbCurrentPos.SelectedItem.ToString());
			if (rbBackward.IsChecked == true) {
				myTransformerInfo.IsForwardHandoff = true;
			}
			else {
				myTransformerInfo.IsForwardHandoff = false;
			}
			int id = TransformerInfoSetting.AccessService.Select(t=>t.CompanyName==myTransformerInfo.CompanyName&&t.TransformerName==myTransformerInfo.TransformerName).ID;
#if Access
			TransformerInfoSetting.AccessService.Update(myTransformerInfo, t => t.ID == id);
#else
			TransformerInfoSetting.service.SaveChange();
#endif

			#endregion
			myParameterInfo = systemSettingWindow.ParameterInfo;
			double flag = myParameterInfo.CurrentFlag;
			#region 检查存储目录
			string access = "";
			if (myTransformerInfo.IsForwardHandoff && myTransformerInfo.CurrentPos + 1 <= myTransformerInfo.EndPos) {
				access = "分接位[" + myTransformerInfo.CurrentPos + "]切[" + (myTransformerInfo.CurrentPos + 1) + "]";
			}
			if (!myTransformerInfo.IsForwardHandoff && myTransformerInfo.CurrentPos - 1 >= myTransformerInfo.StartPos) {
				access = "分接位[" + myTransformerInfo.CurrentPos + "]切[" + (myTransformerInfo.CurrentPos - 1) + "]";
			}
			_testDataPath = RootPath + @"\TestData\" + myTransformerInfo.CompanyName + "\\" + myTransformerInfo.TransformerName + "\\" + access + "\\" + myTransformerInfo.Date.ToString("yy-MM-dd") + "\\" + "测试数据[" + myTransformerInfo.Date.Hour + "点" + myTransformerInfo.Date.Minute + "分" + myTransformerInfo.Date.Second + "秒" + "]";
			if (!Directory.Exists(_testDataPath)) {
				Directory.CreateDirectory(_testDataPath);
			}
			_testDataPath += "\\原始数据.bin";
			save_data(_testDataPath, createFileHeaderInfo(myTransformerInfo));
			#endregion

			#region 更新TreeView
			Common.TreeViewHelper.TreeViewUpdateLocal(myParentTree, 6, myTransformerInfo);
			#endregion
			DoSomethingForStart();
			UDPSever.Send(Model.Commander.StartCode);
			Thread group = new Thread(groupData);
			Thread draw = new Thread(dataProcessForThread);
			Thread SaveData = new Thread(dataSaveForThread);
			group.IsBackground = true;
			draw.IsBackground = true;
			SaveData.IsBackground = true;
			group.Start();
			draw.Start();
			SaveData.Start();
			_canstartTest.Visibility = System.Windows.Visibility.Hidden;
		}

		private void SaveData(object state) {
			dataSaveForThread();
		}

		private void DrawData(object state) {
			dataProcessForThread();
		}

		private void GroupData(object state) {
			groupData();
		}
		BLL.TransformerService servicTrans = new BLL.TransformerService();
	


		void btnCancelStart_Click(object sender, RoutedEventArgs e) {
			_canstartTest.Visibility = System.Windows.Visibility.Hidden;
		}

		#endregion

		#region FFT
		private void btnFFTchange_Click(object sender, RoutedEventArgs e) {
			//_testDataPath = @"F:\dahe\GZDL411C#\HZGZDL.YZFJKGZXFXY.UI\HZGZDL.YZFJKGZXFXY.UI\TestData\2017\9135533";
			//for (int i = 0; i < MyFileHelper.getLength(_testDataPath + "\\原始测试数据.bin"); i += 964) {
			//	byte[] x = MyFileHelper.OpenFile(_testDataPath + "\\原始测试数据.bin", 964, i);
			//	group(x);
			//}
		}
		#endregion

		#region 停止测试
		#region 停止测量 界面逻辑
		void MeasureReSet() {
			btnStop.IsEnabled = false;
			btnStartTest.IsEnabled = true;
			is_Measuring = false;
			is_ContinueProcessData = false;
			UDPSever.IsDataReceive = false;
			IsMesureIng = false;
		}
		#endregion
		private void btnStop_Click(object sender, RoutedEventArgs e) {

			is_ContinueProcessData = false;
			UDPSever.Send(Model.Commander.StopCode);
		}
		

		#endregion

		#region 打开文件
		private void btnOpenFile_Click(object sender, RoutedEventArgs e) {
			openDictortyWindow openDic = new openDictortyWindow();
			openDic.Show();
		}
		#endregion

		#region 窗口加载  
		private void Window_Loaded(object sender, RoutedEventArgs e) {
			#region 颜色初始化
			ColorList.Add(Color.FromRgb(0xff, 0x75, 0));
			ColorList.Add(Color.FromRgb(0x1b, 0xd1, 0xa5));
			ColorList.Add(Color.FromRgb(0xff, 0x33, 0));

			ColorList.Add(Color.FromRgb(0xff, 0x75, 0));
			ColorList.Add(Color.FromRgb(0x1b, 0xd1, 0xa5));
			ColorList.Add(Color.FromRgb(0xff, 0x33, 0));
			//服务器数据颜色
			for (int i = 0; i < AxisCount; i++) {
				ColorList.Add(Colors.DeepSkyBlue);
			}
			BrushList.Add(new SolidColorBrush(ColorList[0]));
			BrushList.Add(new SolidColorBrush(ColorList[1]));
			BrushList.Add(new SolidColorBrush(ColorList[2]));
			BrushList.Add(new SolidColorBrush(ColorList[3]));
			BrushList.Add(new SolidColorBrush(ColorList[4]));
			BrushList.Add(new SolidColorBrush(ColorList[5]));

			lab1.Foreground = BrushList[0];
			lab2.Foreground = BrushList[1];
			lab3.Foreground = BrushList[2];
			lab4.Foreground = BrushList[3];
			lab5.Foreground = BrushList[4];
			lab6.Foreground = BrushList[5];
			#endregion

			#region Chart初始化
			initTeeChart(Chart);//表格初始化
			initTeeChart(ChartAD);//表格初始化
			initTeeChart(ChartFFT);//表格初始化
			#endregion

			#region 消息监听开启
			showSysMsg = new Thread(showSysMsg_fun);
			showSysMsg.IsBackground = true;
			showSysMsg.Start();
			ThreadPool.QueueUserWorkItem(new WaitCallback(waitCallBack));
			#endregion
			myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + "系统初始化成功,消息监听开启成功!");
			#region  Canvas 事件注册
			SetCanvesAutoOut_Left_SreenWhenMouseEnter(myCanvas);
			SetCanvesAutoEnter_Left_SreenWhenMouseLeave(myCanvas, 20);
			_canstartTest.IsVisibleChanged+=_canstartTest_IsVisibleChanged;
			#endregion

			#region  checkBox事件注册
			cb_ShowCursor.Click += (RoutedEventHandler)delegate {
				CurorV.Active = (bool)cb_ShowCursor.IsChecked;
			};
			cb_followMouse.Click += (RoutedEventHandler)delegate {
				CurorV.FollowMouse = (bool)cb_followMouse.IsChecked;
			};
			cb_CurrentChannel_1.Click += cb_CurrentChannel_1_Checked;
			cb_CurrentChannel_2.Click += cb_CurrentChannel_1_Checked;
			cb_CurrentChannel_3.Click += cb_CurrentChannel_1_Checked;
			cb_WaveChannel_1.Click += cb_CurrentChannel_1_Checked;
			cb_WaveChannel_2.Click += cb_CurrentChannel_1_Checked;
			cb_WaveChannel_3.Click += cb_CurrentChannel_1_Checked;
			#endregion
			
			#region Cursor事件注册
			StringBuilder sbCurrent = new StringBuilder();
			StringBuilder sbWave = new StringBuilder();
			CurorV.Change += (Steema.TeeChart.WPF.Tools.CursorChangeEventHandler)delegate {
				if (!is_Measuring) {
					try {
						lab1.Content = "电流[通道1]:" + LineList[0].YValues[(int)(CurorV.XValue * _current10sCount*0.1)].ToString("0.##")+"A";
						lab2.Content = "电流[通道2]:" + LineList[1].YValues[(int)(CurorV.XValue * _current10sCount * 0.1)].ToString("0.##") + "A";
						lab3.Content = "电流[通道3]:" + LineList[2].YValues[(int)(CurorV.XValue * _current10sCount * 0.1)].ToString("0.##") + "A";
						lab4.Content = "振动[通道1]:" + LineList[3].YValues[(int)(CurorV.XValue * _wave10sCount * 0.1)].ToString("0.##") + "G";
						lab5.Content = "振动[通道2]:" + LineList[4].YValues[(int)(CurorV.XValue * _wave10sCount * 0.1)].ToString("0.##") + "G";
						lab6.Content = "振动[通道3]:" + LineList[5].YValues[(int)(CurorV.XValue * _wave10sCount * 0.1)].ToString("0.##") + "G";
					}
					catch {

					}
				
				}
			};
			#endregion

			#region Button点击事件注册
				btnConnectDevice.Click += btnConnectDevice_Click;
				btnOpenFile.Click +=btnOpenFile_Click;
				btnStartTest.Click +=btnStartTest_Click;
				btnStop.Click+=btnStop_Click;
				btnCancelStart.Click += btnCancelStart_Click;
				btnConfrimStart.Click += btnConfrimStart_Click;
				btnAddNew.Click += btnAddNew_Click;
				btnDelete.Click += btnDelete_Click;
				btnLocalTestData.Click += btnLocalTestData_Click;
				btnSeverTestData.Click += btnSeverTestData_Click;
				btnLeftMove.Click += btnLeftMove_Click;
				btnRightMove.Click += btnRightMove_Click;
				btnEnlarge.Click += btnEnlarge_Click;
				btnNarrow.Click += btnNarrow_Click;
				btnShowFFT.Checked += btnShowFFT_Checked;
				btnShowFFT.Unchecked += btnShowFFT_Unchecked;
				btnShowAD.Checked += btnShowAD_Checked;
				btnShowAD.Unchecked += btnShowAD_Unchecked;
			#endregion

			#region TreeView部分
				myParentTree.SelectedItemChanged += myParentTree_SelectedItemChanged;
			myParentTree.MouseDoubleClick +=myParentTree_MouseDoubleClick;
			#endregion

		
			#region 系统设置初始化

			if (systemSettingWindow == null) {
				if (myTransformerInfo != null) {
					myParameterInfo.ID = myTransformerInfo.ID;
				}
				else {
					myParameterInfo.ID = 1;
				}
				systemSettingWindow = new ParameterWindow(myParameterInfo);
				systemSettingWindow.Owner = this;
			}
			#endregion

			#region X坐标初始化
			for (int i = 0; i < 1000000; i++) {
				Wave_X[i] = i * 0.00001;
			}
			for (int i = 0; i < 500; i++) {
				Current_X[i] = i * 0.02;
			}
			#endregion

			AutoUpdateViewTree();
			
			#region 检查连接Timer初始化
			Is_ConnectSuccess = false;
			checkConnextTimer.Interval = TimeSpan.FromMilliseconds(1000);
			checkConnextTimer.Tag = 5;
			checkConnextTimer.Tick += (EventHandler)delegate {
				int flag = (int)checkConnextTimer.Tag;
				checkConnextTimer.Tag = flag - 1;
				if (Is_ConnectSuccess) {
					connectionWindow.lab_ShowTime.Visibility = System.Windows.Visibility.Hidden;
					checkConnextTimer.Tag = 5;
					checkConnextTimer.Stop();
				}
				if (flag == 0) {
					//5秒内 没有应答  判断为失联
					
					this.Dispatcher.Invoke(new Action(delegate {
						
						btnConnectDevice.IsEnabled = true;
						
						if (btnStop.IsEnabled) {
							//正在测试时失联失联  停止测试
							this.Dispatcher.Invoke(new Action(delegate {
								MeasureReSet();
								myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|时间=" + DateTime.Now + "::行号=" + GetLineNum() + "::Msg=" + "测试已停止");
							}));
							_currentByteConcurrentQueue1 = new ConcurrentQueue<byte[]>();
							_currentByteConcurrentQueue2 = new ConcurrentQueue<byte[]>();
							_currentByteConcurrentQueue3 = new ConcurrentQueue<byte[]>();
							_currentDataConcurrentQueue1_Array = new ConcurrentQueue<double[]>();
							_currentDataConcurrentQueue2_Array = new ConcurrentQueue<double[]>();
							_currentDataConcurrentQueue3_Array = new ConcurrentQueue<double[]>();
							_waveByteConcurrentQueue1 = new ConcurrentQueue<byte[]>();
							_waveByteConcurrentQueue2 = new ConcurrentQueue<byte[]>();
							_waveByteConcurrentQueue3 = new ConcurrentQueue<byte[]>();
							_waveDataConcurrentQueue1_Array = new ConcurrentQueue<double[]>();
							_waveDataConcurrentQueue2_Array = new ConcurrentQueue<double[]>();
							_waveDataConcurrentQueue3_Array = new ConcurrentQueue<double[]>();
						}
						btnStartTest.IsEnabled = false;
						//失联 处理: 弹出连接框
						if (connectionWindow == null) {
							connectionWindow = new ConnectionWindow();
							connectionWindow.Owner = this;
							connectionWindow.btnConfirmConnection.Click += connectionConfim;
							connectionWindow.lab_ShowTime.Content = "仪器失联!";
							connectionWindow.lab_ShowTime.Visibility = System.Windows.Visibility.Visible;
							connectionWindow.Show();
						}
						else {
							connectionWindow.lab_ShowTime.Content = "仪器失联!";
							connectionWindow.btnOneKeyChangeLocalIPadress.IsEnabled = false;
							connectionWindow.lab_ShowTime.Visibility = System.Windows.Visibility.Visible;
							connectionWindow.Activate();
							connectionWindow.Show();
						}
					}));
					checkConnextTimer.Stop();
				}
			};
			#endregion

			myProgressBar.Visibility = System.Windows.Visibility.Hidden;
			_canstartTest.Visibility = System.Windows.Visibility.Hidden;
		}

		private void waitCallBack(object state) {
			while (true) {
				this.Chart.Dispatcher.Invoke(new Action(delegate {
					if (Is_ConnectSuccess && IsMesureIng==false) {
						UDPSever.Send(Model.Commander.ConnectCode);
						Is_ConnectSuccess = false;
						checkConnextTimer.Start();
					}
				}));
				Thread.Sleep(5000);
			}
		}
		

	
		

		private void myParentTree_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
		
		}

		void myParentTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (myParentTree.SelectedItem == null) {
				return;
			}
			Common.myTreeViewItem selectedItem = myParentTree.SelectedItem as Common.myTreeViewItem;
			selectedItem.IsExpanded = true;
			if (selectedItem.TabIndex == 5) {
				//var testTimeItem = (selectedItem.Parent as Common.myTreeViewItem);
				var dateItem = (selectedItem.Parent as Common.myTreeViewItem);
				var accPosItem = (dateItem.Parent as Common.myTreeViewItem);
				var transNameItem = (accPosItem.Parent as Common.myTreeViewItem);
				var companyNameItem = (transNameItem.Parent as Common.myTreeViewItem);
				currentItem = dateItem;
				currentSelectItem = selectedItem;
				string path = RootPath + "TestData\\" + companyNameItem.HeaderText + "\\" + transNameItem.HeaderText + "\\" + accPosItem.HeaderText + "\\" + dateItem.HeaderText + "\\" + selectedItem.HeaderText + "\\原始数据.bin";
				Show原始数据(path);
				
			}
		}
		Common.myTreeViewItem currentItem = null;
		Common.myTreeViewItem currentSelectItem = null;
		
		#endregion

		#region AD  FFT 切换
		void setADline(double max, double min) {
			myFFTCanvas.Visibility = System.Windows.Visibility.Hidden;
			myChartCanvas.Visibility = System.Windows.Visibility.Visible;
			LineList[0].Title = "电流[通道1]";
			LineList[1].Title = "电流[通道2]";
			LineList[2].Title = "电流[通道3]";
			LineList[3].Title = "振动[通道1]";
			LineList[4].Title = "振动[通道2]";
			LineList[5].Title = "振动[通道3]";
			AddDataToTheFirstAxes(LineList[0], AxisList[1]);
			AddDataToTheFirstAxes(LineList[1], AxisList[1]);
			AddDataToTheFirstAxes(LineList[2], AxisList[1]);
			AddDataToTheFirstAxes(LineList[3], AxisList[0]);
			AddDataToTheFirstAxes(LineList[4], AxisList[0]);
			AddDataToTheFirstAxes(LineList[5], AxisList[0]);
			reflushChart_current(max, min, Wave_X, CurrentAD_1_Y, LineList[0]);
			reflushChart_current(max, min, Wave_X, CurrentAD_2_Y, LineList[1]);
			reflushChart_current(max, min, Wave_X, CurrentAD_3_Y, LineList[2]);
			reflushChart_wave(max, min, Wave_X, Wave_1_Y, LineList[3]);
			reflushChart_wave(max, min, Wave_X, Wave_2_Y, LineList[4]);
			reflushChart_wave(max, min, Wave_X, Wave_3_Y, LineList[5]);
		}
		void setFFTline(double max, double min) {
			ChartAD.Axes.Bottom.SetMinMax(max, min);
			ChartFFT.Axes.Bottom.SetMinMax(0, 50000);
			myFFTCanvas.Visibility = System.Windows.Visibility.Visible;
			myChartCanvas.Visibility = System.Windows.Visibility.Hidden;

			reflushChart_wave(max, min, Wave_X, CurrentAD_1_Y, LineList[6]);
			reflushChart_wave(max, min, Wave_X, CurrentAD_2_Y, LineList[7]);
			reflushChart_wave(max, min, Wave_X, CurrentAD_3_Y, LineList[8]);
			reflushChart_wave(max, min, Wave_X, Wave_1_Y, LineList[9]);
			reflushChart_wave(max, min, Wave_X, Wave_2_Y, LineList[10]);
			reflushChart_wave(max, min, Wave_X, Wave_3_Y, LineList[11]);

			reflushChart_FFTcurrent(max, min, CurrentComplex_channel_1, LineList[12]);
			reflushChart_FFTcurrent(max, min, CurrentComplex_channel_2, LineList[13]);
			reflushChart_FFTcurrent(max, min, CurrentComplex_channel_3, LineList[14]);
			reflushChart_FFTwave(max, min, WaveComplex_channel_1, LineList[15]);
			reflushChart_FFTwave(max, min, WaveComplex_channel_2, LineList[16]);
			reflushChart_FFTwave(max, min, WaveComplex_channel_3, LineList[17]);

		}

		void btnShowAD_Checked(object sender, RoutedEventArgs e) {
			(sender as RadioButton).Foreground = Brushes.Orange;
			setADline(10, 0);
		}

		void btnShowFFT_Checked(object sender, RoutedEventArgs e) {
			(sender as RadioButton).Foreground = Brushes.Orange;
			setFFTline(10, 0);
		}

		void btnShowAD_Unchecked(object sender, RoutedEventArgs e) {
			(sender as RadioButton).Foreground = Brushes.Wheat;
		}

		void btnShowFFT_Unchecked(object sender, RoutedEventArgs e) {
			(sender as RadioButton).Foreground = Brushes.Wheat;
		}

		private void _canstartTest_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {

		}

		double[] WaveComplex_channel_1 = new double[1000000];
		double[] WaveComplex_channel_2 = new double[1000000];
		double[] WaveComplex_channel_3 = new double[1000000];
		double[] CurrentComplex_channel_1 = new double[1000000];
		double[] CurrentComplex_channel_2 = new double[1000000];
		double[] CurrentComplex_channel_3 = new double[1000000];
		#endregion

		#region Tree选择更换

		#endregion

		#region 显示通道
		void cb_CurrentChannel_1_Checked(object sender, RoutedEventArgs e) {
			var cbx = sender as CheckBox;
			LineList[cbx.TabIndex].Active = (bool)cbx.IsChecked;
			if (btnShowFFT.IsChecked == true) {
				LineList[cbx.TabIndex + 6].Active = (bool)cbx.IsChecked;
				LineList[cbx.TabIndex + 12].Active = (bool)cbx.IsChecked;
			}
			else {
				for (int i = 6; i < 12; i++) {
					LineList[i].Active = false;
				}
			}
		}
		#endregion

		#region 消息对列处理
		private DispatcherTimer checkConnextTimer = new DispatcherTimer();
		void showSysMsg_fun() {
			while (true) {
				//如果有消息 就处理消息
				if (myMessageConcurrentQueue.Count > 0) {
					string temp ;
					myMessageConcurrentQueue.TryDequeue(out temp);
					string Level = temp.Split('|')[0];
					string Msg = temp.Split('|')[1];
					if(Level == "0")
					{
						Common.MyFileHelper.SaveFile_Append(RootPath + "SystemLog.txt", "Level=Waring::" + Msg+"\r\n", 1024);
					}
					if (Level == "1") {
						Common.MyFileHelper.SaveFile_Append(RootPath + "SystemLog.txt", "Level=Info::" + Msg + "\r\n", 1024);
					}
					if (Level == "2") {
						Common.MyFileHelper.SaveFile_Append(RootPath + "SystemLog.txt", "Level=Error::" + Msg + "\r\n", 1024);
					}
					
					if (int.Parse(Level) >= (int)(LogShowLevel)) {
						this.Dispatcher.Invoke(new Action(delegate {
							if (int.Parse(Level) == (int)(Model.SystemMsgLevel.ERROR)) {
								if (UDPSever != null) {
									UDPSever.Send(Model.Commander.StopCode);
									MeasureReSet();
								}
								this.label_Debug.Foreground = Brushes.Red;
								this.label_Debug.Content = Msg;
							}
							else {
								this.label_Debug.Foreground = Brushes.White;
							}
						}));
					}
					
				}
				else {
					this.Dispatcher.Invoke(new Action(delegate {
						this.label_Debug.Content = "";
					}));
				}
				//如果TcpSate发生改变就显示改变

				if (UDPSever != null && UDPSever.State.Message.Count > 0) {
					string Msg = UDPSever.State.Message.Dequeue();
					this.Dispatcher.Invoke(new Action(delegate {
						if (UDPSever.State.Is_Health == false) {
							if (UDPSever != null) {
								UDPSever.Send(Model.Commander.StopCode);
								MeasureReSet();
							}
							btnConnectDevice.IsEnabled = true;
							btnStartTest.IsEnabled = false;
							btnStop.IsEnabled = false;
							connectionWindow.Activate();
							connectionWindow.Show();
							this.label_Debug.Foreground = Brushes.Red;
							this.label_Debug.Content = Msg;
							MessageBox.Show(Msg);
						}
						else {
							this.label_Debug.Foreground = Brushes.White;
						}
					}));
				}
				else {
					this.Dispatcher.Invoke(new Action(delegate {
						this.label_Debug.Content = "";
					}));
				}

				if (voltConcurrentQueue.Count > 0) {
					this.Chart.Dispatcher.Invoke(new Action(delegate {
						string s;
						voltConcurrentQueue.TryDequeue(out s);
						lablVoltge.Content = s;
					}));
				}
				Thread.Sleep(2000);
			}
		}
		
		#endregion

		#region Chart新增 删除

		private void btnAddNew_Click(object sender, RoutedEventArgs e) {
			//Common.TreeViewHelper.TreeViewUpdateLocal(myParentTree, 6, new string[] { "国洲电力", "测试变压器", DateTime.Now.ToString(), "分接位", "第一次测试" });
		}
		private void btnDelete_Click(object sender, RoutedEventArgs e) {
			myTreeViewItem item = (myTreeViewItem)myParentTree.SelectedItem;
			if (item == null) {
				myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.ERROR +"|时间="+DateTime.Now+"::行号=" + GetLineNum() + "::Msg="+"您没有选择需要删除的内容!");
				return;
			}
			else {
				var _itemparent = item.Parent as myTreeViewItem;
				if (_itemparent == null) {
					myParentTree.Items.Remove(item);
				}
				else {
					_itemparent.Items.Remove(item);
				}
				myMessageConcurrentQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|时间="+DateTime.Now+"::行号=" + GetLineNum() + "::Msg="+"删除成功!");
			}
		}
		#endregion

		#region 波形 放大 缩小  左移 右移
		bool is_Enlarge = true;
		bool is_Narrow = false;
		double Max = 10;
		double min = 0;
		void btnNarrow_Click(object sender, RoutedEventArgs e) {
			if (is_Narrow) {
				is_Narrow = false;
				is_Enlarge = true;
			}

		
			if (btnShowAD.IsChecked == true) {
				double offset = Math.Round((Chart.Chart.Axes.Bottom.Maximum - Chart.Chart.Axes.Bottom.Minimum) / 2, 4);
				if (offset >= 5) {
					offset = 5;
					Chart.Axes.Bottom.Increment = 1;
					return;
				}
				Chart.Chart.Axes.Bottom.SetMinMax(Chart.Chart.Axes.Bottom.Minimum - offset, Chart.Chart.Axes.Bottom.Maximum + offset);
				if (Chart.Chart.Axes.Bottom.Minimum - offset < 0) {
					Chart.Chart.Axes.Bottom.SetMinMax(0, Chart.Chart.Axes.Bottom.Maximum);
				}
				if (Chart.Chart.Axes.Bottom.Maximum + offset > 10) {
					Chart.Chart.Axes.Bottom.SetMinMax(Chart.Chart.Axes.Bottom.Minimum, 10);
				}
				double MAXChart = Chart.Axes.Bottom.Maximum;
				double MINChart = Chart.Axes.Bottom.Minimum;
				setADline(MAXChart, MINChart);
			}
			if (btnShowFFT.IsChecked == true) {

				double offset = Math.Round((ChartAD.Chart.Axes.Bottom.Maximum - ChartAD.Chart.Axes.Bottom.Minimum) / 2, 4);
				if (offset >= 5) {
					offset = 5;
					ChartAD.Axes.Bottom.Increment = 1;
					return;
				}
				ChartAD.Chart.Axes.Bottom.SetMinMax(ChartAD.Chart.Axes.Bottom.Minimum - offset, ChartAD.Chart.Axes.Bottom.Maximum + offset);
				if (ChartAD.Chart.Axes.Bottom.Minimum - offset < 0) {
					ChartAD.Chart.Axes.Bottom.SetMinMax(0, ChartAD.Chart.Axes.Bottom.Maximum);
				}
				if (ChartAD.Chart.Axes.Bottom.Maximum + offset > 10) {
					ChartAD.Chart.Axes.Bottom.SetMinMax(ChartAD.Chart.Axes.Bottom.Minimum, 10);
				}
				double MAXChart = ChartAD.Axes.Bottom.Maximum;
				double MINChart = ChartAD.Axes.Bottom.Minimum;
				setFFTline(MAXChart, MINChart);
			}
		}

		void btnEnlarge_Click(object sender, RoutedEventArgs e) {
			if (is_Enlarge) {
				is_Enlarge = false;
				is_Narrow = true;
			}
		
			if (btnShowAD.IsChecked == true) {
				Chart.Axes.Bottom.Increment = 0.000001;
				double offset = Math.Round((Chart.Chart.Axes.Bottom.Maximum - Chart.Chart.Axes.Bottom.Minimum) / 5, 4);
				lablVoltge.Content = offset;
				if (offset <= 0.001) {
					return;
				}
				Chart.Chart.Axes.Bottom.SetMinMax(Chart.Chart.Axes.Bottom.Minimum + offset, Chart.Chart.Axes.Bottom.Maximum - offset);
				double MAXChart = Chart.Axes.Bottom.Maximum;
				double MINChart = Chart.Axes.Bottom.Minimum;
				setADline(MAXChart, MINChart);
			}
			if (btnShowFFT.IsChecked == true) {
				ChartAD.Axes.Bottom.Increment = 0.000001;
				double offset = Math.Round((ChartAD.Chart.Axes.Bottom.Maximum - ChartAD.Chart.Axes.Bottom.Minimum) / 5, 4);
				lablVoltge.Content = offset;
				if (offset <= 0.001) {
					return;
				}
				ChartAD.Chart.Axes.Bottom.SetMinMax(ChartAD.Chart.Axes.Bottom.Minimum + offset, ChartAD.Chart.Axes.Bottom.Maximum - offset);
				double MAXChart = ChartAD.Axes.Bottom.Maximum;
				double MINChart = ChartAD.Axes.Bottom.Minimum;
				setFFTline(MAXChart, MINChart);
			}
			
		}

		void btnRightMove_Click(object sender, RoutedEventArgs e) {
			


		
			if (btnShowAD.IsChecked == true) {
				double step = Chart.Axes.Bottom.Maximum / 10;
				Chart.Chart.Axes.Bottom.Maximum += step;
				if (Chart.Chart.Axes.Bottom.Maximum >= Max) {
					Chart.Chart.Axes.Bottom.Maximum = Max;
					return;
				}
				Chart.Chart.Axes.Bottom.Minimum += step;
				double MAXChart = Chart.Axes.Bottom.Maximum;
				double MINChart = Chart.Axes.Bottom.Minimum;
				setADline(MAXChart, MINChart);
			}
			if (btnShowFFT.IsChecked == true) {
				double step = ChartAD.Axes.Bottom.Maximum / 10;
				ChartAD.Chart.Axes.Bottom.Maximum += step;
				if (ChartAD.Chart.Axes.Bottom.Maximum >= Max) {
					ChartAD.Chart.Axes.Bottom.Maximum = Max;
					return;
				}
				ChartAD.Chart.Axes.Bottom.Minimum += step;
				double MAXChart = ChartAD.Axes.Bottom.Maximum;
				double MINChart = ChartAD.Axes.Bottom.Minimum;
				setFFTline(MAXChart, MINChart);
			}
		}

		void btnLeftMove_Click(object sender, RoutedEventArgs e) {
		
			if (btnShowAD.IsChecked == true) {
				double step = Chart.Axes.Bottom.Maximum / 10;

				Chart.Chart.Axes.Bottom.Minimum -= step;

				if (Chart.Chart.Axes.Bottom.Minimum <= min) {
					Chart.Chart.Axes.Bottom.Minimum = min;
					return;
				}
				Chart.Chart.Axes.Bottom.Maximum -= step;
				double MAXChart = Chart.Axes.Bottom.Maximum;
				double MINChart = Chart.Axes.Bottom.Minimum;
				setADline(MAXChart, MINChart);
			}
			if (btnShowFFT.IsChecked == true) {
				double step = ChartAD.Axes.Bottom.Maximum / 10;

				ChartAD.Chart.Axes.Bottom.Minimum -= step;

				if (ChartAD.Chart.Axes.Bottom.Minimum <= min) {
					ChartAD.Chart.Axes.Bottom.Minimum = min;
					return;
				}
				ChartAD.Chart.Axes.Bottom.Maximum -= step;
				double MAXChart = ChartAD.Axes.Bottom.Maximum;
				double MINChart = ChartAD.Axes.Bottom.Minimum;
				setFFTline(MAXChart, MINChart);
			}
		}
		#endregion

		#region 遍历窗体控件
		private void SetNotEditable(DependencyObject element) {
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
				var child = VisualTreeHelper.GetChild(element, i);
				if (child is Button) {
					Button btn = child as Button;
					btn.MouseEnter += (MouseEventHandler)delegate {
						// btn.Background = new SolidColorBrush(Color.FromRgb(255, 255, 210));
						btn.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0));
						btn.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
					};
					btn.MouseLeave += (MouseEventHandler)delegate {
						// btn.Background = new SolidColorBrush(Color.FromRgb(0xc4, 0xf2, 0xff));
						btn.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
						// btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0xc4, 0xf2, 0xff));
					};
					MessageBox.Show(btn.Name);
				}

				if (child == null) {
					continue;
				}
				else if (child is Grid) {
					this.SetNotEditable(child);
				}
				else if (child is StackPanel) {
					this.SetNotEditable(child);
				}
				else if (child is GroupBox) {
					this.SetNotEditable(child);
				}
				else if (child is DockPanel) {
					this.SetNotEditable(child);
				}
				else if (child is ScrollViewer) {
					this.SetNotEditable(child);
					//ScrollViewer不具有Children属性，无法对其进行遍历，但是具有Content属性，作为容器型控件，一般都可以通过这样的方法来解决。  
				}
			}
		}
		#endregion

		#region 上传和下载

		void btnSeverTestData_Click(object sender, RoutedEventArgs e) {
			myProgressBar.Visibility = System.Windows.Visibility.Visible;
			mySever.Upload(MyFileHelper.OpenFile_getPath(RootPath));
			mySever.progress = ProgressChange;
			mySever.complete = complete;
		}

		void btnLocalTestData_Click(object sender, RoutedEventArgs e) {
			myProgressBar.Visibility = System.Windows.Visibility.Visible;
			mySever.DownloadFile();
			mySever.progress = ProgressChange;
			mySever.complete = complete;
		}
		#endregion

		#region 进度条和 完成
		void ProgressChange(long currentlen, long totalLen) {
			myProgressBar.Maximum = totalLen;
			myProgressBar.Minimum = 0;
			myProgressBar.Value = currentlen;
			myProgressLabel.Content = "进度:" + (((currentlen + 0.1) / totalLen) * 100).ToString("0.##") + "%";
		}
		void complete() {
			myProgressBar.Visibility = System.Windows.Visibility.Hidden;
			myProgressLabel.Content = "数据传输完成!";
		}
		#endregion

		#region 设置一些控件的位置及大小
		void SetContorlsPosition() {

			#region MyParentCanvas
			myParentCanvas.Width = this.ActualWidth-250;
			myParentCanvas.Height = this.ActualHeight - 180;
			#endregion

			#region Chart
			Chart.Width = myParentCanvas.Width - 30;
			Chart.Height = myParentCanvas.Height;
			//设置表格left 范围 为了 自定义坐标的正常显示


			ChartAD.Width = myParentCanvas.Width - 25;
		
			ChartAD.Margin = new Thickness(0, 5, 5, 0);
			ChartFFT.Width = myParentCanvas.Width - 25;

			ChartFFT.Margin = new Thickness(0, 0, 5, 5);
			myFFTgrid.Height = myParentCanvas.Height;
			#endregion
			#region myFFTcanvas
			myChartCanvas.Width = myParentCanvas.Width - 30;
			myChartCanvas.Height = myParentCanvas.Height;
			myChartCanvas.Margin = new Thickness(30, 0, 0, 0);
			myFFTCanvas.Width = myParentCanvas.Width - 30;
			myFFTCanvas.Height = myParentCanvas.Height;
			myFFTCanvas.Margin = new Thickness(30, 0, 0, 0);
			#endregion
			#region  myCanvas
			this.myCanvas.Width = 230;
			this.myCanvas.Height = Chart.Height;
			this.myCanvas.Background = Brushes.Transparent;
			this.myCanvas.Opacity = 1;
			Canvas.SetLeft(myCanvas, -myCanvas.Width + 20);
			#endregion

			#region myParentTree
			myParentTree.Background = Brushes.PowderBlue;
			myParentTree.Height = myCanvas.Height - 70;
			myParentTree.Width = myCanvas.Width - 20;
			myParentTree.Margin = new Thickness(0, 30, 0, 40);
			#endregion

			#region myTreeViewStackPanel
			myTreeViewStackpanel.Background = Brushes.LightGray;
			myTreeViewStackpanel.Height = 30;
			myTreeViewStackpanel.Width = myCanvas.Width-20;
			myTreeViewStackpanel.Margin = new Thickness(0,0,0,0);
			#endregion

			//设置按钮位置
			btnAddNew.Margin = new Thickness(130, (myCanvas.Height - 35), 0, 0);
			btnDelete.Margin = new Thickness(0, (myCanvas.Height - 35),0,0);
			txt_ShowOrHide.Width = 20;
			txt_ShowOrHide.Height = 160;
			txt_ShowOrHide.Margin = new Thickness(230 - 20, (myCanvas.Height - txt_ShowOrHide.Height) / 2, 0, (myCanvas.Height - txt_ShowOrHide.Height) / 2);
		}
		#endregion

		#region 窗口尺寸变化
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
			SetContorlsPosition();
		}
		#endregion

		#region 窗口鼠标移动
		bool is_init = true;
		private void Window_MouseMove(object sender, MouseEventArgs e) {
			if (is_init) {
				is_init = false;
				SetContorlsPosition();
			}

		}
		#endregion

		#region 获取行号
		public static int GetLineNum() {
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
			return st.GetFrame(0).GetFileLineNumber();
		}
		#endregion

		#region Canvas
		#region 设置鼠标移动Canvas
		void SetCanvasFollowMouse(Canvas temp) {
			temp.MouseMove += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
				if (mouse.LeftButton == MouseButtonState.Pressed) {
					Canvas.SetTop(temp, mouse.GetPosition(myParentCanvas).Y - temp.Height / 2);
					Canvas.SetLeft(temp, mouse.GetPosition(myParentCanvas).X - temp.Width / 2);
				}
			};

		}
		#endregion

		#region 设置Canves自动进入左边屏幕
		void SetCanvesAutoEnter_Left_SreenWhenMouseLeave(Canvas targetCanves, int outWidth) {
			targetCanves.MouseLeave += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
				Canvas temp = send as Canvas;
				double currentLeft = Canvas.GetLeft(temp);
				if (currentLeft < outWidth) {
					while (currentLeft >= -temp.Width + outWidth + 10) {
						Canvas.SetLeft(temp, currentLeft - 10);
						currentLeft -= 10;
					}
				}
			};
		}
		#endregion

		#region 设置Canves自动从左边屏幕显示出来
		void SetCanvesAutoOut_Left_SreenWhenMouseEnter(Canvas targetCanves) {
			targetCanves.MouseEnter += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
				Canvas temp = send as Canvas;
				double currentLeft = Canvas.GetLeft(temp);
				if (currentLeft < 0) {
					while (currentLeft < 0) {
						Canvas.SetLeft(temp, currentLeft + 10);
						currentLeft += 10;
					}
				}
			};
		}
		#endregion

		#region 设置Canves自动进入右边屏幕
		void SetCanvesAutoEnter_Right_SreenWhenMouseLeave(Canvas targetCanves, int outWidth) {
			targetCanves.MouseLeave += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
				Canvas temp = send as Canvas;
				double currentRight = myParentCanvas.Width - Canvas.GetLeft(temp) - temp.Width;
				if (currentRight < outWidth) {
					while (currentRight >= -temp.Width + outWidth + 10) {
						Canvas.SetRight(temp, currentRight - 10);
						currentRight -= 10;
					}
				}
			};
		}
		#endregion

		#region 设置Canves自动从右边屏幕显示出来
		void SetCanvesAutoOut_Right_SreenWhenMouseEnter(Canvas targetCanves) {
			targetCanves.MouseEnter += (MouseEventHandler)delegate(object send, MouseEventArgs mouse) {
				Canvas temp = send as Canvas;
				double currentRight = myParentCanvas.Width - Canvas.GetLeft(temp) - temp.Width;
				if (currentRight < 0) {
					while (currentRight < 0) {
						Canvas.SetRight(temp, currentRight + 10);
						currentRight += 10;
					}
				}
			};
		}
		#endregion
		#endregion
		
		#region 压缩
		Common.AccessDbSetHelper<Model.TransformerInfo> accessdb = new Common.AccessDbSetHelper<Model.TransformerInfo>();
		private void btnCompress_Click(object sender, RoutedEventArgs e) {
			string path = MyFileHelper.OpenDirectory(RootPath);
			mySever.CompressZipFile(path + ".zip", path);
			mySever.UnZipFile(path + ".zip", @"G:\");
			//MyFileHelper.OpenFile_getPath(RootPath);
		}

		#endregion

		#region 系统设置
		ParameterWindow systemSettingWindow;
		Model.ParameterInfo myTestSettingInfo = new Model.ParameterInfo();
		private void btnTestSetting_Click(object sender, RoutedEventArgs e) {
			if (systemSettingWindow == null) {
				if (myTransformerInfo != null) {
					myParameterInfo.ID = myTransformerInfo.ID;
				}
				else {
					myParameterInfo.ID = 1;
				}
				systemSettingWindow = new ParameterWindow( myParameterInfo);
				systemSettingWindow.Owner = this;
			}
			systemSettingWindow.Show();
		}
		#endregion

		#region 变压器设置
		TransFormerSetWindow TransformerInfoSetting;
		private void btnSetInfo_Click(object sender, RoutedEventArgs e) {
			if (TransformerInfoSetting == null) {
				TransformerInfoSetting = new TransFormerSetWindow(myTransformerInfo, setTreeView);
				TransformerInfoSetting.Owner = this;
			}
			TransformerInfoSetting.Show();
		}
		void setTreeView() {
			Common.TreeViewHelper.TreeViewUpdateLocal(myParentTree, 3, myTransformerInfo);
		}
		#endregion

		#region 窗口关闭
		private void Window_Closed(object sender, EventArgs e) {
			System.Windows.Application.Current.Shutdown(0);
			System.Environment.Exit(0);
		}
		#endregion

		private void Menu_LogShow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Common.CmdOperation.CmdOpenLog();
		}
	}
}
