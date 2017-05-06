using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoolCreator
{
	/// <summary>
	/// Interaction logic for UserControl_RegisteredPlayers.xaml
	/// </summary>
	public partial class UserControl_RegisteredPlayers : UserControl
	{
		public event RoutedEventHandler Click;

		public UserControl_RegisteredPlayers()
		{
			InitializeComponent();
		}

		private void RemoveButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.Click != null)
			{
				this.Click(sender, e);
			}
		}
	}
}
