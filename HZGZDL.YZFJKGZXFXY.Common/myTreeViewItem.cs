using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HZGZDL.YZFJKGZXFXY.Common {
	public class myTreeViewItem:TreeViewItem {
		static string root_path = (System.AppDomain.CurrentDomain.BaseDirectory);
		 public ImageSource iconSourceFile = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\file2.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceDate = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeView_date.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceTest_Count = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeViewTest_Count.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceData = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeViewData.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceCompany = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeView_Company.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceTransformer = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeViewTransformer.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceNode = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeViewNode.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceSever = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeView_network.png", UriKind.RelativeOrAbsolute));
		 public ImageSource iconSourceLocal = new BitmapImage(new Uri(root_path + @"\Image\treeViewItemICO\TreeView_Local.png", UriKind.RelativeOrAbsolute));

        TextBlock textBlock;
        Image icon;
		public myTreeViewItem()
        {
            StackPanel stack = new StackPanel();
            //　 设置StackPanel中的内容水平排列
            stack.Orientation = Orientation.Horizontal;
            stack.Height = 25;
            stack.Background = Background;
            Header = stack;
            FontSize = 10;
            icon = new Image();
            icon.Source = iconSourceCompany;
            //　 向StackPanel对象中添加一个图标对象
            stack.Children.Add(icon);
            //　 创建用于添加文本信息的TextBlock对象
            textBlock = new TextBlock();
            textBlock.Foreground = Foreground;
            textBlock.Background = Background;
            //　 向StackPanel对象中添加文本信息
            stack.Children.Add(textBlock);
        }
        //　 用于设置或获得节点中的图标对象
        public ImageSource Icon
        {
            set
            {
				iconSourceFile = value;
				icon.Source = iconSourceFile;
                icon.Width = 22;
                icon.Height = 22;
            }
            get
            {
				return iconSourceFile;
            }
        }
        //　 用于设置或获得节点中的文本信息
        public string HeaderText
        {
            set
            {
                textBlock.Text = value;
            }
            get
            {
                return textBlock.Text;
            }
        }
	}
}
