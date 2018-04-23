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

namespace Cecs475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for PromotionWindow.xaml
    /// </summary>
    public partial class PromotionWindow : Window
    {
        private ChessViewModel viewModel;
        private BoardGames.Model.BoardPosition startPos, endPos;
        public PromotionWindow(ChessViewModel chessViewModel, BoardGames.Model.BoardPosition startPosition, BoardGames.Model.BoardPosition endPosition)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            viewModel = chessViewModel;
            startPos = startPosition;
            endPos = endPosition;

            if (chessViewModel.CurrentPlayer == 1)
            {
                blackBishop.Visibility = Visibility.Hidden;
                blackKnight.Visibility = Visibility.Hidden;
                blackRook.Visibility = Visibility.Hidden;
                blackQueen.Visibility = Visibility.Hidden;
            }
            else
            {
                whiteBishop.Visibility = Visibility.Hidden;
                whiteKnight.Visibility = Visibility.Hidden;
                whiteRook.Visibility = Visibility.Hidden;
                whiteQueen.Visibility = Visibility.Hidden;
            }


        }


        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            var image = sender as Image;
            if (border==borderBishop || image==blackBishop || image == whiteBishop)
                borderBishop.Background = Brushes.Aqua;

            if (border == borderKnight || image == blackKnight || image == whiteKnight)
                borderKnight.Background = Brushes.Aqua;

            if (border == borderRook || image == blackRook || image == whiteRook)
                borderRook.Background = Brushes.Aqua;

            if (border == borderQueen || image == blackQueen || image == whiteQueen)
                borderQueen.Background = Brushes.Aqua;


        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var image = sender as Image;

            if (border == borderBishop || image == blackBishop || image == whiteBishop)
            {
                viewModel.ApplyMove(startPos, endPos, Model.ChessPieceType.Bishop);
            }
            else if (border == borderRook || image == blackRook || image == whiteRook)
            {
                viewModel.ApplyMove(startPos, endPos, Model.ChessPieceType.Rook);
            }
            else if (border == borderKnight || image == blackKnight || image == whiteKnight)
            {
                viewModel.ApplyMove(startPos, endPos, Model.ChessPieceType.Knight);
            }
            else if (border == borderQueen || image == blackQueen || image == whiteQueen)
            {
                viewModel.ApplyMove(startPos, endPos, Model.ChessPieceType.Queen);
            }
            this.Close();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            var image = sender as Image;
            if (border == borderBishop )
                borderBishop.Background = Brushes.Tan;

            if (border == borderQueen)
                borderQueen.Background = Brushes.Tan;

            if (border == borderKnight)
                borderKnight.Background = Brushes.Tan;

            if (border == borderRook)
                borderRook.Background = Brushes.Tan;


        }
    }
}