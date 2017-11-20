﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class myFFT {

		public void FFT(ref double[] DataIn_r, ref double[] DataIn_i, ref double[] DataOut_r, ref double[] DataOut_i) {
			int len = DataIn_r.Length;
			if (len == 1)//抽取到只有一个点时，将数据返回
            {
				DataOut_r = DataIn_r;
				DataOut_i = DataIn_i;
				return;
			}
			len = len / 2;
			//申请空间，以存储新抽取的子列
			double[] evenData_r = new double[len];
			double[] evenData_i = new double[len];
			double[] oddData_r = new double[len];
			double[] oddData_i = new double[len];
			//用以存储下一级函数返回的计算结果
			double[] evenResult_r = new double[len];
			double[] evenResult_i = new double[len];
			double[] oddResult_r = new double[len];
			double[] oddResult_i = new double[len];
			//按奇偶迭代
			for (int i = 0; i < len; i++) {
				evenData_r[i] = DataIn_r[i * 2];
				evenData_i[i] = DataIn_i[i * 2];
				oddData_r[i] = DataIn_r[i * 2 + 1];
				oddData_i[i] = DataIn_i[i * 2 + 1];
			}//迭代
			FFT(ref evenData_r, ref evenData_i, ref evenResult_r, ref evenResult_i);
			FFT(ref oddData_r, ref oddData_i, ref oddResult_r, ref oddResult_i);
			//最下一级函数返回后，进行本级的蝶形计算
			double WN_r, WN_i;
			for (int i = 0; i < len; i++) {
				WN_r = Math.Cos(2 * Math.PI / (2 * len) * i);
				WN_i = -Math.Sin(2 * Math.PI / (2 * len) * i);
				DataOut_r[i] = evenResult_r[i] + oddResult_r[i] * WN_r - oddResult_i[i] * WN_i;
				DataOut_i[i] = evenResult_i[i] + oddResult_i[i] * WN_r + oddResult_r[i] * WN_i;
				DataOut_r[i + len] = evenResult_r[i] - oddResult_r[i] * WN_r + oddResult_i[i] * WN_i;
				DataOut_i[i + len] = evenResult_i[i] - oddResult_i[i] * WN_r - oddResult_r[i] * WN_i;
			}
			evenData_r = evenData_i = evenResult_r = evenResult_i = null;
			oddData_i = oddData_r = oddResult_i = oddResult_r = null;
			GC.Collect();//释放本级函数占用的资源
		}

		private void DataSort(ref double[] data_r, ref double[] data_i) {
			if (data_r.Length == 0 || data_i.Length == 0 || data_r.Length != data_i.Length)
				return;
			int len = data_r.Length;
			int[] count = new int[len];
			int M = (int)(Math.Log(len) / Math.Log(2));
			double[] temp_r = new double[len];
			double[] temp_i = new double[len];

			for (int i = 0; i < len; i++) {
				temp_r[i] = data_r[i];
				temp_i[i] = data_i[i];
			}
			for (int l = 0; l < M; l++) {
				int space = (int)Math.Pow(2, l);
				int add = (int)Math.Pow(2, M - l - 1);
				for (int i = 0; i < len; i++) {
					if ((i / space) % 2 != 0)
						count[i] += add;
				}
			}
			for (int i = 0; i < len; i++) {
				data_r[i] = temp_r[count[i]];
				data_i[i] = temp_i[count[i]];
			}
		}

		private void Dit2_FFT(ref double[] data_r, ref double[] data_i, ref double[] result_r, ref double[] result_i) {
			if (data_r.Length == 0 || data_i.Length == 0 || data_r.Length != data_i.Length)
				return;
			int len = data_r.Length;
			double[] X_r = new double[len];
			double[] X_i = new double[len];
			for (int i = 0; i < len; i++)//将源数据复制副本，避免影响源数据的安全性
            {
				X_r[i] = data_r[i];
				X_i[i] = data_i[i];
			}
			DataSort(ref X_r, ref X_i);//位置重排
			double WN_r, WN_i;//旋转因子
			int M = (int)(Math.Log(len) / Math.Log(2));//蝶形图级数
			for (int l = 0; l < M; l++) {
				int space = (int)Math.Pow(2, l);
				int num = space;//旋转因子个数
				double temp1_r, temp1_i, temp2_r, temp2_i;
				for (int i = 0; i < num; i++) {
					int p = (int)Math.Pow(2, M - 1 - l);//同一旋转因子有p个蝶
					WN_r = Math.Cos(2 * Math.PI / len * p * i);
					WN_i = -Math.Sin(2 * Math.PI / len * p * i);
					for (int j = 0, n = i; j < p; j++, n += (int)Math.Pow(2, l + 1)) {
						temp1_r = X_r[n];
						temp1_i = X_i[n];
						temp2_r = X_r[n + space];
						temp2_i = X_i[n + space];//为蝶形的两个输入数据作副本，对副本进行计算，避免数据被修改后参加下一次计算
						X_r[n] = temp1_r + temp2_r * WN_r - temp2_i * WN_i;
						X_i[n] = temp1_i + temp2_i * WN_r + temp2_r * WN_i;
						X_r[n + space] = temp1_r - temp2_r * WN_r + temp2_i * WN_i;
						X_i[n + space] = temp1_i - temp2_i * WN_r - temp2_r * WN_i;
					}
				}
			}
			result_r = X_r;
			result_i = X_i;
		}

		public Complex[] fft_frequency(Complex[] sourceData, int countN) {
			//2的r次幂为N，求出r.r能代表fft算法的迭代次数  
			int r = Convert.ToInt32(Math.Log(countN, 2));


			//分别存储蝶形运算过程中左右两列的结果  
			Complex[] interVar1 = new Complex[countN];
			Complex[] interVar2 = new Complex[countN];
			interVar1 = (Complex[])sourceData.Clone();

			//w代表旋转因子  
			Complex[] w = new Complex[countN / 2];
			//为旋转因子赋值。（在蝶形运算中使用的旋转因子是已经确定的，提前求出以便调用）  
			//旋转因子公式 \  /\  /k __  
			//              \/  \/N  --  exp(-j*2πk/N)  
			//这里还用到了欧拉公式  
			for (int i = 0; i < countN / 2; i++) {
				double angle = -i * Math.PI * 2 / countN;
				w[i] = new Complex(Math.Cos(angle), Math.Sin(angle));
			}

			//蝶形运算  
			for (int i = 0; i < r; i++) {
				//i代表当前的迭代次数，r代表总共的迭代次数.  
				//i记录着迭代的重要信息.通过i可以算出当前迭代共有几个分组，每个分组的长度  

				//interval记录当前有几个组  
				// <<是左移操作符，左移一位相当于*2  
				//多使用位运算符可以人为提高算法速率^_^  
				int interval = 1 << i;

				//halfN记录当前循环每个组的长度N  
				int halfN = 1 << (r - i);

				//循环，依次对每个组进行蝶形运算  
				for (int j = 0; j < interval; j++) {
					//j代表第j个组  

					//gap=j*每组长度，代表着当前第j组的首元素的下标索引  
					int gap = j * halfN;

					//进行蝶形运算  
					for (int k = 0; k < halfN / 2; k++) {
						interVar2[k + gap] = interVar1[k + gap] + interVar1[k + gap + halfN / 2];
						interVar2[k + halfN / 2 + gap] = (interVar1[k + gap] - interVar1[k + gap + halfN / 2]) * w[k * interval];
					}
				}

				//将结果拷贝到输入端，为下次迭代做好准备  
				interVar1 = (Complex[])interVar2.Clone();
			}

			//将输出码位倒置  
			for (uint j = 0; j < countN; j++) {
				//j代表自然顺序的数组元素的下标索引  

				//用rev记录j码位倒置后的结果  
				uint rev = 0;
				//num作为中间变量  
				uint num = j;

				//码位倒置（通过将j的最右端一位最先放入rev右端，然后左移，然后将j的次右端一位放入rev右端，然后左移...）  
				//由于2的r次幂=N，所以任何j可由r位二进制数组表示，循环r次即可  
				for (int i = 0; i < r; i++) {
					rev <<= 1;
					rev |= num & 1;
					num >>= 1;
				}
				interVar2[rev] = interVar1[j];
			}
			return interVar2;

		} 
	}
}
