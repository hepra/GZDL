using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
			System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<Model.myEFTransFormerInfo>());
			btnStartTest.IsEnabled = false;
			btnStop.IsEnabled = false;
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
		/// 绘图比例
		/// </summary>
		static  double RATE = 0.000001;
		double MUT = 1;

		/// <summary>
		/// 坐标轴分区个数
		/// </summary>
		static int AxisCount =6;

		/// <summary>
		/// 坐标轴区域间隔
		/// </summary>
		static int axis_offset = 2;

		/// <summary>
		/// 一个点所占字节数
		/// </summary>
		static	int DataByteCount = 2;

		/// <summary>
		///一个包的点的个数  80
		/// </summary>
		static int DataCount = 80;

		/// <summary>
		/// 测试是否开始
		/// </summary>
		bool is_Measuring = false;

		/// <summary>
		/// 接受的消息Buffer长度
		/// </summary>
		static int dataBuffSzie = 964;

		/// <summary>
		/// 显示时转换成int 的偷点数
		/// </summary>
		static int __偷点数 = 10;

		static double ___每次刷新时间s = 0.2;
		static int ___1S数据对应的包数 = 1250;
		static double ___每次转换List对应时间秒数 = 0.1;
		static int __每个包获取的点数 = DataCount / __偷点数;//8
		static int __平移每次 = __偷点数 * 12;//120
		static int __每次转换所需时间对应的包数 = (int)(___每次转换List对应时间秒数 * ___1S数据对应的包数);  //0.1s 对应125包
		static int ___转换所需时间的Count = __每次转换所需时间对应的包数 * __每个包获取的点数;  //  1000
	
		static  int ___刷新1s对应的点数 = 500;
		static int  ___刷新时取平均值的点Count = 20;
		static int ___每次刷新对应的点数 = (int)(___每次刷新时间s * ___刷新1s对应的点数);
		static double ___X每点刷新增量 = ___每次刷新时间s / ___每次刷新对应的点数;//0.002
		static int ___每次绘图需要的包数 = (int)(___每次刷新对应的点数 / ___每次转换List对应时间秒数 / ___刷新1s对应的点数);
		static int ___X轴最大值 = 10;

		int _电流频率 = 50;

		/// <summary>
		/// 将数据保存到文件
		/// </summary>
		bool Is_TimeToSaveFile = false;

		/// <summary>
		/// 刷新的时间  (秒)
		/// </summary>
		double Time = 0;

		
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

		/// <summary>
		/// 异步TCP服务器
		/// </summary>
		Common.TCPHelper.asyncTcpSever TcpSever;

		/// <summary>
		/// 异步TCP客户端
		/// </summary>
		Common.TCPHelper.asyncTcpClient TcpClient;

		Common.UDPHelper.asycUDPSever UDPSever;

		/// <summary>
		/// 自定义消息对列
		/// </summary>
		Queue<string> myMessageQueue = new Queue<string>();

		/// <summary>
		/// 是否继续处理数据
		/// </summary>
		bool is_ContinueProcessData = true;

		/// <summary>
		/// 接收队列
		/// </summary>
		Queue<byte[]> RecedData = new Queue<byte[]>();
		Queue<byte[]> SavetoFileQueue = new Queue<byte[]>();

		Queue<byte[]> _waveByteQueue1 = new Queue<byte[]>();
		Queue<byte[]> _waveByteQueue2 = new Queue<byte[]>();
		Queue<byte[]> _waveByteQueue3 = new Queue<byte[]>();
		Queue<byte[]> _currentByteQueue1 = new Queue<byte[]>();
		Queue<byte[]> _currentByteQueue2= new Queue<byte[]>();
		Queue<byte[]> _currentByteQueue3 = new Queue<byte[]>();
		Queue<double[]> _waveDataQueue1_Array = new Queue<double[]>();
		Queue<double[]> _waveDataQueue2_Array = new Queue<double[]>();
		Queue<double[]> _waveDataQueue3_Array = new Queue<double[]>();
		Queue<double[]> _currentDataQueue1_Array = new Queue<double[]>();
		Queue<double[]> _currentDataQueue2_Array = new Queue<double[]>();
		Queue<double[]> _currentDataQueue3_Array = new Queue<double[]>();

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
		private void initTeeChart() {

			Chart.Aspect.View3D = false;
			Chart.Legend.Visible = true;
			Chart.Header.Visible = false;
			Chart.Legend.LegendStyle = LegendStyles.Series;
			Steema.TeeChart.WPF.Drawing.ChartPen panelPen = new Steema.TeeChart.WPF.Drawing.ChartPen();
			panelPen.Color = Colors.AliceBlue;
			Chart.Panel.Pen = panelPen;
			Chart.Axes.Left.Inverted = false;
			Chart.Axes.Left.SetMinMax(0, 100);
			Chart.Walls.Visible = false;
			Chart.Axes.Top.Visible = false;
			Chart.Axes.Right.Visible = false;
			Steema.TeeChart.WPF.Axis.AxisLinePen pen = new Steema.TeeChart.WPF.Axis.AxisLinePen();
			pen.Color = Colors.OrangeRed;
			Chart.Axes.Left.PositionUnits = PositionUnits.Percent;
			Chart.Axes.Bottom.AxisPen = pen;
			Chart.Axes.Left.AxisPen = pen;
			Chart.Axes.Bottom.Grid.Visible = false;
			Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			Chart.IsManipulationEnabled = false;
			Chart.ReleaseMouseCapture();
			Chart.MouseMove += Chart_MouseMove;
			Chart.MouseDoubleClick += Chart_MouseDoubleClick;
			//Chart.Axes.Bottom.Increment = 0.000001;
			InitLine();
			InitAxes();
			for (int i = 0; i < AxisCount; i++) {
				if(i==0)LineList[0].Title = "电流[1]曲线";
				if (i == 1) LineList[1].Title = "电流[2]曲线";
				if (i == 2) LineList[2].Title = "电流[3]曲线";
				if (i == 3) LineList[3].Title = "振动[1]曲线";
				if (i == 4) LineList[4].Title = "振动[2]曲线";
				if (i == 5) LineList[5].Title = "振动[3]曲线";
			}
			for (int i = AxisCount; i < AxisCount * 2;i++)
			{
				LineList[i].Active = false;
			}
				InitCursor();
		}

		void Chart_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			Chart.Axes.Left.SetMinMax(0, 100);
			Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
		}

		void Chart_MouseMove(object sender, MouseEventArgs e) {
			if (e.RightButton == MouseButtonState.Pressed) {
				Chart.Axes.Left.SetMinMax(0, 100);
				Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			}
			if (e.LeftButton == MouseButtonState.Pressed) {
				Chart.Axes.Left.SetMinMax(0, 100);
				Chart.Axes.Bottom.SetMinMax(0, ___X轴最大值);
			}
		}

		private void InitLine() {
			for (int i = 0; i < AxisCount*2; i++) {
				FastLine line = new FastLine(Chart.Chart);
				Steema.TeeChart.WPF.Drawing.ChartPen pen = new Steema.TeeChart.WPF.Drawing.ChartPen();
				pen.Color = ColorList[i];
				pen.Width = 1;
				line.LinePen = pen;
				LineList.Add(line);
			}
		}


		private void InitAxes() {
			offset = Common.ChartInit.GetAxisPos(AxisCount);

			Chart.Axes.Left.StartPosition = 0;
			Chart.Axes.Left.EndPosition = Chart.Height;
			Chart.Axes.Left.StartEndPositionUnits = PositionUnits.Pixels;
			for (int i = 0; i < AxisCount; i++) {
				Axis temp = new Axis(Chart.Chart);
				Axis.AxisLinePen pen = new Axis.AxisLinePen();
				pen.Color = ColorList[i%6];
				pen.Width = 1;
				temp.AxisPen = pen;
				temp.Automatic = false;
				temp.SetMinMax(0, 200);
				temp.Grid.Visible = false;
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
			pen.Color = Colors.Black;
			pen.Width = 0.5;
			CurorV.Pen = pen;
			CurorV.FollowMouse = false;
			CurorV.Active = false;
		}

		private void AddDataToTheFirstAxes(FastLine line, Axis axis) {
			line.CustomVertAxis = axis;
		}


	     static	int count = 1500000;
		double[] x = new double[count];
		double[] y = new double[count];


		private void AddTestData() {
		for (int i = 0; i < AxisCount; i++) {
			LineList[i].YValues.Clear();
			LineList[i].XValues.Clear();
		}
		for (int j = 0; j < count; j ++) {
			x[j]=(j * 0.00001);
			y[j] = Math.Sin(((j%20000)*0.001)) * 100;
		}
		reflushChart(10, 0, x, y, LineList[0]);
		reflushChart(10, 0, x, y, LineList[1]);
		reflushChart(10, 0, x, y, LineList[2]);
		for (int i = 0; i < AxisCount; i++) {
			AddDataToTheFirstAxes(LineList[i], AxisList[i]);
		}
		}
		void Fuck() {
			
			int count = 1000;
			double[] x = new double[count];
			double[] y = new double[count];
			Time = 0;
			while (true) {
				if (Time == 10) {
					Time = 0;
					for (int i = 0; i < AxisCount; i++) {
						LineList[i].YValues.Clear();
						LineList[i].XValues.Clear();
					}
				}
				List<double> ArrayX = new List<double>();
				List<double> ArrayY = new List<double>();
				for (int j = 0; j < count; j += 10) {
					ArrayX.Add((j + count * Time) * 0.002);
					ArrayY.Add(Math.Sin((j + count * Time)) * 100);
				}
				this.Dispatcher.Invoke(new Action(delegate {
					
					for (int i = 0; i < AxisCount; i++) {
						LineList[i].Add(ArrayX.ToArray(), ArrayY.ToArray());
						AddDataToTheFirstAxes(LineList[i], AxisList[i]);
					}
					Time += 0.2;
					//getWorkingTime.Stop();
					//lb_123.Items.Add(getWorkingTime.Elapsed.TotalMilliseconds + " ");
					//lb_123.SelectedIndex = lb_123.Items.Count - 1;
					
				}));
				Thread.Sleep(200);
			}
			
		}
		#endregion

		#region 数据采集处理
		#region 数据接受
		private void tcpMsgRecvive(byte[] data_buffer) {
			//连接响应
			if (data_buffer[0] == 0 && data_buffer[1] == 0xff) {
				this.Dispatcher.Invoke(new Action(delegate {
					btnStartTest.IsEnabled = true;
					btnConnectDevice.IsEnabled = false;
					connectionWindow.Hide();
					myMessageQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|" + "下位机连接成功收到返回码:0X00FF00FF");
				}));
			}
			//停止测试响应
			if (data_buffer[0] == 0x05 && data_buffer[1] == 0x55) {
				this.Dispatcher.Invoke(new Action(delegate {
					MeasureReSet();
					myMessageQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|" + "测试已停止收到返回码:0X05550555");
				}));
				_currentByteQueue1.Clear();
				_currentByteQueue2.Clear();
				_currentByteQueue3.Clear();
				_currentDataQueue1_Array.Clear();
				_currentDataQueue2_Array.Clear();
				_currentDataQueue3_Array.Clear();
				_waveByteQueue1.Clear();
				_waveByteQueue2.Clear();
				_waveByteQueue3.Clear();
				_waveDataQueue1_Array.Clear();
				_waveDataQueue2_Array.Clear();
				_waveDataQueue3_Array.Clear();
			}
			//保存数据
			if (data_buffer[0] == 0x09 && data_buffer[1] == 0x09) {
				try {
					byte[] temp = new byte[dataBuffSzie];
					data_buffer.CopyTo(temp, 0);
					//保存到缓存 数据处理
					RecedData.Enqueue(temp);
					//保存到队列  写入文件
					SavetoFileQueue.Enqueue(temp);
					if (SavetoFileQueue.Count > 6250) {
						for (int i = 0; i < 3000; i++) {
							SavetoFileQueue.Dequeue();
						}
						
					}
					//获取下一包数据
					UDPSever.Send(Model.Commander.GetDataCode);

					PageCount++;

				}
				catch (Exception e) {
					myMessageQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|" + "行号:" + GetLineNum() + "错误信息:" + e.Message);
				}
			}
			if (data_buffer[0] == 0x50 && data_buffer[1] == 0x50) {
				double voltage = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, 2))/10;
				this.Dispatcher.Invoke(new Action(delegate {
					lablVoltge.Content = "当前测试电压:" + voltage;
				}));
			}
		}
		#endregion

		#region  数据分组
		private Mutex mutex = new Mutex();
		void groupData() {

			double[] temp__Array1 = new double[___转换所需时间的Count];
			double[] temp__Array2 = new double[___转换所需时间的Count];
			double[] temp__Array3 = new double[___转换所需时间的Count];
			double[] temp__Array4 = new double[___转换所需时间的Count];
			double[] temp__Array5 = new double[___转换所需时间的Count];
			double[] temp__Array6 = new double[___转换所需时间的Count];
			try {
				while (true) {
					if (!is_ContinueProcessData) {
						//break;
						return;
					}

					//分组一次 分 100ms  数据  偷点数
					if (RecedData.Count > __每次转换所需时间对应的包数 * 2) {
						//getWorkingTime.Restart();
						mutex.WaitOne();
						for (int pageindex = 0; pageindex < __每次转换所需时间对应的包数; pageindex++) {
							byte[] data_buffer = RecedData.Dequeue();
							for (int i = 4; i < data_buffer.Length; i += __平移每次) {
								temp__Array1[__每个包获取的点数 * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i))) / MUT;
								temp__Array2[__每个包获取的点数 * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 2))) / MUT;
								temp__Array3[__每个包获取的点数 * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 4))) / MUT;
								temp__Array4[__每个包获取的点数 * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 6))) / MUT;
								temp__Array5[__每个包获取的点数 * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 8))) / MUT;
								temp__Array6[__每个包获取的点数 * pageindex + i / __平移每次] = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 10))) / MUT;
							}
						}
						//20000个点 偷 200个点 为 20ms  数据
						_currentDataQueue1_Array.Enqueue(temp__Array1);
						_currentDataQueue2_Array.Enqueue(temp__Array2);
						_currentDataQueue3_Array.Enqueue(temp__Array3);
						_waveDataQueue1_Array.Enqueue(temp__Array4);
						_waveDataQueue2_Array.Enqueue(temp__Array5);
						_waveDataQueue3_Array.Enqueue(temp__Array6);
						mutex.ReleaseMutex();
					}
					else {
						Thread.Sleep(5);
					}
				}
			}
			catch {
			}
			finally {
				
			}

		}

		#endregion

		#region 数据保存
		void save_data(string fileName, byte[] data) {
			MyFileHelper.SaveFile_Append(fileName, data, dataBuffSzie);
		}

		private Mutex mutexSAVE = new Mutex();
		void dataSaveForThread() {
			_testDataPath = RootPath + @"\TestData\" + DateTime.Now.Year + "\\" + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
			if (!Directory.Exists(_testDataPath)) {
				Directory.CreateDirectory(_testDataPath);
			}
			while (true) {
				if (!is_ContinueProcessData && SavetoFileQueue.Count == 0) {
					return;
				}
				try {
					if (SavetoFileQueue.Count > 0 && Is_TimeToSaveFile) {
						mutexSAVE.WaitOne();
						byte[] temp = SavetoFileQueue.Dequeue();
						save_data(_testDataPath + "\\原始测试数据.bin", temp);
						mutexSAVE.ReleaseMutex();
					}
					else {
						Thread.Sleep(10);
					}
				}
				catch (Exception e) {
					myMessageQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|" + "行号:" + GetLineNum() + "错误信息:" + e.Message);
				}

			}
		}
		#endregion

		#region  数据显示
		private Mutex mutex1 = new Mutex();
		void dataProcessForThread() {

			while (true) {
				if (!is_ContinueProcessData) {
					return;
				}
				try {
					if (_waveDataQueue3_Array.Count > 20) {
						double[] X1 = new double[___每次刷新对应的点数];
						double[] currentY1 = new double[___每次刷新对应的点数];
						double[] currentY2 = new double[___每次刷新对应的点数];
						double[] currentY3 = new double[___每次刷新对应的点数];
						double[] waveY1 = new double[___每次刷新对应的点数];
						double[] waveY2 = new double[___每次刷新对应的点数];
						double[] waveY3 = new double[___每次刷新对应的点数];
						__绘图数据处理程序(_currentDataQueue1_Array, ref currentY1);
						__绘图数据处理程序(_currentDataQueue2_Array, ref currentY2);
						__绘图数据处理程序(_currentDataQueue3_Array, ref currentY3);
						__绘图数据处理程序(_waveDataQueue1_Array, ref waveY1);
						__绘图数据处理程序(_waveDataQueue2_Array, ref waveY2);
						__绘图数据处理程序(_waveDataQueue3_Array, ref waveY3);
						this.Chart.Dispatcher.Invoke(new Action(delegate {
							if (Time >= ___X轴最大值) {
								for (int i = 0; i < AxisCount; i++) {
									LineList[i].YValues.Clear();
									LineList[i].XValues.Clear();
								}
								Time = 0;
							}
							for (int i = 0; i < ___每次刷新对应的点数; i++) {
								X1[i] = Time + i * ___X每点刷新增量;
							}
							DrawLine(X1, currentY1, LineList[0], AxisList[0]);
							DrawLine(X1, currentY2, LineList[1], AxisList[1]);
							DrawLine(X1, currentY3, LineList[2], AxisList[2]);
							DrawLine(X1, waveY1, LineList[3], AxisList[3]);
							DrawLine(X1, waveY2, LineList[4], AxisList[4]);
							DrawLine(X1, waveY3, LineList[5], AxisList[5]);
							Time += ___每次刷新时间s;
						}));

					}
					else {
						Thread.Sleep(200);
					}
				}
				catch (Exception e) {
					myMessageQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|" + "行号:" + GetLineNum() + "错误信息:" + e.Message);
				}
			}
		}
		static int Flags = ___每次刷新对应的点数 / ___每次绘图需要的包数;
		void __绘图数据处理程序(Queue<double[]> data, ref double[] Y) {
			double[] tempArray = new double[___刷新时取平均值的点Count];//20
			for (int index = 0; index < ___每次绘图需要的包数; index++) {//2
				var temp = data.Dequeue();
				for (int j = 0; j < Flags; j++) {//50
					for (int k = 0; k < ___刷新时取平均值的点Count; k++) {//20
						tempArray[k] = temp[j * ___刷新时取平均值的点Count + k];
					}
					Y[index * Flags + j] = tempArray.Average();//index 0,1;
				}
			}
		}

		void DrawLine(double[] X, double[] Y, FastLine line, Axis axis) {
			try {
				line.Add(X, Y, true);
			}
			catch (Exception e) {
				myMessageQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|" + "行号:" + GetLineNum() + "错误信息:" + e.Message);
			}
		}

		#endregion
		#endregion

		#region 数据分析处理
		#region 计算电流有效值
		void calculate_电流有效值(Queue<double[]> source_data,int 一个周期的点数量Count, Queue<double[]> result_storage) {
			//source_data  没个成员含有 100ms的数据   如果电流是 50hz  那么 就是 5个周期
			int 电流周期 = 1 / _电流频率* 1000;
			int 每个队列成员包含的电流有效值个数 = 100 / 电流周期;
			int 计算有效点需要的数据点数 = ___转换所需时间的Count / 每个队列成员包含的电流有效值个数;
			double[] 一个周期的数据buff = new double[计算有效点需要的数据点数];
			double sum = 0;
			double[] 有效值存放 = new double[(int)(每个队列成员包含的电流有效值个数* ___每次刷新时间s*10)];
			for (int i = 0; i < ___每次刷新时间s*10; i++) {
				double[] temp = source_data.Dequeue();
				for (int j = 0; j < 每个队列成员包含的电流有效值个数;j++ ) {
					sum = 0;
					for (int k = 0; k < 计算有效点需要的数据点数; k++) {
						sum += temp[j * 计算有效点需要的数据点数 + k] * temp[j * 计算有效点需要的数据点数 + k];
					}
					有效值存放[i * 每个队列成员包含的电流有效值个数 + j] = sum / 计算有效点需要的数据点数;
					//如果有效值超过阀值  可以开始保存数据
					if (sum / 计算有效点需要的数据点数 >= myParameterInfo.CurrentFlag) {
						Is_TimeToSaveFile = true;
					}
				}
			}
			result_storage.Enqueue(有效值存放);
		}
		#endregion

		#region 增量刷振动数据
		/// <summary>
		/// 根据振动的X轴 最大最小值  确定 振动的取值;
		/// </summary>
		#endregion

		#endregion

		#region 连接仪器

		#region 创建TCP连接
		private void createConnection() {


			if (UDPSever == null) {
				UDPSever = new Common.UDPHelper.asycUDPSever();
				UDPSever.RecieveBufferSize = dataBuffSzie;
				UDPSever.Message_receive = tcpMsgRecvive;
			}
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

			Time = 0;
			for (int i = 0; i < AxisCount; i++) {
				LineList[i].XValues.Clear();
				LineList[i].YValues.Clear();
				AddDataToTheFirstAxes(LineList[i], AxisList[i]);
			}
			myParameterInfo.CurrentFrequency = 50;
			myTransformerInfo.CompanyName = "杭州国洲电力科技有限公司";
			myTransformerInfo.TransformerName = "测试";
			myTransformerInfo.Date = DateTime.Now;
			PageCount = 0;
			btnStop.IsEnabled = true;
			btnStartTest.IsEnabled = false;
			is_Measuring = true;
			is_ContinueProcessData = true;
			UDPSever.IsDataReceive = true;
			Is_TimeToSaveFile = false;
			SavetoFileQueue.Clear();
			RecedData.Clear();
		}
		private void btnStartTest_Click(object sender, RoutedEventArgs e) {
			DoSomethingForStart();
			UDPSever.Send(Model.Commander.StartCode);
			Thread _threadGroupData = new Thread(groupData);
			_threadGroupData.IsBackground = true;
			_threadGroupData.Start();
			Thread _threadProcessData = new Thread(dataProcessForThread);
			_threadProcessData.IsBackground = true;
			_threadProcessData.Start();
			Thread _threadSvaeData = new Thread(dataSaveForThread);
			_threadSvaeData.IsBackground = true;
			_threadSvaeData.Start();
		}

		#endregion

		#region FFT
		private void btnFFTchange_Click(object sender, RoutedEventArgs e) {
			//_testDataPath = @"F:\dahe\GZDL411C#\HZGZDL.YZFJKGZXFXY.UI\HZGZDL.YZFJKGZXFXY.UI\TestData\2017\9135533";
			//for (int i = 0; i < MyFileHelper.getLength(_testDataPath + "\\原始测试数据.bin"); i += 964) {
			//	byte[] x = MyFileHelper.OpenFile(_testDataPath + "\\原始测试数据.bin", 964, i);
			//	group(x);
			//}
			
			AddTestData();
		}

		void  group(byte[] data)
		{
			byte[] temp1 = new byte[160];
			byte[] temp2 = new byte[160];
			byte[] temp3 = new byte[160];
			byte[] temp4 = new byte[160];
			byte[] temp5 = new byte[160];
			byte[] temp6= new byte[160];
			int pos = 0;
			for (int i = 4; i < data.Length; i+=12) {
				pos = i/12*2;
				temp1[pos] = data[i];
				temp1[pos+ 1] = data[i + 1];
				temp2[pos] = data[i+2];
				temp2[pos + 1] = data[i + 3];
				temp3[pos] = data[i+4];
				temp3[pos + 1] = data[i + 5];
				temp4[pos] = data[i+6];
				temp4[pos + 1] = data[i + 7];
				temp5[pos] = data[i+8];
				temp5[pos + 1] = data[i + 9];
				temp6[pos] = data[i+10];
				temp6[pos + 1] = data[i + 11];
			}
			//SaveByteData(_testDataPath + "\\默认1.txt", temp1);
			//SaveByteData(_testDataPath + "\\默认2.txt", temp2);
			//SaveByteData(_testDataPath + "\\默认3.txt", temp3);
			//SaveByteData(_testDataPath + "\\默认4.txt", temp4);
			//SaveByteData(_testDataPath + "\\默认5.txt", temp5);
			//SaveByteData(_testDataPath + "\\默认6.txt", temp6);
			//SaveData(_testDataPath + "\\默认1.txt", getNum默认(temp1));
			//SaveData(_testDataPath + "\\默认2.txt", getNum默认(temp2));
			//SaveData(_testDataPath + "\\默认3.txt", getNum默认(temp3));
			//SaveData(_testDataPath + "\\默认4.txt", getNum默认(temp4));
			//SaveData(_testDataPath + "\\默认5.txt", getNum默认(temp5));
			//SaveData(_testDataPath + "\\默认6.txt", getNum默认(temp6));
			SaveData(_testDataPath + "\\强转1.txt", getNum强转(temp1));
			SaveData(_testDataPath + "\\强转2.txt", getNum强转(temp2));
			SaveData(_testDataPath + "\\强转3.txt", getNum强转(temp3));
			SaveData(_testDataPath + "\\强转4.txt", getNum强转(temp4));
			SaveData(_testDataPath + "\\强转5.txt", getNum强转(temp5));
			SaveData(_testDataPath + "\\强转6.txt", getNum强转(temp6));
		}
		List<int> getNum默认(byte[] data) {
			List<int> temp = new List<int>();
			for (int i = 0; i < data.Length; i+=2) {
				temp.Add(BitConverter.ToInt16(data, i));
			}
			return temp;
		}
		List<int> getNum强转(byte[] data) {
			List<int> temp = new List<int>();
			for (int i = 0; i < data.Length; i += 2) {
				temp.Add(IPAddress.NetworkToHostOrder( BitConverter.ToInt16(data, i)));
			}
			return temp;
		}
		void SaveData( string path,List<int> data) {
			StringBuilder sb = new StringBuilder();
			for(int i=0;i<data.Count;i++)
			{
				sb.Append(data[i] + ", ");
					if(i>=0 & i%10==0)
				{
					sb.Append("\r\n");
				}
			}
			MyFileHelper.SaveFile_Append(path, sb.ToString(), sb.Length);
		}
		void SaveByteData(string path,byte[] data) {
			StringBuilder sb = new StringBuilder();
			sb.Append(BitConverter.ToString(data) + "/r/n");
			MyFileHelper.SaveFile_Append(path, sb.ToString(), sb.Length);
		}
		#endregion

		#region 停止测试
		#region 停止测量 界面逻辑
		void MeasureReSet() {
			btnStop.IsEnabled = false;
			btnStartTest.IsEnabled = true;
			btnFFTchange.IsEnabled = true;
			is_Measuring = false;
			is_ContinueProcessData = false;
			UDPSever.IsDataReceive = false;
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

		#region 窗口加载  消息监听
		void showSysMsg_fun() {
			while (true) {
				//如果有消息 就处理消息
				if (myMessageQueue.Count > 0) {
					string temp = myMessageQueue.Dequeue();
					string Level = temp.Split('|')[0];
					string Msg = temp.Split('|')[1];
					this.Dispatcher.Invoke(new Action(delegate {
						if (int.Parse(Level) == (int)(Model.SystemMsgLevel.ERROR)) {
							if (UDPSever != null) {
								UDPSever.Send(Model.Commander.StopCode);
								MeasureReSet();
							}
							
							this.label_Debug.Foreground = Brushes.Red;
							//MessageBox.Show(Msg);
						}
						else {
							this.label_Debug.Foreground = Brushes.White;
						}
						this.label_Debug.Content = Msg;
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
							MessageBox.Show(Msg);
						}
						else {
							this.label_Debug.Foreground = Brushes.White;
						}
						this.label_Debug.Content = Msg;

					}));
				}
				Thread.Sleep(1000);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			#region 颜色初始化
			ColorList.Add(Colors.Red);
			ColorList.Add(Colors.Orange);
			ColorList.Add(Colors.Gold);
			ColorList.Add(Colors.Green);
			ColorList.Add(Colors.Blue);
			ColorList.Add(Colors.Black);
			ColorList.Add(Colors.Violet);
			//服务器数据颜色
			for (int i = 0; i < AxisCount; i++) {
				ColorList.Add(Colors.DeepSkyBlue);
			}
				BrushList.Add(Brushes.Red);
			BrushList.Add(Brushes.Orange);
			BrushList.Add(Brushes.Gold);
			BrushList.Add(Brushes.Green);
			BrushList.Add(Brushes.Blue);
			BrushList.Add(Brushes.Black);
			BrushList.Add(Brushes.Violet);
			lab1.Foreground = BrushList[0];
			lab2.Foreground = BrushList[1];
			lab3.Foreground = BrushList[2];
			lab4.Foreground = BrushList[3];
			lab5.Foreground = BrushList[4];
			lab6.Foreground = BrushList[5];
			#endregion

			initTeeChart();//表格初始化
			
			#region 消息监听开启
			showSysMsg = new Thread(showSysMsg_fun);
			showSysMsg.IsBackground = true;
			showSysMsg.Start();
			#endregion

			#region  Canvas 事件注册
			SetCanvesAutoOut_Left_SreenWhenMouseEnter(myCanvas);
			SetCanvesAutoEnter_Left_SreenWhenMouseLeave(myCanvas, 20);
			#endregion

			#region  checkBox事件注册
			cb_ShowCursor.Click += (RoutedEventHandler)delegate {
				CurorV.Active = (bool)cb_ShowCursor.IsChecked;
			};
			cb_followMouse.Click += (RoutedEventHandler)delegate {
				CurorV.FollowMouse = (bool)cb_followMouse.IsChecked;
			};
			#endregion
			
			#region Cursor事件注册
			StringBuilder sbCurrent = new StringBuilder();
			StringBuilder sbWave = new StringBuilder();
			CurorV.Change += (Steema.TeeChart.WPF.Tools.CursorChangeEventHandler)delegate {
				if (!is_Measuring) {
					try {
						lab1.Content = "电流[通道1]:" + LineList[0].YValues[(int)(CurorV.XValue * 500)].ToString("0.##");
						lab2.Content = "电流[通道2]:" + LineList[1].YValues[(int)(CurorV.XValue * 500)].ToString("0.##");
						lab3.Content = "电流[通道3]:" + LineList[2].YValues[(int)(CurorV.XValue * 500)].ToString("0.##");
						lab4.Content = "振动[通道1]:" + LineList[3].YValues[(int)(CurorV.XValue * 500)].ToString("0.##");
						lab5.Content = "振动[通道2]:" + LineList[4].YValues[(int)(CurorV.XValue * 500)].ToString("0.##");
						lab6.Content = "振动[通道3]:" + LineList[5].YValues[(int)(CurorV.XValue * 500)].ToString("0.##");
					}
					catch {

					}
				
				}
			};
			#endregion

			#region 主窗体按钮Button点击事件注册
				btnConnectDevice.Click += btnConnectDevice_Click;
				btnFFTchange.Click += btnFFTchange_Click;
				btnOpenFile.Click +=btnOpenFile_Click;
				btnStartTest.Click +=btnStartTest_Click;
				btnStop.Click+=btnStop_Click;
			#endregion

			#region TreeView部分 按钮事件注册
				btnAddNew.Click += btnAddNew_Click;
				btnDelete.Click += btnDelete_Click;
				btnLocalTestData.Click += btnLocalTestData_Click;
				btnSeverTestData.Click += btnSeverTestData_Click;
				btnLeftMove.Click += btnLeftMove_Click;
				btnRightMove.Click += btnRightMove_Click;
				btnEnlarge.Click += btnEnlarge_Click;
				btnNarrow.Click += btnNarrow_Click;
			#endregion

			myProgressBar.Visibility = System.Windows.Visibility.Hidden;
		
		}
		SeverWindow sever = new SeverWindow();
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
		#region 关于Chart和TreeView 注册的事件区域

		bool is_Enlarge = true;
		bool is_Narrow = false;
		double Max = 10;
		double min = 0;
		void btnNarrow_Click(object sender, RoutedEventArgs e) {
			if (is_Narrow) {
				is_Narrow = false;
				is_Enlarge = true;
			}

			double offset = Math.Round((Chart.Chart.Axes.Bottom.Maximum - Chart.Chart.Axes.Bottom.Minimum)  / 2, 4);
			if (offset >= 5) {
				offset = 5;
				return;
			}
			Chart.Chart.Axes.Bottom.SetMinMax(Chart.Chart.Axes.Bottom.Minimum - offset, Chart.Chart.Axes.Bottom.Maximum + offset);
			if (Chart.Chart.Axes.Bottom.Minimum - offset < 0) {
				Chart.Chart.Axes.Bottom.SetMinMax(0, Chart.Chart.Axes.Bottom.Maximum);
			}
			if (Chart.Chart.Axes.Bottom.Maximum + offset > 10) {
				Chart.Chart.Axes.Bottom.SetMinMax(Chart.Chart.Axes.Bottom.Minimum, 10);
			}
			
			double MAXChart1 = Chart.Axes.Bottom.Maximum;
			double MINChart1 = Chart.Axes.Bottom.Minimum;
			reflushChart(MAXChart1, MINChart1, x, y, LineList[0]);
			reflushChart(MAXChart1, MINChart1, x, y, LineList[1]);
			reflushChart(MAXChart1, MINChart1, x, y, LineList[2]);
			for (int i = 0; i < AxisCount; i++) {
				AddDataToTheFirstAxes(LineList[i], AxisList[i]);
			}
		}

		void btnEnlarge_Click(object sender, RoutedEventArgs e) {
			if (is_Enlarge) {
				is_Enlarge = false;
				is_Narrow = true;
			}
			double offset = Math.Round((Chart.Chart.Axes.Bottom.Maximum - Chart.Chart.Axes.Bottom.Minimum) /4,4);
			lablVoltge.Content = offset;
			
			if (offset<=0.0425) {
				return;
			}
			Chart.Chart.Axes.Bottom.SetMinMax(Chart.Chart.Axes.Bottom.Minimum + offset, Chart.Chart.Axes.Bottom.Maximum - offset);
			double MAXChart = Chart.Axes.Bottom.Maximum;
			double MINChart = Chart.Axes.Bottom.Minimum;
			reflushChart(MAXChart, MINChart, x, y, LineList[0]);
			reflushChart(MAXChart, MINChart, x, y, LineList[1]);
			reflushChart(MAXChart, MINChart, x, y, LineList[2]);
			for (int i = 0; i < AxisCount; i++) {
				AddDataToTheFirstAxes(LineList[i], AxisList[i]);
			}
		}
		
		void btnRightMove_Click(object sender, RoutedEventArgs e) {
			double step = Chart.Axes.Bottom.Maximum/10;
			
			
			Chart.Chart.Axes.Bottom.Maximum += step;
			if (Chart.Chart.Axes.Bottom.Maximum >= Max) {
				Chart.Chart.Axes.Bottom.Maximum = Max;
				return;
			}
			Chart.Chart.Axes.Bottom.Minimum += step;
			double MAXChart1 = Chart.Axes.Bottom.Maximum;
			double MINChart1 = Chart.Axes.Bottom.Minimum;
			reflushChart(MAXChart1, MINChart1, x, y, LineList[0]);
			reflushChart(MAXChart1, MINChart1, x, y, LineList[1]);
			reflushChart(MAXChart1, MINChart1, x, y, LineList[2]);
			for (int i = 0; i < AxisCount; i++) {
				AddDataToTheFirstAxes(LineList[i], AxisList[i]);
			}
		}

		void btnLeftMove_Click(object sender, RoutedEventArgs e) {
			double step = Chart.Axes.Bottom.Maximum / 10;
			
			Chart.Chart.Axes.Bottom.Minimum -= step;
			
			if (Chart.Chart.Axes.Bottom.Minimum <= min) {
				Chart.Chart.Axes.Bottom.Minimum = min;
				return;
			}
			Chart.Chart.Axes.Bottom.Maximum -= step;
			double MAXChart1 = Chart.Axes.Bottom.Maximum;
			double MINChart1 = Chart.Axes.Bottom.Minimum;
			reflushChart(MAXChart1, MINChart1, x, y, LineList[0]);
			reflushChart(MAXChart1, MINChart1, x, y, LineList[1]);
			reflushChart(MAXChart1, MINChart1, x, y, LineList[2]);
			for (int i = 0; i < AxisCount; i++) {
				AddDataToTheFirstAxes(LineList[i], AxisList[i]);
			}
		}

		void rbtnSeverTestData_Checked(object sender, RoutedEventArgs e) {
			//连接远程服务器 并导入 远程 数据信息;
			Common.TreeViewHelper.TreeViewUpdateFromSever(myParentTree, new string[] { "国洲电力", "测试变压器", DateTime.Now.ToString(), "分接位", "第一次测试" });
		}

		private void rbtnLocalTestData_Checked(object sender, RoutedEventArgs e) {
			// 导入本地数据信息;
		}

		private void btnAddNew_Click(object sender, RoutedEventArgs e) {
			Common.TreeViewHelper.TreeViewUpdateLocal(myParentTree, 6, new string[] { "国洲电力", "测试变压器", DateTime.Now.ToString(), "分接位", "第一次测试" });
		}
		private void btnDelete_Click(object sender, RoutedEventArgs e) {
			myTreeViewItem item = (myTreeViewItem)myParentTree.SelectedItem;
			if (item == null) {
				myMessageQueue.Enqueue((int)Model.SystemMsgLevel.ERROR + "|" + "您没有选择需要删除的内容!");
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
				myMessageQueue.Enqueue((int)Model.SystemMsgLevel.INFO + "|" + "删除成功!");
			}
		}
		#endregion
	
		#endregion

		#region 设置一些控件的位置及大小
		void SetContorlsPosition() {

			#region MyParentCanvas
			myParentCanvas.Width = this.ActualWidth-250;
			myParentCanvas.Height = this.ActualHeight - 195;
			#endregion

			#region Chart
			Chart.Width = myParentCanvas.Width - 30;
			Chart.Height = myParentCanvas.Height;
			Chart.Margin = new Thickness(30, 0,0, 0);
			//设置表格left 范围 为了 自定义坐标的正常显示
			Chart.Axes.Left.StartPosition = 0;
			Chart.Axes.Left.EndPosition = Chart.Height;
			#endregion

			#region MyChartCanvas
			myChartCanvas.Width =  Chart.Width/10+20 ;
			myChartCanvas.Height = Chart.Height/2;
			Canvas.SetLeft(myChartCanvas, Chart.Width - myChartCanvas.Width +15);
			Canvas.SetBottom(myChartCanvas, 0);
			myGroupBoxInChart.Width = myChartCanvas.Width;
			myGroupBoxInChart.Height = myChartCanvas.Height;
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
					while (currentLeft >= -temp.Width + outWidth+10) {
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
				double currentRight = myParentCanvas.Width- Canvas.GetLeft(temp)-temp.Width;
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

		private void btnInitChart_Click(object sender, RoutedEventArgs e) {

			myTransformerInfo.CompanyName = "测试项目";
			myTransformerInfo.TransformerName = "测试变压器";
			myTransformerInfo.TransformerProductCode = "0000";
			myTransformerInfo.Date = System.DateTime.Now;
			myTransformerInfo.CurrentPos = 0;
			myTransformerInfo.EndPos = 10;
			myTransformerInfo.Phase = 3;
			myTransformerInfo.StartPos = 0;
			myTransformerInfo.SwitchCompanyName = "未知";
			myTransformerInfo.SwitchName = "未知";
			myTransformerInfo.SwitchProductCode = "ss";
			myTransformerInfo.Winding = 2;
			BLL.TransformerService s = new BLL.TransformerService();
			MessageBox.Show(s.ADD(myTransformerInfo));
			//AxisCount = int.Parse(tbAxisCount.Text);
			//Chart.Axes.Custom.Clear();
			//Chart.Series.Clear();
			//LineList.Clear();
			//AxisList.Clear();
			//initTeeChart();
		}

		void reflushChart(double Max, double Min, double[] x, double[] y,FastLine line) {
			line.XValues.Clear();
			line.YValues.Clear();
			int arrayCount = (int)( Math.Round(Max - Min,4) * 100000);
			int offset = arrayCount / 5000;
			double[] tempx = new double[5000];
			double[] tempy = new double[5000];
			for (int i = 0; i < 5000; i++) {
				tempx[i] = x[ (int)(Min * 100000+(i)*offset)];
				tempy[i] = y[ (int)(Min * 100000+(i)*offset)];
			}
			line.Add(tempx, tempy);
		}

		private void btnCompress_Click(object sender, RoutedEventArgs e) {
			string path = MyFileHelper.OpenDirectory(RootPath);
			mySever.CompressZipFile(path + ".zip", path);
			MyFileHelper.OpenFile_getPath(RootPath);
		}
	}
}
