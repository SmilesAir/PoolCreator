using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// Interaction logic for UserControl_TournamentDetails.xaml
	/// </summary>
	public partial class UserControl_TournamentDetails : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		MainWindow parentWindow;
		public string TournamentName
		{
			get { return parentWindow.tournamentData.TournamentName; }
			set
			{
				parentWindow.tournamentData.TournamentName = value;
				OnPropertyChanged("TournamentName");
			}
		}
		public string TournamentSubtitle
		{
			get { return parentWindow.tournamentData.TournamentSubtitle; }
			set
			{
				parentWindow.tournamentData.TournamentSubtitle = value;
				OnPropertyChanged("TournamentSubtitle");
			}
		}

		public UserControl_TournamentDetails()
		{
			InitializeComponent();
		}

		public void Init(MainWindow inParentWindow)
		{
			parentWindow = inParentWindow;

			TournamentDetailsGrid.DataContext = this;

			WomenDetails.Init(inParentWindow);
			OpenDetails.Init(inParentWindow);
			MixedDetails.Init(inParentWindow);
			CoopDetails.Init(inParentWindow);
		}
	}
}
