using Cecs475.BoardGames.WpfView;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ChessView : UserControl, IWpfGameView
    {

        private bool mIsSelected;
        private ChessSquare mSelectedSquare;
        public ChessView()
        {
            mIsSelected = false;
           InitializeComponent();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;
            if (!mIsSelected && vm.IsPossibleStartPosition(square.Position))//add current player
            {
               square.IsHighlighted = true;
            }
            if(mIsSelected && vm.IsPossibleEndPosition(mSelectedSquare.Position, square.Position))
            {
                square.IsPossibleEndPosition = true;
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            square.IsHighlighted = false;
            square.IsPossibleEndPosition = false;
        }

        public ChessViewModel ChessViewModel => FindResource("vm") as ChessViewModel;

        public Control ViewControl => this;

        public IGameViewModel ViewModel => ChessViewModel;


        private async void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            ChessSquare possibleSelectedSquare;
            var vm = FindResource("vm") as ChessViewModel;
            mIsSelected = !mIsSelected; //toggling selected
            
            if (mIsSelected)
            {
                possibleSelectedSquare = square;
                
                square.IsSelected = true;
                mSelectedSquare = square;
                
            } else if (vm.IsPossibleEndPosition(mSelectedSquare.Position, square.Position))
            {
                if (mSelectedSquare.Piece.PieceType==Model.ChessPieceType.Pawn && (square.Position.Row == 0 || square.Position.Row == 7))//promotion
                {
                    var promotionWindow = new PromotionWindow(vm, mSelectedSquare.Position, square.Position);
                    promotionWindow.ShowDialog();
                }
                else
                {
                    await vm.ApplyMove(mSelectedSquare.Position, square.Position,Model.ChessPieceType.Empty);
                }

                square.IsHighlighted = true;
                mSelectedSquare.IsSelected = false;
            }
            else
            {
                mIsSelected = false;
                mSelectedSquare.IsSelected = false;
            }
        }
    }
}
