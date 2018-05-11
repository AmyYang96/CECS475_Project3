using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Cecs475.BoardGames.WpfApp
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            LoadGames();
            
        }

        private async void LoadGames()
        {
            var client = new RestClient("https://cecs475-boardamges.herokuapp.com/");
            var request = new RestRequest("api/games", Method.GET);
            var task = client.ExecuteTaskAsync(request);
            var response = await task;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("No games found");
            }
            else
            {
                GameChoiceWindow gameChoiceWindow = new GameChoiceWindow();
                this.Close();
                gameChoiceWindow.Show();
                
                JArray games = JArray.Parse(response.Content);
                foreach(var game in games)
                {
                    WebClient webClient = new WebClient();
                    Uri uri = new Uri(game["Files"][0]["Url"].ToString());
                    string fileName = game["Files"][0]["FileName"].ToString();
                    await webClient.DownloadFileTaskAsync(uri, fileName);
                    uri = new Uri(game["Files"][1]["Url"].ToString());
                    fileName = game["Files"][1]["FileName"].ToString();
                    await webClient.DownloadFileTaskAsync(uri, fileName);
                }
               
            }
        }
    }
}
