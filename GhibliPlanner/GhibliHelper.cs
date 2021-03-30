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
using System.Threading.Tasks;

namespace GhibliPlanner
{
    public class GhibliHelper
    {
        static HttpClient client = new HttpClient();

        static public string BaseURL = @"https://ghibliapi.herokuapp.com/";
        static public string FilmEndpoint = @"films";

        static string CreateURL(string movieName)
        {
            return string.Concat(BaseURL,FilmEndpoint,movieName);
        }

        public FilmResponse GetFilm(string filmName)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(CreateURL("?title"));

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(string.Concat("=",filmName)).Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync();
                json.Wait();
                Debug.WriteLine(json);
                FilmResponse flm = JsonConvert.DeserializeObject<List<FilmResponse>>(json.Result)[0];
                client.Dispose();
                return flm;
            }

            return null;
        }

        public List<FilmResponse> GetFilms()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(CreateURL(""));

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync("").Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync();
                json.Wait();

                List<FilmResponse> lst = JsonConvert.DeserializeObject<List<FilmResponse>>(json.Result);
                client.Dispose();
                return lst;
            }

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
