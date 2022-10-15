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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PoolCreator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		Random random = new Random();

		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
#if !DEBUG
			this.WindowState = WindowState.Maximized;
#endif

			InitializeComponent();
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void RankingsURL_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TopLevelMainWindowGrid.DataContext = this;

			tournamentData = TournamentData.LoadFromDisk();

			tournamentData.Init();

			Init();
		}

		void Init()
		{
			InitData();
			InitPlayerRankings();
			InitEnterPlayerNames();
			InitRegisteredPlayers();
			InitTeamsRegisteredPlayers();
			InitPools();
			InitJudges();
			InitExport();
			TournamentDetails.Init(this);
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				tournamentData.SaveToDisk();

				TabControl tabControl = sender as TabControl;
				if (tabControl.SelectedIndex == 3)
				{
					OnPoolsTabSelected();
				}
				else if (tabControl.SelectedIndex == 4)
				{
					OnJudgeTabSelected();
				}
			}
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			tournamentData.SaveToDisk();

			ShutdownExport();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			tournamentData.SaveToDisk();
		}

		private void ImportItem_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
			if (dialog.ShowDialog() == true)
			{
				string namesXmlFilename = (new FileInfo(dialog.FileName)).DirectoryName + "\\names.xml";
				if (TryImportNames(namesXmlFilename))
				{
					XmlDocument xml = new XmlDocument();
					xml.Load(dialog.FileName);

					XmlNode root = xml.LastChild.FirstChild;

					TournamentData importedData = new TournamentData();
					EDivision division = EDivision.Open;
					foreach (XmlNode divisionNode in root.ChildNodes) // DivisionData
					{
						ImportDivisionData(importedData, divisionNode, division);
						division += 1;
					}

					tournamentData = importedData;

					Init();
				}
			}
		}

		private bool TryImportNames(string filename)
		{
			if (File.Exists(filename))
			{
				XmlDocument xml = new XmlDocument();
				xml.Load(filename);

				XmlNode root = xml.LastChild.FirstChild;

				ImportNames(root);

				return true;
			}

			return false;
		}

		void ImportNames(XmlNode node)
		{
			TournamentData.importedNames.Clear();

			foreach (XmlNode nameDataNode in node.ChildNodes)
			{
				ImportedName newName = new ImportedName();

				XmlNode propNode = nameDataNode.FirstChild;
				newName.Id = int.Parse(propNode.FirstChild.Value);

				propNode = propNode.NextSibling;
				newName.FirstName = propNode.FirstChild.Value;

				propNode = propNode.NextSibling;
				newName.LastName = propNode.FirstChild.Value;

				TournamentData.importedNames.Add(newName);
			}
		}

		void ImportDivisionData(TournamentData importedData, XmlNode node, EDivision division)
		{
			DivisionData divisionData = new DivisionData(division);
			foreach (XmlNode roundNode in node.ChildNodes) // Rounds
			{
				ERound round = ERound.Finals;
				foreach (XmlNode roundDataNode in roundNode.ChildNodes)
				{
					RoundData roundData = new RoundData(division, round);
					EPool pool = EPool.A;
					foreach (XmlNode poolDataNode in roundDataNode.FirstChild.ChildNodes) // Pools
					{
						PoolData poolData = new PoolData(pool);
						foreach (XmlNode poolDataChildNode in poolDataNode.ChildNodes)
						{
							if (poolDataChildNode.Name == "PoolName")
							{
								poolData.pool = (EPool)Enum.Parse(typeof(EPool), poolDataChildNode.FirstChild.Value);
							}
							else if (poolDataChildNode.Name == "Teams")
							{
								ImportTeams(importedData, divisionData, poolData, poolDataChildNode);
								ImportJudges(importedData, divisionData, poolData, poolDataChildNode);
							}
							else if (poolDataChildNode.Name == "ResultsByTeamIndex")
							{
								ImportPoolResults(poolData, poolDataChildNode);
							}
						}

						roundData.pools.Add(poolData);

						++pool;
					}

					divisionData.rounds.Add(roundData);

					++round;
				}
			}

			importedData.divisions.Add(divisionData);
		}

		EJudgeCategory GetJudgeCategory(string str)
		{
			if (str.StartsWith("ExAi"))
			{
				return EJudgeCategory.ExAi;
			}
			else if (str.StartsWith("Variety"))
			{
				return EJudgeCategory.Variety;
			}
			else if (str.StartsWith("Diff"))
			{
				return EJudgeCategory.Difficulty;
			}

			return EJudgeCategory.ExAi;
		}

		void ImportJudges(TournamentData importedData, DivisionData divisionData, PoolData poolData, XmlNode node)
		{
			foreach (XmlNode teamDataNode in node.ChildNodes)
			{
				TeamData teamData = new TeamData();

				foreach (XmlNode dataNode in teamDataNode.FirstChild.ChildNodes)
				{
					if (dataNode.Name == "RoutineScores")
					{
						foreach (XmlNode resultsNode in dataNode.ChildNodes)
						{
							if (resultsNode.Name.Contains("Results"))
							{
								foreach (XmlNode judgeDataNode in resultsNode.ChildNodes)
								{
									int judgeNameId = int.Parse(judgeDataNode.FirstChild.FirstChild.Value);
									RegisteredPlayer newJudge = new RegisteredPlayer();
									if (tournamentData.FindOrAddImportedRegisterPlayer(judgeNameId, ref newJudge))
									{
										poolData.judgesData.Add(newJudge, GetJudgeCategory(resultsNode.Name));
									}
								}
							}
						}
					}
				}
			}
		}

		void ImportPoolResults(PoolData poolData, XmlNode node)
		{
			poolData.resultRank.Clear();
			for (int i = 0; i < node.ChildNodes.Count; ++i)
			{
				poolData.resultRank.Add(0);
			}

			int rank = 1;
			foreach (XmlNode resultIndexNode in node.ChildNodes)
			{
				int teamIndex = int.Parse(resultIndexNode.FirstChild.Value);
				poolData.resultRank[teamIndex] = rank;

				++rank;
			}
		}

		void ImportTeams(TournamentData importedData, DivisionData divisionData, PoolData poolData, XmlNode node)
		{
			foreach (XmlNode teamDataNode in node.ChildNodes)
			{
				TeamData teamData = new TeamData();

				foreach (XmlNode playerDataNode in teamDataNode.FirstChild.FirstChild.ChildNodes)
				{
					if (playerDataNode.Name == "PlayerData")
					{
						RegisteredPlayer newPlayer = new RegisteredPlayer();

						if (tournamentData.FindOrAddImportedRegisterPlayer(int.Parse(playerDataNode.FirstChild.FirstChild.Value), ref newPlayer))
						{
							teamData.players.Add(newPlayer);

							importedData.AddRegisteredPlayer(newPlayer);
						}
					}
				}

				poolData.teamList.teams.Add(teamData);

				bool bNewTeam = true;
				foreach (TeamData td in divisionData.teamList.teams)
				{
					if (td.Equals(teamData))
					{
						bNewTeam = false;
						break;
					}
				}

				if (bNewTeam)
				{
					divisionData.teamList.teams.Add(teamData);
				}
			}
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			DateTime buildDate = new DateTime(2000, 1, 1)
				.AddDays(version.Build)
				.AddSeconds(version.Revision * 2);

			MessageBox.Show("Build Time: " + buildDate.ToString(), "Pool Creator by Ryan Young");
		}
	}

	public static partial class NameFinder
	{
		public static char[] splitPlayerChars = { '-' };
		private static char[] splitNameChars = { ' ', ',' };
		private static List<HashSet<char>> alternateChars = new List<HashSet<char>>();

		public enum ENameMatch
		{
			None,
			Exact,
			Close,
			Max
		};

		public static float EXACT_MATCH_RATING = -1f;
		public static float FULLNAME_RATING_CUTOFF = 2.5f;
		public static float EXACT_MATCH_RATING_VALUE = 100f;

		private static bool CompareChar(char c1, char c2)
		{
			if (c1 == c2)
			{
				return true;
			}

#region Alternate Chars Init
			if (alternateChars.Count == 0)
			{
				HashSet<char> charZ = new HashSet<char>();
				charZ.Add('z');
				charZ.Add('ž');
				alternateChars.Add(charZ);

				HashSet<char> charI = new HashSet<char>();
				charI.Add('i');
				charI.Add('¡');
				charI.Add('ì');
				charI.Add('í');
				charI.Add('î');
				charI.Add('ï');
				alternateChars.Add(charI);

				HashSet<char> charA = new HashSet<char>();
				charA.Add('a');
				charA.Add('à');
				charA.Add('á');
				charA.Add('â');
				charA.Add('ã');
				charA.Add('ä');
				charA.Add('å');
				alternateChars.Add(charA);

				HashSet<char> charC = new HashSet<char>();
				charC.Add('c');
				charC.Add('ç');
				alternateChars.Add(charC);

				HashSet<char> charE = new HashSet<char>();
				charE.Add('e');
				charE.Add('è');
				charE.Add('é');
				charE.Add('ê');
				charE.Add('ë');
				alternateChars.Add(charE);

				HashSet<char> charN = new HashSet<char>();
				charN.Add('n');
				charN.Add('ñ');
				alternateChars.Add(charN);

				HashSet<char> charO = new HashSet<char>();
				charO.Add('o');
				charO.Add('ò');
				charO.Add('ó');
				charO.Add('ô');
				charO.Add('õ');
				charO.Add('ö');
				alternateChars.Add(charO);

				HashSet<char> charU = new HashSet<char>();
				charU.Add('u');
				charU.Add('ù');
				charU.Add('ú');
				charU.Add('û');
				charU.Add('ü');
				alternateChars.Add(charU);
			}
#endregion

			foreach (HashSet<char> charHash in alternateChars)
			{
				if (charHash.Contains(c1) && charHash.Contains(c2))
				{
					return true;
				}
			}

			return false;
		}

		private static int GetNumberOfSequentialMatchingChars(string matchTo, string matchFrom)
		{
			int ret = 0;
			
			for (int matchToIndex = 0; matchToIndex < matchTo.Length; ++matchToIndex)
			{
				int foundMatchCount = 0;
				for (int i = 0; i < matchFrom.Length; ++i)
				{
					int peekMatchToIndex = matchToIndex + foundMatchCount;
					if (peekMatchToIndex < matchTo.Length && CompareChar(matchFrom[i], matchTo[peekMatchToIndex]))
					{
						++foundMatchCount;
					}
				}

				ret = Math.Max(foundMatchCount, ret);
			}

			return ret;
		}

		private static int GetNumberOfStartingMatchingChars(string string1, string string2)
		{
			int ret = 0;

			int shortestStrLen = Math.Min(string1.Length, string2.Length);
			for (int i = 0; i < shortestStrLen; ++i)
			{
				if (CompareChar(string1[i], string2[i]))
				{
					++ret;
				}
				else
				{
					return ret;
				}
			}

			return ret;
		}

		private static float GetMatchRating(string string1, string string2)
		{
			if (string1.Length == 0 || string2.Length == 0)
			{
				return 0;
			}

			if (string1 == string2)
			{
				return EXACT_MATCH_RATING;
			}

			float rating = Math.Max(GetNumberOfSequentialMatchingChars(string1, string2), GetNumberOfSequentialMatchingChars(string2, string1));
			int longestStrLen = Math.Min(string1.Length, string2.Length);
			rating *= rating / longestStrLen;

			int matchingStartingChars = GetNumberOfStartingMatchingChars(string1, string2);
			if (matchingStartingChars == string2.Length || matchingStartingChars > 2)
			{
				rating += FULLNAME_RATING_CUTOFF;
			}

			return rating;
		}

		public static float GetMatchRating(string firstName, string lastName, string name1, string name2)
		{
			firstName = firstName.ToLower();
			lastName = lastName.ToLower();
			name1 = name1.ToLower();
			name2 = name2.ToLower();

			if ((name1 == null && name2 == null) || (name1 == "" && name2 == ""))
			{
				return EXACT_MATCH_RATING;
			}

			if ((name1 == firstName && name2 == lastName) || (name2 == firstName && name1 == lastName))
			{
				return EXACT_MATCH_RATING;
			}

			float rating1a = GetMatchRating(firstName, name1);
			rating1a = rating1a == EXACT_MATCH_RATING ? EXACT_MATCH_RATING_VALUE : rating1a;
			float rating1b = GetMatchRating(lastName, name2);
			rating1b = rating1b == EXACT_MATCH_RATING ? EXACT_MATCH_RATING_VALUE : rating1b;

			float rating2a = GetMatchRating(firstName, name2);
			rating2a = rating2a == EXACT_MATCH_RATING ? EXACT_MATCH_RATING_VALUE : rating2a;
			float rating2b = GetMatchRating(lastName, name1);
			rating2b = rating2b == EXACT_MATCH_RATING ? EXACT_MATCH_RATING_VALUE : rating2b;

			float rating1 = rating1a + rating1b;
			float rating2 = rating2a + rating2b;
			if (rating1 >= FULLNAME_RATING_CUTOFF || rating2 >= FULLNAME_RATING_CUTOFF)
			{
				return Math.Max(rating1, rating2);
			}

			return 0;
		}

		private static bool ContainsSplitNameChar(string str)
		{
			for (int i = 0; i < splitNameChars.Length; ++i)
			{
				if (str.Contains(splitNameChars[i]))
				{
					return true;
				}
			}

			return false;
		}

		public static int GetValidNames(string name, ref string name1, ref string name2)
		{
			bool bFoundName1 = false;
			int foundCount = 0;
			string[] splits = name.Split(splitNameChars);
			for (int i = 0; i < splits.Length; ++i)
			{
				string split = splits[i];
				if (split.Length > 0 && !ContainsSplitNameChar(split))
				{
					if (!bFoundName1)
					{
						bFoundName1 = true;
						name1 = split;

						foundCount = 1;
					}
					else
					{
						name2 = split;
						return 2;
					}
				}
			}

			return foundCount;
		}

		public static ObservableCollection<PlayerRanking> GetFilteredNames(ObservableCollection<PlayerRanking> playerRankings, string name)
		{
			string name1 = "";
			string name2 = "";
			GetValidNames(name, ref name1, ref name2);

			if (name1 == "")
			{
				return playerRankings;
			}

			return GetFilteredNames(playerRankings, name1, name2);
		}

		public static ObservableCollection<PlayerRanking> GetFilteredNames(ObservableCollection<PlayerRanking> playerRankings, string name1, string name2)
		{
			ObservableCollection<PlayerRanking> ret = new ObservableCollection<PlayerRanking>();
			List<PlayerRankingSortData> sortedPlayerRankings = GetFilteredNamesRaw(playerRankings, name1, name2);

			foreach (PlayerRankingSortData prsd in sortedPlayerRankings)
			{
				ret.Add(prsd.playerRanking);
			}

			return ret;
		}

		private static List<PlayerRankingSortData> GetFilteredNamesRaw(ObservableCollection<PlayerRanking> playerRankings, string name1, string name2)
		{
			if (name1 == "" && name2 == "")
			{
				return new List<PlayerRankingSortData>();
			}

			List<PlayerRankingSortData> sortedPlayerRankings = new List<PlayerRankingSortData>();
			foreach (PlayerRanking pr in playerRankings)
			{
				float matchRating = GetMatchRating(pr.firstName, pr.lastName, name1, name2);
				if (matchRating == EXACT_MATCH_RATING || matchRating >= FULLNAME_RATING_CUTOFF)
				{
					sortedPlayerRankings.Add(new PlayerRankingSortData(pr, matchRating));
				}
			}

			sortedPlayerRankings.Sort();

			return sortedPlayerRankings;
		}

		public static ObservableCollection<RegisteredPlayer> GetFilteredNames(ObservableCollection<RegisteredPlayer> registeredPlayers, string name)
		{
			string name1 = "";
			string name2 = "";
			GetValidNames(name, ref name1, ref name2);

			if (name1 == "")
			{
				return registeredPlayers;
			}

			return GetFilteredNames(registeredPlayers, name1, name2);
		}

		public static ObservableCollection<RegisteredPlayer> GetFilteredNames(ObservableCollection<RegisteredPlayer> registeredPlayers, string name1, string name2)
		{
			ObservableCollection<RegisteredPlayer> ret = new ObservableCollection<RegisteredPlayer>();
			List<PlayerRankingSortData> sortedPlayerRankings = GetFilteredNamesRaw(registeredPlayers, name1, name2);

			foreach (PlayerRankingSortData prsd in sortedPlayerRankings)
			{
				ret.Add(prsd.registeredPlayer);
			}

			return ret;
		}

		private static List<PlayerRankingSortData> GetFilteredNamesRaw(ObservableCollection<RegisteredPlayer> registeredPlayers, string name1, string name2)
		{
			if (name1 == "" && name2 == "")
			{
				return new List<PlayerRankingSortData>();
			}

			List<PlayerRankingSortData> sortedPlayerRankings = new List<PlayerRankingSortData>();
			foreach (RegisteredPlayer rp in registeredPlayers)
			{
				float matchRating = GetMatchRating(rp.firstName, rp.lastName, name1, name2);
				if (matchRating == EXACT_MATCH_RATING || matchRating >= FULLNAME_RATING_CUTOFF)
				{
					sortedPlayerRankings.Add(new PlayerRankingSortData(rp, matchRating));
				}
			}

			sortedPlayerRankings.Sort();

			return sortedPlayerRankings;
		}

		public static bool GetClosestName(ObservableCollection<PlayerRanking> playerRankings, string name, ref PlayerRanking outPlayerRanking, ref bool outExactMatch)
		{
			string name1 = "";
			string name2 = "";
			GetValidNames(name, ref name1, ref name2);

			if (name1 == "")
			{
				return false;
			}

			List<PlayerRankingSortData> sortedPlayers = GetFilteredNamesRaw(playerRankings, name1, name2);
			if (sortedPlayers.Count > 0)
			{
				outPlayerRanking = sortedPlayers[0].playerRanking;
				outExactMatch = sortedPlayers[0].rating == EXACT_MATCH_RATING;

				return true;
			}

			return false;
		}

		public class PlayerRankingSortData : IComparable<PlayerRankingSortData>
		{
			public PlayerRanking playerRanking;
			public RegisteredPlayer registeredPlayer;
			public float rating;

			public PlayerRankingSortData() { }
			public PlayerRankingSortData(PlayerRanking inPlayerRanking, float inRating)
			{
				playerRanking = inPlayerRanking;
				rating = inRating;
			}

			public PlayerRankingSortData(RegisteredPlayer inRegisteredPlayer, float inRating)
			{
				registeredPlayer = inRegisteredPlayer;
				rating = inRating;
			}

			public int CompareTo(PlayerRankingSortData other)
			{
				if (other.rating == EXACT_MATCH_RATING)
				{
					return 1;
				}
				else if (rating == EXACT_MATCH_RATING)
				{
					return -1;
				}
				else if (other.rating > rating)
				{
					return 1;
				}
				else if (other.rating < rating)
				{
					return -1;
				}

				return 0;
			}
		};
	};
}
