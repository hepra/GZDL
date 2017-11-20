using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class Complex2D {
		double[] _Complex2D_Re;
		double[] _Complex2D_Im;
		public int Width { get; private set; }
		public int Height { get; private set; }
		public Complex this[int Row, int Column] {
			get {
				return new Complex(_Complex2D_Re[Row * Width + Column], _Complex2D_Im[Row * Width + Column]);
			}
			set {
				_Complex2D_Re[Row * Width + Column] = ((Complex)value).Re;
				_Complex2D_Im[Row * Width + Column] = ((Complex)value).Im;
			}
		}

		public Complex2D(int width, int height) {
			Width = width;
			Height = height;
			int lenght = Width * Height;
			_Complex2D_Re = new double[lenght];
			_Complex2D_Im = new double[lenght];
		}

		public void Clear() {
			Array.Clear(_Complex2D_Re, 0, _Complex2D_Re.Length);
			Array.Clear(_Complex2D_Im, 0, _Complex2D_Im.Length);
		}

		public ComplexList GetColumn(int index) {
			ComplexList ret = new ComplexList(Height);
			for (int i = 0; i < Height; i++) {
				ret[i] = this[i, index];
			}
			return ret;
		}
		public int SetColumn(int index, ComplexList src) {
			for (int i = 0; i < Height; i++) {
				this[i, index] = (i < src.Lenght) ? src[i] : new Complex(0);
			}
			return 0;
		}
		public ComplexList GetRow(int index) {
			ComplexList ret = new ComplexList(Width);
			for (int i = 0; i < Width; i++) {
				ret[i] = this[index, i];
			}
			return ret;
		}
		public int SetRow(int index, ComplexList src) {
			for (int i = 0; i < Width; i++) {
				this[index, i] = (i < src.Lenght) ? src[i] : new Complex(0);
			}
			return 0;
		}
		public Complex2D GetAmplitude() {
			Complex2D ret = new Complex2D(Width, Height);
			for (int i = 0; i < Height; i++) {
				for (int j = 0; j < Width; j++) {
					ret[i, j] = new Complex(this[i, j].Modulus());
				}
			}
			return ret;
		}
	}
}
