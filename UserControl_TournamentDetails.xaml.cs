using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
	/// Interaction logic for UserControl_TournamentDetails.xaml
	/// </summary>
	public partial class UserControl_TournamentDetails : UserControl, INotifyPropertyChanged
	{
		public EventData eventData = new EventData();

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
		public string TournamentName
		{
			get { return parentWindow.tournamentData.TournamentName; }
			set
			{
				parentWindow.tournamentData.TournamentName = value;
				EventSummaryData eventSummaryData = eventData.allEventSummaryData.FirstOrDefault(x => x.Value.eventName == value).Value;
				parentWindow.tournamentData.EventKey = eventSummaryData != null ? eventSummaryData.key : "";
				OnPropertyChanged("TournamentName");
			}
		}
		public string TournamentSubtitle
		{
			get { return parentWindow.tournamentData.TournamentSubtitle; }
			set
			{
				parentWindow.tournamentData.TournamentSubtitle = value;
				OnPropertyChanged("TournamentSubtitle");
			}
		}

		public UserControl_TournamentDetails()
		{
			InitializeComponent();
		}

		public void Init(MainWindow inParentWindow)
		{
			parentWindow = inParentWindow;

			TournamentDetailsGrid.DataContext = this;

			QueryEventData();

			WomenDetails.Init(inParentWindow);
			OpenDetails.Init(inParentWindow);
			MixedDetails.Init(inParentWindow);
			CoopDetails.Init(inParentWindow);
		}

		private void QueryEventData()
		{
			BackgroundWorker getRankingsWorker = new BackgroundWorker();
			getRankingsWorker.DoWork += delegate { QueryMicroserviceRankings_DoWork(); };
			getRankingsWorker.RunWorkerAsync();
		}

		private void QueryMicroserviceRankings_DoWork()
		{
			using (WebClient client = new WebClient())
			{
				string json = client.DownloadString("https://wyach4oti8.execute-api.us-west-2.amazonaws.com/production/getAllEvents");
				using (StringReader textStream = new StringReader(json))
				{
					string jsonString = textStream.ReadToEnd();

					eventData = JsonConvert.DeserializeObject<EventData>(jsonString);
				}
			}

		}
	}

	public class EventSummaryData
	{
		public string key;
		public Int64 createdAt;
		public string eventName;
		public string startDate;
		public string endDate;
	};

	public class EventData
	{
		public Dictionary<string, EventSummaryData> allEventSummaryData = new Dictionary<string, EventSummaryData>();
	};
}
