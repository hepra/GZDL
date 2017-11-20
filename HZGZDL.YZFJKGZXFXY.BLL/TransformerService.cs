using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public class TransformerService {
		DEL.TransFormerDal dbService = new DEL.TransFormerDal();
		public string ADD(Model.TransformerInfo entity)
		{
			 if(dbService.AddEntity(entity)>0)
			 {
				 return "添加成功";
			 }
			else
			 {
				 return "添加失败";
			 }
		}
		public string Delete(Model.TransformerInfo entity) {
			if (dbService.DeleteEntity(entity)) {
				return "删除成功";
			}
			else {
				return "删除失败";
			}
		}
		public string Edit(Model.TransformerInfo entity) {
			if (dbService.EditEntity(entity)) {
				return "修改成功";
			}
			else {
				return "修改失败";
			}
		}
		public Model.TransformerInfo SelectByCompanyName(string CompanyName) {
			return dbService.LoadEntities(t => t.CompanyName == CompanyName).FirstOrDefault();
		}
	}
}
