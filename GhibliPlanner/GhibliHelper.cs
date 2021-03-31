using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GhibliPlanner
{
    public class GhibliHelper
    {
        WebClient client = new WebClient();

        static public string BaseURL = @"https://ghibliapi.herokuapp.com/";
        static public string FilmEndpoint = @"films";

        static string CreateURL()
        {
            return string.Concat(BaseURL,FilmEndpoint);
        }

        public FilmResponse GetFilm(string filmName)
        {
            client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    
            string response = client.DownloadString(string.Concat(CreateURL()));
            if (!string.IsNullOrEmpty(response))
            {
                FilmResponse flm = JsonConvert.DeserializeObject<List<FilmResponse>>(response)[0];

                //MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(">", Thread.CurrentThread.Name, " has successfully gotten film info.");
                return flm;
            }

            //MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(">", Thread.CurrentThread.Name, " is unable to access GetFilms() because its busy.");
            return null;

        }

        public List<FilmResponse> GetFilms()
        {
            client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            string response = client.DownloadString(string.Concat(CreateURL()));
            if (!string.IsNullOrEmpty(response))
            {
                List<FilmResponse> lst = JsonConvert.DeserializeObject<List<FilmResponse>>(response);

                //MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(">", Thread.CurrentThread.Name, " has successfully gotten film info.");
                return lst;
            }

            //MainWindow.Instance.TxtBlkThreadInfo.Text = string.Concat(">",Thread.CurrentThread.Name," is unable to access GetFilms() because its busy.");
            return null;
        }
    }

    [Serializable]
    public class FilmResponse
    {
        public string id;

        public string title;
        public string original_title;
        public string original_title_romanised;
        public string description;

        public string director;
        public string producer;

        public string release_date;
        public string running_time;

        public string rt_score;

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
    }
}
