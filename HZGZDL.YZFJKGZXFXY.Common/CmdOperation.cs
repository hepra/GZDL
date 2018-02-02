using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class CmdOperation {
		//Shell打开 文件
		static string RootPath = (System.AppDomain.CurrentDomain.BaseDirectory);
		static public bool CmdOpenLog() {
			Process proc = null;
			try {
				proc = new Process();
				proc.StartInfo.WorkingDirectory = RootPath;
				proc.StartInfo.FileName = "SystemLog.txt";
				proc.StartInfo.CreateNoWindow = true;
				proc.Start();
				proc.WaitForExit();
				proc.Close();
				return true;
			}
			catch (Exception ex) {
				return false;
			}
		}
	}
}
