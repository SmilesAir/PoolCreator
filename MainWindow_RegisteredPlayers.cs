using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		ObservableCollection<RegisteredPlayer> registeredPlayers = new ObservableCollection<RegisteredPlayer>();
		public ObservableCollection<RegisteredPlayer> RegisteredPlayers { get { return registeredPlayers; } }

		private void InitRegisteredPlayers()
		{
			registeredPlayers = tournamentData.registeredPlayers;

			RegisteredPlayersUserControl.RegisteredPlayersItemsControl.ItemsSource = registeredPlayers;
		}

		private bool IsPlayerRegistered(string firstName, string lastName)
		{
			foreach (RegisteredPlayer rp in registeredPlayers)
			{
				if (firstName == rp.firstName && lastName == rp.lastName)
				{
					return true;
				}
			}

			return false;
		}

		private void AddRegisteredPlayer(PlayerRanking player)
		{
			RegisteredPlayer newPlayer = new RegisteredPlayer(player.firstName, player.lastName, player.points, player.rank);
			registeredPlayers.Add(newPlayer);
		}

		private void AddRegisteredPlayer(PotentialPlayer player)
		{
			RegisteredPlayer newPlayer = new RegisteredPlayer(player);
			registeredPlayers.Add(newPlayer);
		}

		private void RemoveRegisteredPlayer(string firstName, string lastName)
		{
			for (int i = 0; i < registeredPlayers.Count; ++i)
			{
				if (registeredPlayers[i].firstName == firstName && registeredPlayers[i].lastName == lastName)
				{
					registeredPlayers.RemoveAt(i);
					return;
				}
			}
		}

		private void RemoveRegisteredPlayer(RegisteredPlayer player)
		{
			RemoveRegisteredPlayer(player.firstName, player.lastName);
		}

		private void RemoveRegisteredPlayer_Click(object sender, RoutedEventArgs e)
		{
			RegisteredPlayer player = (sender as Button).Tag as RegisteredPlayer;
			RemoveRegisteredPlayer(player);
		}

		public bool TryFindRegisteredPlayer(string playerName, out RegisteredPlayer outRegisteredPlayer)
		{
			foreach (RegisteredPlayer rp in registeredPlayers)
			{
				if (rp.FullName == playerName)
				{
					outRegisteredPlayer = rp;

					return true;
				}
			}

			outRegisteredPlayer = new RegisteredPlayer();

			return false;
		}
	}

	public class RegisteredPlayer
	{
		public string firstName { get; set; }
		public string lastName { get; set; }
		public float points { get; set; }
		public float womenPoints { get; set; }
		public int rank { get; set; }
		public int totalJudgingCount { get; set; }

		public string FullName { get { return firstName + " " + lastName; } }
		public string FullNameAndRank { get { return FullName + " #" + rank; } }

		public RegisteredPlayer() { }
		public RegisteredPlayer(string inFirstName, string inLastName, float inPoints, int inRank)
		{
			firstName = inFirstName;
			lastName = inLastName;
			points = inPoints;
			rank = inRank;
		}
		public RegisteredPlayer(PotentialPlayer player)
		{
			firstName = player.firstName;
			lastName = player.lastName;
			points = player.points;
			womenPoints = player.womenPoints;
			rank = player.rank;
		}
		public RegisteredPlayer(PlayerRanking player)
		{
			firstName = player.firstName;
			lastName = player.lastName;
			points = player.points;
			womenPoints = player.womenPoints;
			rank = player.rank;
		}
	};

	public class ItemToIndexConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			//ObservableCollection<RegisteredPlayer> itemSource = parameter as ObservableCollection<RegisteredPlayer>;
			//IEnumerable<object> items = itemSource.Source as IEnumerable<object>;

			//return items.IndexOf(value as object);

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Binding.DoNothing;
		}
	}
}
