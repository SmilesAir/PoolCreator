using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// Interaction logic for UserControl_JudgePlayingTeams.xaml
	/// </summary>
	public partial class UserControl_JudgePlayingTeams : UserControl
	{
		MainWindow parentWindow;
		int controlIndex = 0;
		ObservableCollection<TeamData> playingTeams = new ObservableCollection<TeamData>();

		public UserControl_JudgePlayingTeams()
		{
			InitializeComponent();
		}

		public void Init(MainWindow parent, int inControlIndex)
		{
			parentWindow = parent;

			parentWindow.OnJudgesPoolChange += OnJudgesPoolChange;

			controlIndex = inControlIndex;
		}

		void OnJudgesPoolChange(EDivision division, ERoundJudgeDisplay round)
		{
			if (division != EDivision.None && round != ERoundJudgeDisplay.None)
			{
				parentWindow.GetPlayingTeams(division, round, controlIndex, out playingTeams);
				PoolTeamsControl.ItemsSource = playingTeams;

				string poolString = "";
				if (round == ERoundJudgeDisplay.Finals)
				{
					if (controlIndex == 0)
					{
						PoolNameLabel.Content = division.ToString() + " - " + round.ToString();
					}
					else
					{
						PoolNameLabel.Content = "";
					}
				}
				else
				{
					poolString = EnumConverter.ConvertPoolValue(round, controlIndex).ToString();

					PoolNameLabel.Content = division.ToString() + " - " + round.ToString() + " - Pool " + poolString;
				}
			}
			else
			{
				PoolTeamsControl.ItemsSource = null;

				PoolNameLabel.Content = "";
			}
		}
	}
}
