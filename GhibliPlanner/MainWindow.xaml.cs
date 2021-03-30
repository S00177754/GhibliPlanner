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
        static MainWindow Instance;

        static GhibliHelper helper = new GhibliHelper();

        static Thread Thrd_FilmAPI_Sync = new Thread(new ThreadStart(UpdateFilmList));
        static Thread Thrd_FilmAPI_Info = new Thread(new ParameterizedThreadStart(RetrieveFilm));
        //static Thread Thrd_EventNotifier = new Thread(new ThreadStart);
        //static Thread Thrd_EventSaving = new Thread(new ThreadStart());
        //static Thread Thrd_NoteSaving = new Thread(new ThreadStart());

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            ThreadInitializer();
            Thrd_FilmAPI_Sync.Start();
        }

        void ThreadInitializer()
        {
            Thrd_FilmAPI_Sync.Name = "Film API Sync";
            Thrd_FilmAPI_Sync.Priority = ThreadPriority.BelowNormal;

            Thrd_FilmAPI_Info.Name = "Film API Info";
            Thrd_FilmAPI_Info.Priority = ThreadPriority.AboveNormal;
            
        }

        static public void UpdateFilmList()
        {
            List<FilmResponse> films = helper.GetFilms();
            Instance.LstBxGhibliMovies.Dispatcher.Invoke(() => { Instance.LstBxGhibliMovies.ItemsSource = films; });
            
        }

        static public void RetrieveFilm(object film)
        {
            string filmName = (string)film;
            FilmResponse response = helper.GetFilms().Where(f => f.title == filmName).FirstOrDefault();

            Instance.TxtBlkMovieInfo.Dispatcher.Invoke(() => { Instance.TxtBlkMovieInfo.Text = response.MovieInfo(); });
        }




        private void LstBxGhibliMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filmName = LstBxGhibliMovies.SelectedItem.ToString();
            Thrd_FilmAPI_Info.Start(filmName);
        }

        private void BtnRefreshList_Click(object sender, RoutedEventArgs e)
        {
            TxtBlkThreadInfo.Text = Thrd_FilmAPI_Sync.ThreadState.ToString();
        }
    }
}
