using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ADOX;

namespace HZGZDL.YZFJKGZXFXY.Common {
	/// <summary>
	/// Access操作类
	/// </summary>
	public class AccessDbSetHelper<T> where T:class,new() {
			//Database connection strings
			
			private static StringBuilder strCmd_Select = new StringBuilder();
			private static StringBuilder strCmd_Insert = new StringBuilder();
			private static StringBuilder strCmd_Update = new StringBuilder();
			private static StringBuilder strCmd_Delete = new StringBuilder();
			static string root_path = (System.AppDomain.CurrentDomain.BaseDirectory) + "DataBase\\";
			static string mdb_path = root_path + "Fra.mdb";
			private static readonly string strConnection = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + mdb_path;

			// Hashtable to store cached parameters
			private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

			#region 创建数据库 和表
			public bool CreateDataBase(T entity) {
				//TransFormer 参数初始化
				Catalog catalog = new Catalog();
				if (!Directory.Exists(root_path)) {
					Directory.CreateDirectory(root_path);
				}
				//数据库不存在则创建
				if (!File.Exists(mdb_path)) {
					try {
						catalog.Create(strConnection);
					}
					catch (System.Exception ex) {
						MessageBox.Show("Access数据库初始化失败:" + ex.Message + "\r\n准备安装数据库组件:\r\n" + root_path + "Lib\r\n安装后请重启软件");
						RunCmd2("");
						System.Windows.Application.Current.Shutdown(0);
						System.Environment.Exit(0);
						System.Environment.Exit(0);
					}
				}
				//创建表格
				try {
					//测试信息表格
					CreateTable(entity, catalog);
				}
				//若异常 则表格已存在
				catch (Exception ex) {
					MessageBox.Show("OleHelper行号58:" + ex.Message);
					return false;
				}
				return true;
			}
			void CreateTable(T entity, Catalog catalog) {

				ADODB.Connection cn = new ADODB.Connection();
				cn.Open(strConnection, null, null, -1);
				catalog.ActiveConnection = cn;
				Table table = new Table();

				var DI = ClassPropertyHelper.GetPropertyNameAndValue<T>(entity);
				var DIT = ClassPropertyHelper.GetPropertyNameAndType<T>(entity);
				table.Name = ClassPropertyHelper.GetClassName<T>(entity);
				for (int i = 0; i < DI.Count; i++) {
					if (DI.ElementAt(i).Key == "ID") {
						ADOX.Column column = new ADOX.Column();
						column.ParentCatalog = catalog;
						column.Name = "ID";
						column.Type = DataTypeEnum.adInteger;
						column.DefinedSize = 32;
						column.Properties["AutoIncrement"].Value = true;
						table.Columns.Append(column, DataTypeEnum.adInteger, 32);
						table.Keys.Append("PrimaryKey", ADOX.KeyTypeEnum.adKeyPrimary, "ID", "", ""); 
					}
					else {
						table.Columns.Append(DI.ElementAt(i).Key, ConverType(DIT.ElementAt(i).Value), 64);
					}
					
				}
				try {
					catalog.Tables.Append(table);
				}
				catch (Exception ex) {
					//MessageBox.Show("OleHelper行号92:" + ex.Message);
				}
				finally {
					cn.Close();
				}
			}

			#endregion

			#region 通用的实体增删查改
			#region 拼接CMD字符串
			//插入命令创建
			 string Create_CMD_Insert(T entity) {
				strCmd_Insert.Clear();
				strCmd_Insert.Append("insert into [" + ClassPropertyHelper.GetClassName<T>(entity) + "]([");
				var DI = ClassPropertyHelper.GetPropertyNameAndValue<T>(entity);
				for (int i = 0; i < DI.Count; i++) {
					if (DI.ElementAt(i).Key == "ID") {
						continue;
					}
					if (i == DI.Count - 1) {
						strCmd_Insert.Append(DI.ElementAt(i).Key + "])");
						break;
					}
					strCmd_Insert.Append(DI.ElementAt(i).Key + "],[");
				}
				strCmd_Insert.Append(" values(");
				for (int i = 0; i < DI.Count; i++) {
					if (DI.ElementAt(i).Key == "ID") {
						continue;
					}
					if (i == DI.Count - 1) {
						strCmd_Insert.Append("@"+ DI.ElementAt(i).Key + ")");
						break;
					}
					strCmd_Insert.Append("@" + DI.ElementAt(i).Key + ",");
				}
				return strCmd_Insert.ToString();
			}

			//Update命令创建
			 string Create_CMD_Update(T entity,int Id) {
				strCmd_Update.Clear();
				strCmd_Update.Append("update  [" + ClassPropertyHelper.GetClassName<T>(entity) + "]  set  ");

				var DI = ClassPropertyHelper.GetPropertyNameAndValue<T>(entity);
				for (int j = 0; j < DI.Count; j++) {
					if (DI.ElementAt(j).Key == "ID") {
						continue;
					}
					if (j == DI.Count - 1) {
						strCmd_Update.Append("["+DI.ElementAt(j).Key + "]=@" + DI.ElementAt(j).Key + "");
						break;
					}
					strCmd_Update.Append("[" + DI.ElementAt(j).Key + "]=@" + DI.ElementAt(j).Key + ", ");
				}
				strCmd_Update.Append("   where  [ID]=@ID");
				return strCmd_Update.ToString();
			}

			//Delete删除命令创建
			 string Create_CMD_Delete(T entity ,int Id) {
				strCmd_Delete.Clear();
				strCmd_Delete.Append("delete from [" + ClassPropertyHelper.GetClassName<T>(entity));
				strCmd_Delete.Append("]  where ID="+Id);
				return strCmd_Delete.ToString();
			}
			#endregion

			#region 类型转换
			OleDbType ConverOleType(string SystemType) {
				switch (SystemType) {
					case "Int32": return OleDbType.Integer;
					case "String": return OleDbType.LongVarWChar;
					case "Double": return OleDbType.Double;
					case "DateTime": return OleDbType.Date;
					case "Boolean": return OleDbType.Boolean;
					default: return OleDbType.LongVarWChar;
				}
			}
			DataTypeEnum ConverType(string SystemType) {
				switch (SystemType) {
					case "Int32": return DataTypeEnum.adInteger;
					case "String": return DataTypeEnum.adLongVarWChar;
					case "Double": return DataTypeEnum.adDouble;
					case "DateTime": return DataTypeEnum.adDate;
					case "Boolean": return DataTypeEnum.adBoolean;
					default: return DataTypeEnum.adLongVarWChar;
				}
			}
			#endregion

			#region 创建Parmeters
			OleDbParameter[] CreateParameters(T entity) {
				var DI = ClassPropertyHelper.GetPropertyNameAndValue<T>(entity);
				var DIT = ClassPropertyHelper.GetPropertyNameAndType<T>(entity);
				OleDbParameter[] parameters = new OleDbParameter[DI.Count-1];
				for (int i = 0; i < DI.Count-1; i++) {
					OleDbParameter para = new OleDbParameter();
					para.OleDbType = ConverOleType(DIT.ElementAt(i+1).Value);
					para.ParameterName = DI.ElementAt(i+1).Key;
					para.Value = StingToSysType(DIT.ElementAt(i+1).Value,DI.ElementAt(i+1).Value);
					parameters[i] = para;
				}
					return parameters;
			}
			object StingToSysType(string type, string value) {
				switch (type) {
					case "Int32": return int.Parse(value);
					case "String": return value;
					case "Double": return double.Parse(value);
					case "DateTime": return DateTime.Parse(value);
					case "Boolean": return Boolean.Parse(value);
					default: return value;
				}
			}
			#endregion

			#region 插入
			 public void Insert(T entity) {
				ExecuteNonQuery(Create_CMD_Insert(entity),CreateParameters(entity));
			}
			#endregion

			#region 查询数据  重载
			 public T Select(Func<T, bool> whereLambda) {
				 var list = SelectAll();

				 var entity = list.Where<T>(whereLambda).Select(T=>T);
				 return entity.FirstOrDefault();
			 }
			 public List<T> SelectList(Func<T, bool> whereLambda) {
				 var list = SelectAll();

				 var entity = list.Where<T>(whereLambda).Select(T => T);
				 return entity.ToList<T>();
			 }

			/// <summary>
			/// 查询变压器参数
			/// </summary>
			/// <param name="Filed_Name"></param>
			/// <param name="Filed_Value"></param>
			/// <returns></returns>
			 public List<T> SelectAll() {
				strCmd_Select.Clear();
				T entity = new T();
				strCmd_Select.Append("select * from  [" + ClassPropertyHelper.GetClassName<T>(entity) + " ] ");
				return PutAllVal(entity, ExecuteDataSet(strCmd_Select.ToString()));
			}
			#endregion

			#region 修改数据
			public  void Update(T entity,int Id) {
				try {
						var DI = ClassPropertyHelper.GetPropertyNameAndValue<T>(entity);
						var DIT = ClassPropertyHelper.GetPropertyNameAndType<T>(entity);
						OleDbParameter[] parameters = new OleDbParameter[DI.Count];
					var  temp = CreateParameters(entity);
					for(int i=0;i<DI.Count-1;i++)
					{
						parameters[i]=temp[i];
					}
					OleDbParameter para = new OleDbParameter();
					para.OleDbType =	 OleDbType.Integer;
					para.ParameterName ="ID";
					para.Value = Id;
					parameters[DI.Count-1] = para;

					ExecuteNonQuery(Create_CMD_Update(entity, Id), parameters);
				}
				catch (Exception error) {
					MessageBox.Show(error.Message);
				}
			}
			#endregion

			#region 删除数据
			public  void Delete(T entity,int Id) {
				try {
					ExecuteNonQuery(Create_CMD_Delete(entity,Id));
				}
				catch (Exception error) {
					MessageBox.Show(error.Message);
				}
			}
			#endregion


			#region DataSetToModel

			  List<T> PutAllVal(T entity, DataSet ds) {
				List<T> lists = new List<T>();
				if (ds.Tables[0].Rows.Count > 0) {
					foreach (DataRow row in ds.Tables[0].Rows) {
						lists.Add(PutVal(new T(), row));
					}
				}
				return lists;
			}
			  T PutVal(T entity, DataRow row){
				//初始化 如果为null
				if (entity == null) {
					entity = new T();
				}
				//得到类型
				Type type = typeof(T);
				//取得属性集合
				PropertyInfo[] pi = type.GetProperties();
				foreach (PropertyInfo item in pi) {
					//给属性赋值
					if (row[item.Name] != null && row[item.Name] != DBNull.Value) {
						if (item.PropertyType == typeof(System.Nullable<System.DateTime>)) {
							item.SetValue(entity, Convert.ToDateTime(row[item.Name].ToString()), null);
						}
						else {
							item.SetValue(entity, Convert.ChangeType(row[item.Name], item.PropertyType), null);
						}
					}
				}
				return entity;
			}
			#endregion

			#endregion

			#region shell安装Access环境
			static bool RunCmd2(string cmdStr) {
				Process proc = null;
				try {
					proc = new Process();
					proc.StartInfo.WorkingDirectory = root_path + "Libs";
					proc.StartInfo.FileName = "AccessDatabaseEngine.exe";
					//proc.StartInfo.Arguments = string.Format("10");//this is argument
					// proc.StartInfo.CreateNoWindow = true;
					proc.Start();
					// proc.StandardInput.AutoFlush = true;
					proc.WaitForExit();
					proc.Close();
					return true;
				}
				catch (Exception ex) {
					return false;
				}
			}
			
			#endregion

			#region ExecuteNonQuery
			/**/
			/// <summary>
			/// Execute a OleDbCommand (that returns no resultset) against the database specified in the connection string 
			/// using the provided parameters.
			/// </summary>
			/// <remarks>
			/// e.g.:  
			///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
			/// </remarks>
			/// <param name="connectionString">a valid connection string for a OleDbConnection</param>
			/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
			/// <param name="commandText">the stored procedure name or T-OleDb command</param>
			/// <param name="commandParameters">an array of OleDbParamters used to execute the command</param>
			/// <returns>an int representing the number of rows affected by the command</returns>
			public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {

				OleDbCommand cmd = new OleDbCommand();

				using (OleDbConnection conn = new OleDbConnection(connectionString)) {
					PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
					int val = cmd.ExecuteNonQuery();
					cmd.Parameters.Clear();
					return val;
				}
			}

			/**/
			/// <summary>
			/// 使用默认连接
			/// </summary>
			/// <param name="cmdType">命令文本类型</param>
			/// <param name="cmdText">命令文本</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>int</returns>
			public static int ExecuteNonQuery(CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();

				using (OleDbConnection conn = new OleDbConnection(strConnection)) {
					PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
					int val = cmd.ExecuteNonQuery();
					cmd.Parameters.Clear();
					return val;
				}
			}

			/**/
			/// <summary>
			/// 使用默认连接,CommandType默认为StoredProcedure
			/// </summary>
			/// <param name="cmdText">存储过程名</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>int</returns>
			private static int ExecuteNonQuery(string cmdText, params OleDbParameter[] commandParameters) {

				OleDbCommand cmd = new OleDbCommand();

				using (OleDbConnection conn = new OleDbConnection(strConnection)) {
					PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, commandParameters);
					int val = cmd.ExecuteNonQuery();
					cmd.Parameters.Clear();
					return val;
				}
			}
			/**/
			/// <summary>
			/// Execute a OleDbCommand (that returns no resultset) against an existing database connection 
			/// using the provided parameters.
			/// </summary>
			/// <remarks>
			/// e.g.:  
			///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
			/// </remarks>
			/// <param name="conn">an existing database connection</param>
			/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
			/// <param name="commandText">the stored procedure name or T-OleDb command</param>
			/// <param name="commandParameters">an array of OleDbParamters used to execute the command</param>
			/// <returns>an int representing the number of rows affected by the command</returns>
			public static int ExecuteNonQuery(OleDbConnection connection, CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {

				OleDbCommand cmd = new OleDbCommand();

				PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
				int val = cmd.ExecuteNonQuery();
				cmd.Parameters.Clear();
				return val;
			}

			/**/
			/// <summary>
			/// Execute a OleDbCommand (that returns no resultset) using an existing OleDb Transaction 
			/// using the provided parameters.
			/// </summary>
			/// <remarks>
			/// e.g.:  
			///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
			/// </remarks>
			/// <param name="trans">an existing OleDb transaction</param>
			/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
			/// <param name="commandText">the stored procedure name or T-OleDb command</param>
			/// <param name="commandParameters">an array of OleDbParamters used to execute the command</param>
			/// <returns>an int representing the number of rows affected by the command</returns>
			public static int ExecuteNonQuery(OleDbTransaction trans, CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();
				PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
				int val = cmd.ExecuteNonQuery();
				cmd.Parameters.Clear();
				return val;
			}
			public static int ExecuteNonQuery(string cmdText) {
				using (OleDbConnection conn = new OleDbConnection(strConnection)) {
					OleDbCommand cmd = new OleDbCommand();
					cmd.CommandText = cmdText;
					cmd.CommandType = CommandType.Text;
					if (conn.State != ConnectionState.Open)
						conn.Open();
					cmd.Connection = conn;
					return cmd.ExecuteNonQuery();
				}
			}

			/**/
			/// <summary>
			/// Execute a OleDbCommand that returns a resultset against the database specified in the connection string 
			/// using the provided parameters.
			/// </summary>
			/// <remarks>
			/// e.g.:  
			///  OleDbDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
			/// </remarks>
			/// <param name="connectionString">a valid connection string for a OleDbConnection</param>
			/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
			/// <param name="commandText">the stored procedure name or T-OleDb command</param>
			/// <param name="commandParameters">an array of OleDbParamters used to execute the command</param>
			/// <returns>A OleDbDataReader containing the results</returns>

			#endregion

			#region ExecuteReader
			public static OleDbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();
				OleDbConnection conn = new OleDbConnection(connectionString);

				// we use a try/catch here because if the method throws an exception we want to 
				// close the connection throw code, because no datareader will exist, hence the 
				// commandBehaviour.CloseConnection will not work
				try {
					PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
					OleDbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					cmd.Parameters.Clear();
					return rdr;
				}
				catch {
					conn.Close();
					throw;
				}
			}

			/**/
			/// <summary>
			/// 使用默认连接
			/// </summary>
			/// <param name="cmdType">命令文本类型</param>
			/// <param name="cmdText">命令文本</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>OleDbDataReader</returns>
			public static OleDbDataReader ExecuteReader(CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();
				OleDbConnection conn = new OleDbConnection(strConnection);

				// we use a try/catch here because if the method throws an exception we want to 
				// close the connection throw code, because no datareader will exist, hence the 
				// commandBehaviour.CloseConnection will not work
				try {
					PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
					OleDbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					cmd.Parameters.Clear();
					return rdr;
				}
				catch {
					conn.Close();
					throw;
				}
			}

			/**/
			/// <summary>
			/// 使用默认连接,CommandType默认为StoredProcedure
			/// </summary>
			/// <param name="cmdText">存储过程名</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>OleDbDataReader</returns>
			public static OleDbDataReader ExecuteReader(string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();
				OleDbConnection conn = new OleDbConnection(strConnection);

				// we use a try/catch here because if the method throws an exception we want to 
				// close the connection throw code, because no datareader will exist, hence the 
				// commandBehaviour.CloseConnection will not work
				try {
					PrepareCommand(cmd, conn, null, CommandType.StoredProcedure, cmdText, commandParameters);
					OleDbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					cmd.Parameters.Clear();
					return rdr;
				}
				catch {
					conn.Close();
					throw;
				}
			}
			#endregion

			#region ExecuteScalar
			/**/
			/// <summary>
			/// Execute a OleDbCommand that returns the first column of the first record against the database specified in the connection string 
			/// using the provided parameters.
			/// </summary>
			/// <remarks>
			/// e.g.:  
			///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
			/// </remarks>
			/// <param name="connectionString">a valid connection string for a OleDbConnection</param>
			/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
			/// <param name="commandText">the stored procedure name or T-OleDb command</param>
			/// <param name="commandParameters">an array of OleDbParamters used to execute the command</param>
			/// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
			public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();

				using (OleDbConnection connection = new OleDbConnection(connectionString)) {
					PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
					object val = cmd.ExecuteScalar();
					cmd.Parameters.Clear();
					return val;
				}
			}
			/**/
			/// <summary>
			/// 使用定义好的连接字符串
			/// </summary>
			/// <param name="cmdType">命令文本类型</param>
			/// <param name="cmdText">命令文本</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>object</returns>
			public static object ExecuteScalar(CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();

				using (OleDbConnection connection = new OleDbConnection(strConnection)) {
					PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
					object val = cmd.ExecuteScalar();
					cmd.Parameters.Clear();
					return val;
				}
			}
			/**/
			/// <summary>
			/// 使用定义好的连接字符串,CommandType默认为StoredProcedure
			/// </summary>
			/// <param name="cmdText">存储过程名</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>object</returns>
			public static object ExecuteScalar(string cmdText, params OleDbParameter[] commandParameters) {
				OleDbCommand cmd = new OleDbCommand();

				using (OleDbConnection connection = new OleDbConnection(strConnection)) {
					PrepareCommand(cmd, connection, null, CommandType.StoredProcedure, cmdText, commandParameters);
					object val = cmd.ExecuteScalar();
					cmd.Parameters.Clear();
					return val;
				}
			}
			/**/
			/// <summary>
			/// Execute a OleDbCommand that returns the first column of the first record against an existing database connection 
			/// using the provided parameters.
			/// </summary>
			/// <remarks>
			/// e.g.:  
			///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
			/// </remarks>
			/// <param name="conn">an existing database connection</param>
			/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
			/// <param name="commandText">the stored procedure name or T-OleDb command</param>
			/// <param name="commandParameters">an array of OleDbParamters used to execute the command</param>
			/// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
			public static object ExecuteScalar(OleDbConnection connection, CommandType cmdType, string cmdText, params OleDbParameter[] commandParameters) {

				OleDbCommand cmd = new OleDbCommand();

				PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
				object val = cmd.ExecuteScalar();
				cmd.Parameters.Clear();
				return val;
			}

			#endregion

			#region ExecuteDataSet
			/**/
			/// <summary>
			/// 返加dataset
			/// </summary>
			/// <param name="connectionString">连接字符串</param>
			/// <param name="cmdType">命令类型，如StoredProcedure,Text</param>
			/// <param name="cmdText">the stored procedure name or T-OleDb command</param>
			/// <returns>DataSet</returns>
			public static DataSet ExecuteDataSet(string connectionString, CommandType cmdType, string cmdText) {
				OleDbConnection OleDbDataConn = new OleDbConnection(connectionString);
				OleDbCommand OleDbComm = new OleDbCommand(cmdText, OleDbDataConn);
				OleDbComm.CommandType = cmdType;
				OleDbDataAdapter OleDbDA = new OleDbDataAdapter(OleDbComm);
				DataSet DS = new DataSet();
				OleDbDA.Fill(DS);
				return DS;
			}

			/**/
			/// <summary>
			/// 使用定义好的连接字符串
			/// </summary>
			/// <param name="cmdType">命令文本类型</param>
			/// <param name="cmdText">命令文本</param>
			/// <returns>DataSet</returns>
			public static DataSet ExecuteDataSet(CommandType cmdType, string cmdText) {
				OleDbConnection OleDbDataConn = new OleDbConnection(strConnection);
				OleDbCommand OleDbComm = new OleDbCommand(cmdText, OleDbDataConn);
				OleDbComm.CommandType = cmdType;
				OleDbDataAdapter OleDbDA = new OleDbDataAdapter(OleDbComm);
				DataSet DS = new DataSet();
				OleDbDA.Fill(DS);
				return DS;
			}
			/**/
			/// <summary>
			/// 使用定义好的连接字符串,CommandType默认为StoredProcedure
			/// </summary>
			/// <param name="cmdText">存储过程名</param>
			/// <returns>object</returns>
			public static DataSet ExecuteDataSet(string cmdText) {
				OleDbConnection OleDbDataConn = new OleDbConnection(strConnection);
				OleDbCommand OleDbComm = new OleDbCommand(cmdText, OleDbDataConn);
				OleDbComm.CommandType = CommandType.Text;
				OleDbDataAdapter OleDbDA = new OleDbDataAdapter(OleDbComm);
				DataSet DS = new DataSet();
				OleDbDA.Fill(DS);
				return DS;
			}
			/**/
			/// <summary>
			/// 返加dataset
			/// </summary>
			/// <param name="connectionString">连接字符串</param>
			/// <param name="cmdType">命令类型，如StoredProcedure,Text</param>
			/// <param name="cmdText">the stored procedure name or T-OleDb command</param>
			/// <param name="OleDbparams">参数集</param>
			/// <returns>DataSet</returns>
			public static DataSet ExecuteDataSet(string connectionString, CommandType cmdType, string cmdText, params OleDbParameter[] OleDbparams) {
				OleDbConnection OleDbDataConn = new OleDbConnection(connectionString);
				OleDbCommand OleDbComm = AddOleDbParas(OleDbparams, cmdText, cmdType, OleDbDataConn);
				OleDbDataAdapter OleDbDA = new OleDbDataAdapter(OleDbComm);
				DataSet DS = new DataSet();
				OleDbDA.Fill(DS);
				return DS;
			}

			public static DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params OleDbParameter[] OleDbparams) {
				OleDbConnection OleDbDataConn = new OleDbConnection(strConnection);
				OleDbCommand OleDbComm = AddOleDbParas(OleDbparams, cmdText, cmdType, OleDbDataConn);
				OleDbDataAdapter OleDbDA = new OleDbDataAdapter(OleDbComm);
				DataSet DS = new DataSet();
				OleDbDA.Fill(DS);
				return DS;
			}
			/**/
			/// <summary>
			/// 使用定义好的连接字符串,CommandType默认为StoredProcedure
			/// </summary>
			/// <param name="cmdText">存储过程名</param>
			/// <param name="commandParameters">参数集</param>
			/// <returns>DataSet</returns>
			public static DataSet ExecuteDataSet(string cmdText, params OleDbParameter[] OleDbparams) {
				OleDbConnection OleDbDataConn = new OleDbConnection(strConnection);
				OleDbCommand OleDbComm = AddOleDbParas(OleDbparams, cmdText, CommandType.Text, OleDbDataConn);
				OleDbDataAdapter OleDbDA = new OleDbDataAdapter(OleDbComm);
				DataSet DS = new DataSet();
				OleDbDA.Fill(DS);
				return DS;
			}
			#endregion

			#region CacheParameters
			/**/
			/// <summary>
			/// add parameter array to the cache
			/// </summary>
			/// <param name="cacheKey">Key to the parameter cache</param>
			/// <param name="cmdParms">an array of OleDbParamters to be cached</param>
			public static void CacheParameters(string cacheKey, params OleDbParameter[] commandParameters) {
				parmCache[cacheKey] = commandParameters;
			}

			#endregion

			#region GetCachedParameters
			/**/
			/// <summary>
			/// Retrieve cached parameters
			/// </summary>
			/// <param name="cacheKey">key used to lookup parameters</param>
			/// <returns>Cached OleDbParamters array</returns>
			public static OleDbParameter[] GetCachedParameters(string cacheKey) {
				OleDbParameter[] cachedParms = (OleDbParameter[])parmCache[cacheKey];

				if (cachedParms == null)
					return null;

				OleDbParameter[] clonedParms = new OleDbParameter[cachedParms.Length];

				for (int i = 0, j = cachedParms.Length; i < j; i++)
					clonedParms[i] = (OleDbParameter)((ICloneable)cachedParms[i]).Clone();

				return clonedParms;
			}

			#endregion

			#region PrepareCommand
			/**/
			/// <summary>
			/// Prepare a command for execution
			/// </summary>
			/// <param name="cmd">OleDbCommand object</param>
			/// <param name="conn">OleDbConnection object</param>
			/// <param name="trans">OleDbTransaction object</param>
			/// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
			/// <param name="cmdText">Command text, e.g. Select * from Products</param>
			/// <param name="cmdParms">OleDbParameters to use in the command</param>
			private static void PrepareCommand(OleDbCommand cmd, OleDbConnection conn, OleDbTransaction trans, CommandType cmdType, string cmdText, OleDbParameter[] cmdParms) {

				if (conn.State != ConnectionState.Open)
					conn.Open();

				cmd.Connection = conn;
				cmd.CommandText = cmdText;

				if (trans != null)
					cmd.Transaction = trans;

				cmd.CommandType = cmdType;

				if (cmdParms != null) {
					foreach (OleDbParameter parm in cmdParms)
						cmd.Parameters.Add(parm);
				}
			}

			#endregion

			#region AddOleDbParas
			/**/
			/// <summary>
			/// 获得一个完整的Command
			/// </summary>
			/// <param name="OleDbParas">OleDb的参数数组</param>
			/// <param name="CommandText">命令文本</param>
			/// <param name="IsStoredProcedure">命令文本是否是存储过程</param>
			/// <param name="OleDbDataConn">数据连接</param>
			/// <returns></returns>
			private static OleDbCommand AddOleDbParas(OleDbParameter[] OleDbParas, string cmdText, CommandType cmdType, OleDbConnection OleDbDataConn) {
				OleDbCommand OleDbComm = new OleDbCommand(cmdText, OleDbDataConn);
				OleDbComm.CommandType = cmdType;
				if (OleDbParas != null) {
					foreach (OleDbParameter p in OleDbParas) {
						OleDbComm.Parameters.Add(p);
					}
				}
				return OleDbComm;
			}
			#endregion
		}
}
