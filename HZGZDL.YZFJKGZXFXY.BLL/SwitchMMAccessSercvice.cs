using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public class SwitchMMAccessSercvice:AccessBaseService<Model.Switch_Manufactory_Model> {
		public override void Delete(Model.Switch_Manufactory_Model entity, Func<Model.Switch_Manufactory_Model, bool> whereLambda) {
			var tInfo = this.Select(whereLambda);
			if (tInfo == null) {
				return;
			}
			int id = tInfo.ID;
			this.Service.Delete(entity, id);
		}
		public override void Update(Model.Switch_Manufactory_Model entity, Func<Model.Switch_Manufactory_Model, bool> whereLambda) {
			var tInfo = this.Select(whereLambda);
			if (tInfo == null) {
				return;
			}
			int id = tInfo.ID;
			this.Service.Update(entity, id);
		}
	}
}
