using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public class TransAccessService:AccessBaseService<Model.TransformerInfo> {
		public override void Delete(Model.TransformerInfo entity, Func<Model.TransformerInfo, bool> whereLambda) {
			var tInfo = this.Select(whereLambda);
			if (tInfo == null) {
				return;
			}
			int id = tInfo.ID;
			this.Service.Delete(entity, id);
		}
		public override void Update(Model.TransformerInfo entity, Func<Model.TransformerInfo, bool> whereLambda) {
			var tInfo = this.Select(whereLambda);
			if (tInfo == null) {
				return;
			}
			int id = tInfo.ID;
			this.Service.Update(entity, id);
		}
	}
}
