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
		}
	public class TransformerInfo {
		[System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }
		public DateTime Date { get; set; }
		public string CompanyName { get; set; }
		public string TransformerName { get; set; }
		public string TransformerProductCode { get; set; }
		public int Phase { get; set; }
		public int Winding { get; set; }

		public string SwitchCompanyName { get; set; }
		public string SwitchName { get; set; }
		public string SwitchProductCode { get; set; }
		public int CurrentPos { get; set; }
		public int StartPos { get; set; }
		public int EndPos { get; set; }
	}
}
