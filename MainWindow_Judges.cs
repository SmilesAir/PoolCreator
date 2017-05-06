using System;
using System.ComponentModel;
using System.Windows;

namespace PoolCreator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public delegate void OnJudgesPoolChangeDelegate(EDivision division, ERoundJudgeDisplay round);
		public OnJudgesPoolChangeDelegate OnJudgesPoolChange;

		public delegate void OnJudgeAddDelegate(JudgeInventoryItemData judge, EJudgeCategory category, int controlIndex);
		public OnJudgeAddDelegate OnJudgeAdd;

		public delegate void OnJudgeRemoveDelegate(JudgeInventoryItemData judge);
		public OnJudgeRemoveDelegate OnJudgeRemove;

		private void InitJudges()
		{
			JudgePlayingTeamsA.Init(this, 0);
			JudgePlayingTeamsB.Init(this, 1);

			JudgeJudgesTeamsA.Init(this, 0);
			JudgeJudgesTeamsB.Init(this, 1);

			JudgeInventory.Init(this);
		}

		private void OnJudgeTabSelected()
		{
			JudgeInventory.OnJudgeTabSelected();
		}

		public JudgeInventoryItemData CreateJudgeInventoryItemData(RegisteredPlayer rp)
		{
			return new JudgeInventoryItemData(rp);
		}

		public JudgeInventoryItemData CreateJudgeInventoryItemData(string fullname)
		{
			foreach (RegisteredPlayer rp in RegisteredPlayers)
			{
				if (rp.FullName == fullname)
				{
					return new JudgeInventoryItemData(rp);
				}
			}

			return null;
		}
	}
}
