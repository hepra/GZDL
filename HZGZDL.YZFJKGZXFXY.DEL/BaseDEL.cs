using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace HZGZDL.YZFJKGZXFXY.DEL {
	public class BaseDal<T> where T : class,new() {
	Model.myEFTransFormerInfo db { get{
		return DbsessionFactory.GetInstence();
	} }
		/// <summary>
		/// 插入数据库
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int AddEntity(T entity) {
			try {
				db.Set<T>().Add(entity);
				int  len =  db.SaveChanges();
				return len;
			}
			catch (System.Data.Entity.Infrastructure.DbUpdateException err) {
				return err.Message.Length;
			}
			
		}
		/// <summary>
		/// 删除
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool DeleteEntity(T entity) {
			db.Entry<T>(entity).State = System.Data.Entity.EntityState.Deleted;
			return db.SaveChanges() > 0;
		}
		/// <summary>
		/// 修改
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool EditEntity(T entity,T newEntity) {
			
			db.Entry<T>(entity).CurrentValues.SetValues(newEntity);
			return db.SaveChanges() > 0;
		}
		/// <summary>
		/// 修改
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="newEntity"></param>
		/// <returns></returns>
		public bool Modfiy(T Entity) {
			int itemCount = 0;
			if (db.Entry<T>(Entity).State == EntityState.Modified) {
				itemCount = db.SaveChanges();
			}
			else if (db.Entry<T>(Entity).State == EntityState.Detached) {
				try {
					db.Set<T>().Attach(Entity);
					db.Entry<T>(Entity).State = EntityState.Modified;
				}
				catch (InvalidOperationException) {
				}
				itemCount= db.SaveChanges();
			}
			return itemCount>0;  

		}
		/// <summary>
		/// 查询
		/// </summary>
		/// <param name="whereLambda"></param>
		/// <returns></returns>
		public IQueryable<T> LoadEntities(System.Linq.Expressions.Expression<Func<T, bool>> whereLambda) {
			return db.Set<T>().Where(whereLambda);
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
			var temp = db.Set<T>().Where(whereLambda);
			totalCount = temp.Count();
			if (isAsc) {
				temp = temp.OrderBy(orderByLambda).Skip((pageIndex - 1) * pageSize).Take(pageSize);
			}
			else {
				temp = temp.OrderByDescending(orderByLambda).Skip((pageIndex - 1) * pageSize).Take(pageSize);
			}
			return temp;
		}
		public bool SaveChange() {
			return db.SaveChanges() > 0;
		}
	}
}
