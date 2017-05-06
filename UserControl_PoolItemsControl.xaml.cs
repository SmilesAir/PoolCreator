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
	/// Interaction logic for UserControl_PoolItemsControl.xaml
	/// </summary>
	public partial class UserControl_PoolItemsControl : UserControl
	{
		public ObservableCollection<TeamData> teams = new ObservableCollection<TeamData>();
		public ObservableCollection<int> teamsRank = new ObservableCollection<int>();
		MainWindow parentWindow = null;
		TeamData futureTeamData = null;
		const int INVALID_TEAM_INDEX = -1;
		int futureTeamDataIndex = INVALID_TEAM_INDEX;
		int currentTeamDataIndex = INVALID_TEAM_INDEX;
		bool bEnableRankPicking = false;
		int nextTeamRank = 1;
		ERound round = ERound.None;
		EPool pool = EPool.A;

		public static DependencyProperty PoolNameProperty =
			DependencyProperty.Register("PoolName", typeof(string), typeof(UserControl_PoolItemsControl),
				new PropertyMetadata(new PropertyChangedCallback(PoolNamePropertyChanged)));
		public string PoolName
		{
			get { return (string)GetValue(PoolNameProperty); }
			set { SetValue(PoolNameProperty, value); }
		}

		public UserControl_PoolItemsControl()
		{
			InitializeComponent();
		}

		public void Init(MainWindow parent, ERound inRound, EPool inPool)
		{
			parentWindow = parent;

			round = inRound;
			pool = inPool;
		}

		public void SetItemsSource(PoolData itemsSource)
		{
			teams = itemsSource.teamList.teams;
			PoolItemsControl.ItemsSource = teams;
			teamsRank = itemsSource.resultRank;
			PoolRankItemsControl.ItemsSource = teamsRank;
		}

		private void PoolItemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TeamData td = (sender as Border).Tag as TeamData;

			if (bEnableRankPicking)
			{
				int curTeamRank = teamsRank[teams.IndexOf(td)];

				if (curTeamRank == 0)
				{
					teamsRank[teams.IndexOf(td)] = nextTeamRank;

					++nextTeamRank;
				}

				return;
			}

			if (teamsRank.Count > 0)
			{
				return;
			}

			parentWindow.OverlayTeamData = td;
			Point offset = e.GetPosition(sender as Border);
			parentWindow.OverlayTeamDataOffset = offset;

			td.OverlayDragState = EOverlayTeamDataState.DragFrom;

			parentWindow.OverlayCanvas_MouseLeftButtonDown(sender, e);

			//teams.Remove(td);

			futureTeamData = td;
			currentTeamDataIndex = teams.IndexOf(td);
		}

		private void PoolItemsControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (futureTeamDataIndex != INVALID_TEAM_INDEX)
			{
				teams.RemoveAt(futureTeamDataIndex);
			}

			if (parentWindow.OverlayTeamData != null)
			{
				TeamData td = parentWindow.OverlayTeamData;

				parentWindow.RemoveTeamDataFromPools(td);

				td.OverlayDragState = EOverlayTeamDataState.Normal;

				if (futureTeamDataIndex != INVALID_TEAM_INDEX)
				{
					futureTeamDataIndex = Math.Min(futureTeamDataIndex, teams.Count);

					teams.Insert(futureTeamDataIndex, td);
				}
				else
				{
					teams.Insert(currentTeamDataIndex, td);
				}
			}

			futureTeamDataIndex = INVALID_TEAM_INDEX;
			ClearFutureData();

			parentWindow.OverlayCanvas_MouseLeftButtonUp(sender, e);
		}

		private void PoolItemsControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (futureTeamData != null)
			{
				// Show a ghost team where this would be placed if released
				int newFutureTeamIndex = INVALID_TEAM_INDEX;
				Point mousePos = e.GetPosition(PoolItemsControl);
				double teamDataHeight = parentWindow.OverlayTeam.ActualHeight;
				double mousePosY = mousePos.Y;
				newFutureTeamIndex = (int)Math.Round(mousePosY / teamDataHeight);
				newFutureTeamIndex = Math.Max(0, Math.Min(newFutureTeamIndex, teams.Count - 1));

				// Don't place a ghost next to previous self
				if (currentTeamDataIndex != INVALID_TEAM_INDEX)
				{
					if (currentTeamDataIndex == newFutureTeamIndex || currentTeamDataIndex + 1 == newFutureTeamIndex)
					{
						if (teams.Count - 1 != newFutureTeamIndex)
						{
							RemoveFutureTeam();
						}

						return;
					}
				}

				// Don't place next to existing ghost
				if (futureTeamDataIndex != INVALID_TEAM_INDEX)
				{
					if (futureTeamDataIndex == newFutureTeamIndex)
					{
						return;
					}

					if (futureTeamDataIndex + 1 == newFutureTeamIndex)
					{
						newFutureTeamIndex = (int)Math.Round((mousePosY - teamDataHeight / 2f) / teamDataHeight);
						newFutureTeamIndex = Math.Max(0, Math.Min(newFutureTeamIndex, teams.Count - 1));
					}
				}

				if (newFutureTeamIndex != futureTeamDataIndex)
				{
					if (futureTeamDataIndex != INVALID_TEAM_INDEX)
					{
						RemoveFutureTeam();

						if (futureTeamDataIndex != INVALID_TEAM_INDEX && futureTeamDataIndex < newFutureTeamIndex)
						{
							--newFutureTeamIndex;
						}
					}

					teams.Insert(newFutureTeamIndex, futureTeamData);

					if (currentTeamDataIndex > newFutureTeamIndex)
					{
						++currentTeamDataIndex;
					}
				}
				
				futureTeamDataIndex = newFutureTeamIndex;
			}

			parentWindow.OverlayCanvas_MouseMove(sender, e);
		}

		private void RemoveFutureTeam()
		{
			if (futureTeamDataIndex != INVALID_TEAM_INDEX)
			{
				teams.RemoveAt(futureTeamDataIndex);
				
				if (currentTeamDataIndex > futureTeamDataIndex)
				{
					--currentTeamDataIndex;
				}

				futureTeamDataIndex = INVALID_TEAM_INDEX;
			}
		}

		private void PoolItemsControl_MouseEnter(object sender, MouseEventArgs e)
		{
			if (parentWindow.OverlayTeamData != null)
			{
				// Show a ghost team where this would be placed if released
				futureTeamData = parentWindow.OverlayTeamData;
			}
		}

		private void PoolItemsControl_MouseLeave(object sender, MouseEventArgs e)
		{
			if (futureTeamData != null)
			{
				ClearFutureData();
			}
		}

		private void ClearFutureData()
		{
			RemoveFutureTeam();
			futureTeamData = null;
			futureTeamDataIndex = INVALID_TEAM_INDEX;
		}

		private static void PoolNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			UserControl_PoolItemsControl myUserControl = sender as UserControl_PoolItemsControl;
			if (myUserControl != null)
			{
				myUserControl.PoolNameLabel.Content = e.NewValue as string;
			}
		}

		private void PickRank_Click(object sender, RoutedEventArgs e)
		{
			bEnableRankPicking = !bEnableRankPicking;

			Button button = sender as Button;
			button.Content = bEnableRankPicking ? "Finish Picking Rank" : "Pick Rank";
			button.Background = bEnableRankPicking ? Brushes.IndianRed : Brushes.LightCyan;

			if (bEnableRankPicking)
			{
				nextTeamRank = 1;
				teamsRank.Clear();

				foreach (TeamData td in teams)
				{
					teamsRank.Add(0);
				}
			}
			else
			{
				// If finish picking rank, callback that our results changed
				parentWindow.OnPoolRankingsChanged(round, pool);
			}
		}

		private void ClearRank_Click(object sender, RoutedEventArgs e)
		{
			teamsRank.Clear();

			if (bEnableRankPicking)
			{
				nextTeamRank = 1;

				foreach (TeamData td in teams)
				{
					teamsRank.Add(0);
				}
			}
		}

		private void ClearTeams_Click(object sender, RoutedEventArgs e)
		{
			if (System.Windows.MessageBox.Show(
				"Are you sure you want to Delete all the teams from this pool?",
				"Confirm Delete",
				MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				teams.Clear();
				teamsRank.Clear();
			}
		}
	}
}
