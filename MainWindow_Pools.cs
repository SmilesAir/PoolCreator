using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		ObservableCollection<TeamData> poolsAllTeamsForDivision = new ObservableCollection<TeamData>();
		public Visibility DrawTeamVisible
		{
			get { return overlayTeamData == null ? Visibility.Hidden : Visibility.Visible; }
		}
		TeamData overlayTeamData = null;
		public TeamData OverlayTeamData
		{
			get { return overlayTeamData; }
			set
			{
				overlayTeamData = value;
				OnPropertyChanged("OverlayTeamData");
				OnPropertyChanged("DrawTeamVisible");
			}
		}
		EDivision poolsDivision = EDivision.None;
		public EDivisionDisplay PoolsSelectedDivision
		{
			get { return EnumConverter.ConvertDivisionValue(poolsDivision); }
			set
			{
				poolsDivision = EnumConverter.ConvertDivisionValue(value);
				OnPropertyChanged("poolsSelectedDivision");

				PoolsUpdateBindings();
			}
		}
		ERound poolsRound = ERound.None;
		public ERoundDisplay PoolsSelectedRound
		{
			get { return EnumConverter.ConvertRoundValue(poolsRound); }
			set
			{
				poolsRound = EnumConverter.ConvertRoundValue(value);
				OnPropertyChanged("poolsSelectedRound");

				PoolsUpdateBindings();
			}
		}
		public int FinalTeamCount
		{
			get
			{
				if (poolsDivision == EDivision.None)
				{
					return 0;
				}

				RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Finals);
				
				return rd.maxTeams;
			}
			set
			{
				if (poolsDivision != EDivision.None)
				{
					RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Finals);
					rd.maxTeams = value;

					OnPropertyChanged("FinalTeamCount");
				}
			}
		}
		public int SemiTeamCount
		{
			get
			{
				if (poolsDivision == EDivision.None)
				{
					return 0;
				}

				RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Semifinals);

				return rd.maxTeams;
			}
			set
			{
				if (poolsDivision != EDivision.None)
				{
					RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Semifinals);
					rd.maxTeams = value;

					OnPropertyChanged("SemiTeamCount");
				}
			}
		}
		public int QuarterTeamCount
		{
			get
			{
				if (poolsDivision == EDivision.None)
				{
					return 0;
				}

				RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Quarterfinals);

				return rd.maxTeams;
			}
			set
			{
				if (poolsDivision != EDivision.None)
				{
					RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Quarterfinals);
					rd.maxTeams = value;

					OnPropertyChanged("QuarterTeamCount");
				}
			}
		}

		public Point OverlayTeamDataOffset = new Point();

		private void InitPools()
		{
			PoolsSettingsGrid.DataContext = this;
			OverlayCanvas.DataContext = this;

			FinalsPoolItemsControl.Init(this, ERound.Finals, EPool.A);
			SemiAPoolItemsControl.Init(this, ERound.Semifinals, EPool.A);
			SemiBPoolItemsControl.Init(this, ERound.Semifinals, EPool.B);
			QuarterAPoolItemsControl.Init(this, ERound.Quarterfinals, EPool.A);
			QuarterBPoolItemsControl.Init(this, ERound.Quarterfinals, EPool.B);
			QuarterCPoolItemsControl.Init(this, ERound.Quarterfinals, EPool.C);
			QuarterDPoolItemsControl.Init(this, ERound.Quarterfinals, EPool.D);
			PrelimAPoolItemsControl.Init(this, ERound.Prelims, EPool.A);
			PrelimBPoolItemsControl.Init(this, ERound.Prelims, EPool.B);
			PrelimCPoolItemsControl.Init(this, ERound.Prelims, EPool.C);
			PrelimDPoolItemsControl.Init(this, ERound.Prelims, EPool.D);

			SetPoolItemsSources();

			// Try to generate any future pools
			GenerateFuturePools();

			PoolsScrollViewer.ScrollToRightEnd();
		}

		public void GenerateFuturePools()
		{
			for (EDivision division = EDivision.Open; division < EDivision.Max; ++division)
			{
				for (ERound round = ERound.Finals; round != ERound.Max; ++round)
				{
					for (EPool pool = EPool.A; pool != EPool.Max; ++pool)
					{
						GenerateNextPool(division, round, pool);
					}
				}
			}
		}

		private void SetPoolItemsSources()
		{
			FinalsPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Finals, EPool.A));
			SemiAPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Semifinals, EPool.A));
			SemiBPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Semifinals, EPool.B));
			QuarterAPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Quarterfinals, EPool.A));
			QuarterBPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Quarterfinals, EPool.B));
			QuarterCPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Quarterfinals, EPool.C));
			QuarterDPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Quarterfinals, EPool.D));

			PrelimAPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.A));
			PrelimBPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.B));
			PrelimCPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.C));
			PrelimDPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.D));
		}

		public void OverlayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			UpdateDraggingTeam(e.GetPosition(OverlayCanvas));
		}

		public void OverlayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			OverlayTeamData = null;
		}

		public void OverlayCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			UpdateDraggingTeam(e.GetPosition(OverlayCanvas));
		}

		private void UpdateDraggingTeam(Point mousePos)
		{
			if (OverlayTeamData != null)
			{
				Vector adjustMousePos = mousePos - OverlayTeamDataOffset;
				Canvas.SetLeft(OverlayTeam, adjustMousePos.X);
				Canvas.SetTop(OverlayTeam, adjustMousePos.Y);
			}
		}

		private void PoolsUpdateBindings()
		{
			PoolsUpdateTeamList();
			PoolsTeamsItemsControl.ItemsSource = poolsAllTeamsForDivision;

			OnPropertyChanged("FinalTeamCount");
			OnPropertyChanged("SemiTeamCount");
			OnPropertyChanged("QuarterTeamCount");

			SetPoolItemsSources();
		}

		private void PoolsUpdateTeamList()
		{
			poolsAllTeamsForDivision = tournamentData.GetAllTeams(poolsDivision);

			poolsAllTeamsForDivision = new ObservableCollection<TeamData>(poolsAllTeamsForDivision.OrderByDescending(td => td.TeamRankingPoints));
		}

		private void PoolsTab_GotFocus(object sender, RoutedEventArgs e)
		{
			PoolsUpdateTeamList();
		}

		private void SeedPrelimPools(EDivision division, int numPools)
		{
			RoundData rd = tournamentData.GetRound(division, ERound.Prelims);
			rd.pools.Clear();
			for (int i = 0; i < numPools; ++i)
			{
				rd.pools.Add(new PoolData((EPool)i));
			}

			List<TeamData> teams = new List<TeamData>();
			for (int i = 24; i < poolsAllTeamsForDivision.Count(); ++i)
			{
				teams.Add(poolsAllTeamsForDivision[i]);
			}
			
			PrelimAPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.A));

			if (numPools > 1)
			{
				PrelimBPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.B));
			}
			if (numPools > 2)
			{
				PrelimCPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.C));
			}
			if (numPools > 3)
			{
				PrelimDPoolItemsControl.SetItemsSource(tournamentData.GetPool(poolsDivision, ERound.Prelims, EPool.D));
			}


			FillPoolTeams(poolsDivision, poolsRound, teams);
		}

		private void PoolsSeedRound_Click(object sender, RoutedEventArgs e)
		{
			if (poolsDivision != EDivision.None && poolsRound != ERound.None)
			{
				int teamCount = poolsAllTeamsForDivision.Count();
				if (poolsRound == ERound.Prelims && teamCount >= 33)
				{
					if (teamCount == 33)
					{
						SeedPrelimPools(poolsDivision, 1);
					}
					else if (teamCount <= 40)
					{
						SeedPrelimPools(poolsDivision, 2);
					}
					else if (teamCount <= 48)
					{
						SeedPrelimPools(poolsDivision, 4);
					}
				}
				else
				{
					FillPoolTeams(poolsDivision, poolsRound, poolsAllTeamsForDivision.ToList());
				}
			}
		}

		private void FillPoolTeams(EDivision division, ERound round, List<TeamData> teams)
		{
			FillPoolTeams(division, round, teams, true);
		}

		private void FillPoolTeams(EDivision division, ERound round, List<TeamData> teams, bool bClearPreviousTeams)
		{
			int rawPoolIndex = 0;
			int adjustedPoolIndex = 0;
			bool bReversePoolAssignmentDirection = false;
			RoundData rd = tournamentData.GetRound(division, round);
			int maxPoolCount = rd.pools.Count;

			if (bClearPreviousTeams)
			{
				for (int poolIndex = 0; poolIndex < maxPoolCount; ++poolIndex)
				{
					PoolData pd = tournamentData.GetPool(division, round, (EPool)poolIndex);
					pd.teamList.teams.Clear();
				}
			}

			for (int teamIndex = 0; teamIndex < teams.Count; ++teamIndex)
			{
				if (rawPoolIndex >= maxPoolCount)
				{
					bReversePoolAssignmentDirection = !bReversePoolAssignmentDirection;
					rawPoolIndex = 0;

					if (bReversePoolAssignmentDirection)
					{
						adjustedPoolIndex = maxPoolCount - 1;
					}
					else
					{
						adjustedPoolIndex = 0;
					}
				}

				PoolData pd = tournamentData.GetPool(division, round, (EPool)adjustedPoolIndex);
				pd.teamList.teams.Add(teams[teamIndex]);

				if (bReversePoolAssignmentDirection)
				{
					--adjustedPoolIndex;
				}
				else
				{
					++adjustedPoolIndex;
				}
				++rawPoolIndex;
			}

			// Reverse the teams in the pool so first play is at the top
			foreach (PoolData pd in rd.pools)
			{
				pd.teamList.teams = new ObservableCollection<TeamData>(pd.teamList.teams.Reverse());
			}

			PoolsUpdateBindings();
		}

		public void RemoveTeamDataFromPools(TeamData removeTeam)
		{
			DivisionData dd = tournamentData.GetDivision(poolsDivision);
			foreach (RoundData rd in dd.rounds)
			{
				foreach (PoolData pd in rd.pools)
				{
					pd.teamList.teams.Remove(removeTeam);
				}
			}
		}

		private bool TryGetAutoGeneratePools(EDivision division, ERound round, EPool pool, out PoolData pool1, out PoolData pool2, out PoolData nextPool)
		{
			if (round == ERound.Quarterfinals)
			{
				if (pool == EPool.A || pool == EPool.C)
				{
					pool1 = tournamentData.GetPool(division, ERound.Quarterfinals, EPool.A);
					pool2 = tournamentData.GetPool(division, ERound.Quarterfinals, EPool.C);
					nextPool = tournamentData.GetPool(division, ERound.Semifinals, EPool.A);
				}
				else
				{
					pool1 = tournamentData.GetPool(division, ERound.Quarterfinals, EPool.B);
					pool2 = tournamentData.GetPool(division, ERound.Quarterfinals, EPool.D);
					nextPool = tournamentData.GetPool(division, ERound.Semifinals, EPool.B);
				}
			}
			else if (round == ERound.Semifinals)
			{
				pool1 = tournamentData.GetPool(division, ERound.Semifinals, EPool.A);
				pool2 = tournamentData.GetPool(division, ERound.Semifinals, EPool.B);
				nextPool = tournamentData.GetPool(division, ERound.Finals, EPool.A);
			}
			else
			{
				pool1 = null;
				pool2 = null;
				nextPool = null;

				return false;
			}

			if (pool1.resultRank.Count > 0 && pool2.resultRank.Count > 0)
			{
				foreach (int rank in pool1.resultRank)
				{
					if (rank == 0)
					{
						return false;
					}
				}

				foreach (int rank in pool2.resultRank)
				{
					if (rank == 0)
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}

		private bool TryGetAutoGeneratePools(ERound round, EPool pool, out PoolData pool1, out PoolData pool2, out PoolData nextPool)
		{
			return TryGetAutoGeneratePools(poolsDivision, round, pool, out pool1, out pool2, out nextPool);
		}

		public void OnPoolRankingsChanged(ERound round, EPool pool)
		{
			GenerateNextPool(poolsDivision, round, pool);
		}

		private int GetMaxTeamsForPool(EDivision division, ERound round)
		{
			return tournamentData.GetRound(division, round).maxTeams;
		}

		public void GenerateNextPool(EDivision division, ERound round, EPool pool)
		{
			PoolData pool1;
			PoolData pool2;
			PoolData nextPool;

			if (!TryGetAutoGeneratePools(division, round, pool, out pool1, out pool2, out nextPool))
			{
				return;
			}

			var nextPoolTeams = nextPool.teamList.teams;
			nextPoolTeams.Clear();
			if (nextPoolTeams.Count == 0)
			{
				var pool1Teams = pool1.teamList.teams;
				var pool2Teams = pool2.teamList.teams;
				int maxTeamsInPool = Math.Max(pool1Teams.Count, pool2Teams.Count);
				for (int rank = 1; rank <= maxTeamsInPool; ++rank)
				{
					int teamIndex1 = GetRankTeamIndex(pool1.resultRank, rank);
					int teamIndex2 = GetRankTeamIndex(pool2.resultRank, rank);

					if (teamIndex1 < 0)
					{
						nextPoolTeams.Insert(0, pool2Teams[teamIndex2]);
						continue;
					}
					else if (teamIndex2 < 0)
					{
						nextPoolTeams.Insert(0, pool1Teams[teamIndex1]);
						continue;
					}

					if (pool1Teams[teamIndex1].TeamRankingPoints < pool2Teams[teamIndex2].TeamRankingPoints)
					{
						nextPoolTeams.Insert(0, pool2Teams[teamIndex2]);
						nextPoolTeams.Insert(0, pool1Teams[teamIndex1]);
					}
					else
					{
						nextPoolTeams.Insert(0, pool1Teams[teamIndex1]);
						nextPoolTeams.Insert(0, pool2Teams[teamIndex2]);
					}

					if (nextPoolTeams.Count >= GetMaxTeamsForPool(division, round))
					{
						break;
					}
				}
			}
			else
			{
				// Warning about not being able to generate next teams pools because there is already data
			}
		}

		private int GetRankTeamIndex(ObservableCollection<int> resultsRank, int rank)
		{
			for (int i = 0; i < resultsRank.Count; ++i)
			{
				if (resultsRank[i] == rank)
				{
					return i;
				}
			}

			return -1;
		}

		private void PoolsFinalSeed_Click(object sender, RoutedEventArgs e)
		{
			GenerateNextPool(poolsDivision, ERound.Semifinals, EPool.A);
		}

		private void PoolsSemiSeed_Click(object sender, RoutedEventArgs e)
		{
			GenerateNextPool(poolsDivision, ERound.Quarterfinals, EPool.A);
			GenerateNextPool(poolsDivision, ERound.Quarterfinals, EPool.B);
		}

		private void PoolsQuarterSeed_Click(object sender, RoutedEventArgs e)
		{
			// Don't do anything if there are already data
			RoundData rd = tournamentData.GetRound(poolsDivision, ERound.Quarterfinals);
			foreach (PoolData pd in rd.pools)
			{
				if (pd.teamList.teams.Count > 0)
				{
					return;
				}
			}

			// Seed the first 24 teams like normal
			List<TeamData> byeTeams = new List<TeamData>();
			for (int i = 0; i < poolsAllTeamsForDivision.Count() && i < 24; ++i)
			{
				byeTeams.Add(poolsAllTeamsForDivision[i]);
			}
			FillPoolTeams(poolsDivision, ERound.Quarterfinals, byeTeams);

			// Take the top 8 from prelims
			List<TeamData> prelimMadeCutTeams = new List<TeamData>();
			RoundData prelimData = tournamentData.GetRound(poolsDivision, ERound.Prelims);
			int prelimPoolIndex = 0;
			int prelimRank = 1;
			for (int teamCount = 0; teamCount < 8; ++teamCount)
			{
				if (prelimPoolIndex >= prelimData.pools.Count)
				{
					// Error, something is wrong
					break;
				}

				int teamIndex = GetRankTeamIndex(prelimData.pools[prelimPoolIndex].resultRank, prelimRank);
				if (teamIndex < 0)
				{
					// Ran out of prelim teams
					break;
				}

				prelimMadeCutTeams.Add(prelimData.pools[prelimPoolIndex].teamList.teams[teamIndex]);

				if (prelimPoolIndex + 1 < prelimData.pools.Count)
				{
					++prelimPoolIndex;
				}
				else
				{
					prelimPoolIndex = 0;
					++prelimRank;
				}
			}

			FillPoolTeams(poolsDivision, ERound.Quarterfinals, prelimMadeCutTeams, false);
		}

		private void PoolsPrelimRandomizePlayOrder_Click(object sender, RoutedEventArgs e)
		{
			PoolsRandomizePlayOrder(poolsDivision, ERound.Prelims);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			PoolsRandomizePlayOrder(poolsDivision, ERound.Quarterfinals);
		}

		private void PoolsRandomizePlayOrder(EDivision division, ERound round)
		{
			RoundData rd = tournamentData.GetRound(poolsDivision, round);
			foreach (PoolData pd in rd.pools)
			{
				ObservableCollection<TeamData> teams = pd.teamList.teams;
				ObservableCollection<TeamData> newTeamList = new ObservableCollection<TeamData>();
				while (teams.Count > 0)
				{
					int pickIndex = random.Next(0, teams.Count());
					newTeamList.Add(teams[pickIndex]);
					teams.RemoveAt(pickIndex);
				}

				pd.teamList.teams = newTeamList;
			}

			PoolsUpdateBindings();
		}
	}
}
