using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public class TransformerService:BaseService<Model.TransformerInfo> {
		public override void SetCurrentSession() {
			this.CurrentDal = new DEL.BaseDal<Model.TransformerInfo>();
		}
		public string ADD(Model.TransformerInfo entity)
		{
			 if(this.CurrentDal.AddEntity(entity)>0)
			 {
				 return "添加成功";
			 }
			else
			 {
				 return "添加失败";
			 }
		}
		public string Delete(Model.TransformerInfo entity) {
			if (this.CurrentDal.DeleteEntity(entity)) {
				return "删除成功";
			}
			else {
				return "删除失败";
			}
		}
		public List<Model.TransformerInfo> SelectAll() {
			return this.CurrentDal.LoadEntities(t => true).ToList<Model.TransformerInfo>();
		}
		public Model.TransformerInfo SelectByTransformerName(string CompanyName,string TransformerName) {
			 return this.CurrentDal.LoadEntities(t => t.CompanyName == CompanyName&&t.TransformerName == TransformerName).FirstOrDefault();
		}
		public bool ISCompanyExit(string CompanyName) {
			return this.CurrentDal.LoadEntities(t => t.CompanyName == CompanyName).ToList<Model.TransformerInfo>().Count>0;
		}
		public bool ISTransformerExit(string CompanyName,string TransformerName) {
			return this.CurrentDal.LoadEntities(t => t.CompanyName == CompanyName && t.TransformerName == TransformerName).ToList<Model.TransformerInfo>().Count > 0;
		}
	}
}
