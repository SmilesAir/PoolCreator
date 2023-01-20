using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		HttpClient httpClientDev = new HttpClient();
		HttpClient httpClientProd = new HttpClient();

		string outputText = "";
		public string ExportOutputTextBox
		{
			get { return outputText; }
			set
			{
				outputText = value;
				OnPropertyChanged("ExportOutputTextBox");
			}
		}

		public bool ShouldExportWomensPrelims
		{
			get { return tournamentData.GetRound(EDivision.Women, ERound.Prelims).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Women, ERound.Prelims).shouldExport = value;
				OnPropertyChanged("ShouldExportWomensPrelims");
			}
		}
		public bool ShouldExportWomensQuarters
		{
			get { return tournamentData.GetRound(EDivision.Women, ERound.Quarterfinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Women, ERound.Quarterfinals).shouldExport = value;
				OnPropertyChanged("ShouldExportWomensQuarters");
			}
		}
		public bool ShouldExportWomensSemis
		{
			get { return tournamentData.GetRound(EDivision.Women, ERound.Semifinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Women, ERound.Semifinals).shouldExport = value;
				OnPropertyChanged("ShouldExportWomensSemis");
			}
		}
		public bool ShouldExportWomensFinals
		{
			get { return tournamentData.GetRound(EDivision.Women, ERound.Finals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Women, ERound.Finals).shouldExport = value;
				OnPropertyChanged("ShouldExportWomensFinals");
			}
		}
		public bool ShouldExportOpenPrelims
		{
			get { return tournamentData.GetRound(EDivision.Open, ERound.Prelims).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Open, ERound.Prelims).shouldExport = value;
				OnPropertyChanged("ShouldExportOpenPrelims");
			}
		}
		public bool ShouldExportOpenQuarters
		{
			get { return tournamentData.GetRound(EDivision.Open, ERound.Quarterfinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Open, ERound.Quarterfinals).shouldExport = value;
				OnPropertyChanged("ShouldExportOpenQuarters");
			}
		}
		public bool ShouldExportOpenSemis
		{
			get { return tournamentData.GetRound(EDivision.Open, ERound.Semifinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Open, ERound.Semifinals).shouldExport = value;
				OnPropertyChanged("ShouldExportOpenSemis");
			}
		}
		public bool ShouldExportOpenFinals
		{
			get { return tournamentData.GetRound(EDivision.Open, ERound.Finals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Open, ERound.Finals).shouldExport = value;
				OnPropertyChanged("ShouldExportOpenFinals");
			}
		}
		public bool ShouldExportMixedPrelims
		{
			get { return tournamentData.GetRound(EDivision.Mixed, ERound.Prelims).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Mixed, ERound.Prelims).shouldExport = value;
				OnPropertyChanged("ShouldExportMixedPrelims");
			}
		}
		public bool ShouldExportMixedQuarters
		{
			get { return tournamentData.GetRound(EDivision.Mixed, ERound.Quarterfinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Mixed, ERound.Quarterfinals).shouldExport = value;
				OnPropertyChanged("ShouldExportMixedQuarters");
			}
		}
		public bool ShouldExportMixedSemis
		{
			get { return tournamentData.GetRound(EDivision.Mixed, ERound.Semifinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Mixed, ERound.Semifinals).shouldExport = value;
				OnPropertyChanged("ShouldExportMixedSemis");
			}
		}
		public bool ShouldExportMixedFinals
		{
			get { return tournamentData.GetRound(EDivision.Mixed, ERound.Finals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Mixed, ERound.Finals).shouldExport = value;
				OnPropertyChanged("ShouldExportMixedFinals");
			}
		}
		public bool ShouldExportCoopPrelims
		{
			get { return tournamentData.GetRound(EDivision.Coop, ERound.Prelims).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Coop, ERound.Prelims).shouldExport = value;
				OnPropertyChanged("ShouldExportCoopPrelims");
			}
		}
		public bool ShouldExportCoopQuarters
		{
			get { return tournamentData.GetRound(EDivision.Coop, ERound.Quarterfinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Coop, ERound.Quarterfinals).shouldExport = value;
				OnPropertyChanged("ShouldExportCoopQuarters");
			}
		}
		public bool ShouldExportCoopSemis
		{
			get { return tournamentData.GetRound(EDivision.Coop, ERound.Semifinals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Coop, ERound.Semifinals).shouldExport = value;
				OnPropertyChanged("ShouldExportCoopSemis");
			}
		}
		public bool ShouldExportCoopFinals
		{
			get { return tournamentData.GetRound(EDivision.Coop, ERound.Finals).shouldExport; }
			set
			{
				tournamentData.GetRound(EDivision.Coop, ERound.Finals).shouldExport = value;
				OnPropertyChanged("ShouldExportCoopFinals");
			}
		}

		public delegate void AppendOutputLineCallback(string line);

		public void InitExport()
		{
			httpClientDev.DefaultRequestHeaders.Accept.Add(
			   new MediaTypeWithQualityHeaderValue("application/json"));
			httpClientProd.DefaultRequestHeaders.Accept.Add(
			   new MediaTypeWithQualityHeaderValue("application/json"));

			httpClientDev.BaseAddress = new Uri("https://0uzw9x3t5g.execute-api.us-west-2.amazonaws.com/");
			httpClientProd.BaseAddress = new Uri("https://w0wkbj0dd9.execute-api.us-west-2.amazonaws.com/");
		}

		public void ShutdownExport()
		{
		}

		private void InvokeAppendOutputLine(string line)
		{
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
				new Action(() => ExportOutputTextBox += line + Environment.NewLine));
		}
		
		private void ExportCompleteJudging_Click(object sender, RoutedEventArgs e)
		{
			tournamentData.SaveToDisk();

			ExportToJson();
		}

		private void UploadDevCompleteJudging_Click(object sender, RoutedEventArgs e)
		{
			tournamentData.SaveToDisk();

			string jsonStr = JsonConvert.SerializeObject(tournamentData);
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			ByteArrayContent byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			HttpResponseMessage response = httpClientDev.PostAsync("development/createTournament", byteContent).Result;

			jsonStr = "";

			ExportOutputTextBox = "";
			InvokeAppendOutputLine("Uploaded to development at " + DateTime.Now.ToString());

			OutputLinks(false);

			InvokeAppendOutputLine("Finished");
		}

		private void UploadProdCompleteJudging_Click(object sender, RoutedEventArgs e)
		{
			tournamentData.SaveToDisk();

			string jsonStr = JsonConvert.SerializeObject(tournamentData);
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			ByteArrayContent byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			HttpResponseMessage response = httpClientProd.PostAsync("production/createTournament", byteContent).Result;

			jsonStr = "";

			ExportOutputTextBox = "";
			InvokeAppendOutputLine("Uploaded to PRODUCTION at " + DateTime.Now.ToString());

			OutputLinks(true);

			InvokeAppendOutputLine("Finished");
		}

		private void UploadDevV3_Click(object sender, RoutedEventArgs e)
		{
			tournamentData.SaveToDisk();

			string jsonStr = JsonConvert.SerializeObject(tournamentData);
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			ByteArrayContent byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			ExportOutputTextBox = "";

			using (HttpClient httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Accept.Add(
				   new MediaTypeWithQualityHeaderValue("application/json"));

				httpClient.BaseAddress = new Uri("https://8er0vxrmr4.execute-api.us-west-2.amazonaws.com/development/");

				HttpResponseMessage response = httpClient.PostAsync($"importEventFromPoolCreator/{tournamentData.EventKey}", byteContent).Result;

				response.Content.ReadAsStringAsync().Wait();
				InvokeAppendOutputLine(response.Content.ReadAsStringAsync().Result);
			}

			jsonStr = "";

			
			//InvokeAppendOutputLine("Uploaded to V3 Dev at " + DateTime.Now.ToString());

			//OutputLinks(true);

			InvokeAppendOutputLine("Finished");
		}

		private void UploadProdV3_Click(object sender, RoutedEventArgs e)
		{
			tournamentData.SaveToDisk();

			string jsonStr = JsonConvert.SerializeObject(tournamentData);
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			ByteArrayContent byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			ExportOutputTextBox = "";

			using (HttpClient httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Accept.Add(
				   new MediaTypeWithQualityHeaderValue("application/json"));

				httpClient.BaseAddress = new Uri("https://xf4cu1wy10.execute-api.us-west-2.amazonaws.com/production/");

				HttpResponseMessage response = httpClient.PostAsync($"importEventFromPoolCreator/{tournamentData.EventKey}", byteContent).Result;

				response.Content.ReadAsStringAsync().Wait();
				InvokeAppendOutputLine(response.Content.ReadAsStringAsync().Result);
			}

			jsonStr = "";


			//InvokeAppendOutputLine("Uploaded to V3 Dev at " + DateTime.Now.ToString());

			//OutputLinks(true);

			InvokeAppendOutputLine("Finished");
		}

		private void OutputLinks(bool isProd)
		{
			string endpointStr = isProd ? "d5rsjgoyn07f8" : "d27wqtus28jqqk";
			InvokeAppendOutputLine("Tournament Info: https://" + endpointStr + ".cloudfront.net/index.html?startup=info&header=false&tournamentName=" + tournamentData.TournamentName);
			InvokeAppendOutputLine("Head Judge: https://" + endpointStr + ".cloudfront.net/index.html?startup=head&header=false&tournamentName=" + tournamentData.TournamentName);
			InvokeAppendOutputLine("Scoreboard: https://" + endpointStr + ".cloudfront.net/index.html?startup=scoreboard&alwaysUpdate=true&tournamentName=" + tournamentData.TournamentName);

			int maxExJudgeCount = 0;
			int maxAiJudgeCount = 0;
			int maxDiffJudgeCount = 0;
			foreach (DivisionData dd in tournamentData.divisions)
			{
				foreach (RoundData rd in dd.rounds)
				{
					foreach (PoolData pd in rd.pools)
					{
						maxExJudgeCount = Math.Max(pd.judgesData.judgesEx.Count, maxExJudgeCount);
						maxAiJudgeCount = Math.Max(pd.judgesData.judgesAi.Count, maxAiJudgeCount);
						maxDiffJudgeCount = Math.Max(pd.judgesData.judgesDiff.Count, maxDiffJudgeCount);
					}
				}
			}

			for (int i = 0; i < maxExJudgeCount; ++i)
			{
				InvokeAppendOutputLine("Artistic Expression Judge: https://" + endpointStr +
					".cloudfront.net/index.html?startup=exAiCombined&header=false&tournamentName="+ tournamentData.TournamentName + "&judgeIndex=" + i);
			}
			for (int i = 0; i < maxAiJudgeCount; ++i)
			{
				InvokeAppendOutputLine("Variety Judge: https://" + endpointStr +
					".cloudfront.net/index.html?startup=variety&header=false&tournamentName=" + tournamentData.TournamentName + "&judgeIndex=" + (i + maxExJudgeCount));
			}
			for (int i = 0; i < maxDiffJudgeCount; ++i)
			{
				InvokeAppendOutputLine("Difficulty Judge: https://" + endpointStr +
					".cloudfront.net/index.html?startup=diff&header=false&tournamentName=" + tournamentData.TournamentName + "&judgeIndex=" + (i + maxExJudgeCount + maxAiJudgeCount));
			}
		}

		private void ExportToJson()
		{
			string exportStr = JsonConvert.SerializeObject(tournamentData);

			InvokeAppendOutputLine(exportStr);
		}
	}
}

namespace LisaHelperClasses
{
	public class PlayerData
	{
		public int NameId;
		public float RankingPoints;
		public int Rank = -1;

		public PlayerData() { }

		public PlayerData(PoolCreator.RegisteredPlayer player)
		{
			NameId = PoolCreator.TournamentData.FindLisaHelperNameId(player);
			RankingPoints = player.points;
			Rank = player.rank;
		}
	}

	public class ExData
	{
		public int JudgeNameId = -1;

		public ExData() { }

		public ExData(PoolCreator.RegisteredPlayer judge)
		{
			JudgeNameId = PoolCreator.TournamentData.FindLisaHelperNameId(judge);
		}
	}

	public class AIData
	{
		public int JudgeNameId = -1;

		public AIData() { }

		public AIData(PoolCreator.RegisteredPlayer judge)
		{
			JudgeNameId = PoolCreator.TournamentData.FindLisaHelperNameId(judge);
		}
	}

	public class DiffData
	{
		public int JudgeNameId = -1;

		public DiffData() { }

		public DiffData(PoolCreator.RegisteredPlayer judge)
		{
			JudgeNameId = PoolCreator.TournamentData.FindLisaHelperNameId(judge);
		}
	}

	public class RoutineScoresData
	{
		public PoolCreator.EDivision Division;
		public PoolCreator.ERound Round;
		public List<ExData> ExResults = new List<ExData>();
		public List<AIData> AIResults = new List<AIData>();
		public List<DiffData> DiffResults = new List<DiffData>();

		public RoutineScoresData() { }

		public RoutineScoresData(PoolCreator.PoolKey poolKey, PoolCreator.PoolData poolData)
		{
			if (poolKey.round == PoolCreator.ERound.Max)
			{
				return;
			}

			Division = poolKey.division;
			Round = poolKey.round;

			foreach (PoolCreator.RegisteredPlayer judge in poolData.judgesData.judgesEx)
			{
				ExResults.Add(new ExData(judge));
			}

			foreach (PoolCreator.RegisteredPlayer judge in poolData.judgesData.judgesAi)
			{
				AIResults.Add(new AIData(judge));
			}

			foreach (PoolCreator.RegisteredPlayer judge in poolData.judgesData.judgesDiff)
			{
				DiffResults.Add(new DiffData(judge));
			}
		}
	}

	public class TeamData
	{
		public List<PlayerData> Players = new List<PlayerData>();
		public float TotalRankPoints = 0;
		public RoutineScoresData RoutineScores = new RoutineScoresData();

		public TeamData() { }

		public TeamData(PoolCreator.PoolKey poolKey, PoolCreator.PoolData poolData, PoolCreator.TeamData teamData)
		{
			foreach (PoolCreator.RegisteredPlayer player in teamData.players)
			{
				Players.Add(new PlayerData(player));
			}

			TotalRankPoints = teamData.TeamRankingPoints;

			if (poolKey.division == PoolCreator.EDivision.Open && poolKey.round == PoolCreator.ERound.Finals)
			{
				RoutineScores = new RoutineScoresData(poolKey, poolData);
			}
		}
	}

	public class TeamDataDisplay
	{
		public TeamData Data = new TeamData();

		public TeamDataDisplay() { }

		public TeamDataDisplay(PoolCreator.PoolKey poolKey, PoolCreator.PoolData poolData, PoolCreator.TeamData teamData)
		{
			Data = new TeamData(poolKey, poolData, teamData);
		}
	}

	public class PoolData
	{
		public string PoolName = "A";
		public List<TeamDataDisplay> Teams = new List<TeamDataDisplay>();
		public int JudgersId = -1;

		public PoolData() { }

		public PoolData(List<JudgeData> judgeList, PoolCreator.PoolKey poolKey, PoolCreator.PoolData poolData)
		{
			PoolName = poolData.pool.ToString();

			poolKey.pool = poolData.pool;

			foreach (PoolCreator.TeamData td in poolData.teamList.teams)
			{
				Teams.Add(new TeamDataDisplay(poolKey, poolData, td));
			}

			if (poolKey.round != PoolCreator.ERound.Max && poolData.judgesData.HasJudges())
			{
				JudgersId = judgeList.Count;

				judgeList.Add(new JudgeData(JudgersId, poolKey, poolData.judgesData));
			}
		}
	}

	public class RoundData
	{
		public List<PoolData> Pools = new List<PoolData>();
		public float RoutineLengthMinutes = 4f;

		public RoundData() { }

		public RoundData(List<JudgeData> judgeList, PoolCreator.PoolKey poolKey, PoolCreator.RoundData roundData)
		{
			RoutineLengthMinutes = roundData.routineLength;

			foreach (PoolCreator.PoolData pd in roundData.pools)
			{
				Pools.Add(new PoolData(judgeList, poolKey, pd));
			}
		}
	}

	public class DivisionData
	{
		public List<RoundData> Rounds = new List<RoundData>();

		public DivisionData() { }

		public DivisionData(List<JudgeData> judgeList, PoolCreator.DivisionData divisionData)
		{
			foreach (PoolCreator.RoundData rd in divisionData.rounds)
			{
				Rounds.Add(new RoundData(judgeList, new PoolCreator.PoolKey(divisionData.division, rd.round, PoolCreator.EPool.None), rd));
			}
		}
	}

	public class JudgeData
	{
		public int Id = -1;
		public PoolCreator.EDivision Division;
		public string Round;
		public PoolCreator.EPool Pool = PoolCreator.EPool.None;
		public List<int> AIJudgeIds = new List<int>();
		public List<int> ExJudgeIds = new List<int>();
		public List<int> DiffJudgeIds = new List<int>();

		public JudgeData() { }

		public JudgeData(int id, PoolCreator.PoolKey poolKey, PoolCreator.JudgesData judgesData)
		{
			Id = id;
			Division = poolKey.division;
			switch (poolKey.round)
			{
				case PoolCreator.ERound.Finals:
					Round = "Finals";
					break;
				case PoolCreator.ERound.Semifinals:
					Round = "Semifinals";
					break;
				case PoolCreator.ERound.Quarterfinals:
					Round = "Quaterfinals";
					break;
				case PoolCreator.ERound.Prelims:
					Round = "Prelims";
					break;
			}
			Pool = poolKey.pool;

			foreach (PoolCreator.RegisteredPlayer judge in judgesData.judgesAi)
			{
				AIJudgeIds.Add(PoolCreator.TournamentData.FindLisaHelperNameId(judge));
			}

			foreach (PoolCreator.RegisteredPlayer judge in judgesData.judgesEx)
			{
				ExJudgeIds.Add(PoolCreator.TournamentData.FindLisaHelperNameId(judge));
			}

			foreach (PoolCreator.RegisteredPlayer judge in judgesData.judgesDiff)
			{
				DiffJudgeIds.Add(PoolCreator.TournamentData.FindLisaHelperNameId(judge));
			}
		}
	}

	public class TournamentRootData
	{
		public List<DivisionData> AllDivisions = new List<DivisionData>();
		public List<JudgeData> JudgeList = new List<JudgeData>();

		public TournamentRootData() { }

		public TournamentRootData(PoolCreator.TournamentData tournamentData)
		{
			foreach (PoolCreator.DivisionData dd in tournamentData.divisions)
			{
				AllDivisions.Add(new DivisionData(JudgeList, dd));
			}
		}
	}

	public class NameData
	{
		public int Id = -1;
		public string FirstName;
		public string LastName;
		public List<string> FirstAliases = new List<string>();
		public List<string> LastAliases = new List<string>();

		public NameData()
		{
			FirstName = "None";
			LastName = "None";

			Id = NameDatabase.GetNextNameId();
		}

		public NameData(string InFirstName, string InLastName)
		{
			FirstName = InFirstName;
			if (FirstName.Length > 0)
			{
				StringBuilder FormattedFirstName = new StringBuilder();
				FormattedFirstName.Append(FirstName.Substring(0, 1).ToUpper());
				FormattedFirstName.Append(FirstName.Substring(1).ToLower());
				FirstName = FormattedFirstName.ToString();
			}
			LastName = InLastName;
			if (LastName.Length > 0)
			{
				StringBuilder FormattedLastName = new StringBuilder();
				FormattedLastName.Append(LastName.Substring(0, 1).ToUpper());
				FormattedLastName.Append(LastName.Substring(1).ToLower());
				LastName = FormattedLastName.ToString();
			}

			FirstAliases.Add(InFirstName);
			LastAliases.Add(InLastName);

			Id = NameDatabase.GetNextNameId();
		}
	}

	public class NameDatabase
	{
		public List<NameData> AllNames = new List<NameData>();

		public List<NameData> AllJudges = new List<NameData>();

		private static int NextNameId = 0;

		public NameDatabase()
		{
			NextNameId = 0;
		}

		public NameDatabase(PoolCreator.TournamentData tournamentData)
		{
			NextNameId = 0;

			foreach (PoolCreator.RegisteredPlayer rp in tournamentData.registeredPlayers)
			{
				AllNames.Add(new NameData(rp.firstName, rp.lastName));
			}
		}

		public static int GetNextNameId()
		{
			return NextNameId++;
		}
	}
}
