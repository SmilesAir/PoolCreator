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
		ObservableCollection<PotentialPlayer> foundPlayers = new ObservableCollection<PotentialPlayer>();
		ObservableCollection<PotentialPlayer> partialMatches = new ObservableCollection<PotentialPlayer>();
		string fixBoxFirstName = "";
		public string FixBoxFirstName
		{
			get { return fixBoxFirstName; }
			set
			{
				fixBoxFirstName = value;
				OnPropertyChanged("FixBoxFirstName");
			}
		}
		string fixBoxLastName = "";
		public string FixBoxLastName
		{
			get { return fixBoxLastName; }
			set
			{
				fixBoxLastName = value;
				OnPropertyChanged("FixBoxLastName");
			}
		}

		private void InitEnterPlayerNames()
		{
			FoundNamesItemsControl.ItemsSource = foundPlayers;

			FillRankingsTextBox();

			PartialNameMatchItemsControl.ItemsSource = partialMatches;

			FixPotentialPlayerGrid.DataContext = this;
		}

		private void RegisteredPlayersEnterTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			foundPlayers.Clear();

			using (StringReader reader = new StringReader(RegisteredPlayersEnterTextBox.Text))
			{
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					PlayerRanking player = new PlayerRanking();
					bool bExactMatch = false;
					if (NameFinder.GetClosestName(playerRankingData.playerRankings, line, ref player, ref bExactMatch))
					{
						if (bExactMatch)
						{
							foundPlayers.Add(new PotentialPlayer(player, true));
						}
						else
						{
							foundPlayers.Add(new PotentialPlayer(line));
						}
					}
				}
			}
		}

		private void AddAllExactNameMatches_Click(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < foundPlayers.Count; ++i)
			{
				if (foundPlayers[i].bExactMatch)
				{
					AddNameFromRegisteredPlayersEnterTextBox(foundPlayers[i]);

					--i;
				}
			}
		}

		private void AddNameFromRegisteredPlayersEnterTextBox(PotentialPlayer player)
		{
			AddRegisteredPlayer(player);

			foundPlayers.Remove(player);
		}

		private void PotentialPlayer_Click(object sender, RoutedEventArgs e)
		{
			PotentialPlayer player = (sender as Button).Tag as PotentialPlayer;
			if (player.bExactMatch)
			{
				AddNameFromRegisteredPlayersEnterTextBox(player);
			}
			else
			{
				FixBoxFirstName = player.firstName;
				FixBoxLastName = player.lastName;
			}
		}

		private void FixBoxName_TextChanged(object sender, TextChangedEventArgs e)
		{
			partialMatches.Clear();

			ObservableCollection<PlayerRanking> potentialPlayerMatches =
				NameFinder.GetFilteredNames(playerRankingData.playerRankings, FixBoxFirstName, FixBoxLastName);

			foreach (PlayerRanking pr in potentialPlayerMatches)
			{
				if (!IsPlayerRegistered(pr.firstName, pr.lastName))
				{
					partialMatches.Add(new PotentialPlayer(pr, false));
				}
			}
		}

		private void AddRegisteredPlayerFromPotentialMatches_Click(object sender, RoutedEventArgs e)
		{
			PotentialPlayer player = (sender as Button).Tag as PotentialPlayer;
			AddNameFromRegisteredPlayersEnterTextBox(player);
		}

		private void AddNewName_Click(object sender, RoutedEventArgs e)
		{
			PlayerRanking pr = new PlayerRanking();
			pr.firstName = FixBoxFirstName;
			pr.lastName = FixBoxLastName;
			pr.points = 0;
			pr.rank = 0;
			pr.IsRegistered = true;

			AddRegisteredPlayer(pr);
		}
	}

	public class PotentialPlayer
	{
		public string firstName { get; set; }
		public string lastName { get; set; }
		public float points { get; set; }
		public float womenPoints { get; set; }
		public int rank { get; set; }
		public bool bExactMatch { get; set; }
		public bool bImperfectMatch { get { return !bExactMatch; } }
		public string FullName { get { return firstName + " " + lastName; } }
		public string ButtonText { get { return bExactMatch ? "Add" : "Fix"; } }

		public PotentialPlayer() { }
		public PotentialPlayer(string first, string last)
		{
			firstName = first;
			lastName = last;
		}
		public PotentialPlayer(PlayerRanking pr, bool inbExactMatch)
		{
			firstName = pr.firstName;
			lastName = pr.lastName;
			points = pr.points;
			womenPoints = pr.womenPoints;
			rank = pr.rank;
			bExactMatch = inbExactMatch;
		}
		public PotentialPlayer(RegisteredPlayer rp, bool inbExactMatch)
		{
			firstName = rp.firstName;
			lastName = rp.lastName;
			points = rp.points;
			womenPoints = rp.womenPoints;
			rank = rp.rank;
			bExactMatch = inbExactMatch;
		}
		public PotentialPlayer(string nameLine)
		{
			string f = "", l = "";
			NameFinder.GetValidNames(nameLine, ref f, ref l);
			firstName = f;
			lastName = l;
		}
	};
}
