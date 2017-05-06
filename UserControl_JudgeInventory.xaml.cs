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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoolCreator
{
	/// <summary>
	/// Interaction logic for UserControl_JudgeInventory.xaml
	/// </summary>
	public partial class UserControl_JudgeInventory : UserControl, INotifyPropertyChanged
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

		MainWindow parentWindow = null;
		ObservableCollection<JudgeInventoryItemData> judgeInventoryItems = new ObservableCollection<JudgeInventoryItemData>();
		public ObservableCollection<JudgeInventoryItemData> JudgeInventoryItems { get { return judgeInventoryItems; } }

		EDivision division = EDivision.None;
		public EDivisionDisplay SelectedDivision
		{
			get { return EnumConverter.ConvertDivisionValue(division); }
			set
			{
				division = EnumConverter.ConvertDivisionValue(value);
				OnPropertyChanged("SelectedDivision");

				parentWindow.OnJudgesPoolChange(division, round);

				UpdateJudgeInventory();
			}
		}
		ERoundJudgeDisplay round = ERoundJudgeDisplay.None;
		public ERoundJudgeDisplay SelectedRound
		{
			get { return round; }
			set
			{
				round = value;
				OnPropertyChanged("SelectedRound");

				parentWindow.OnJudgesPoolChange(division, round);

				UpdateJudgeInventory();
			}
		}
		string filterText;
		public string FilterText
		{
			get { return filterText; }
			set
			{
				filterText = value;
				OnPropertyChanged("FilterText");

				UpdateJudgeInventory();
			}
		}

		public UserControl_JudgeInventory()
		{
			InitializeComponent();
		}

		public void Init(MainWindow inParentWindow)
		{
			parentWindow = inParentWindow;

			parentWindow.OnJudgeRemove += OnRemoveJudge;

			TopLevelGrid.DataContext = this;

			DivisionComboBox.ItemsSource = parentWindow.EDivisionTypeValues;
			RoundComboBox.ItemsSource = parentWindow.ERoundJudgeTypeValues;

			UpdateJudgeInventory();
		}

		public void OnJudgeTabSelected()
		{
			UpdateJudgeInventory();
		}

		private void UpdateJudgeInventory()
		{
			if (SelectedDivision == EDivisionDisplay.None || SelectedRound == ERoundJudgeDisplay.None)
			{
				return;
			}

			judgeInventoryItems.Clear();
			
			bool bUseFilter = FilterText != null && FilterText != "";
			ObservableCollection<RegisteredPlayer> playerList;
			if (bUseFilter)
			{
				playerList = NameFinder.GetFilteredNames(parentWindow.RegisteredPlayers, FilterText);
			}
			else
			{
				playerList = parentWindow.RegisteredPlayers;
			}

			// Create all the judge inventory items
			List<JudgeInventoryItemData> judgeItems = new List<JudgeInventoryItemData>();
			foreach (RegisteredPlayer rp in playerList)
			{
				judgeItems.Add(parentWindow.CreateJudgeInventoryItemData(rp));
			}

			UpdateAddJudgeButtons(judgeItems);

			if (!bUseFilter)
			{
				// Sort
				judgeItems.Sort();
			}

			foreach (JudgeInventoryItemData jid in judgeItems)
			{
				judgeInventoryItems.Add(jid);
			}

			// Set the judge counts and compete times
		}

		private void UpdateAddJudgeButtons(List<JudgeInventoryItemData> judgeItems)
		{
			PoolData pd1 = parentWindow.tournamentData.GetPool(division, round, 0);
			PoolData pd2 = parentWindow.tournamentData.GetPool(division, round, 1);

			foreach (JudgeInventoryItemData judge in judgeItems)
			{
				judge.ButtonAEnabled = true;
				judge.ButtonBEnabled = true;

				if (IsPlayingInPool(judge.PlayerName, 0))
				{
					judge.ButtonAEnabled = false;
				}

				if (IsPlayingInPool(judge.PlayerName, 1))
				{
					judge.ButtonBEnabled = false;
				}

				if (round == ERoundJudgeDisplay.Finals)
				{
					judge.ButtonBEnabled = false;
				}

				RegisteredPlayer rp;
				if (parentWindow.TryFindRegisteredPlayer(judge.PlayerName, out rp))
				{
					if (pd1.judgesData.Contains(rp))
					{
						judge.ButtonAEnabled = false;
					}

					if (pd2.judgesData.Contains(rp))
					{
						judge.ButtonBEnabled = false;
					}
				}
			}
		}

		private bool IsPlayingInPool(string playerName, int controlIndex)
		{
			ObservableCollection<TeamData> playingTeams;
			parentWindow.GetPlayingTeams(division, round, controlIndex, out playingTeams);

			if (playingTeams != null)
			{
				foreach (TeamData td in playingTeams)
				{
					foreach (RegisteredPlayer rp in td.players)
					{
						if (rp.FullName == playerName)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private void AddJudgePool1_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			JudgeInventoryItemData judge = button.Tag as JudgeInventoryItemData;
			
			AddJudge(judge, button.Content as string, 0);
		}

		private void AddJudgePool2_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			JudgeInventoryItemData judge = button.Tag as JudgeInventoryItemData;

			AddJudge(judge, button.Content as string, 1);
		}

		private void AddJudge(JudgeInventoryItemData judge, string buttonContent, int controlIndex)
		{
			EJudgeCategory category = EJudgeCategory.Execution;
			switch (buttonContent)
			{
				case "Ex":
					category = EJudgeCategory.Execution;
					break;
				case "Ai":
					category = EJudgeCategory.ArtisticImpression;
					break;
				case "Diff":
					category = EJudgeCategory.Difficulty;
					break;
			}

			parentWindow.OnJudgeAdd(judge, category, controlIndex);

			UpdateJudgeInventory();
		}

		public void OnRemoveJudge(JudgeInventoryItemData judge)
		{
			UpdateJudgeInventory();
		}
	}

	public class JudgeInventoryItemData : INotifyPropertyChanged, IComparable<JudgeInventoryItemData>
	{
		public string PlayerName { get; set; }
		public int TimesJudged { get; set; }
		public int Rank { get; set; }
		string timeToNextCompete = "1.5 hours";
		public string TimeToNextCompete
		{
			get { return timeToNextCompete; }
			set { timeToNextCompete = value; }
		}
		public string CountryOfOrigin { get; set; }
		public string ButtonAText { get; set; }
		bool buttonAEnabled = true;
		public bool ButtonAEnabled
		{
			get { return buttonAEnabled; }
			set
			{
				buttonAEnabled = value;
				OnPropertyChanged("ButtonAEnabled");
			}
		}
		public string ButtonBText { get; set; }
		bool buttonBEnabled = true;
		public bool ButtonBEnabled
		{
			get { return buttonBEnabled; }
			set
			{
				buttonBEnabled = value;
				OnPropertyChanged("ButtonBEnabled");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public JudgeInventoryItemData()
		{
			InitData();
			
		}

		public JudgeInventoryItemData(RegisteredPlayer rp)
		{
			InitData();

			PlayerName = rp.FullName;
			Rank = rp.rank;

		}

		void InitData()
		{
			TimesJudged = 0;
			CountryOfOrigin = "USA";

			ButtonAText = "Pool 1";
			ButtonAEnabled = false;
			ButtonBText = "Pool 2";
			ButtonBEnabled = false;
		}

		private int CompareRank(JudgeInventoryItemData other)
		{
			if (Rank == other.Rank)
			{
				return 0;
			}

			if (Rank == 0)
			{
				return 1;
			}
			else if (other.Rank == 0)
			{
				return -1;
			}

			return Rank - other.Rank;
		}

		public int CompareTo(JudgeInventoryItemData other)
		{
			if (ButtonAEnabled == other.ButtonAEnabled && ButtonBEnabled == other.ButtonBEnabled)
			{
				return CompareRank(other);
			}

			if (ButtonAEnabled && ButtonBEnabled)
			{
				return -1;
			}
			else if (other.ButtonAEnabled && other.ButtonBEnabled)
			{
				return 1;
			}
			else if ((ButtonAEnabled || ButtonBEnabled) && !(other.ButtonAEnabled || other.ButtonBEnabled))
			{
				return -1;
			}
			else if (!(ButtonAEnabled || ButtonBEnabled) && (other.ButtonAEnabled || other.ButtonBEnabled))
			{
				return 1;
			}

			return CompareRank(other);
		}
	}
}
