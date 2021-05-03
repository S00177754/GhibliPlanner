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
        }

        private void Setup()
        {
            
        }

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

        private void BtnCancelProgOperation_Click(object sender, RoutedEventArgs e)
        {
            RetrieveFilmCancel.Cancel();
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            LstBxEvents.ItemsSource = Core.EventRecords;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Core.EventRecords.Remove(button.DataContext as EventRecord);
            LstBxEvents.Items.Refresh();
        }
    }

    
}
