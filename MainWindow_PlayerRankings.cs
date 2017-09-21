using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace PoolCreator
{
	/// <summary>
	/// Summary description for MainWindow
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		string playerRankingSaveFilename = System.AppDomain.CurrentDomain.BaseDirectory + "\\PlayerRankings.xml";
		PlayerRankingData playerRankingData = new PlayerRankingData();
		List<PlayerRanking> asyncRetrievedPlayerRankings = new List<PlayerRanking>();

		private void InitPlayerRankings()
		{
			playerRankingData.playerRankings = new ObservableCollection<PlayerRanking>();

			if (File.Exists(playerRankingSaveFilename))
			{
				using (StreamReader saveFile = new StreamReader(playerRankingSaveFilename))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(PlayerRankingData));
					playerRankingData = (PlayerRankingData)serializer.Deserialize(saveFile);

					FillRankingsTextBox();
				}
			}
		}

		private void UpdateRegisteredPlayerRankAndPoints(RegisteredPlayer outPlayer)
		{
			foreach (PlayerRanking originalPlayer in playerRankingData.playerRankings)
			{
				if (outPlayer.FullName == originalPlayer.FullName)
				{
					outPlayer.rank = originalPlayer.rank;
					outPlayer.points = originalPlayer.points;
					outPlayer.womenPoints = originalPlayer.womenPoints;

					return;
				}
			}
		}

		private void UpdateRankingPointsInCurrentData()
		{
			foreach (RegisteredPlayer rp in tournamentData.registeredPlayers)
			{
				UpdateRegisteredPlayerRankAndPoints(rp);
			}

			foreach (DivisionData dd in tournamentData.divisions)
			{
				foreach (TeamData td in dd.teamList.teams)
				{
					foreach (RegisteredPlayer rp in td.players)
					{
						UpdateRegisteredPlayerRankAndPoints(rp);
					}
				}

				foreach (RoundData rd in dd.rounds)
				{
					foreach (PoolData pd in rd.pools)
					{
						foreach (TeamData td in pd.teamList.teams)
						{
							foreach (RegisteredPlayer rp in td.players)
							{
								UpdateRegisteredPlayerRankAndPoints(rp);
							}
						}
					}
				}
			}
		}

		private void UpdateRankingsButton_Click(object sender, RoutedEventArgs e)
		{
			GetAndSavePlayerRankings();
		}

		private void GetAndSavePlayerRankings()
		{
			LastUpdatedLabel.Content = "Querying internet for rankings.  May take up to 15 seconds.";
			playerRankingData.playerRankings.Clear();

			BackgroundWorker getRankingsWorker = new BackgroundWorker();
			string url = RankingsURL.Text;
			string womenUrl = WomenRankingsURL.Text;
			getRankingsWorker.DoWork += delegate { GetRankingsWorker_DoWork(url, true); };
			getRankingsWorker.DoWork += delegate { GetRankingsWorker_DoWork(womenUrl, false); };
			getRankingsWorker.RunWorkerCompleted += delegate { GetRankingsWorker_RunWorkerCompleted(); };
			getRankingsWorker.RunWorkerAsync();
		}

		private void GetRankingsWorker_RunWorkerCompleted()
		{
			foreach (PlayerRanking pr in asyncRetrievedPlayerRankings)
			{
				playerRankingData.playerRankings.Add(pr);
			}

			FillRankingsTextBox();

			UpdateRankingPointsInCurrentData();

			SaveRankingsToDisk();
		}

		private void GetRankingsWorker_DoWork(string url, bool bIsOpenRankings)
		{
			using (WebClient client = new WebClient())
			{
				string htmlSource = client.DownloadString(url);

				if (htmlSource != null && htmlSource.Length > 0)
				{
					playerRankingData.time = DateTime.Now;

					string line = null;
					StringReader textStream = new StringReader(htmlSource);
					bool bFoundFirstPlayer = false;
					string rankTag = "<td height=19 class=xl6330694 style='height:14.4pt'>";
					string nameClass = "xl1530694";
					string pointsClass = "xl6330694";
					while ((line = textStream.ReadLine()) != null)
					{
						line = line.Trim();

						if (!bFoundFirstPlayer && line.Contains(">1<"))
						{
							bFoundFirstPlayer = true;

							rankTag = line.Substring(0, line.IndexOf(">1<") + 1);

							line = textStream.ReadLine();
							line = textStream.ReadLine().Trim();
							line = line.Replace("<td class=", "");
							nameClass = line.Substring(0, line.IndexOf(">"));

							line = textStream.ReadLine();
							line = textStream.ReadLine();
							line = textStream.ReadLine().Trim();
							line = line.Replace("<td class=", "");
							pointsClass = line.Substring(0, line.IndexOf(">"));

							textStream = new StringReader(htmlSource);
						}
						else if (line.StartsWith(rankTag))
						{
							PlayerRanking newPlayer = new PlayerRanking();
							string rankStr = line.Trim().Replace(rankTag, "");
							rankStr = rankStr.Replace("</td>", "").Replace("T", "");
							int.TryParse(rankStr, out newPlayer.rank);
							newPlayer.womenPoints = 0;
							textStream.ReadLine();

							string nameLine = textStream.ReadLine().Trim().Replace("<td class=" + nameClass + ">", "");
							int nameLineCommaIndex = nameLine.IndexOf(',');
							if (nameLineCommaIndex == -1)
							{
								continue;
							}
							string lastName = nameLine.Substring(0, nameLineCommaIndex);
							string firstName = nameLine.Substring(nameLineCommaIndex + 2, nameLine.IndexOf("</td>") - nameLineCommaIndex - 2);
							newPlayer.firstName = firstName;
							newPlayer.lastName = lastName;


							textStream.ReadLine();
							if (bIsOpenRankings)
							{
								// Open rankings has extra line for gender
								textStream.ReadLine();
							}
							string pointsLine = textStream.ReadLine().Trim();
							pointsLine = pointsLine.Replace("<td class=" + pointsClass + ">", "").Replace("</td>", "").Replace(",", ".");
							float.TryParse(pointsLine, out newPlayer.points);

							if (bIsOpenRankings)
							{
								asyncRetrievedPlayerRankings.Add(newPlayer);
							}
							else
							{
								// Women points need to be queried after open
								foreach (PlayerRanking pr in asyncRetrievedPlayerRankings)
								{
									if (pr.FullName == newPlayer.FullName)
									{
										pr.womenPoints = newPlayer.points;
									}
								}
							}
						}
					}
				}
			}
		}

		private void FillRankingsTextBox()
		{
			LastUpdatedLabel.Content = "Last Updated: " + playerRankingData.time.ToString();
			
			TournamentData.filteredPlayerRankings =
				NameFinder.GetFilteredNames(playerRankingData.playerRankings, PlayerRankingsFilterTextBox.Text);

			foreach (PlayerRanking pr in TournamentData.filteredPlayerRankings)
			{
				pr.IsRegistered = IsPlayerRegistered(pr.firstName, pr.lastName);
			}

			PlayerRankingsItemsControl.ItemsSource = null;
			PlayerRankingsItemsControl.ItemsSource = TournamentData.filteredPlayerRankings;
		}

		private void SaveRankingsToDisk()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(PlayerRankingData));
			using (StringWriter retString = new StringWriter())
			{
				serializer.Serialize(retString, playerRankingData);
				// Encoding.GetEncoding("iso-8859-1")
				using (StreamWriter saveFile = new StreamWriter(playerRankingSaveFilename))
				{
					saveFile.Write(retString.ToString());
				}
			}
		}

		private void PlayerRankingsFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			FillRankingsTextBox();
		}

		private void RankingPlayer_Click(object sender, RoutedEventArgs e)
		{
			PlayerRanking player = (sender as Button).Tag as PlayerRanking;

			if (player.IsRegistered)
			{
				RemoveRegisteredPlayer(player.firstName, player.lastName);
			}
			else
			{
				AddRegisteredPlayer(player);
			}

			FillRankingsTextBox();
		}
	}

	public class PlayerRanking : INotifyPropertyChanged
	{
		public string firstName;
		public string lastName;
		public float points;
		public float womenPoints;
		public int rank;
		private bool bIsRegistered = false;
		public bool IsRegistered
		{
			set
			{
				bIsRegistered = value;
				OnPropertyChanged("IsRegistered");
			}
			get
			{
				return bIsRegistered;
			}
		}
		public string FullName { get { return firstName + " " + lastName; } }
		public string ButtonText { get { return !bIsRegistered ? "Add" : "Remove"; } }
		public event PropertyChangedEventHandler PropertyChanged;

		public override string ToString()
		{
			return rank + " " + firstName + " " + lastName + " - " + points;
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	};
}

