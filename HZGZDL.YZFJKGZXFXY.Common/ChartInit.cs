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
			 double offset = (100 - (axisCount * 2)) / axisCount;
			 for (int i = 0; i < axisCount; i ++) {
				 temp.Add(offset*(i+1)+i*2);
			 }
			 return temp;
		}
	}
}
