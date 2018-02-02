using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class FreqAnalyzer {

		/// <summary>
		/// 求复数myComplex.Complex数组的模modul数组
		/// </summary>
		/// <param name="input">复数数组</param>
		/// <returns>模数组</returns>
		public static double[] Cmp2Mdl(myComplex.Complex[] input) {
			///有输入数组的长度确定输出数组的长度
			double[] output = new double[input.Length];

			///对所有输入复数求模
			for (int i = 0; i < input.Length; i++) {
				if (input[i].Real > 0) {
					output[i] = -input[i].ToModul();
				}
				else {
					output[i] = input[i].ToModul();
				}
			}

			///返回模数组
			return output;
		}
		/// <summary>
		/// 傅立叶变换或反变换，递归实现多级蝶形运算
		/// 作为反变换输出需要再除以序列的长度
		/// ！注意：输入此类的序列长度必须是2^n
		/// </summary>
		/// <param name="input">输入实序列</param>
		/// <param name="invert">false=正变换，true=反变换</param>
		/// <returns>傅立叶变换或反变换后的序列</returns>
		public static myComplex.Complex[] FFT(double[] input, bool invert) {
			///由输入序列确定输出序列的长度
			myComplex.Complex[] output = new myComplex.Complex[input.Length];

			///将输入的实数转为复数
			for (int i = 0; i < input.Length; i++) {
				output[i] = new myComplex.Complex(input[i]);
			}

			///返回FFT或IFFT后的序列
			return output = FFT(output, invert);
		}

		/// <summary>
		/// 傅立叶变换或反变换，递归实现多级蝶形运算
		/// 作为反变换输出需要再除以序列的长度
		/// ！注意：输入此类的序列长度必须是2^n
		/// </summary>
		/// <param name="input">复数输入序列</param>
		/// <param name="invert">false=正变换，true=反变换</param>
		/// <returns>傅立叶变换或反变换后的序列</returns>
		private static myComplex.Complex[] FFT(myComplex.Complex[] input, bool invert) {
			///输入序列只有一个元素，输出这个元素并返回
			if (input.Length == 1) {
				return new myComplex.Complex[] { input[0] };
			}

			///输入序列的长度
			int length = input.Length;

			///输入序列的长度的一半
			int half = length / 2;

			///有输入序列的长度确定输出序列的长度
			myComplex.Complex[] output = new myComplex.Complex[length];

			///正变换旋转因子的基数
			double fac = -2.0 * Math.PI / length;

			///反变换旋转因子的基数是正变换的相反数
			if (invert) {
				fac = -fac;
			}

			///序列中下标为偶数的点
			myComplex.Complex[] evens = new myComplex.Complex[half];

			for (int i = 0; i < half; i++) {
				evens[i] = input[2 * i];
			}

			///求偶数点FFT或IFFT的结果，递归实现多级蝶形运算
			myComplex.Complex[] evenResult = FFT(evens, invert);

			///序列中下标为奇数的点
			myComplex.Complex[] odds = new myComplex.Complex[half];

			for (int i = 0; i < half; i++) {
				odds[i] = input[2 * i + 1];
			}

			///求偶数点FFT或IFFT的结果，递归实现多级蝶形运算
			myComplex.Complex[] oddResult = FFT(odds, invert);

			for (int k = 0; k < half; k++) {
				///旋转因子
				double fack = fac * k;

				///进行蝶形运算
				myComplex.Complex oddPart = oddResult[k] * new myComplex.Complex(Math.Cos(fack), Math.Sin(fack));
				output[k] = evenResult[k] + oddPart;
				output[k + half] = evenResult[k] - oddPart;
			}

			///返回FFT或IFFT的结果
			return output;
		}

		/// <summary>
		/// 频域滤波器
		/// </summary>
		/// <param name="data">待滤波的数据</param>
		/// <param name="Nc">滤波器截止波数</param>
		/// <param name="Hd">滤波器的权函数</param>
		/// <returns>滤波后的数据</returns>
		public  static double[] FreqFilter(double[] data, myComplex.Complex[] Hd) {
			///最高波数==数据长度
			int N = data.Length;

			///输入数据进行FFT
			myComplex.Complex[] fData = FreqAnalyzer.FFT(data, false);

			///频域滤波
			for (int i = 0; i < N; i++) {
				fData[i] = Hd[i] * fData[i];
			}

			///滤波后数据计算IFFT
			///！未除以数据长度
			fData = FreqAnalyzer.FFT(fData, true);

			///复数转成模
			data = FreqAnalyzer.Cmp2Mdl(fData);

			///除以数据长度
			for (int i = 0; i < N; i++) {
				data[i] = -data[i] / N;
			}

			///返回滤波后的数据
			return data;
		}
	}
}
