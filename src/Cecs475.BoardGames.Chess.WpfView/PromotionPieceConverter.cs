using Cecs475.BoardGames.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Chess.WpfView
{
    public class PromotionPieceConverter : IMultiValueConverter
    {
        private static SolidColorBrush LIGHT_BRUSH = Brushes.Tan;
        private static SolidColorBrush DARK_BRUSH = Brushes.Brown;
        private static SolidColorBrush HIGHLIGHT_BRUSH = Brushes.LightBlue;
        private static SolidColorBrush SELECTED_BRUSH = Brushes.Red;
        private static SolidColorBrush POSSIBLE_BRUSH = Brushes.LightGreen;
        private static SolidColorBrush INCHECK_BRUSH = Brushes.Yellow;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Model.ChessPieceType type = (Model.ChessPieceType)values[0];
            int player = (int ) values[1];
            return null;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
