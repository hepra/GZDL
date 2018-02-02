using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public  class TreeViewHelper {
		#region TreeView Local 更新

		static public void TreeViewUpdateLocal(TreeView parentTree, int level,Model.TransformerInfo transInfo) {
			foreach (Common.myTreeViewItem severItem in parentTree.Items) {
				if (severItem.HeaderText == "本地数据源") {
					TreeViewUpdate(severItem, level, transInfo);
					return;
				}
			}
			Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
			first_stage.FontSize = 16;
			first_stage.HeaderText = "本地数据源";
			first_stage.TabIndex = 0;
			first_stage.Icon = first_stage.iconSourceLocal;
			parentTree.Items.Add(first_stage);
			TreeViewUpdate(first_stage, level, transInfo);
		}

		static public void TreeViewUpdateLocal(TreeView parentTree, int level, string[] transInfo) {
			foreach (Common.myTreeViewItem severItem in parentTree.Items) {
				if (severItem.HeaderText == "本地数据源") {
					TreeViewUpdate(severItem, level, transInfo);
					return;
				}
			}
			Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
			first_stage.FontSize = 16;
			first_stage.HeaderText = "本地数据源";
			first_stage.TabIndex = 0;
			first_stage.Icon = first_stage.iconSourceLocal;
			parentTree.Items.Add(first_stage);
			TreeViewUpdate(first_stage, level, transInfo);
		}
		#endregion
		#region TreeView  Rmote 更新
		static public void TreeViewUpdateFromSever(TreeView parentTree, Model.TransformerInfo transInfo) {
			foreach (Common.myTreeViewItem severItem in parentTree.Items) {
				if (severItem.HeaderText == "服务器数据源") {
					TreeViewUpdate(severItem, 6, transInfo);
					return;
				}
			}
			Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
			first_stage.FontSize = 16;
			first_stage.HeaderText = "服务器数据源";
			first_stage.TabIndex = 0;
			first_stage.Icon = first_stage.iconSourceSever;
			parentTree.Items.Add(first_stage);
			TreeViewUpdate(first_stage, 6, transInfo);
		}

		static public void TreeViewUpdateFromSever(TreeView parentTree, string[] transInfo) {
			foreach (Common.myTreeViewItem severItem in parentTree.Items) {
				if (severItem.HeaderText == "服务器数据源") {
					TreeViewUpdate(severItem, 6, transInfo);
					return;
				}
			}
			Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
			first_stage.FontSize = 16;
			first_stage.HeaderText = "服务器数据源";
			first_stage.TabIndex = 0;
			first_stage.Icon = first_stage.iconSourceSever;
			parentTree.Items.Add(first_stage);
			TreeViewUpdate(first_stage, 6, transInfo);
		}
		#endregion



		static public void TreeViewUpdate(myTreeViewItem parentTree, int Level, Model.TransformerInfo transInfo) {
			#region TreeView 更新
			string access = "";
			if (transInfo.IsForwardHandoff && transInfo.CurrentPos + 1 <= transInfo.EndPos) {
				access = "分接位[" + transInfo.CurrentPos + "]切[" + (transInfo.CurrentPos + 1) + "]";
			}
			if (!transInfo.IsForwardHandoff && transInfo.CurrentPos - 1 >= transInfo.StartPos) {
				access = "分接位[" + transInfo.CurrentPos + "]切[" + (transInfo.CurrentPos - 1) + "]";
			}
			string[] Info = new string[4];
			Info[0] = transInfo.CompanyName;
			Info[1] = transInfo.TransformerName;
			Info[3] = transInfo.Date.ToString("yy-MM-dd");
			Info[2] = access;
			bool isExist = false;
			bool isExist2 = false;
			bool isExist3 = false;
			bool isExist4 = false;
			//树根节点不存在时候 新建根节点
			if (parentTree.Items.Count == 0) {
				//当变压器属性为空的时候 啥也不做 这属于错误操作！
				if (String.IsNullOrEmpty(transInfo.TransformerName)) {
					return;
				}
				//不为空时 第1层节点 为 变压器所属单位名称
				Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
				first_stage.FontSize = 14;
				first_stage.HeaderText = transInfo.CompanyName;
				first_stage.TabIndex = 1;
				parentTree.Items.Add(first_stage);
				//不为空时 第2层节点 为 变压器名称
				Common.myTreeViewItem second_stage = new Common.myTreeViewItem();
				second_stage.HeaderText =transInfo.TransformerName;
				second_stage.Icon = first_stage.iconSourceTransformer;
				second_stage.TabIndex = 2;
				second_stage.FontSize = 14;
				first_stage.Items.Add(second_stage);
				if (Level >= 5) {
					//不为空时 第3层节点 为 测试日期
					Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
					three_stage.HeaderText = transInfo.Date.ToString("yyyy-MM-dd");
					three_stage.Icon = three_stage.iconSourceDate;
					three_stage.TabIndex = 3;
					second_stage.Items.Add(three_stage);
					//不为空时 第4层节点 为 测试的分接位
					Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
				
					four_stage.HeaderText = access;
					four_stage.TabIndex = 4;
					four_stage.Icon = four_stage.iconSourceNode;
					three_stage.Items.Add(four_stage);
					if (Level >= 6) {
						//不为空时 第5层节点 为 测试次数
						Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
						five_stage.HeaderText = "测试数据["+transInfo.Date.ToLongTimeString()+"]";
						five_stage.Icon = five_stage.iconSourceTest_Count;
						five_stage.TabIndex = 5;
						four_stage.Items.Add(five_stage);
					}

				}

			}
			//树根节点存在时候 比较节点信息
			else {
				//错误操作时 啥也不做 返回
				if (String.IsNullOrEmpty(Info[0])) {
					return;
				}
				foreach (Common.myTreeViewItem item in parentTree.Items) {
					#region 如果 变压器所属单位已存在于TreeView中
					if (item.HeaderText == Info[0]) {
						isExist = true;
						foreach (Common.myTreeViewItem tvii in item.Items) {
							#region 如果变压器名称已存在于TreeView中

							if (tvii.HeaderText.ToString() == Info[1]) {
								isExist2 = true;
								if (Level >= 5) {
									foreach (Common.myTreeViewItem tviii in tvii.Items) {

										#region 如果测试日期已存在于TreeView中
										if (tviii.HeaderText.ToString() == Info[2]) {
											isExist3 = true;
											foreach (Common.myTreeViewItem tviiii in tviii.Items) {

												#region 如果测试分接位已存在于 TreeView中
												if (tviiii.HeaderText == Info[3]) {
													isExist4 = true;
													//不为空时 第5层节点 为 测试次数
													if (Level == 6) {
														Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
														five_stage.HeaderText =  "测试数据[" + transInfo.Date.Hour + "点" + transInfo.Date.Minute+"分"+transInfo.Date.Second+"秒"+ "]";
														five_stage.Icon = five_stage.iconSourceTest_Count;
														five_stage.TabIndex = 5;
														tviiii.Items.Add(five_stage);
														tviiii.IsExpanded = true;
													}
												}
												#endregion
												#region 否则
												else if (!isExist3) {
													isExist4 = false;
												}
												#endregion
											}
											if (!isExist4) {
												//不为空时 第4层节点 为 测试的分接位
												Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
												four_stage.HeaderText = Info[3];
												four_stage.TabIndex = 4;
												four_stage.Icon = four_stage.iconSourceNode;
												tviii.Items.Add(four_stage);
												//不为空时 第5层节点 为 测试次数
												if (Level == 6) {
													Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
													five_stage.HeaderText =  "测试数据[" + transInfo.Date.Hour + "点" + transInfo.Date.Minute+"分"+transInfo.Date.Second+"秒"+ "]";
													five_stage.Icon = five_stage.iconSourceTest_Count;
													five_stage.TabIndex = 5;
													four_stage.Items.Add(five_stage);
												}

											}
										}
										#endregion
										#region 否则
										else {
											isExist3 = false;
										}
										#endregion
									}
									if (!isExist3) {
										//不为空时 第3层节点 为 测试日期
										Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
										three_stage.HeaderText = Info[2];
										three_stage.Icon = three_stage.iconSourceDate;
										three_stage.TabIndex = 3;
										tvii.Items.Add(three_stage);
										//不为空时 第4层节点 为 测试的分接位
										Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
										//如果是自动连续测量
										four_stage.HeaderText = Info[3];
										four_stage.TabIndex = 4;
										four_stage.Icon = four_stage.iconSourceNode;
										three_stage.Items.Add(four_stage);
										if (Level == 6) {
											Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
											five_stage.HeaderText =  "测试数据[" + transInfo.Date.Hour + "点" + transInfo.Date.Minute+"分"+transInfo.Date.Second+"秒"+ "]";
											five_stage.Icon = five_stage.iconSourceTest_Count;
											five_stage.TabIndex = 5;
											four_stage.Items.Add(five_stage);
										}
									}
								}

							}
							#endregion
							#region 否则
							//如果是最后一项
							else if (!isExist2) {
								isExist2 = false;
							}
							#endregion
						}
						if (!isExist2) {
							//不为空时 第2层节点 为 变压器名称
							Common.myTreeViewItem second_stage = new Common.myTreeViewItem();
							second_stage.HeaderText = Info[1];
							second_stage.Icon = item.iconSourceTransformer;
							second_stage.TabIndex = 2;
							second_stage.FontSize = 14;
							item.Items.Add(second_stage);
							if (Level >= 5) {
								//不为空时 第3层节点 为 测试日期
								Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
								three_stage.HeaderText = Info[2];
								three_stage.Icon = three_stage.iconSourceDate;
								three_stage.TabIndex = 3;
								second_stage.Items.Add(three_stage);
								//不为空时 第4层节点 为 测试的分接位
								Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
								four_stage.HeaderText = Info[3];
								four_stage.TabIndex = 4;
								four_stage.Icon = four_stage.iconSourceNode;
								three_stage.Items.Add(four_stage);
								if (Level == 6) {
									//不为空时 第5层节点 为 测试次数
									Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
									five_stage.HeaderText =  "测试数据[" + transInfo.Date.Hour + "点" + transInfo.Date.Minute+"分"+transInfo.Date.Second+"秒"+ "]";
									five_stage.Icon = five_stage.iconSourceTest_Count;
									five_stage.TabIndex = 5;
									four_stage.Items.Add(five_stage);
								}

							}

						}
					}
					#endregion
					#region 否则
					else if (isExist != true) {
						isExist = false;
					}
					#endregion
				}
				if (!isExist) {
					//当变压器属性为空的时候 啥也不做 这属于错误操作！
					if (Info[1] == "" || Info[1] == null) {
						return;
					}
					//不为空时 第1层节点 为 变压器所属单位名称
					Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
					first_stage.HeaderText = Info[0];
					first_stage.TabIndex = 1;
					first_stage.FontSize = 14;
					parentTree.Items.Add(first_stage);
					//不为空时 第2层节点 为 变压器名称
					Common.myTreeViewItem second_stage = new Common.myTreeViewItem();
					second_stage.HeaderText = Info[1];
					second_stage.Icon = first_stage.iconSourceTransformer;
					second_stage.TabIndex = 2;
					second_stage.FontSize = 14;
					first_stage.Items.Add(second_stage);
					if (Level >= 5) {
						//不为空时 第3层节点 为 测试日期
						Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
						three_stage.HeaderText = Info[2];
						three_stage.Icon = three_stage.iconSourceDate;
						three_stage.TabIndex = 3;
						second_stage.Items.Add(three_stage);
						//不为空时 第4层节点 为 测试的分接位
						Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
						four_stage.HeaderText = Info[3];
						four_stage.TabIndex = 4;
						four_stage.Icon = four_stage.iconSourceNode;
						three_stage.Items.Add(four_stage);
						if (Level == 6) {
							//不为空时 第5层节点 为 测试次数
							Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
							five_stage.HeaderText =  "测试数据[" + transInfo.Date.Hour + "点" + transInfo.Date.Minute+"分"+transInfo.Date.Second+"秒"+ "]";
							five_stage.Icon = five_stage.iconSourceTest_Count;
							five_stage.TabIndex = 5;
							four_stage.Items.Add(five_stage);
						}

					}
				}
			}
			#endregion
		}
		static public void TreeViewUpdate(myTreeViewItem parentTree, int Level, string[] transInfo) {
			#region TreeView 更新
			bool isExist = false;
			bool isExist2 = false;
			bool isExist3 = false;
			bool isExist4 = false;
			//树根节点不存在时候 新建根节点
			if (parentTree.Items.Count == 0) {
				//当变压器属性为空的时候 啥也不做 这属于错误操作！
				if (String.IsNullOrEmpty(transInfo[0])) {
					return;
				}
				//不为空时 第1层节点 为 变压器所属单位名称
				Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
				first_stage.FontSize = 14;
				first_stage.HeaderText = transInfo[0];
				first_stage.TabIndex = 1;
				parentTree.Items.Add(first_stage);
				//不为空时 第2层节点 为 变压器名称
				Common.myTreeViewItem second_stage = new Common.myTreeViewItem();
				second_stage.HeaderText = transInfo[1];
				second_stage.Icon = first_stage.iconSourceTransformer;
				second_stage.TabIndex = 2;
				second_stage.FontSize = 14;
				first_stage.Items.Add(second_stage);
				if (Level >= 5) {
					//不为空时 第3层节点 为 测试日期
					Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
					three_stage.HeaderText = transInfo[2];
					three_stage.Icon = three_stage.iconSourceDate;
					three_stage.TabIndex = 3;
					second_stage.Items.Add(three_stage);
					//不为空时 第4层节点 为 测试的分接位
					Common.myTreeViewItem four_stage = new Common.myTreeViewItem();

					four_stage.HeaderText = transInfo[3];
					four_stage.TabIndex = 4;
					four_stage.Icon = four_stage.iconSourceNode;
					three_stage.Items.Add(four_stage);
					if (Level >= 6) {
						//不为空时 第5层节点 为 测试次数
						Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
						five_stage.HeaderText =transInfo[4];
						five_stage.Icon = five_stage.iconSourceTest_Count;
						five_stage.TabIndex = 5;
						four_stage.Items.Add(five_stage);
					}

				}

			}
			//树根节点存在时候 比较节点信息
			else {
				//错误操作时 啥也不做 返回
				if (String.IsNullOrEmpty(transInfo[0])) {
					return;
				}
				foreach (Common.myTreeViewItem item in parentTree.Items) {
					#region 如果 变压器所属单位已存在于TreeView中
					if (item.HeaderText == transInfo[0]) {
						isExist = true;
						foreach (Common.myTreeViewItem tvii in item.Items) {
							#region 如果变压器名称已存在于TreeView中

							if (tvii.HeaderText.ToString() == transInfo[1]) {
								isExist2 = true;
								if (Level >= 5) {
									foreach (Common.myTreeViewItem tviii in tvii.Items) {

										#region 如果测试日期已存在于TreeView中
										if (tviii.HeaderText.ToString() == transInfo[2]) {
											isExist3 = true;
											foreach (Common.myTreeViewItem tviiii in tviii.Items) {

												#region 如果测试分接位已存在于 TreeView中
												if (tviiii.HeaderText == transInfo[3]) {
													isExist4 = true;
													//不为空时 第5层节点 为 测试次数
													if (Level == 6) {
														Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
														five_stage.HeaderText = transInfo[4];
														five_stage.Icon = five_stage.iconSourceTest_Count;
														five_stage.TabIndex = 5;
														tviiii.Items.Add(five_stage);
														tviiii.IsExpanded = true;
													}
												}
												#endregion
												#region 否则
												else if (!isExist3) {
													isExist4 = false;
												}
												#endregion
											}
											if (!isExist4) {
												//不为空时 第4层节点 为 测试的分接位
												Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
												four_stage.HeaderText = transInfo[3];
												four_stage.TabIndex = 4;
												four_stage.Icon = four_stage.iconSourceNode;
												tviii.Items.Add(four_stage);
												//不为空时 第5层节点 为 测试次数
												if (Level == 6) {
													Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
													five_stage.HeaderText = transInfo[4];
													five_stage.Icon = five_stage.iconSourceTest_Count;
													five_stage.TabIndex = 5;
													four_stage.Items.Add(five_stage);
												}

											}
										}
										#endregion
										#region 否则
										else {
											isExist3 = false;
										}
										#endregion
									}
									if (!isExist3) {
										//不为空时 第3层节点 为 测试日期
										Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
										three_stage.HeaderText = transInfo[2];
										three_stage.Icon = three_stage.iconSourceDate;
										three_stage.TabIndex = 3;
										tvii.Items.Add(three_stage);
										//不为空时 第4层节点 为 测试的分接位
										Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
										//如果是自动连续测量
										four_stage.HeaderText = transInfo[3];
										four_stage.TabIndex = 4;
										four_stage.Icon = four_stage.iconSourceNode;
										three_stage.Items.Add(four_stage);
										if (Level == 6) {
											Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
											five_stage.HeaderText = transInfo[4];
											five_stage.Icon = five_stage.iconSourceTest_Count;
											five_stage.TabIndex = 5;
											four_stage.Items.Add(five_stage);
										}
									}
								}

							}
							#endregion
							#region 否则
							//如果是最后一项
							else if (!isExist2) {
								isExist2 = false;
							}
							#endregion
						}
						if (!isExist2) {
							//不为空时 第2层节点 为 变压器名称
							Common.myTreeViewItem second_stage = new Common.myTreeViewItem();
							second_stage.HeaderText = transInfo[1];
							second_stage.Icon = item.iconSourceTransformer;
							second_stage.TabIndex = 2;
							second_stage.FontSize = 14;
							item.Items.Add(second_stage);
							if (Level >= 5) {
								//不为空时 第3层节点 为 测试日期
								Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
								three_stage.HeaderText = transInfo[2];
								three_stage.Icon = three_stage.iconSourceDate;
								three_stage.TabIndex = 3;
								second_stage.Items.Add(three_stage);
								//不为空时 第4层节点 为 测试的分接位
								Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
								four_stage.HeaderText = transInfo[3];
								four_stage.TabIndex = 4;
								four_stage.Icon = four_stage.iconSourceNode;
								three_stage.Items.Add(four_stage);
								if (Level == 6) {
									//不为空时 第5层节点 为 测试次数
									Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
									five_stage.HeaderText = transInfo[4];
									five_stage.Icon = five_stage.iconSourceTest_Count;
									five_stage.TabIndex = 5;
									four_stage.Items.Add(five_stage);
								}

							}

						}
					}
					#endregion
					#region 否则
					else if (isExist != true) {
						isExist = false;
					}
					#endregion
				}
				if (!isExist) {
					//当变压器属性为空的时候 啥也不做 这属于错误操作！
					if (transInfo[1] == "" || transInfo[1] == null) {
						return;
					}
					//不为空时 第1层节点 为 变压器所属单位名称
					Common.myTreeViewItem first_stage = new Common.myTreeViewItem();
					first_stage.HeaderText = transInfo[0];
					first_stage.TabIndex = 1;
					first_stage.FontSize = 14;
					parentTree.Items.Add(first_stage);
					//不为空时 第2层节点 为 变压器名称
					Common.myTreeViewItem second_stage = new Common.myTreeViewItem();
					second_stage.HeaderText = transInfo[1];
					second_stage.Icon = first_stage.iconSourceTransformer;
					second_stage.TabIndex = 2;
					second_stage.FontSize = 14;
					first_stage.Items.Add(second_stage);
					if (Level >= 5) {
						//不为空时 第3层节点 为 测试日期
						Common.myTreeViewItem three_stage = new Common.myTreeViewItem();
						three_stage.HeaderText = transInfo[2];
						three_stage.Icon = three_stage.iconSourceDate;
						three_stage.TabIndex = 3;
						second_stage.Items.Add(three_stage);
						//不为空时 第4层节点 为 测试的分接位
						Common.myTreeViewItem four_stage = new Common.myTreeViewItem();
						four_stage.HeaderText = transInfo[3];
						four_stage.TabIndex = 4;
						four_stage.Icon = four_stage.iconSourceNode;
						three_stage.Items.Add(four_stage);
						if (Level == 6) {
							//不为空时 第5层节点 为 测试次数
							Common.myTreeViewItem five_stage = new Common.myTreeViewItem();
							five_stage.HeaderText = transInfo[4];
							five_stage.Icon = five_stage.iconSourceTest_Count;
							five_stage.TabIndex = 5;
							four_stage.Items.Add(five_stage);
						}

					}
				}
			}
			#endregion
		}
		static public int GetNumInString(string t) {
			int last = t.Length - 1;
			int gewei = 0;
			int shiwei = 0;
			int shuzigeshu = 0;
			for (int k = 0; k < last; k++) {
				if (Char.IsDigit(t[k])) {
					shuzigeshu += 1;
					if (shuzigeshu == 1) {
						gewei = int.Parse(t[k].ToString());
					}
					if (shuzigeshu == 2) {
						shiwei = gewei;
						gewei = int.Parse(t[k].ToString());
					}
				}
			}
			if (shuzigeshu == 0) {
				return 0;
			}
			else {
				return shiwei * 10 + gewei;
			}
		}

		static private Common.myTreeViewItem initTreeViewItemProterity(string header, Brush brush, int tabIndex) {
			Common.myTreeViewItem temp = new Common.myTreeViewItem();
			temp.HeaderText = header;
			temp.TabIndex = tabIndex;
			temp.Foreground = brush;
			temp.Icon = temp.iconSourceData;
			return temp;
		}

	}
}
