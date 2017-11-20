using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.DEL {
	public class DbsessionFactory {
		static private Model.myEFTransFormerInfo db;
		static public Model.myEFTransFormerInfo GetInstence()
		{
			if (db == null) {
				db = new Model.myEFTransFormerInfo();
			}
			return db;
		}
	}
}
