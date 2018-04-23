using Cecs475.BoardGames.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Chess.WpfView
{
    public class ChessSquareConverter : IMultiValueConverter
    {
        private static SolidColorBrush LIGHT_BRUSH = Brushes.Tan;
        private static SolidColorBrush DARK_BRUSH = Brushes.Brown;
        private static SolidColorBrush SELECTED_BRUSH = Brushes.Red;
        private static SolidColorBrush POSSIBLE_BRUSH = Brushes.LightGreen;
        private static SolidColorBrush INCHECK_BRUSH = Brushes.Yellow;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            BoardPosition pos = (BoardPosition)values[0];
            bool isHighLighted = (bool)values[1];
            bool isSelected = (bool)values[2];
            bool isPossibleEndPosition = (bool)values[3];
            bool isInCheck = (bool)values[4];
            
            if (isSelected) return SELECTED_BRUSH;
            else if (isHighLighted) return POSSIBLE_BRUSH;
            else if (isInCheck) return INCHECK_BRUSH;
            else if (isPossibleEndPosition) return POSSIBLE_BRUSH;
            else if ((pos.Row % 2 == 1 && pos.Col % 2 == 1) || (pos.Row % 2 == 0 && pos.Col % 2 == 0)) return LIGHT_BRUSH;
            else return DARK_BRUSH;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
