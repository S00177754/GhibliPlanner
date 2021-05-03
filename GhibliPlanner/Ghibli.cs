using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GhibliPlanner
{
    [Serializable]
    public class Ghibli //join on final thread to occur
    {
        public const string FileName = "records.txt";
        IsolatedStorageFile isoFile;

        Thread SetMoviesThread;
        Thread GetMoviesThread;
        Thread PersistanceThread;

        public List<MovieFile> Movies = new List<MovieFile>();
        public List<DiscordRecord> DiscordRecords = new List<DiscordRecord>();
        public List<EventRecord> EventRecords = new List<EventRecord>();

        public MovieFile RetrievedMovie = new MovieFile();

        public Ghibli()
        {
            isoFile = IsolatedStorageFile.GetUserStoreForDomain();
        }

        #region Discord Records

        /// <summary>
        /// Allows the addition of a discord record.
        /// </summary>
        /// <param name="record">Server Name and Webhook URL.</param>
        public void AddRecord(DiscordRecord record)
        {
            if (Monitor.TryEnter(DiscordRecords))
            {
                DiscordRecords.Add(record);
                Monitor.Exit(DiscordRecords);
                Monitor.PulseAll(DiscordRecords);
            }
        }

        public void RemoveRecord(string serverName)
        {
            if (Monitor.TryEnter(DiscordRecords))
            {
                DiscordRecord record = DiscordRecords.Where(d => d.ServerName == serverName).SingleOrDefault();
                DiscordRecords.Remove(record);
                Monitor.Exit(DiscordRecords);
                Monitor.Pulse(DiscordRecords);
            }
        }

        #endregion

        #region Movie Files

        public void GetMovie(object movieName)
        {
            if (SetMoviesThread.ThreadState == ThreadState.Running)
                SetMoviesThread.Join();

            try
            {
                Thread.MemoryBarrier(); //Gaurantees that if thread is run after Refresh List Movies then it will retrieve new data

                if (Monitor.TryEnter(Movies, 3000))
                {
                    RetrievedMovie = Movies.Where(m => m.title == (string)movieName).SingleOrDefault();
                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkMovieInfo.Text = RetrievedMovie.MovieInfo());
                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(Thread.CurrentThread.Name, " - Movie Info Retrieved and displayed."));
                    Monitor.Exit(Movies);
                }
            }
            catch (ThreadInterruptedException)
            {
                MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(Thread.CurrentThread.Name, " - Get Movie has been interrupted."));
            }

        }

        public void SetMovies(object req)
        {
            try
            {
                SetMovieRequest data = (SetMovieRequest)req;

                if (Monitor.TryEnter(Movies,3000))
                {
                    List<MovieFile> movieList;

                    if (Movies.Count > 0 )
                    {
                        Monitor.Pulse(Movies);
                        Monitor.Wait(Movies,100);
                    }

                    Movies.Clear();
                    movieList = Utility.ConvertFromJson(data.JsonData);
                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkProgressMsg.Text = "Downloading:");

                    for (int i = 0; i < movieList.Count; i++)
                    {
                        Movies.Add(movieList[i]);
                        
                        double percent = (((double)i + 1) / (double)movieList.Count);
                        MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.PrgBrStatusBar.Value = percent);
                        Thread.Sleep(300);

                        if (data.Token.IsCancellationRequested)
                        {
                            MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat("> ",Thread.CurrentThread.Name," Set Movie Thread has been cancelled."));
                            Monitor.Exit(Movies);
                            return;
                        }
                    }

                    string threadName = Thread.CurrentThread.Name;

                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.LstBxGhibliMovies.ItemsSource = Movies);
                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat("> ", threadName, " has retrieved updated list."));

                    Monitor.Exit(Movies);
                }
            }
            catch (ThreadInterruptedException)
            {
                MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(Thread.CurrentThread.Name," - Set Movies has been interrupted."));
            }
        }

        #endregion

        #region Thread Creation

        public void RetrieveFilms(ref CancellationTokenSource tokenSource)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(SetMovies));
            thread.Name = "Create Retrieval Thread";
            thread.Priority = ThreadPriority.Highest;

            if (SetMoviesThread != null)
            {
                if (tokenSource != null && !tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
                else if(tokenSource == null)
                {
                    MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(Thread.CurrentThread.Name, " - Retrieve Film has detected a rogue thread."));
                }
            }

            SetMoviesThread = null;
            SetMoviesThread = thread;
            tokenSource = new CancellationTokenSource();
            SetMoviesThread.Start(new SetMovieRequest(GhibliHelper.GetFilms(),tokenSource.Token));
        }

        public void GetFilm(string movieName)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(GetMovie));
            thread.Name = "Get Movie Thread";
            thread.Priority = ThreadPriority.AboveNormal;
            thread.IsBackground = true;

            if (GetMoviesThread != null)
                GetMoviesThread.Abort();

            GetMoviesThread = null;
            GetMoviesThread = thread;
            GetMoviesThread.Start(movieName);
        }

        #endregion

        #region Record Persistance

        public void Save()
        {
            try
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, isoFile))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    if (Monitor.TryEnter(DiscordRecords))
                    {
                        formatter.Serialize(isoStream,DiscordRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception Occured", ex.Message);
            }
        }

        public void Load()
        {
            try
            {
                using(IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, isoFile))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    if (Monitor.TryEnter(DiscordRecords))
                    {
                        DiscordRecords = (List<DiscordRecord>)formatter.Deserialize(isoStream);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception Occured", ex.Message);
            }
        }

        #endregion

        public bool IsSetMovieActive()
        {
            if (SetMoviesThread != null)
                return SetMoviesThread.IsAlive;
            else
                return false;
        }

        public bool IsGetMovieActive()
        {
            if (GetMoviesThread != null)
                return GetMoviesThread.IsAlive;
            else
                return false;
        }
    }

    [Serializable]
    public class MovieFile
    {
        public string id;
        public string title;
        public string original_title;
        public string original_title_romanised;
        public string description;
        public string director;
        public string producer;
        public string release_date;
        public int running_time;
        public int rt_score;

        public List<string> people;
        public List<string> species;
        public List<string> locations;
        public List<string> vehicles;

        public string url;

        public string MovieInfo()
        {
            return string.Concat(title, "\n", "Running Time:", running_time, "\n", "Release Date:", release_date, "\n", "Description:\n", description);
        }

        public override string ToString()
        {
            return title;
        }

        //Include method to set variables form moviefile object
    }

    /// <summary>
    /// A record object for a discord channel, this includes the server name and webhook URL for the desired discord channel.
    /// </summary>
    [Serializable]
    public class DiscordRecord
    {
        public string ServerName;
        public string WebhookURL;

        public DiscordRecord() { }

        public DiscordRecord(string serverName, string webhookURL)
        {
            ServerName = serverName;
            WebhookURL = webhookURL;
        }
    }

    /// <summary>
    /// A record object for a movie event, this includes info such as the title and Sate the event will occur.
    /// </summary>
    [Serializable]
    public class EventRecord
    {
        public string MovieTitle { get; set; }
        public DateTime Date { get; set; }

        public EventRecord() { }

        public EventRecord(string movieTitle,DateTime date)
        {
            MovieTitle = movieTitle;
            Date = date;
        }
    }

    /// <summary>
    /// Input data for Set Movie Thread
    /// </summary>
    public class SetMovieRequest
    {
        public string JsonData;
        public CancellationToken Token;

        public SetMovieRequest () {}

        public SetMovieRequest(string json,CancellationToken ct)
        {
            JsonData = json;
            Token = ct;
        }
    }
}
