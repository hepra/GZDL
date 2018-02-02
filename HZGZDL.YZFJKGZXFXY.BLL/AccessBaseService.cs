using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public abstract class AccessBaseService<T> where T:class,new() {
		public Common.AccessDbSetHelper<T> Service = new Common.AccessDbSetHelper<T>();
		public T Select(Func<T, bool> whereLambda) {
			return Service.Select(whereLambda);
		}
		public List<T> SelectList(Func<T, bool> whereLambda) {
			return Service.SelectList(whereLambda);
		}
		public void Insert(T entity) {
			Service.Insert(entity);
		}
		public abstract void Update(T entity, Func<T, bool> whereLambda);
		public abstract void Delete(T entity, Func<T, bool> whereLambda);
	}
}
