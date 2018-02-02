using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace HZGZDL.YZFJKGZXFXY.Common {
	/// <summary>
	/// 解析原始数据的类
	/// </summary>
	public class DataFromFile {


		static public void CreateDataFromFile(string Filepath) {
			path = Filepath;
			IsReadComplete = false;
			GetHeader();
			GetAllChannelData();
		}
		static public long FileLength { get; set; }
		static public int currentPos { get; set; }
		/// <summary>
		/// 公司名称|变压器名称|日期|分接位|切换方式
		/// </summary>
		static public string[] Header { get; set; }
		static private string path { get; set; }
		static public bool IsReadComplete { get; set; }
		static public List<double> Current_Channel_1_Data = new List<double>();
		static public List<double> Current_Channel_2_Data = new List<double>();
		static public List<double> Current_Channel_3_Data = new List<double>();
		static public List<double> Wave_Channel_1_Data = new List<double>();
		static public List<double> Wave_Channel_2_Data = new List<double>();
		static public List<double> Wave_Channel_3_Data = new List<double>();
		static Thread readData;
		static	Queue<byte[]> dataQueue = new Queue<byte[]>();
		static void GetAllChannelData() {
			Current_Channel_1_Data.Clear();
			Current_Channel_2_Data.Clear();
			Current_Channel_3_Data.Clear();
			Wave_Channel_1_Data.Clear();
			Wave_Channel_2_Data.Clear();
			Wave_Channel_3_Data.Clear();
			dataQueue.Clear();
			
			readData = new Thread(readToQueue);
			readData.IsBackground = true;
			readData.Start();
		}
		static void GetHeader() {
			string temp =Encoding.Unicode.GetString(MyFileHelper.OpenFile(path, 964, 0));
			int stringlength = 0;
			for (int i = 0; i < temp.Length; i++) {
				if (temp[i] == '\0') {
					stringlength = i;
					break;
				}
			}
			temp = temp.Substring(0, stringlength);
			Header = temp.Split('|');
		}
		static int __平移每次 = 12;
		static double MUT = 1;
		/// <summary>
		/// 数据转换成double并存入各通道对呀的队列中
		/// </summary>
		static void readToQueue() {
			long len =	MyFileHelper.getLength(path);
			FileLength = len;
			int offset = 964;
			int _conuntPageConvert = 50;
			while (offset < len) {
				dataQueue.Enqueue(MyFileHelper.OpenFile(path, 964, offset));
				offset += 964;
				currentPos = offset;
				//分组一次 分 4ms  数据  偷点数
				if (dataQueue.Count >= _conuntPageConvert) {
					for (int pageindex = 0; pageindex < _conuntPageConvert; pageindex++) {
						byte[] data_buffer = dataQueue.Dequeue();
						if (data_buffer == null) {
							continue;
						}
						for (int i = 4; i < data_buffer.Length; i += __平移每次) {
							double current1 = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i)));
							double current2 = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i+2)));
							double current3 = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i+4))); 
							double wave1 = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 6)));
							double wave2 = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 8)));
							double wave3 = (IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data_buffer, i + 10)));
							Current_Channel_1_Data.Add(current1*25/32768);
							Current_Channel_2_Data.Add(current2 * 25 / 32768);
							Current_Channel_3_Data.Add(current3 * 25 / 32768);
							Wave_Channel_1_Data.Add((wave1 * 625) / 32768);
							Wave_Channel_2_Data.Add((wave2 * 625) / 32768);
							Wave_Channel_3_Data.Add((wave3 * 625) / 32768);
						}
					}
				}
			}
			dataQueue.Clear();
			IsReadComplete = true;
		}
		/// <summary>
		/// 计算电流有效值
		/// </summary>
		/// <param name="dataList">原始数据</param>
		/// <returns></returns>
		static public double[] CulcateCurrentEffect(List<double> dataList) {
			double[] temp = new double[500];
			double sum = 0;
			for (int i = 0; i < 500; i++) {
					sum = 0;
					for (int k = 0; k < 2000; k++) {
						double _currentConvetValue = dataList[i * 2000 + k];
						sum += _currentConvetValue * _currentConvetValue;
					}
					double __currentFffectValue = Math.Sqrt(sum / 2000) * 2;
					temp[i] = __currentFffectValue;
			}
			dataList.Clear();
			return temp;
		}

		static public string FindMutationposition(List<double> dataList,int peakValue,double rateOfChange,double rateParameter,int minPoint,int maxPoint,int[] changePointArray) {
			StringBuilder message = new StringBuilder();
			try {
				Fun_找一段曲线里的变化点(dataList, rateOfChange, rateParameter, minPoint, maxPoint, changePointArray);
			}
			catch(Exception er) {
				message.Append("查找变化点异常 Error:" + er.Message+"\r\n");
				return message.ToString();
			}
			try {
				Fun_变化点位置第二次优化(dataList, peakValue, changePointArray);
			}
			catch (Exception er) {
				message.Append("优化变化点异常 Error:" + er.Message + "\r\n");
				return message.ToString();
			}
			message.Append("ok");
			for(int i=0;i<=changePointArray.Length;i++)
			{
					if (changePointArray[i] == 0) {
						continue;
					}
				else {
					message.Append("找到:第" + (i + 1) + "个变化点\r\n");
				}
			}
			return message.ToString();
		}

		/// <summary>
		/// 不同采样率对应的每个周期对应的数据点数
		/// </summary>
		static int Page_Max_count = 0;
		/// <summary>
		/// 根据零点 找出完整的正常的正弦波数据
		/// </summary>
		/// <param name="data">原始数据</param>
		/// <param name="nomal_wave">单个周期正弦波 列表</param>
		public static void Fun_找正常波形并加入列表(List<int> data, List<int[]> nomal_wave) {
			//存零点位置  即 一个正常周期的起始位置
			int[] lingdian1 = new int[80];
			//零点位置的标签
			int lingdian = 0;
			for (int i = 0; i < data.Count - 1; i++) {
				if (data[i] >= 0 && data[i + 1] <= 0 && Math.Abs(data[i + 1]) < 10000) {
					if (lingdian < lingdian1.Length) {
						lingdian1[lingdian] = i;
						lingdian++;
					}
					else {
						break;
					}
				}
			}
			int len = 0;
			//获取正常波形数据 并加入列表            
			for (int i = 0; i < lingdian1.Length - 2; i++) {
				for (int j = i + 1; j < lingdian1.Length - 1; j++) {
					if (Math.Abs(lingdian1[i] - lingdian1[j]) >= Page_Max_count / 3 * 18 && Math.Abs(lingdian1[i] - lingdian1[j]) <= Page_Max_count / 3 * 22) {
						len = Math.Abs(lingdian1[i] - lingdian1[j]);
						int[] temp = new int[len];
						for (int k = lingdian1[i]; k < lingdian1[i] + len; k++) {
							temp[k - lingdian1[i]] = data[k];
						}
						nomal_wave.Add(temp);
						i = j;
						break;
					}
				}
			}
		}
		/// <summary>
		/// 找每个周期 每相邻两点之间的最大变化率
		/// </summary>
		/// <param name="nomal_wave">单个周期正弦波 列表</param>
		/// <param name="lenth">一个周期正弦波的最大长度</param>
		/// <returns></returns>
		private static double Fun_计算周期两点之间最大差异(List<int[]> nomal_wave, int lenth) {
			if (nomal_wave.Count == 0) {
				//MessageBox.Show("数据异常");
				return 0;
			}
			double rate = 0;
			int[] buffer = new int[(lenth + 100) * (nomal_wave.Count + 1)];
			int sum = 0;
			for (int i = 0; i < nomal_wave.Count; i++) {
				for (int j = 0; j < nomal_wave[i].Length - 1; j++) {
					buffer[i * nomal_wave[i].Length + j] = Math.Abs(nomal_wave[i][j] - nomal_wave[i][j + 1]);
				}
			}
			Array.Sort(buffer);
			for (int i = buffer.Length - Page_Max_count * 4; i < buffer.Length - 10; i++) {
				sum += buffer[i];
			}
			rate = (sum + 0.1) / (Page_Max_count * 4 - 10);
			return rate;
		}

		/// <summary>
		/// 在原始曲线中寻找突变点记录在数组中
		/// </summary>
		/// <param name="Var_原始曲线">原始数据</param>
		/// <param name="double_变化率">相邻两点变化率</param>
		/// <param name="double_误差比例">允许的误差修正值</param>
		/// <param name="int_最小持续变化时间对应的点">最小变化区间对应的点数,意思是变化的区间要超过一定时间才算是[变化点]</param>
		/// <param name="int_最大持续不变时间对应的点">最大无变化区间对应的点数,意思是在这个区间范围里没有变化就认为此区间为[正常波形]</param>
		/// <param name="int_峰值">辅助判断</param>
		/// <param name="Var_存放变化点的位置信息">记录变化点的数组</param>
		private static void Fun_找一段曲线里的变化点(List<double> Var_原始曲线, double double_变化率, double double_误差比例, int int_最小持续变化时间对应的点, int int_最大持续不变时间对应的点, int[] Var_存放变化点的位置信息) {
			bool find_start = false;
			//找到触发结束标记
			bool find_end = true;
			//变化点标签
			int index = 0;
			//变化点1差值
			double xxx = 0;
			//变化点后续差值
			double yyy = 0;
			double var_上升还是下降 = 0;
			double rate_阈值 = double_变化率 * (double_误差比例 + 1.0 / double_变化率);
			int 上下沿相隔个数 = 4;
			int 差值个数 = 1;
			if (Page_Max_count == 600) {
				上下沿相隔个数 = 12;
				差值个数 = 5;
				int_最小持续变化时间对应的点 = int_最小持续变化时间对应的点 / 2;
			}
			int _flag = 0;

			for (int i = 10; i < Var_原始曲线.Count - 100; i++) {
				yyy = 0;
				var_上升还是下降 = Var_原始曲线[i] - Var_原始曲线[i + 上下沿相隔个数];
				if (var_上升还是下降 == 0) {
					continue;
				}
				xxx = Var_原始曲线[i] - Var_原始曲线[i + 差值个数];
				//如果波形变化超过理论变化率开始分析是否是变化点
				if (Math.Abs(xxx) >= rate_阈值) {
					//开始
					#region 开始
					if (!find_start) {
						//分析后面的点 如果全部是变化点 那么 这个点是变化点
						for (int j = i + 差值个数; j < i + int_最小持续变化时间对应的点; j++) {
							if (var_上升还是下降 < 0) {
								yyy = Var_原始曲线[i] + (j - i - 差值个数) * (double_变化率) - Var_原始曲线[j];
							}
							else if (var_上升还是下降 > 0) {
								yyy = Var_原始曲线[i] - (j - i - 差值个数) * (double_变化率) - Var_原始曲线[j];
							}
							//
							if (Math.Abs(yyy) <= double_变化率) {
								i = j;
								break;
							}
							if (j >= i + int_最小持续变化时间对应的点 - 2) {
								if (index < 10) {
									Var_存放变化点的位置信息[index++] = i;
									i = j;
									find_start = true;
									find_end = false;
									break;
								}
							}
						}
					}
					#endregion
				}
				else {
					if (!find_end) {
						_flag = 0;
						#region 结束
						//分析后面的点 如果全部是变化点 那么 这个点是变化点
						int count = i + Page_Max_count * 100;
						if (Var_原始曲线.Count - count <= 差值个数 * 5) {
							count = Var_原始曲线.Count - 差值个数 * 5 - 1;
						}
						for (int j = i + 差值个数; j < count; j++) {
							for (int num = 0; num < 差值个数 * 4; num++) {
								if (Math.Abs(Var_原始曲线[j] - Var_原始曲线[j + num + 差值个数]) >= (rate_阈值 + num * rate_阈值)) {
									_flag++;
								}
							}//相邻的num个 点 来比较变化 

							if (_flag >= 差值个数 * 4 - 3) {
								i = j + 差值个数 * 4;
								break;
							}//这个点还在变化  此次分析终止 重新找

							if (j >= i + int_最大持续不变时间对应的点 - 差值个数 * 4) {
								if (index < 10) {
									int 变化区间 = i - Var_存放变化点的位置信息[index - 1];
									if (变化区间 >= Page_Max_count / 3/*大于1ms并且小于150ms*/ && 变化区间 <= Page_Max_count * 50) {
										Var_存放变化点的位置信息[index++] = i;
									}
									else {
										index--;
										Var_存放变化点的位置信息[index] = 0;
									}
									i = j;
									find_start = false;
									find_end = true;
									break;
								}
							}//如果在 最大持续不变区间里 都没有变化点了  那么这个点就是 变化结束点
						}
						#endregion
					}
				}
			}
		}

		/// <summary>
		/// 找到变化区间之后 修正突变位置 使其更加准确
		/// </summary>
		/// <param name="Var_原始曲线">原始数据</param>
		/// <param name="int_峰值">辅助判断</param>
		/// <param name="Var_存放变化点的位置信息">记录了变化点的数组</param>
		private static void Fun_变化点位置第二次优化(List<double> Var_原始曲线, int int_峰值, int[] Var_存放变化点的位置信息) {

			int 前后范围 = Page_Max_count * 1;
			int 间隔的点 = Page_Max_count / 30;
			int 变化趋势 = int_峰值 * 2 / (6000 / Page_Max_count + 50);
			int 变化_flag0 = 0;
			int 变化_flag1 = 0;
			int 前段变化次数 = 0;
			int 后段变化次数 = 0;
			int len = 0;
				//判断有效值个数
				for (int s = 0; s < Var_存放变化点的位置信息.Length; s++) {
					if (Var_存放变化点的位置信息[s] == 0) {
						break;
					}
					len++;
				}
				for (int count = 0; count < len - 1; count += 2) {
					前段变化次数 = 0;
					后段变化次数 = 0;
					//前段变化
					for (int i = Var_存放变化点的位置信息[count]; i > Var_存放变化点的位置信息[count] - 前后范围; i--) {
						if ((i - 2 * 间隔的点) <= 0) {
							continue;
						}
						if ((Var_原始曲线[i] - Var_原始曲线[i - 间隔的点] > 变化趋势 && Var_原始曲线[i - 间隔的点] - Var_原始曲线[i - 2 * 间隔的点] < -变化趋势) || (Var_原始曲线[i] - Var_原始曲线[i - 间隔的点] < -变化趋势 && Var_原始曲线[i - 间隔的点] - Var_原始曲线[i - 2 * 间隔的点] > 变化趋势)) {
							前段变化次数++;
							变化_flag0 = i;
							for (int j = i - 1; j > i - 间隔的点; j--) {
								if (Math.Abs((Var_原始曲线[j] - Var_原始曲线[j - 1] + 0.0001) / (Var_原始曲线[j - 1] - Var_原始曲线[j - 2]) + 0.0001) > 4 || (Math.Abs((Var_原始曲线[j] - Var_原始曲线[j - 1] + 0.0001) / (Var_原始曲线[j - 1] - Var_原始曲线[j - 2] + 0.0001))) < 0.25) {
									变化_flag0 = j - 2;
									// break;
								}
							}
						}
					}
					//后段变化
					for (int i = Var_存放变化点的位置信息[count + 1]; i < Var_存放变化点的位置信息[count + 1] + 前后范围; i++) {
						if ((i + 2 * 间隔的点) >= Var_原始曲线.Count) {
							continue;
						}
						if ((Var_原始曲线[i] - Var_原始曲线[i + 间隔的点] > 变化趋势 && Var_原始曲线[i + 间隔的点] - Var_原始曲线[i + 2 * 间隔的点] < -变化趋势) || (Var_原始曲线[i] - Var_原始曲线[i + 间隔的点] < -变化趋势 && Var_原始曲线[i + 间隔的点] - Var_原始曲线[i + 2 * 间隔的点] > 变化趋势)) {
							后段变化次数++;
							变化_flag1 = i;
							for (int j = i + 1; j < i + 间隔的点; j++) {
								if (Math.Abs((Var_原始曲线[j] - Var_原始曲线[j + 1] + 0.0001) / (Var_原始曲线[j + 1] - Var_原始曲线[j + 2] + 0.0001)) > 4 || Math.Abs((Var_原始曲线[j] - Var_原始曲线[j + 1] + 0.0001) / (Var_原始曲线[j + 1] - Var_原始曲线[j + 2] + 0.0001)) < 0.25) {
									变化_flag1 = j + 2;
									// break;
								}
							}
						}
					}
					if (前段变化次数 >= 3) {
						Var_存放变化点的位置信息[count] = 变化_flag0;
					}
					if (后段变化次数 >= 3) {
						Var_存放变化点的位置信息[count + 1] = 变化_flag1;
					}
				}

		}
	}
}
