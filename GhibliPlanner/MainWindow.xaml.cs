using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace GhibliPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public Ghibli Core = new Ghibli();

        CancellationTokenSource RetrieveFilmCancel;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            Core.RetrieveFilms(ref RetrieveFilmCancel);

            Core.CreateLoadDiscordThread();
            Core.CreateLoadEventThread();
        }

        //GENERAL UI EVENTS
        private void BtnCancelProgOperation_Click(object sender, RoutedEventArgs e)
        {
            RetrieveFilmCancel.Cancel();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TabControl))
            {
                switch ((e.Source as TabControl).SelectedIndex)
                {
                    case 0: //Movie Info

                        break;

                    case 1: //Scheduled Events
                        CmbBxDiscord.ItemsSource = Core.DiscordRecords;
                        CmbBxDiscord.Items.Refresh();
                        CmbBxDiscord.SelectedIndex = 0;

                        LstBxEvents.ItemsSource = Core.EventRecords;
                        LstBxEvents.Items.Refresh();
                        break;

                    case 2: //Discord Records
                        LstBxDiscord.ItemsSource = Core.DiscordRecords;
                        LstBxDiscord.Items.Refresh();
                        break;
                }
            }
        }


        //MOVIE INFO UI EVENTS
        private void LstBxGhibliMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Core.IsGetMovieActive())
            {
                Core.GetFilm(LstBxGhibliMovies.SelectedItem.ToString());
            }
        }

        private void BtnRefreshList_Click(object sender, RoutedEventArgs e)
        {
            Core.RetrieveFilms(ref RetrieveFilmCancel);
        }

        private void BtnCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            if(DtPck.SelectedDate != null 
                && Core.RetrievedMovie != null 
                && !Core.EventRecords.Contains(new EventRecord(Core.RetrievedMovie.title, DtPck.SelectedDate.Value)))
            {
                EventRecord eventRecord = new EventRecord(Core.RetrievedMovie.title, DtPck.SelectedDate.Value);
                Core.EventRecords.Add(eventRecord);
                LstBxEvents.Items.Refresh();
            }
        }


        //SCHEDULED EVENTS UI EVENTS
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Core.EventRecords.Remove(button.DataContext as EventRecord);
            LstBxEvents.Items.Refresh();
        }

        private void BtnSendReminder_Click(object sender, RoutedEventArgs e)
        {
            EventRecord er = (LstBxEvents.SelectedItem as EventRecord);
            GhibliHelper.SendToWebHook((CmbBxDiscord.SelectedItem as DiscordRecord).WebhookURL,string.Concat("Movie: ",er.MovieTitle,"\nDate: ",er.Date.ToShortDateString(),"\n",TxtBxDiscordMsg.Text),"Ghibli Planner v0.1");
        }

        private void BtnSaveEventLists_Click(object sender, RoutedEventArgs e)
        {
            Core.CreateSaveEventThread();
        }

        private void BtnLoadEventLists_Click(object sender, RoutedEventArgs e)
        {
            Core.CreateLoadEventThread();
            
        }


        //DISCORD RECORDS UI EVENTS
        private void BtnSaveDiscord_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(TxtBxServerName.Text) && !string.IsNullOrWhiteSpace(TxtBxWebhookURL.Text))
            {
                DiscordRecord discRec = new DiscordRecord(TxtBxServerName.Text, TxtBxWebhookURL.Text);
                Core.DiscordRecords.Add(discRec);

                LstBxDiscord.ItemsSource = Core.DiscordRecords;
                LstBxDiscord.Items.Refresh();
            }
        }

        private void BtnClearFields_Click(object sender, RoutedEventArgs e)
        {
            TxtBxServerName.Text = "";
            TxtBxWebhookURL.Text = "";
        }

        private void RemoveDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Core.DiscordRecords.Remove(button.DataContext as DiscordRecord);
            LstBxDiscord.Items.Refresh();
        }

        private void BtnSaveDiscordList_Click(object sender, RoutedEventArgs e)
        {
            Core.CreateSaveDiscordThread();
        }

        private void BtnLoadDiscordList_Click(object sender, RoutedEventArgs e)
        {
            Core.CreateLoadDiscordThread();
            LstBxDiscord.Items.Refresh();
        }


        //EXTRA FUNCTIONALITY
        public void UpdateEventStatus()
        {
            if (Core.EventRecords.Count > 0)
            {
                EventRecord eventRec = Core.EventRecords.OrderBy(e => e.Date).ToList()[0];
                TxtBlkEventStatus.Text = string.Concat("Next Event: ", eventRec.MovieTitle, " - ", eventRec.Date.ToShortDateString());
            }
            else
            {
                TxtBlkEventStatus.Text = "Next Event: ";
            }
        }

        private void CmbBxDiscord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
