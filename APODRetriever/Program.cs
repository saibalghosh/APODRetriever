using System;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace APODRetriever
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Retrieving and storing today's APOD...");
            PushToApodDB(DateTime.Now.Date.ToString("yyyy-MM-dd"));
        }

        static void PushToApodDB(string date)
        {
            string html;
            dynamic ApodData = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format("https://api.nasa.gov/planetary/apod?date={0}&hd=true&api_key=DEMO_KEY", date));
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                ApodData = JsonConvert.DeserializeObject(html);
            
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ApodDbConnection"].ConnectionString))
                using (var command = new SqlCommand("StoreDailyApod", conn) { CommandType = CommandType.StoredProcedure })
                {
                    conn.Open();
                    command.Parameters.Add("@epoch", SqlDbType.Date).Value = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    command.Parameters.Add("@copyright", SqlDbType.NVarChar).Value = ApodData.copyright == null ? String.Empty : ApodData.copyright;
                    command.Parameters.Add("@explanation", SqlDbType.NVarChar).Value = ApodData.explanation == null ? String.Empty : ApodData.explanation;
                    command.Parameters.Add("@hdurl", SqlDbType.NVarChar).Value = ApodData.hdurl == null ? String.Empty : ApodData.hdurl;
                    command.Parameters.Add("@title", SqlDbType.NVarChar).Value = ApodData.title == null ? String.Empty : ApodData.title;
                    command.Parameters.Add("@url", SqlDbType.NVarChar).Value = ApodData.url == null ? String.Empty : ApodData.url;

                    command.ExecuteNonQuery();
                }
            }

            catch
            {
                Environment.Exit(1);
            }
        }
    }
}