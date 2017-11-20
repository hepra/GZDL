using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.Model {
	static public class Commander {
		static private byte[] commanderConnect = {0,255,0,255};
		static public byte[] ConnectCode { get { return commanderConnect; } }
		static private byte[] commanderStart = {3,85,3,85};
		static public byte[] StartCode { get { return commanderStart; }  }

		static private byte[] commanderStop = {5,85,5,85};
	    static	public byte[] StopCode { get { return commanderStop; } }

		static private byte[] commanderGetData = {0x30,0xff,0x30,0xff};
		static public byte[] GetDataCode { get { return commanderGetData; } }

	}
}
