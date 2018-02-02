namespace HZGZDL.YZFJKGZXFXY.Model {
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;

	public partial class myEFTransFormerInfo : DbContext {
		public myEFTransFormerInfo()
			: base("name=myEFTransFormerInfo") {
		}
			public virtual DbSet<TransformerInfo>TransformerInfoEntity { get; set; }
			public virtual DbSet<ParameterInfo> TestInfoEntity { get; set; }
			public virtual DbSet<Trans_Manufactory_Model> TransMMEntity { get; set; }
			public virtual DbSet<Switch_Manufactory_Model> SwitchMMEntity { get; set; }
		}
	public class TransformerInfo {
		[System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }
		public DateTime Date { get; set; }
		public string CompanyName { get; set; }
		public string TransformerName { get; set; }
		public string TransformerManufactory { get; set; }
		public string TransformerModel{ get; set; }
		public int Phase { get; set; }
		public int Winding { get; set; }
		public int SwitchColumnCount { get; set; }
		public string SwitchManufactory { get; set; }
		public string SwitchModel{ get; set; }
		public int CurrentPos { get; set; }
		public int StartPos { get; set; }
		public int EndPos { get; set; }
		public bool IsForwardHandoff { get; set; }
	}
	public class ParameterInfo {
		[System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }
		public DateTime Date { get; set; }
		public double CurrentFlag { get; set; }
		public int CurrentFrequency { get; set; }

		//电流参数  显示电流x = (AD值*参考电压*衰减倍数*量程*校准系数)/(32768*输出电压)
		//量程
		public double currentChannel_1_Range { get; set; }
		public double currentChannel_2_Range { get; set; }
		public double currentChannel_3_Range { get; set; }
		//振动输出电压
		public double currentChannel_1_OutVolt { get; set; }
		public double currentChannel_2_OutVolt{ get; set; }
		public double currentChannel_3_OutVolt{ get; set; }
		//校准系数
		public double currentChannel_1_CheckPara{ get; set; }
		public double currentChannel_2_CheckPara { get; set; }
		public double currentChannel_3_CheckPara { get; set; }
		//参考电压
		private double current_referenceVolt = 2.5;
		public double currentChannel_1_referenceVolt { get { return current_referenceVolt; }}
		public double currentChannel_2_referenceVolt { get { return current_referenceVolt; }  }
		public double currentChannel_3_referenceVolt { get { return current_referenceVolt; }  }
		//衰减倍数
		private double current_multiple= 1;
		public double currentChannel_1_multiple { get { return current_multiple; } }
		public double currentChannel_2_multiple { get { return current_multiple; }  }
		public double currentChannel_3_multiple { get { return current_multiple; }  }

		//振动参数 显示加速度x = (AD值*参考电压*衰减倍数*量程*校准系数)/(32768*输出电压)
		//量程
		public double shakeChannel_1_Range { get; set; }
		public double shakeChannel_2_Range { get; set; }
		public double shakeChannel_3_Range { get; set; }
		//输出电压
		public double shakeChannel_1_OutVolt{ get; set; }
		public double shakeChannel_2_OutVolt{ get; set; }
		public double shakeChannel_3_OutVolt{ get; set; }
		//校准系数
		public double shakeChannel_1_CheckPara { get; set; }
		public double shakeChannel_2_CheckPara { get; set; }
		public double shakeChannel_3_CheckPara { get; set; }
		//参考电压
		private double shake_referenceVolt = 2.5;
		public double shakeChannel_1_referenceVolt { get { return shake_referenceVolt;} }
		public double shakeChannel_2_referenceVolt { get { return shake_referenceVolt; }  }
		public double shakeChannel_3_referenceVolt { get { return shake_referenceVolt; }  }
		//衰减倍数
		private double shake_multiple = 5;
		public double shakeChannel_1_multiple { get { return shake_multiple; }  }
		public double shakeChannel_2_multiple { get { return shake_multiple; } }
		public double shakeChannel_3_multiple { get { return shake_multiple; }  }


	}

	public class Trans_Manufactory_Model {
		[System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }
		public string Manufactory { get; set; }
		public string Model { get; set; }
	}
	public class Switch_Manufactory_Model {
		[System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }
		public string Manufactory { get; set; }
		public string Model { get; set; }
	}
}
