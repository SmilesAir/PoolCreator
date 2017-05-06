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

			SetPoolItemsSources();

			// Try to generate any future pools
			GenerateFuturePools();

			//TeamData td = new TeamData();
			//td.players.Add(new RegisteredPlayer("Ryan", "Young", 0, 0));
			//td.players.Add(new RegisteredPlayer("Randy", "Silvey", 0, 0));

			//TeamData td2 = new TeamData();
			//td2.players.Add(new RegisteredPlayer("James", "Wiseman", 0, 0));
			//td2.players.Add(new RegisteredPlayer("Marco", "Prati", 0, 0));

			//tournamentData.GetPool(EDivision.Open, ERound.Semifinals, EPool.B).teamList.teams.Add(td);
			//tournamentData.GetPool(EDivision.Open, ERound.Semifinals, EPool.B).teamList.teams.Add(td2);
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
			poolsAllTeamsForDivision = tournamentData.GetAllTeams(poolsDivision);
			PoolsTeamsItemsControl.ItemsSource = poolsAllTeamsForDivision;

			OnPropertyChanged("FinalTeamCount");
			OnPropertyChanged("SemiTeamCount");
			OnPropertyChanged("QuarterTeamCount");

			SetPoolItemsSources();
		}

		private void PoolsSeedRound_Click(object sender, RoutedEventArgs e)
		{
			if (poolsDivision != EDivision.None && poolsRound != ERound.None)
			{
				int rawPoolIndex = 0;
				int adjustedPoolIndex = 0;
				bool bReversePoolAssignmentDirection = false;
				int maxPoolCount = tournamentData.GetRound(poolsDivision, poolsRound).pools.Count;
				for (int poolIndex = 0; poolIndex < maxPoolCount; ++poolIndex)
				{
					PoolData pd = tournamentData.GetPool(poolsDivision, poolsRound, (EPool)poolIndex);
					pd.teamList.teams.Clear();
				}

				for (int teamIndex = 0; teamIndex < poolsAllTeamsForDivision.Count; ++teamIndex)
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

					PoolData pd = tournamentData.GetPool(poolsDivision, poolsRound, (EPool)adjustedPoolIndex);
					pd.teamList.teams.Add(poolsAllTeamsForDivision[teamIndex]);

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
			}
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

		}
	}
}
