using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class ComplexList {
		double[] _ComplexList_Re;
		double[] _ComplexList_Im;
		public int Lenght { get; private set; }
		public Complex this[int Index] {
			get {
				return new Complex(_ComplexList_Re[Index], _ComplexList_Im[Index]);
			}
			set {
				_ComplexList_Re[Index] = ((Complex)value).Re;
				_ComplexList_Im[Index] = ((Complex)value).Im;
			}
		}

		public ComplexList(int lenght) {
			Lenght = lenght;
			_ComplexList_Re = new double[Lenght];
			_ComplexList_Im = new double[Lenght];
		}
		public ComplexList(double[] re) {
			Lenght = re.Length;
			_ComplexList_Re = re;
			_ComplexList_Im = new double[Lenght];
		}
		public ComplexList(double[] re, double[] im) {
			Lenght = Math.Max(re.Length, im.Length);
			if (re.Length == im.Length) {
				_ComplexList_Re = re;
				_ComplexList_Im = im;
			}
			else {
				_ComplexList_Re = new double[Lenght];
				_ComplexList_Im = new double[Lenght];
				for (int i = 0; i < re.Length; i++) _ComplexList_Re[i] = re[i];
				for (int i = 0; i < im.Length; i++) _ComplexList_Im[i] = im[i];
			}
		}

		public void Clear() {
			Array.Clear(_ComplexList_Re, 0, _ComplexList_Re.Length);
			Array.Clear(_ComplexList_Im, 0, _ComplexList_Im.Length);
		}

		public double[] GetRePtr() {
			return _ComplexList_Re;
		}
		public double[] GetImPtr() {
			return _ComplexList_Im;
		}
		public ComplexList Clone() {
			return new ComplexList((double[])(_ComplexList_Re.Clone()), (double[])(_ComplexList_Im.Clone()));
		}
		public ComplexList GetAmplitude() {
			double[] amp = new double[Lenght];
			for (int i = 0; i < Lenght; i++) {
				amp[i] = this[i].Modulus();
			}
			return new ComplexList(amp);
		}
	}
}
