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
using Excel = Microsoft.Office.Interop.Excel;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		Excel.Application excelApp = new Excel.Application();

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
		public string ExportPath
		{
			get { return tournamentData.exportPath; }
			set
			{
				tournamentData.exportPath = value;
				OnPropertyChanged("ExportPath");
			}
		}
		public string ExcelTemplatePath
		{
			get { return tournamentData.excelTemplatePath; }
			set
			{
				tournamentData.excelTemplatePath = value;
				OnPropertyChanged("ExcelTemplatePath");
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
			ExportPathText.DataContext = this;
			ExcelTemplatePathText.DataContext = this;

			httpClientDev.DefaultRequestHeaders.Accept.Add(
			   new MediaTypeWithQualityHeaderValue("application/json"));
			httpClientProd.DefaultRequestHeaders.Accept.Add(
			   new MediaTypeWithQualityHeaderValue("application/json"));

			httpClientDev.BaseAddress = new Uri("https://0uzw9x3t5g.execute-api.us-west-2.amazonaws.com/");
			httpClientProd.BaseAddress = new Uri("https://w0wkbj0dd9.execute-api.us-west-2.amazonaws.com/");
		}

		public void ShutdownExport()
		{
			excelApp.Quit();
		}

		private void ExportExcel_Click(object sender, RoutedEventArgs e)
		{
			ExportOutputTextBox = "";

			System.Threading.Thread backgroundThread = new System.Threading.Thread(new System.Threading.ThreadStart(ExportExcelAll));
			backgroundThread.Start();
		}

		private void InvokeAppendOutputLine(string line)
		{
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
				new Action(() => ExportOutputTextBox += line + Environment.NewLine));
		}

		private void ExportAllPools(Action<PoolKey> exportAction)
		{
			for (int divisionIndex = 0; divisionIndex < 4; ++divisionIndex)
			{
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Prelims, EPool.A));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Prelims, EPool.B));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Prelims, EPool.C));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Prelims, EPool.D));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Quarterfinals, EPool.A));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Quarterfinals, EPool.B));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Quarterfinals, EPool.C));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Quarterfinals, EPool.D));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Semifinals, EPool.A));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Semifinals, EPool.B));
				exportAction(new PoolKey(EDivision.Open + divisionIndex, ERound.Finals, EPool.A));
			}
		}

		private void ExportExcelAll()
		{
			InvokeAppendOutputLine("Starting Export");

			ExportAllPools(ExportExcel);

			InvokeAppendOutputLine("Export Finished");
		}

		private void ExportExcel(PoolKey poolKey)
		{
			if (!tournamentData.GetRound(poolKey.division, poolKey.round).shouldExport)
			{
				return;
			}

			try
			{
				Excel.Workbook workbook = excelApp.Workbooks.Open(Path.GetFullPath(ExcelTemplatePath), 0, false);
				Excel.Sheets worksheets = workbook.Worksheets;

				WriteTournamentDetails(poolKey, worksheets);
				WritePoolDetails(poolKey, worksheets);
				WriteTeams(poolKey, worksheets);
				WriteJudges(poolKey, worksheets);

				string newFilename = Path.GetFullPath(ExportPath + @"\" + poolKey.division.ToString() +
					"-" + GetScoresheetRoundName(poolKey.round) +
					(poolKey.round != ERound.Finals ? "-" + poolKey.pool.ToString() : "") +
					".xlsm");
				File.Delete(newFilename);
				workbook.SaveAs(newFilename);

				workbook.Close();

				// Process.Start(newFilename);

				InvokeAppendOutputLine("Exported: " + newFilename);
			}
			catch
			{
				InvokeAppendOutputLine("Error exporting: " + poolKey.division.ToString() + " " + poolKey.round.ToString() +
					" " + poolKey.pool.ToString());
			}
		}

		private void WriteTeams(PoolKey poolKey, Excel.Sheets worksheets)
		{
			Excel.Worksheet starterSheet = worksheets.get_Item("StarterList");
			PoolData pd = tournamentData.GetPool(poolKey);
			if (pd != null && starterSheet != null)
			{
				for (int teamIndex = 0; teamIndex < 10 && teamIndex < pd.teamList.teams.Count; ++teamIndex)
				{
					TeamData td = pd.teamList.teams[teamIndex];
					for (int playerIndex = 0; playerIndex < 3 && playerIndex < td.players.Count; ++playerIndex)
					{
						starterSheet.Cells[13 + teamIndex, 2 + 2 * playerIndex] = td.players[playerIndex].FullName;
					}
				}
			}
		}

		private void WriteJudges(PoolKey poolKey, Excel.Sheets worksheets)
		{
			Excel.Worksheet starterSheet = worksheets.get_Item("StarterList");
			PoolData pd = tournamentData.GetPool(poolKey);
			if (pd != null && starterSheet != null)
			{
				for (int exIndex = 0; exIndex < 3 && exIndex < pd.judgesData.judgesEx.Count; ++exIndex)
				{
					starterSheet.Cells[13 + exIndex, 10] = pd.judgesData.judgesEx[exIndex].FullName;
				}

				for (int aiIndex = 0; aiIndex < 3 && aiIndex < pd.judgesData.judgesAi.Count; ++aiIndex)
				{
					starterSheet.Cells[16 + aiIndex, 10] = pd.judgesData.judgesAi[aiIndex].FullName;
				}

				for (int diffIndex = 0; diffIndex < 3 && diffIndex < pd.judgesData.judgesDiff.Count; ++diffIndex)
				{
					starterSheet.Cells[19 + diffIndex, 10] = pd.judgesData.judgesDiff[diffIndex].FullName;
				}
			}
		}

		private string GetScoresheetDivisionName(EDivision division)
		{
			switch (division)
			{
				case EDivision.Open:
					return "Open Pairs";
				case EDivision.Mixed:
					return "Mixed Pairs";
				case EDivision.Coop:
					return "Open Coop";
				case EDivision.Women:
					return "Women Pairs";
			}

			return "No Division Name";
		}

		private string GetScoresheetRoundName(ERound round)
		{
			if (round == ERound.Finals)
			{
				return "Final";
			}

			return round.ToString();
		}

		private void WritePoolDetails(PoolKey poolKey, Excel.Sheets worksheets)
		{
			Excel.Worksheet starterSheet = worksheets.get_Item("StarterList");
			DivisionData dd = tournamentData.GetDivision(poolKey.division);
			RoundData rd = tournamentData.GetRound(poolKey.division, poolKey.round);
			if (dd != null && rd != null && starterSheet != null)
			{
				starterSheet.Cells[4, 2] = GetScoresheetDivisionName(poolKey.division);
				starterSheet.Cells[4, 4] = GetScoresheetRoundName(poolKey.round);
				starterSheet.Cells[4, 6] = poolKey.pool.ToString();

				starterSheet.Cells[3, 10] = rd.scheduleTime.ToShortDateString();
				starterSheet.Cells[4, 10] = rd.scheduleTime.ToShortTimeString();
				starterSheet.Cells[5, 10] = rd.routineLength;
				starterSheet.Cells[6, 10] = dd.headJudge;
				starterSheet.Cells[7, 10] = dd.directors;
				starterSheet.Range["J9:J10"].Value = dd.committee;
			}
		}

		private void WriteTournamentDetails(PoolKey poolKey, Excel.Sheets worksheets)
		{
			Excel.Worksheet settingsSheet = worksheets.get_Item("Settings");
			settingsSheet.Cells[3, 2] = tournamentData.TournamentName;
			settingsSheet.Cells[4, 2] = tournamentData.TournamentSubtitle;
		}

		private void BrowseExportPath_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ExportPath = dialog.SelectedPath;
			}
		}

		private void BrowseExcelTemplate_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
			if (dialog.ShowDialog() == true)
			{
				ExcelTemplatePath = dialog.FileName;
			}
		}

		private LisaHelperClasses.NameDatabase FillImportedNamesForExport()
		{
			LisaHelperClasses.NameDatabase newDatabase = new LisaHelperClasses.NameDatabase(tournamentData);

			TournamentData.importedNames.Clear();

			foreach (LisaHelperClasses.NameData nd in newDatabase.AllNames)
			{
				TournamentData.importedNames.Add(new ImportedName(nd.Id, nd.FirstName, nd.LastName));
			}

			return newDatabase;
		}

		private void ExportToLisaHelper_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				XmlSerializer namesSerializer = new XmlSerializer(typeof(LisaHelperClasses.NameDatabase));
				using (StringWriter newString = new StringWriter())
				{
					LisaHelperClasses.NameDatabase nameDatabase = FillImportedNamesForExport();

					namesSerializer.Serialize(newString, nameDatabase);

					string outXml = newString.ToString();

					using (StreamWriter saveFile = new StreamWriter(ExportPath + @"\names.xml",
						false, System.Text.Encoding.Unicode))
					{
						saveFile.Write(outXml);
					}
				}

				XmlSerializer saveSerializer = new XmlSerializer(typeof(LisaHelperClasses.TournamentRootData));
				using (StringWriter newString = new StringWriter())
				{
					LisaHelperClasses.TournamentRootData testData = new LisaHelperClasses.TournamentRootData(tournamentData);

					saveSerializer.Serialize(newString, testData);

					string outXml = newString.ToString();

					using (StreamWriter saveFile = new StreamWriter(ExportPath + @"\save.xml",
						false, System.Text.Encoding.Unicode))
					{
						saveFile.Write(outXml);
					}
				}

				InvokeAppendOutputLine("Finished Exporting to Lisa Helper");
			}
			catch (Exception exception)
			{
				InvokeAppendOutputLine("Error Exporting to Lisa Helper: " + exception.Message);
			}
		}

		private void ExportToPotlatch_Click(object sender, RoutedEventArgs e)
		{
			string saveFilename = ExportPath + @"\PotlatchJudgerNames.txt";
			try
			{
				using (StreamWriter saveFile = new StreamWriter(saveFilename))
				{
					ExportPotlatchPool(saveFile, new PoolKey(EDivision.Coop, ERound.Finals, EPool.A));
				}
			}
			catch (Exception exception)
			{
				InvokeAppendOutputLine("Error Exporting to Potlatch Judger: " + exception.Message);
			}
		}

		private void ExportPotlatchPool(StreamWriter stream, PoolKey poolKey)
		{
			stream.WriteLine("Division: " + poolKey.division + " Round: " + poolKey.round + " Pool: " + poolKey.pool);

			PoolData pd = tournamentData.GetPool(poolKey);
			for (int teamIndex = 0; teamIndex < 10 && teamIndex < pd.teamList.teams.Count; ++teamIndex)
			{
				TeamData td = pd.teamList.teams[teamIndex];
				stream.WriteLine(td.PlayerNames);
			}

			stream.WriteLine();
		}

		private void ExportDialJudgerImportText(PoolKey poolKey)
		{
			if (!tournamentData.GetRound(poolKey.division, poolKey.round).shouldExport)
			{
				return;
			}

			PoolData pd = tournamentData.GetPool(poolKey);
			if (pd.teamList.teams.Count == 0 && !pd.judgesData.HasJudges())
			{
				return;
			}

			string outputText = "";
			string exportName = poolKey.division.ToString() + "-" +
				poolKey.round.ToString() + "-" +
				poolKey.pool.ToString();

			outputText += Environment.NewLine;
			outputText += "///////////////////////////////////////" + Environment.NewLine;
			outputText += "// " + exportName + Environment.NewLine;

			foreach (TeamData td in pd.teamList.teams)
			{
				outputText += td.PlayerNamesCommaSeparated + Environment.NewLine;
			}

			outputText += Environment.NewLine;
			outputText += "// Judges" + Environment.NewLine;

			foreach (RegisteredPlayer rp in pd.judgesData.judgesAi)
			{
				outputText += rp.FullName + ", ArtisticImpression" + Environment.NewLine;
			}
			foreach (RegisteredPlayer rp in pd.judgesData.judgesDiff)
			{
				outputText += rp.FullName + ", Difficulty" + Environment.NewLine;
			}
			foreach (RegisteredPlayer rp in pd.judgesData.judgesEx)
			{
				outputText += rp.FullName + ", Execution" + Environment.NewLine;
			}

			outputText += "///////////////////////////////////////" + Environment.NewLine;
			outputText += Environment.NewLine;

			ExportOutputTextBox += outputText;

			try
			{
				using (StreamWriter saveFile = new StreamWriter(ExportPath + @"\" + exportName + ".txt",
						false, System.Text.Encoding.Unicode))
				{
					saveFile.Write(outputText);
				}
			}
			catch
			{
			}
		}

		private void ExportDialJudger_Click(object sender, RoutedEventArgs e)
		{
			ExportOutputTextBox = "------------ Starting Generation ------------" + Environment.NewLine;

			ExportAllPools(ExportDialJudgerImportText);

			ExportOutputTextBox += "------------ Finished Generation ------------";
		}

		private void ExportCompleteJudging_Click(object sender, RoutedEventArgs e)
		{
			ExportToJson();
		}

		private void UploadDevCompleteJudging_Click(object sender, RoutedEventArgs e)
		{
			string jsonStr = JsonConvert.SerializeObject(tournamentData);
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			ByteArrayContent byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			HttpResponseMessage response = httpClientDev.PostAsync("development/createTournament", byteContent).Result;

			jsonStr = "";

			ExportOutputTextBox = "";
			InvokeAppendOutputLine("Uploaded to development at " + DateTime.Now.ToString());

			OutputLinks(false);
		}

		private void UploadProdCompleteJudging_Click(object sender, RoutedEventArgs e)
		{
			string jsonStr = JsonConvert.SerializeObject(tournamentData);
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			ByteArrayContent byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			HttpResponseMessage response = httpClientProd.PostAsync("production/createTournament", byteContent).Result;

			jsonStr = "";

			ExportOutputTextBox = "";
			InvokeAppendOutputLine("Uploaded to PRODUCTION at " + DateTime.Now.ToString());

			OutputLinks(true);
		}

		private void OutputLinks(bool isProd)
		{
			string endpointStr = isProd ? "d5rsjgoyn07f8" : "d27wqtus28jqqk";
			InvokeAppendOutputLine("Tournament Info: https://" + endpointStr + ".cloudfront.net/index.html?startup=info&header=false&tournamentName=" + tournamentData.TournamentName);
			InvokeAppendOutputLine("Head Judge: https://" + endpointStr + ".cloudfront.net/index.html?startup=head&header=false&tournamentName=" + tournamentData.TournamentName);

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
