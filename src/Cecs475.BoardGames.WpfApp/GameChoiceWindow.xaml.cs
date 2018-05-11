using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Cecs475.BoardGames.WpfApp {
	/// <summary>
	/// Interaction logic for GameChoiceWindow.xaml
	/// </summary>
	public partial class GameChoiceWindow : Window {
		public GameChoiceWindow() {
			InitializeComponent();
            Type gameFactoryType = typeof(IWpfGameFactory);
            string gamesDirectory = AppDomain.CurrentDomain.BaseDirectory +  "games\\";
            List<IWpfGameFactory> games = new List<IWpfGameFactory>();
            try
            {
                var dllFiles = Directory.EnumerateFiles(gamesDirectory, "*.dll", SearchOption.AllDirectories);

                foreach (string currentFile in dllFiles)
                {
                    //string loadSemantics = System.IO.Path.GetFileNameWithoutExtension(currentFile) + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=68e71c13048d452a";
                    //MessageBox.Show(System.IO.Path.GetFileNameWithoutExtension(currentFile));
                   // Assembly.Load(loadSemantics);
                   
                    Assembly.LoadFrom(currentFile);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            

            var gameTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => gameFactoryType
                .IsAssignableFrom(t) && t.IsClass);
            foreach (var gameType in gameTypes)
            {
                var gameConstr = gameType.GetConstructor(Type.EmptyTypes);
                var game = (IWpfGameFactory)gameConstr.Invoke(new object[0]);
                games.Add(game);
            }

            this.Resources.Add("GameTypes", games);

		}

        private void Button_Click(object sender, RoutedEventArgs e) {
			Button b = sender as Button;
			// Retrieve the game type bound to the button
			IWpfGameFactory gameType = b.DataContext as IWpfGameFactory;
            // Construct a GameWindow to play the game.
            var gameWindow = new GameWindow(gameType,
                mHumanBtn.IsChecked.Value ? NumberOfPlayers.Two : NumberOfPlayers.One)
            {
                Title = gameType.GameName
            };
            // When the GameWindow closes, we want to show this window again.
            gameWindow.Closed += GameWindow_Closed;

			// Show the GameWindow, hide the Choice window.
			gameWindow.Show();
			this.Hide();
		}

		private void GameWindow_Closed(object sender, EventArgs e) {
			this.Show();
		}
	}
}
