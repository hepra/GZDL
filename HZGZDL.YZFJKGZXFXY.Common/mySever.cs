using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class mySever {
		public static readonly string RemoteUrl = "http://localhost:5328/ShowTransFormerInfo.aspx";
		public  delegate void ProgressChange(long currentlen,long totalLen);
		public static ProgressChange progress;
		public delegate void Complete();
		public static Complete complete; 
		public static void DownloadUpdateFile(string callExeName, string updateFileDir) {
			string url = RemoteUrl + callExeName + "/update.zip";
			var client = new System.Net.WebClient();
			client.DownloadDataCompleted += (sender, e) => {
				string zipFilePath = System.IO.Path.Combine(updateFileDir, "update.zip");
				byte[] data = e.Result;
				BinaryWriter writer = new BinaryWriter(new FileStream(zipFilePath, FileMode.OpenOrCreate));
				writer.Write(data);
				writer.Flush();
				writer.Close();

				string tempDir = System.IO.Path.Combine(updateFileDir, "temp");
				if (!Directory.Exists(tempDir)) {
					Directory.CreateDirectory(tempDir);
				}
					//用开源库 解压 文件到 新目录
					UnZipFile(zipFilePath, tempDir);
						try {
							//清空缓存文件夹
							string rootUpdateDir = updateFileDir.Substring(0, updateFileDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
							foreach (string p in System.IO.Directory.EnumerateDirectories(rootUpdateDir)) {
								if (!p.ToLower().Equals(updateFileDir.ToLower())) {
									System.IO.Directory.Delete(p, true);
								}
							}
						}
						catch (Exception ex) {
							//MessageBox.Show(ex.Message);
						}
				};

			client.DownloadDataAsync(new Uri(url));
		}

		public static bool Upload(string filename)  
       {  
           WebClient myWebClient = new WebClient();  
           myWebClient.Credentials = CredentialCache.DefaultCredentials; //获取或设置发送到主机并用于请求进行身份验证的网络凭据  
		   myWebClient.UploadFileAsync(new Uri("http://localhost:5328/ashx/SaveFile.ashx?filePath=" + filename), filename);
		   myWebClient.UploadProgressChanged += myWebClient_UploadProgressChanged;
           myWebClient.UploadFileCompleted +=myWebClient_UploadFileCompleted;  
           return true;  
       }

		static void myWebClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e) {
			progress(e.BytesSent, e.TotalBytesToSend);
		}
		public static void Download(string URL = "http://localhost:5328/Data/2017/11/15/" + "917507.zip.zip", string filename = @"F:\dahe\123") {
			WebClient myWebClient = new WebClient();
			myWebClient.Credentials = CredentialCache.DefaultCredentials; //获取或设置发送到主机并用于请求进行身份验证的网络凭据  
			myWebClient.DownloadFileAsync(new Uri(URL), filename);
			myWebClient.DownloadProgressChanged += myWebClient_DownloadProgressChanged;
			myWebClient.DownloadFileCompleted += myWebClient_DownloadFileCompleted;
		}

		static void myWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
			complete();
		}

		static void myWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			progress(e.BytesReceived,e.TotalBytesToReceive);
		}
	
       private static void myWebClient_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)  
       {
		   complete();
       }
	   /// <summary>
	   /// 下载文件
	   /// </summary>
	   /// <param name="URL">下载文件地址</param>
	   /// <param name="Filename">下载后的存放地址</param>
	   public static void DownloadFile(string URL = "http://localhost:5328/Data/2017/11/15/" + "917507.zip.zip", string filename = @"F:\dahe\123") {
		   try {
			   System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
			   System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
			   long totalBytes = myrp.ContentLength;

			   System.IO.Stream st = myrp.GetResponseStream();
			   System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
			   long totalDownloadedByte = 0;
			   byte[] by = new byte[1024];
			   int osize = st.Read(by, 0, (int)by.Length);
			   while (osize > 0) {
				   totalDownloadedByte = osize + totalDownloadedByte;
				   so.Write(by, 0, osize);

				   osize = st.Read(by, 0, (int)by.Length);
			   }
			   so.Close();
			   st.Close();
		   }
		   catch (System.Exception) {
			   throw new Exception("下载错误");
		   }
	   }

		/// <summary>
		/// 解压zip压缩包
		/// </summary>
		/// <param name="zipFilePath">zip文件目录</param>
		/// <param name="targetDir">解压后保存路径</param>
		public static void UnZipFile(string zipFilePath, string targetDir) {
			ICCEmbedded.SharpZipLib.Zip.FastZipEvents evt = new ICCEmbedded.SharpZipLib.Zip.FastZipEvents();
			ICCEmbedded.SharpZipLib.Zip.FastZip fz = new ICCEmbedded.SharpZipLib.Zip.FastZip(evt);
			fz.ExtractZip(zipFilePath, targetDir, "");
		}
		/// <summary>
		/// 将文件压缩成zip 文件
		/// </summary>
		/// <param name="zipFilePath"></param>
		/// <param name="sourceDir"></param>
		public static void CompressZipFile(string zipFilePath, string sourceDir) {
			ICCEmbedded.SharpZipLib.Zip.FastZipEvents evt = new ICCEmbedded.SharpZipLib.Zip.FastZipEvents();
			ICCEmbedded.SharpZipLib.Zip.FastZip fz = new ICCEmbedded.SharpZipLib.Zip.FastZip(evt);
			try {
				fz.CreateEmptyDirectories = true;
				fz.CreateZip(zipFilePath, sourceDir, false, "");
				
			}
			catch (ICCEmbedded.SharpZipLib.SharpZipBaseException e) {
				string msg = e.Message;
			}
			
		}
	}
}
