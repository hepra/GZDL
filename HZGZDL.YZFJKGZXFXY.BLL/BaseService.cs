using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.BLL {
	public abstract class BaseService<T>where T:class,new() {
			public DEL.BaseDal<T> CurrentDal { get; set; }
			public abstract void SetCurrentSession();
			public BaseService() {
				SetCurrentSession();//子类一定要实现抽象方法
			}
			/// <summary>
			/// 插入数据库
			/// </summary>
			/// <param name="entity"></param>
			/// <returns></returns>
			public int AddEntity(T entity) {
				return CurrentDal.AddEntity(entity);
			}
			/// <summary>
			/// 删除
			/// </summary>
			/// <param name="entity"></param>
			/// <returns></returns>
			public bool DeleteEntity(T entity) {
				return CurrentDal.DeleteEntity(entity);
			}
			/// <summary>
			/// 修改
			/// </summary>
			/// <param name="entity"></param>
			/// <returns></returns>
			public bool EditEntity(T entity,T newEntity) {
				return CurrentDal.EditEntity(entity,newEntity);
			}
		/// <summary>
		/// 修改
		/// </summary>
		/// <param name="Entity"></param>
		/// <param name="proprityName"></param>
		/// <returns></returns>
			public bool Modfiy(T Entity) {
				return CurrentDal.Modfiy(Entity);
			}
			/// <summary>
			/// 查询
			/// </summary>
			/// <param name="whereLambda"></param>
			/// <returns></returns>
			public List<T> LoadEntities(System.Linq.Expressions.Expression<Func<T, bool>> whereLambda) {
				return CurrentDal.LoadEntities(whereLambda).ToList<T>();
			}
			/// <summary>
			/// 分页查询
			/// </summary>
			/// <typeparam name="S"></typeparam>
			/// <param name="pageIndex"></param>
			/// <param name="pageSize"></param>
			/// <param name="totalCount"></param>
			/// <param name="whereLambda"></param>
			/// <param name="orderByLambda"></param>
			/// <param name="isAsc"></param>
			/// <returns></returns>
			public IQueryable<T> LoadPageEntities<S>(int pageIndex, int pageSize, out int totalCount, System.Linq.Expressions.Expression<Func<T, bool>> whereLambda, System.Linq.Expressions.Expression<Func<T, S>> orderByLambda, bool isAsc) {
				return CurrentDal.LoadPageEntities<S>(pageIndex, pageSize, out totalCount, whereLambda, orderByLambda, isAsc); ;
			}
			public bool SaveChange() {
				return CurrentDal.SaveChange();
			}
	}
}
