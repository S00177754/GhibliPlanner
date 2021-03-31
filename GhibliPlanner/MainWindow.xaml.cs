using System;
using System.Collections.Generic;
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
        static public MainWindow Instance;

        GhibliHelper helper = new GhibliHelper();

        Thread Thrd_FilmAPI_Sync = CreateFilmSyncThread();
        Thread Thrd_FilmAPI_Info = CreateFilmInfoThread();
        //static Thread Thrd_EventNotifier = new Thread(new ThreadStart);
        //static Thread Thrd_EventSaving = new Thread(new ThreadStart());
        //static Thread Thrd_NoteSaving = new Thread(new ThreadStart());

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Thrd_FilmAPI_Sync.Start();
        }

        static Thread CreateFilmSyncThread()
        {
            Thread thrd = new Thread(new ThreadStart(UpdateFilmList));
            thrd.Name = "Film API Sync";
            thrd.Priority = ThreadPriority.BelowNormal;
            return thrd;
        }

        static Thread CreateFilmInfoThread()
        {
            Thread thrd = new Thread(new ParameterizedThreadStart(RetrieveFilm));
            thrd.Name = "Film API Info";
            thrd.Priority = ThreadPriority.AboveNormal;
            return thrd;
        }

        static void UpdateFilmList()
        {
            List<FilmResponse> films = Instance.helper.GetFilms();
            Instance.LstBxGhibliMovies.Dispatcher.Invoke(() => { Instance.LstBxGhibliMovies.ItemsSource = films; });
            
        }

        static void RetrieveFilm(object film)
        {
            string filmName = (string)film;
            FilmResponse response = Instance.helper.GetFilms().Where(f => f.title == filmName).FirstOrDefault();

            Instance.TxtBlkMovieInfo.Dispatcher.Invoke(() => { Instance.TxtBlkMovieInfo.Text = response.MovieInfo(); });
        }




        private void LstBxGhibliMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = LstBxGhibliMovies.SelectedItem;
            if (selected != null)
            {
                string filmName = selected.ToString();
                Thrd_FilmAPI_Info = CreateFilmInfoThread();
                Thrd_FilmAPI_Info.Start(filmName);
            }
        }

        private void BtnRefreshList_Click(object sender, RoutedEventArgs e)
        {
            Thrd_FilmAPI_Sync = CreateFilmSyncThread();
            Thrd_FilmAPI_Sync.Start();
            LstBxGhibliMovies.SelectedIndex = 0;
        }

        private void BtnCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            DiscordHelper.SendToWebHook(".");
        }
    }
}
