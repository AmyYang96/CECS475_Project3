using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
 
namespace Cecs475.BoardGames.Chess.WpfView
{
    public class ChessPieceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            const int WHITE = 1, BLACK = 2;
            ChessPiece c = (ChessPiece) value;
            if (!c.PieceType.Equals(ChessPieceType.Empty))
            {
                string src = c.ToString().ToLower().Replace(" ", "_");
                Image i = new Image();
                    
                if(c.PieceType==ChessPieceType.Bishop && c.Player == WHITE)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_bishop.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.King && c.Player == WHITE)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_king.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Knight && c.Player == WHITE)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_knight.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Pawn && c.Player == WHITE)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_pawn.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Queen && c.Player == WHITE)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_queen.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Rook && c.Player == WHITE)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/white_rook.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Bishop && c.Player == BLACK)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_bishop.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.King && c.Player == BLACK)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_king.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Knight && c.Player == BLACK)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_knight.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Pawn && c.Player == BLACK)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_pawn.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Queen && c.Player == BLACK)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_queen.png", UriKind.Relative));
                }
                if (c.PieceType == ChessPieceType.Rook && c.Player == BLACK)
                {
                    i.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resources/black_rook.png", UriKind.Relative));
                }
                return i;
            }
                else return null;
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}