using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Steema.TeeChart.WPF;
using Steema.TeeChart.WPF.Styles;

namespace HZGZDL.YZFJKGZXFXY.Common {
	 public class ChartInit {
		 static public List<double> GetAxisPos(int axisCount) {
			 List<double> temp = new List<double>();
			 int gap = 5;
			 double offset = (100 - (axisCount * gap)) / axisCount;
			 for (int i = 0; i < axisCount; i ++) {
				 temp.Add(offset * (i + 1) + i * gap);
			 }
			 return temp;
		}
	}
}
