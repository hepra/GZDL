using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class Complex {
		public double Re;
		public double Im;
		public Complex() {
			Re = 0;
			Im = 0;
		}
		public Complex(double re) {
			Re = re;
			Im = 0;
		}
		public Complex(double re, double im) {
			Re = re;
			Im = im;
		}

		public double Modulus() {
			return Math.Sqrt(Re * Re + Im * Im);
		}

		public override string ToString() {
			string retStr;
			if (Math.Abs(Im) < 0.0001) retStr = Re.ToString("f4");
			else if (Math.Abs(Re) < 0.0001) {
				if (Im > 0) retStr = "j" + Im.ToString("f4");
				else retStr = "- j" + (0 - Im).ToString("f4");
			}
			else {
				if (Im > 0) retStr = Re.ToString("f4") + "+ j" + Im.ToString("f4");
				else retStr = Re.ToString("f4") + "- j" + (0 - Im).ToString("f4");
			}
			retStr += " ";
			return retStr;
		}

		//操作符重载
		public static Complex operator +(Complex c1, Complex c2) {
			return new Complex(c1.Re + c2.Re, c1.Im + c2.Im);
		}
		public static Complex operator +(double d, Complex c) {
			return new Complex(d + c.Re, c.Im);
		}
		public static Complex operator -(Complex c1, Complex c2) {
			return new Complex(c1.Re - c2.Re, c1.Im - c2.Im);
		}
		public static Complex operator -(double d, Complex c) {
			return new Complex(d - c.Re, -c.Im);
		}
		public static Complex operator *(Complex c1, Complex c2) {
			return new Complex(c1.Re * c2.Re - c1.Im * c2.Im, c1.Re * c2.Im + c2.Re * c1.Im);
		}
		public static Complex operator *(Complex c, double d) {
			return new Complex(c.Re * d, c.Im * d);
		}
		public static Complex operator *(double d, Complex c) {
			return new Complex(c.Re * d, c.Im * d);
		}
		public static Complex operator /(Complex c, double d) {
			return new Complex(c.Re / d, c.Im / d);
		}
		public static Complex operator /(double d, Complex c) {
			double temp = d / (c.Re * c.Re + c.Im * c.Im);
			return new Complex(c.Re * temp, -c.Im * temp);
		}
		public static Complex operator /(Complex c1, Complex c2) {
			double temp = 1 / (c2.Re * c2.Re + c2.Im * c2.Im);
			return new Complex((c1.Re * c2.Re + c1.Im * c2.Im) * temp, (-c1.Re * c2.Im + c2.Re * c1.Im) * temp);
		}
	}
}
