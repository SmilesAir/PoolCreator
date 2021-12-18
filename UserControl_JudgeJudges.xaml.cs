using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// Interaction logic for UserControl_JudgeJudges.xaml
	/// </summary>
	public partial class UserControl_JudgeJudges : UserControl
	{
		MainWindow parentWindow;
		JudgesDataDisplay judgeData = new JudgesDataDisplay();
		int controlIndex = 0;
		EDivision division;
		ERoundJudgeDisplay round;

		ERound ConvertedRound
		{
			get { return EnumConverter.ConvertRoundValue(round); }
		}

		public UserControl_JudgeJudges()
		{
			InitializeComponent();
		}

		public void Init(MainWindow parent, int inControlIndex)
		{
			parentWindow = parent;
			controlIndex = inControlIndex;

			parentWindow.OnJudgesPoolChange += OnJudgesPoolChange;
			parentWindow.OnJudgeAdd += OnJudgeAdd;

			JudgesExAiControl.ItemsSource = judgeData.judgesExAi;
			JudgesVarietyControl.ItemsSource = judgeData.judgesVariety;
			JudgesDiffControl.ItemsSource = judgeData.judgesDiff;
		}

		void OnJudgesPoolChange(EDivision inDivision, ERoundJudgeDisplay inRound)
		{
			division = inDivision;
			round = inRound;

			PoolData pd = parentWindow.tournamentData.GetPool(division, round, controlIndex);

			judgeData.CopyFrom(pd.judgesData, parentWindow);
		}

		public void OnJudgeAdd(JudgeInventoryItemData judge, EJudgeCategory category, int inControlIndex)
		{
			if (inControlIndex != controlIndex)
			{
				return;
			}
			
			AddJudgeToPoolData(judge, category, division, ConvertedRound, EnumConverter.ConvertPoolValue(round, controlIndex));

			judgeData.Add(judge, category);
		}

		private void AddJudgeToPoolData(JudgeInventoryItemData judge, EJudgeCategory category, EDivision division, ERound round, EPool pool)
		{
			PoolData pd = parentWindow.tournamentData.GetPool(division, round, pool);

			RegisteredPlayer rp;
			if (parentWindow.TryFindRegisteredPlayer(judge.PlayerName, out rp))
			{
				pd.judgesData.Add(rp, category);
			}
		}

		private void RemoveJudge_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			JudgeInventoryItemData judge = button.Tag as JudgeInventoryItemData;

			judgeData.Remove(judge);

			RemoveJudgeFromPoolData(judge, division, ConvertedRound, EnumConverter.ConvertPoolValue(round, controlIndex));

			parentWindow.OnJudgeRemove(judge);
		}

		private void RemoveJudgeFromPoolData(JudgeInventoryItemData judge, EDivision division, ERound round, EPool pool)
		{
			PoolData pd = parentWindow.tournamentData.GetPool(division, round, pool);

			RegisteredPlayer rp;
			if (parentWindow.TryFindRegisteredPlayer(judge.PlayerName, out rp))
			{
				pd.judgesData.Remove(rp);
			}
		}

		public class JudgesDataDisplay
		{
			public ObservableCollection<JudgeInventoryItemData> judgesExAi = new ObservableCollection<JudgeInventoryItemData>();
			public ObservableCollection<JudgeInventoryItemData> judgesVariety = new ObservableCollection<JudgeInventoryItemData>();
			public ObservableCollection<JudgeInventoryItemData> judgesDiff = new ObservableCollection<JudgeInventoryItemData>();

			public JudgesDataDisplay()
			{
			}

			public void Add(JudgeInventoryItemData judge, EJudgeCategory category)
			{
				switch (category)
				{
					case EJudgeCategory.ExAi:
						judgesExAi.Add(judge);
						break;
					case EJudgeCategory.Variety:
						judgesVariety.Add(judge);
						break;
					case EJudgeCategory.Difficulty:
						judgesDiff.Add(judge);
						break;
				}
			}

			public void Remove(JudgeInventoryItemData judge)
			{
				judgesExAi.Remove(judge);
				judgesVariety.Remove(judge);
				judgesDiff.Remove(judge);
			}

			public void CopyFrom(JudgesData jd, MainWindow parentWindow)
			{
				judgesExAi.Clear();
				foreach (RegisteredPlayer rp in jd.judgesEx)
				{
					judgesExAi.Add(parentWindow.CreateJudgeInventoryItemData(rp));
				}

				judgesVariety.Clear();
				foreach (RegisteredPlayer rp in jd.judgesAi)
				{
					judgesVariety.Add(parentWindow.CreateJudgeInventoryItemData(rp));
				}

				judgesDiff.Clear();
				foreach (RegisteredPlayer rp in jd.judgesDiff)
				{
					judgesDiff.Add(parentWindow.CreateJudgeInventoryItemData(rp));
				}
			}
		}
	}
}
