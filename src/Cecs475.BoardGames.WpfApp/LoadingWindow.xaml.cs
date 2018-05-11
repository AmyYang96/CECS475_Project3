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
//using RestSharp;

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
            /*var client = new RestClient("https://cecs475-boardgames.herokuapp.com/");
            var request = new RestRequest("api/games", Method.GET);
            var task = client.ExecuteTaskAsync(request);
            var response = await task;

            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("No games found");
            } else
            {

            }*/
        }
    }
}
