using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		ObservableCollection<RegisteredPlayer> teamsRegisteredPlayers = new ObservableCollection<RegisteredPlayer>();
		ObservableCollection<PotentialTeam> foundTeams = new ObservableCollection<PotentialTeam>();
		ObservableCollection<RegisteredPlayer> teamsPartialMatches = new ObservableCollection<RegisteredPlayer>();
		public ObservableCollection<TeamData> registeredTeams = new ObservableCollection<TeamData>();
		EDivision teamsDivision = EDivision.None;
		public EDivisionDisplay TeamsSelectedDivision
		{
			get { return EnumConverter.ConvertDivisionValue(teamsDivision); }
			set
			{
				teamsDivision = EnumConverter.ConvertDivisionValue(value);
				OnPropertyChanged("TeamsSelectedDivision");
				
				registeredTeams = tournamentData.GetAllTeams(teamsDivision);
				TeamsRegisteredTeamsItemsControl.ItemsSource = registeredTeams;
			}
		}
		PotentialPlayer teamsFixingPlayer = null;
		string teamsFixBoxFirstName = "";
		public string TeamsFixBoxFirstName
		{
			get { return teamsFixBoxFirstName; }
			set
			{
				teamsFixBoxFirstName = value;
				OnPropertyChanged("TeamsFixBoxFirstName");
			}
		}
		string teamsFixBoxLastName = "";
		public string TeamsFixBoxLastName
		{
			get { return teamsFixBoxLastName; }
			set
			{
				teamsFixBoxLastName = value;
				OnPropertyChanged("TeamsFixBoxLastName");
			}
		}

		private void InitTeamsRegisteredPlayers()
		{
			TeamsRegisteredPlayersUserControl.RegisteredPlayersItemsControl.ItemsSource = teamsRegisteredPlayers;
			TeamsFoundTeamsItemsControl.ItemsSource = foundTeams;
			TeamsFixPotentialPlayerGrid.DataContext = this;
			TeamsPartialNameMatchItemsControl.ItemsSource = teamsPartialMatches;

			//PotentialTeam pt = new PotentialTeam();
			//pt.potentialPlayers.Add(new PotentialPlayer("Ryan", "Young"));
			//pt.potentialPlayers.Add(new PotentialPlayer("Ryan", "Young"));
			//foundTeams.Add(pt);
			//foundTeams.Add(pt);

			UpdateTeamsRegisteredPlayers();
		}

		private void TeamsRegisteredPlayersUserControl_Click(object sender, RoutedEventArgs e)
		{

		}

		private void UpdateTeamsRegisteredPlayers()
		{
			teamsRegisteredPlayers.Clear();

			foreach (RegisteredPlayer rp in tournamentData.registeredPlayers)
			{
				teamsRegisteredPlayers.Add(rp);
			}
		}

		private void TeamsTabItem_Selected(object sender, RoutedEventArgs e)
		{
			UpdateTeamsRegisteredPlayers();
		}

		private void TeamsEnterTeamsTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			foundTeams.Clear();

			using (StringReader reader = new StringReader(TeamsEnterTeamsTextBox.Text))
			{
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					PotentialTeam newTeam = new PotentialTeam();

					string[] names = line.Split(NameFinder.splitPlayerChars);
					foreach (string nameLine in names)
					{
						PlayerRanking player = new PlayerRanking();
						bool bExactMatch = false;
						if (NameFinder.GetClosestName(playerRankingData.playerRankings, nameLine, ref player, ref bExactMatch))
						{
							if (bExactMatch)
							{
								newTeam.registeredPlayers.Add(new RegisteredPlayer(new PotentialPlayer(player, true)));
							}
							else
							{
								newTeam.potentialPlayers.Add(new PotentialPlayer(nameLine));
							}
						}
					}

					if (newTeam.potentialPlayers.Count > 0 || newTeam.RegisteredPlayers.Count > 0)
					{
						foundTeams.Add(newTeam);
					}
				}
			}
		}

		private void TeamsPotentialPlayerFix_Click(object sender, RoutedEventArgs e)
		{
			PotentialPlayer player = (sender as Button).Tag as PotentialPlayer;
			TeamsFixBoxFirstName = player.firstName;
			TeamsFixBoxLastName = player.lastName;

			teamsFixingPlayer = player;
		}

		private void TeamsFixBoxName_TextChanged(object sender, TextChangedEventArgs e)
		{
			teamsPartialMatches.Clear();

			ObservableCollection<RegisteredPlayer> potentialPlayerMatches =
				NameFinder.GetFilteredNames(registeredPlayers, TeamsFixBoxFirstName, TeamsFixBoxLastName);

			foreach (RegisteredPlayer rp in potentialPlayerMatches)
			{
				teamsPartialMatches.Add(rp);
			}
		}

		private void UpdateTeamsPotentialPlayer_Click(object sender, RoutedEventArgs e)
		{
			RegisteredPlayer player = (sender as Button).Tag as RegisteredPlayer;

			// Go through all potential team members and replace teamsFixingPlayer with the found registered player
			foreach (PotentialTeam pt in foundTeams)
			{
				if (pt.potentialPlayers.Remove(teamsFixingPlayer))
				{
					pt.registeredPlayers.Insert(0, player);

					pt.OnPropertyChanged("IsValidTeam");
				}
			}


			teamsFixingPlayer = null;
		}

		private void TeamsAddTeam_Click(object sender, RoutedEventArgs e)
		{
			PotentialTeam pt = (sender as Button).Tag as PotentialTeam;
			TeamData td = new TeamData(pt);

			int insertIndex = 0;
			for (; insertIndex < registeredTeams.Count; ++insertIndex)
			{
				if (td.TeamRankingPoints < registeredTeams[insertIndex].TeamRankingPoints)
				{
					break;
				}
			}

			registeredTeams.Insert(insertIndex, td);

			foundTeams.Remove(pt);
		}


		private void TeamsRemoveTeam_Click(object sender, RoutedEventArgs e)
		{
			TeamData td = (sender as Button).Tag as TeamData;

			registeredTeams.Remove(td);
		}
	}

	public class PotentialTeam : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public ObservableCollection<PotentialPlayer> PotentialPlayers { get { return potentialPlayers; } }
		public ObservableCollection<PotentialPlayer> potentialPlayers = new ObservableCollection<PotentialPlayer>();
		public ObservableCollection<RegisteredPlayer> RegisteredPlayers { get { return registeredPlayers; } }
		public ObservableCollection<RegisteredPlayer> registeredPlayers = new ObservableCollection<RegisteredPlayer>();
		public bool IsValidTeam
		{
			get
			{
				return PotentialPlayers.Count == 0 && RegisteredPlayers.Count > 0;
			}
		}

		public PotentialTeam()
		{
		}
	}
}
