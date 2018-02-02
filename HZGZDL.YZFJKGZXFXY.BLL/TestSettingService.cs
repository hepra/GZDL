using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public class TestSettingService:BaseService<Model.ParameterInfo> {
		public override void SetCurrentSession() {
			this.CurrentDal = new DEL.TestSettingDal();
		}
		public bool IsExit(System.Linq.Expressions.Expression<Func<Model.ParameterInfo, bool>> whereLambda) {
			 return CurrentDal.LoadEntities(whereLambda).Count() > 0;
		}
	}
}
