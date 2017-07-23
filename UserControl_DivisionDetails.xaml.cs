using System;
using System.Collections.Generic;
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
	/// Interaction logic for UserControl_DivisionDetails.xaml
	/// </summary>
	public partial class UserControl_DivisionDetails : UserControl, INotifyPropertyChanged
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

		MainWindow parentWindow;
		EDivision division = EDivision.None;
		public string DivisionName
		{
			get { return division.ToString(); }
			set { division = (EDivision)Enum.Parse(typeof(EDivision), value); }
		}
		public string HeadJudge
		{
			get { return GetDivisionData().headJudge; }
			set
			{
				GetDivisionData().headJudge = value;
				OnPropertyChanged("HeadJudge");
			}
		}
		public string Directors
		{
			get { return GetDivisionData().directors; }
			set
			{
				GetDivisionData().directors = value;
				OnPropertyChanged("Directors");
			}
		}
		public string Committee
		{
			get { return GetDivisionData().committee; }
			set
			{
				GetDivisionData().committee = value;
				OnPropertyChanged("Committee");
			}
		}
		public ERoutineLength PrelimSelectedTime
		{
			get { return GetRoutineLength(ERound.Prelims); }
			set
			{
				SetRoutineLength(ERound.Prelims, value);

				OnPropertyChanged("PrelimSelectedTime");
			}
		}
		public ERoutineLength QuarterSelectedTime
		{
			get { return GetRoutineLength(ERound.Quarterfinals); }
			set
			{
				SetRoutineLength(ERound.Quarterfinals, value);

				OnPropertyChanged("QuarterSelectedTime");
			}
		}
		public ERoutineLength SemiSelectedTime
		{
			get { return GetRoutineLength(ERound.Semifinals); }
			set
			{
				SetRoutineLength(ERound.Semifinals, value);

				OnPropertyChanged("SemiSelectedTime");
			}
		}
		public ERoutineLength FinalSelectedTime
		{
			get { return GetRoutineLength(ERound.Finals); }
			set
			{
				SetRoutineLength(ERound.Finals, value);

				OnPropertyChanged("FinalSelectedTime");
			}
		}
		public DateTime PrelimDateTime
		{
			get { return GetRoundData(ERound.None).GetScheduleTime(); }
			set
			{
				GetRoundData(ERound.None).scheduleTime = value;
				OnPropertyChanged("PrelimDateTime");
			}
		}
		public DateTime QuarterDateTime
		{
			get { return GetRoundData(ERound.Quarterfinals).GetScheduleTime(); }
			set
			{
				GetRoundData(ERound.Quarterfinals).scheduleTime = value;
				OnPropertyChanged("QuarterDateTime");
			}
		}
		public DateTime SemiDateTime
		{
			get { return GetRoundData(ERound.Semifinals).GetScheduleTime(); }
			set
			{
				GetRoundData(ERound.Semifinals).scheduleTime = value;
				OnPropertyChanged("SemiDateTime");
			}
		}
		public DateTime FinalDateTime
		{
			get { return GetRoundData(ERound.Finals).GetScheduleTime(); }
			set
			{
				GetRoundData(ERound.Finals).scheduleTime = value;
				OnPropertyChanged("FinalDateTime");
			}
		}

		public UserControl_DivisionDetails()
		{
			InitializeComponent();
		}

		public void Init(MainWindow inParentWindow)
		{
			parentWindow = inParentWindow;

			TopLevelGrid.DataContext = this;
		}

		public DivisionData GetDivisionData()
		{
			return parentWindow.tournamentData.GetDivision(division);
		}

		public RoundData GetRoundData(ERound inRound)
		{
			RoundData rd = parentWindow.tournamentData.GetRound(division, inRound);
			if (rd != null)
			{
				return rd;
			}

			return null;
		}

		public void SetRoutineLength(ERound inRound, ERoutineLength length)
		{
			RoundData rd = GetRoundData(inRound);
			if (rd != null)
			{
				rd.routineLength = RoutineLengthToFloat(length);
			}
		}

		public ERoutineLength GetRoutineLength(ERound inRound)
		{
			RoundData rd = GetRoundData(inRound);
			if (rd != null)
			{
				return RoutineLengthFromFloat(rd.routineLength);
			}

			return ERoutineLength.None;
		}

		public enum ERoutineLength
		{
			None,
			_2_Minutes,
			_2_Minutes_30_Seconds,
			_3_Minutes,
			_4_Minutes,
			_5_Minutes
		}

		public IEnumerable<ERoutineLength> ERoutineLengthTypeValues
		{
			get
			{
				return Enum.GetValues(typeof(ERoutineLength)).Cast<ERoutineLength>();
			}
		}

		public float RoutineLengthToFloat(ERoutineLength length)
		{
			switch (length)
			{
				case ERoutineLength._2_Minutes:
					return 2f;
				case ERoutineLength._2_Minutes_30_Seconds:
					return 2.5f;
				case ERoutineLength._3_Minutes:
					return 3f;
				case ERoutineLength._4_Minutes:
					return 4f;
				case ERoutineLength._5_Minutes:
					return 5f;
			}

			return 0f;
		}

		bool SoftRoutineLengthEquals(float length, float target)
		{
			return Math.Abs(length - target) < .3f;
		}

		public ERoutineLength RoutineLengthFromFloat(float lengthInMinutes)
		{
			if (SoftRoutineLengthEquals(lengthInMinutes, 2f))
			{
				return ERoutineLength._2_Minutes;
			}
			else if (SoftRoutineLengthEquals(lengthInMinutes, 2.5f))
			{
				return ERoutineLength._2_Minutes_30_Seconds;
			}
			else if (SoftRoutineLengthEquals(lengthInMinutes, 3f))
			{
				return ERoutineLength._3_Minutes;
			}
			else if (SoftRoutineLengthEquals(lengthInMinutes, 4f))
			{
				return ERoutineLength._4_Minutes;
			}
			else if (SoftRoutineLengthEquals(lengthInMinutes, 5f))
			{
				return ERoutineLength._5_Minutes;
			}

			return ERoutineLength.None;
		}
	}
}
