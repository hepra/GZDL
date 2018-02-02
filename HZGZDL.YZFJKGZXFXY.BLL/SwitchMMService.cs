using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public class SwitchMMService:BaseService<Model.Switch_Manufactory_Model> {
		public override void SetCurrentSession() {
			this.CurrentDal = new DEL.SwitchMMDal();
		}
		public bool IsManufactoryExit(string ManufactoryName) {
			var temp = CurrentDal.LoadEntities(t => t.Manufactory == ManufactoryName).FirstOrDefault();
			if (temp == null) {
				return false;
			}
			return true;
		}
		public bool IsModelExit(string Model) {
			var temp = CurrentDal.LoadEntities(t => t.Model == Model).FirstOrDefault();
			if (temp == null) {
				return false;
			}
			return true;
		}
	}
}
