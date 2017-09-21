using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public TournamentData tournamentData = new TournamentData();

		public IEnumerable<EDivisionDisplay> EDivisionTypeValues
		{
			get
			{
				return Enum.GetValues(typeof(EDivisionDisplay)).Cast<EDivisionDisplay>();
			}
		}

		public IEnumerable<ERoundDisplay> ERoundTypeValues
		{
			get
			{
				return Enum.GetValues(typeof(ERoundDisplay)).Cast<ERoundDisplay>();
			}
		}

		public IEnumerable<ERoundJudgeDisplay> ERoundJudgeTypeValues
		{
			get
			{
				return Enum.GetValues(typeof(ERoundJudgeDisplay)).Cast<ERoundJudgeDisplay>();
			}
		}

		public void InitData()
		{
			TeamsDivisionComboBox.DataContext = this;
		}

		public void GetPlayingTeams(EDivision division, ERoundJudgeDisplay round, int controlIndex, out ObservableCollection<TeamData> outPlayingTeams)
		{
			EPool pool = EnumConverter.ConvertPoolValue(round, controlIndex);

			if (pool != EPool.None)
			{
				PoolData pd = tournamentData.GetPool(division, EnumConverter.ConvertRoundValue(round), pool);
				outPlayingTeams = pd.teamList.teams;
			}
			else
			{
				outPlayingTeams = null;
			}
		}
	}

	public static class EnumConverter
	{
		public static EDivision ConvertDivisionValue(EDivisionDisplay divisionDisplay)
		{
			if (divisionDisplay == EDivisionDisplay.None)
			{
				return EDivision.None;
			}
			else
			{
				return (EDivision)divisionDisplay;
			}
		}

		public static EDivisionDisplay ConvertDivisionValue(EDivision division)
		{
			if (division == EDivision.None)
			{
				return EDivisionDisplay.None;
			}
			else
			{
				return (EDivisionDisplay)division;
			}
		}

		public static ERound ConvertRoundValue(ERoundDisplay roundDisplay)
		{
			if (roundDisplay == ERoundDisplay.None)
			{
				return ERound.None;
			}
			else
			{
				return (ERound)roundDisplay;
			}
		}

		public static ERoundDisplay ConvertRoundValue(ERound round)
		{
			if (round == ERound.None)
			{
				return ERoundDisplay.None;
			}
			else
			{
				return (ERoundDisplay)round;
			}
		}

		public static ERound ConvertRoundValue(ERoundJudgeDisplay roundDisplay)
		{
			switch (roundDisplay)
			{
				case ERoundJudgeDisplay.PrelimsAC:
					return ERound.Prelims;
				case ERoundJudgeDisplay.PrelimsBD:
					return ERound.Prelims;
				case ERoundJudgeDisplay.QuarterfinalsAC:
					return ERound.Quarterfinals;
				case ERoundJudgeDisplay.QuarterfinalsBD:
					return ERound.Quarterfinals;
				case ERoundJudgeDisplay.Semifinals:
					return ERound.Semifinals;
				case ERoundJudgeDisplay.Finals:
					return ERound.Finals;
				default:
					return ERound.None;
			}
		}

		public static EPool ConvertPoolValue(ERoundJudgeDisplay round, int controlIndex)
		{
			if (round == ERoundJudgeDisplay.Finals)
			{
				return controlIndex == 0 ? EPool.A : EPool.None;
			}
			else if (round == ERoundJudgeDisplay.Semifinals)
			{
				return controlIndex == 0 ? EPool.A : EPool.B;
			}
			else if (round == ERoundJudgeDisplay.QuarterfinalsAC)
			{
				return controlIndex == 0 ? EPool.A : EPool.C;
			}
			else if (round == ERoundJudgeDisplay.QuarterfinalsBD)
			{
				return controlIndex == 0 ? EPool.B : EPool.D;
			}
			else if (round == ERoundJudgeDisplay.PrelimsAC)
			{
				return controlIndex == 0 ? EPool.A : EPool.C;
			}
			else if (round == ERoundJudgeDisplay.PrelimsBD)
			{
				return controlIndex == 0 ? EPool.B : EPool.D;
			}

			return EPool.None;
		}
	}

	public class PoolKey
	{
		public EDivision division = EDivision.None;
		public ERound round = ERound.None;
		public EPool pool = EPool.None;

		public PoolKey()
		{
		}

		public PoolKey(EDivision inDivision, ERound inRound, EPool inPool)
		{
			division = inDivision;
			round = inRound;
			pool = inPool;
		}
	};

	public enum EDivision
	{
		Open,
		Mixed,
		Coop,
		Women,
		Max,
		None
	}

	public enum EDivisionDisplay
	{
		Open,
		Mixed,
		Coop,
		Women,
		None
	}

	public enum ERound
	{
		Finals,
		Semifinals,
		Quarterfinals,
		Prelims,
		Max,
		None
	}

	public enum ERoundDisplay
	{
		Finals,
		Semifinals,
		Quarterfinals,
		Prelims,
		None
	}

	public enum ERoundJudgeDisplay
	{
		Finals,
		Semifinals,
		QuarterfinalsAC,
		QuarterfinalsBD,
		PrelimsAC,
		PrelimsBD,
		None
	}

	public enum EPool
	{
		A,
		B,
		C,
		D,
		Max,
		None
	}

	[XmlRoot("PlayerRankingData")]
	public class PlayerRankingData
	{
		public DateTime time;

		[XmlArray("playerRankings")]
		[XmlArrayItem("PlayerRanking")]
		public ObservableCollection<PlayerRanking> playerRankings { get; set; }
	};

	public class ImportedName
	{
		public int Id;
		public string FirstName;
		public string LastName;

        public ImportedName()
        {
        }

        public ImportedName(int inId, string inFirstName, string inLastName)
        {
            Id = inId;
            FirstName = inFirstName;
            LastName = inLastName;
        }
    };

	[XmlRoot("TournamentData")]
	public class TournamentData
	{
		static string tournamentSaveFilename = System.AppDomain.CurrentDomain.BaseDirectory + "\\TounamentData.xml";

		[XmlArray("registeredPlayers")]
		[XmlArrayItem("RegisteredPlayer")]
		public ObservableCollection<RegisteredPlayer> registeredPlayers = new ObservableCollection<RegisteredPlayer>();

		[XmlArray("divisions")]
		[XmlArrayItem("DivisionData")]
		public List<DivisionData> divisions = new List<DivisionData>();

		public string TournamentName;
		public string TournamentSubtitle;

		public string exportPath = @"C:\Users\Ryan\Desktop\PoolCreator\Export";
		public string excelTemplatePath = @"C:\Users\Ryan\Desktop\PoolCreator\Export\Scoresheets_2.10.xlsm";

		[XmlIgnoreAttribute]
		public static ObservableCollection<PlayerRanking> filteredPlayerRankings = new ObservableCollection<PlayerRanking>();

		[XmlIgnoreAttribute]
		public static List<ImportedName> importedNames = new List<ImportedName>();

		public TournamentData()
		{
		}

		public void Init()
		{
			if (divisions.Count == 0)
			{
				for (int i = 0; i < (int)EDivision.Max; ++i)
				{
					DivisionData dd = new DivisionData((EDivision)i);
					dd.CreateData();
					divisions.Add(dd);
				}
			}
			else
			{
				EDivision division = EDivision.Open;
				foreach (DivisionData dd in divisions)
				{
					if (dd.division == EDivision.None)
					{
						dd.division = division;
					}

					ERound round = ERound.Finals;
					foreach (RoundData rd in dd.rounds)
					{
						if (rd.round == ERound.None || rd.round == ERound.Max)
						{
							rd.round = round;
						}

						if (rd.maxTeams == 0)
						{
							rd.InitMaxTeams(division, round);
						}

						EPool pool = EPool.A;
						foreach (PoolData pd in rd.pools)
						{
							if (pd.pool == EPool.None)
							{
								pd.pool = pool;
							}

							++pool;
						}

						++round;
					}

					++division;
				}
			}
		}

		public void AddRegisteredPlayer(RegisteredPlayer player)
		{
			registeredPlayers.Add(player);
		}

		public static TournamentData LoadFromDisk()
		{
			if (File.Exists(tournamentSaveFilename))
			{
				using (StreamReader saveFile = new StreamReader(tournamentSaveFilename))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(TournamentData));
					return (TournamentData)serializer.Deserialize(saveFile);
				}
			}

			return new TournamentData();
		}

		public void SaveToDisk()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(TournamentData));
			using (StringWriter retString = new StringWriter())
			{
				serializer.Serialize(retString, this);
				using (StreamWriter saveFile = new StreamWriter(tournamentSaveFilename))
				{
					saveFile.Write(retString.ToString());
				}
			}
		}

		public ObservableCollection<TeamData> GetAllTeams(EDivision division)
		{
			int divisionIndex = (int)division;
			if (divisionIndex >= 0 && divisionIndex < (int)EDivision.Max)
			{
				return divisions[divisionIndex].teamList.teams;
			}

			return new ObservableCollection<TeamData>();
		}

		public DivisionData GetDivision(EDivision division)
		{
			int divisionIndex = (int)division;
			if (divisionIndex >= 0 && divisionIndex < (int)EDivision.Max)
			{
				return divisions[(int)division];
			}

			return new DivisionData();
		}

		public RoundData GetRound(EDivision division, ERound round)
		{
			int divisionIndex = (int)division;
			if (divisionIndex >= 0 && divisionIndex < (int)EDivision.Max)
			{
				int roundIndex = (int)round;
				if (roundIndex >= 0 && roundIndex < (int)ERound.Max)
				{
					return divisions[divisionIndex].rounds[roundIndex];
				}
			}

			return new RoundData();
		}

		public PoolData GetPool(EDivision division, ERound round, EPool pool)
		{
			int divisionIndex = (int)division;
			if (divisionIndex >= 0 && divisionIndex < (int)EDivision.Max)
			{
				int roundIndex = (int)round;
				if (roundIndex >= 0 && roundIndex < (int)ERound.Max)
				{
					int PoolIndex = (int)pool;
					if (PoolIndex >= 0 && PoolIndex < divisions[divisionIndex].rounds[roundIndex].pools.Count)
					{
						return divisions[divisionIndex].rounds[roundIndex].pools[PoolIndex];
					}
				}
			}

			return new PoolData();
		}

		public PoolData GetPool(EDivision division, ERoundJudgeDisplay round, int controlIndex)
		{
			if (round == ERoundJudgeDisplay.Finals || round == ERoundJudgeDisplay.Semifinals)
			{
				return GetPool(division, EnumConverter.ConvertRoundValue(round), (EPool)controlIndex);
			}
			else if (round != ERoundJudgeDisplay.None)
			{
				// If round is Quarter or Prelim
				int divisionIndex = (int)division;
				if (divisionIndex >= 0 && divisionIndex < (int)EDivision.Max)
				{
					int roundIndex = (int)EnumConverter.ConvertRoundValue(round);
					if (roundIndex >= 0 && roundIndex < (int)ERoundJudgeDisplay.None)
					{
						int PoolIndex = 2 * controlIndex +
							(round == ERoundJudgeDisplay.QuarterfinalsBD ? 1 : 0) +
							(round == ERoundJudgeDisplay.PrelimsBD ? 1 : 0);
						if (PoolIndex >= 0 && PoolIndex < divisions[divisionIndex].rounds[roundIndex].pools.Count)
						{
							return divisions[divisionIndex].rounds[roundIndex].pools[PoolIndex];
						}
					}
				}
			}

			return new PoolData();
		}

		public PoolData GetPool(PoolKey poolKey)
		{
			return GetPool(poolKey.division, poolKey.round, poolKey.pool);
		}

		public bool FindRegisterPlayer(string firstName, string lastName, ref RegisteredPlayer outRegisteredPlayer)
		{
			foreach (RegisteredPlayer rp in registeredPlayers)
			{
				if (String.Compare(rp.firstName, firstName, true) == 0 && String.Compare(rp.lastName, lastName, true) == 0)
				{
					outRegisteredPlayer = rp;

					return true;
				}
			}

			return false;
		}

		public bool FindOrAddImportedRegisterPlayer(int importNameId, ref RegisteredPlayer outRegisteredPlayer)
		{
			foreach (ImportedName name in importedNames)
			{
				if (name.Id == importNameId)
				{
					if (FindRegisterPlayer(name.FirstName, name.LastName, ref outRegisteredPlayer))
					{
						return true;
					}
					else
					{
						outRegisteredPlayer = new RegisteredPlayer(name.FirstName, name.LastName, 0, 0);
						AddRegisteredPlayer(outRegisteredPlayer);

						return true;
					}
				}
			}

			return false;
		}

		public static int FindLisaHelperNameId(RegisteredPlayer player)
		{
			foreach (ImportedName name in importedNames)
			{
				if (name.FirstName.ToLower() == player.firstName.ToLower() && name.LastName.ToLower() == player.lastName.ToLower())
				{
					return name.Id;
				}
			}

			return -1;
		}
	}

	public class DivisionData
	{
		public EDivision division = EDivision.None;
		public TeamListData teamList = new TeamListData();
		[XmlArray("rounds")]
		[XmlArrayItem("RoundData")]
		public List<RoundData> rounds = new List<RoundData>();
		public string headJudge;
		public string directors;
		public string committee;

		public DivisionData()
		{
		}

		public DivisionData(EDivision inDivision)
		{
			division = inDivision;
		}

		public void CreateData()
		{
			rounds.Add(new RoundData(division, ERound.Finals));
			rounds.Add(new RoundData(division, ERound.Semifinals));
			rounds.Add(new RoundData(division, ERound.Quarterfinals));
			rounds.Add(new RoundData(division, ERound.Prelims));

			foreach (RoundData rd in rounds)
			{
				rd.CreateData();
			}
		}
	}

	public class RoundData
	{
		public ERound round = ERound.None;
		[XmlArray("pools")]
		[XmlArrayItem("PoolData")]
		public List<PoolData> pools = new List<PoolData>();
		public DateTime scheduleTime = new DateTime();
		public float routineLength = 0f;
		public bool shouldExport = false;
		public int maxTeams = 0;

		public RoundData()
		{
		}

		public RoundData(EDivision inDivision, ERound inRound)
		{
			round = inRound;

			InitMaxTeams(inDivision, inRound);
		}

		public void InitMaxTeams(EDivision inDivision, ERound inRound)
		{
			if (inRound == ERound.Finals)
			{
				if (inDivision == EDivision.Coop || inDivision == EDivision.Women || inDivision == EDivision.Mixed)
				{
					maxTeams = 6;

					return;
				}
			}

			maxTeams = 8;
		}

		public void CreateData()
		{
			switch (round)
			{
				case ERound.Finals:
					pools.Add(new PoolData(EPool.A));
					break;
				case ERound.Semifinals:
					pools.Add(new PoolData(EPool.A));
					pools.Add(new PoolData(EPool.B));
					break;
				case ERound.Quarterfinals:
					pools.Add(new PoolData(EPool.A));
					pools.Add(new PoolData(EPool.B));
					pools.Add(new PoolData(EPool.C));
					pools.Add(new PoolData(EPool.D));
					break;
			}

			foreach (PoolData pd in pools)
			{
				pd.CreateData();
			}
		}

		public DateTime GetScheduleTime()
		{
			if (scheduleTime == new DateTime())
			{
				return DateTime.Now;
			}

			return scheduleTime;
		}
	}

	public class PoolData
	{
		public EPool pool;
		public TeamListData teamList = new TeamListData();
		public ObservableCollection<int> resultRank = new ObservableCollection<int>();
		public JudgesData judgesData = new JudgesData();
		public bool HasResults { get { return resultRank.Count > 0; } }

		public PoolData()
		{
		}

		public PoolData(EPool inPool)
		{
			pool = inPool;
		}

		public void CreateData()
		{

		}

		public void ClearResults()
		{
			resultRank.Clear();
		}

		public bool ContainsPlayer(string playerName)
		{
			foreach (TeamData td in teamList.teams)
			{
				foreach (RegisteredPlayer rp in td.players)
				{
					if (rp.FullName == playerName)
					{
						return true;
					}
				}
			}

			return false;
		}
	}

	public class TeamListData
	{
		[XmlArray("teams")]
		[XmlArrayItem("TeamData")]
		public ObservableCollection<TeamData> teams = new ObservableCollection<TeamData>();

		public TeamListData()
		{
		}
	}

	public enum EOverlayTeamDataState
	{
		Normal,
		DragFrom,
		DragTo,
		Max
	}

	public class TeamData : INotifyPropertyChanged
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

		[XmlArray("players")]
		[XmlArrayItem("RegisteredPlayer")]
		public ObservableCollection<RegisteredPlayer> players = new ObservableCollection<RegisteredPlayer>();
		public string PlayerNames
		{
			get
			{
				string ret = "";
				foreach (RegisteredPlayer rp in players)
				{
					if (ret.Length > 0)
					{
						ret += " - ";
					}

					ret += rp.FullName;
				}

				return ret;
			}
		}
		public float TeamRankingPoints
		{
			get
			{
				float ret = 0;
				foreach (RegisteredPlayer rp in players)
				{
					ret += rp.points;
				}

				return ret;
			}
		}
		public float TeamWomenRankingPoints
		{
			get
			{
				float ret = 0;
				foreach (RegisteredPlayer rp in players)
				{
					ret += rp.womenPoints;
				}

				return ret;
			}
		}
		public string TeamBothRankingsPointsString
		{
			get
			{
				return TeamRankingPoints + "/" + TeamWomenRankingPoints;
			}
		}
		[XmlIgnore]
		private EOverlayTeamDataState overlayDragState = EOverlayTeamDataState.Normal;
		public EOverlayTeamDataState OverlayDragState
		{
			get { return overlayDragState; }
			set
			{
				overlayDragState = value;
				OnPropertyChanged("TextColor");
				OnPropertyChanged("BorderColor");
			}
		}
		public System.Windows.Media.Brush TextColor
		{
			get
			{
				switch (OverlayDragState)
				{
					case EOverlayTeamDataState.DragFrom:
						return System.Windows.Media.Brushes.Beige;
					case EOverlayTeamDataState.DragTo:
						return System.Windows.Media.Brushes.Transparent;
					default:
						break;
				}

				return System.Windows.Media.Brushes.Black;
			}
		}
		public System.Windows.Media.Brush BorderColor
		{
			get
			{
				switch (OverlayDragState)
				{
					case EOverlayTeamDataState.DragFrom:
						return System.Windows.Media.Brushes.Beige;
					default:
						break;
				}

				return System.Windows.Media.Brushes.Gray;
			}
		}

		public TeamData()
		{
		}

		public TeamData(PotentialTeam potentialTeam)
		{
			players = potentialTeam.registeredPlayers;
		}

		bool Equals(TeamData other)
		{
			return PlayerNames == other.PlayerNames;
		}
	}

	public class JudgesData
	{
		public ObservableCollection<RegisteredPlayer> judgesEx = new ObservableCollection<RegisteredPlayer>();
		public ObservableCollection<RegisteredPlayer> judgesAi = new ObservableCollection<RegisteredPlayer>();
		public ObservableCollection<RegisteredPlayer> judgesDiff = new ObservableCollection<RegisteredPlayer>();

		public JudgesData()
		{
		}

		public bool JudgeExists(RegisteredPlayer judge, EJudgeCategory category)
		{
			switch (category)
			{
				case EJudgeCategory.Execution:
					return JudgeExists(judge, ref judgesEx);
				case EJudgeCategory.ArtisticImpression:
					return JudgeExists(judge, ref judgesAi);
				case EJudgeCategory.Difficulty:
					return JudgeExists(judge, ref judgesDiff);
			}

			return false;
		}

		public bool JudgeExists(RegisteredPlayer judge, ref ObservableCollection<RegisteredPlayer> judgeList)
		{
			foreach (RegisteredPlayer rp in judgeList)
			{
				if (rp.FullName == judge.FullName)
				{
					return true;
				}
			}

			return false;
		}

		public void Add(RegisteredPlayer judge, EJudgeCategory category)
		{
			if (JudgeExists(judge, category))
			{
				return;
			}

			switch (category)
			{
				case EJudgeCategory.Execution:
					judgesEx.Add(judge);
					break;
				case EJudgeCategory.ArtisticImpression:
					judgesAi.Add(judge);
					break;
				case EJudgeCategory.Difficulty:
					judgesDiff.Add(judge);
					break;
			}
		}

		public void Remove(RegisteredPlayer judge)
		{
			RemovePlayer(judgesEx, judge);
			RemovePlayer(judgesAi, judge);
			RemovePlayer(judgesDiff, judge);
		}

		private void RemovePlayer(ObservableCollection<RegisteredPlayer> judgeList, RegisteredPlayer rp)
		{
			for (int i = 0; i < judgeList.Count; ++i)
			{
				if (judgeList[i].FullName == rp.FullName)
				{
					judgeList.RemoveAt(i);

					--i;
				}
			}
		}

		public bool Contains(RegisteredPlayer judge)
		{
			return JudgeExists(judge, EJudgeCategory.Execution) ||
				JudgeExists(judge, EJudgeCategory.ArtisticImpression) ||
				JudgeExists(judge, EJudgeCategory.Difficulty);
		}

		public bool HasJudges()
		{
			return judgesEx.Count > 0 || judgesAi.Count > 0 || judgesDiff.Count > 0;
		}
	}

	public enum EJudgeCategory
	{
		Execution,
		ArtisticImpression,
		Difficulty
	}
}
