using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.DEL {
	public interface IBaseDal<T> where T : class,new() {
		IQueryable<T> LoadEntities(System.Linq.Expressions.Expression<Func<T, bool>> whereLambda);
		IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, System.Linq.Expressions.Expression<Func<T, bool>> whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderByLambda, bool isAsc);
		bool DeleteEntity(T entity);
		bool EditEntity(T entity,T newEntity);
		bool Modfiy(T Entity);
		int AddEntity(T entity);
		bool SaveChange();
	}
}
