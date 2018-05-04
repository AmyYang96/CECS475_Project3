using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Cecs475.BoardGames.WpfView;
using Cecs475.BoardGames.ComputerOpponent;
using System;

using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;

namespace Cecs475.BoardGames.Chess.WpfView
{
    public class ChessSquare : INotifyPropertyChanged
    {
        private int mPlayer;
        public int Player
        {
            get { return mPlayer;  }
            set
            {
                if (value  != mPlayer)
                {
                    mPlayer = value;
                    OnPropertyChanged(nameof(Player));
                }
            }
        }

        public BoardPosition Position
        {
            get; set;
        }

        private bool mIsHighlighted;
        public bool IsHighlighted
        {
            get { return mIsHighlighted;  }
            set
            {
                if (value != mIsHighlighted)
                {
                    mIsHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted));
                }
            }
        }

        private bool mIsSelected;
        public bool IsSelected
        {
            get { return mIsSelected; }
            set
            {
                if (value != mIsSelected)
                {
                    mIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private bool mIsPossibleEndPosition;
        public bool IsPossibleEndPosition
        {
            get { return mIsPossibleEndPosition; }
            set
            {
                if (value != mIsPossibleEndPosition)
                {
                    mIsPossibleEndPosition = value;
                    OnPropertyChanged(nameof(IsPossibleEndPosition));
                }
            }
        }

        private bool mIsInCheck;
        public bool IsInCheck
        {
            get { return mIsInCheck; }
            set
            {
                if( value != mIsInCheck )
                {
                    mIsInCheck = value;
                    OnPropertyChanged(nameof(IsInCheck));
                }
            }
        }
        private ChessPiece mPiece;
        public ChessPiece Piece
        {
            get { return mPiece;  }
            set
            {
                if (!value.Equals(mPiece))
                {
                    mPiece = value;
                    OnPropertyChanged(nameof(Piece));
                }
            }
        }

        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    } 

    public class PromotionPiece:IComparable<PromotionPiece>
    {
        private ChessPieceType  mPieceType;
        private int mPieceValue;

        public PromotionPiece(ChessPieceType type, int value)
        {
            mPieceType = type;
            mPieceValue = value;
        }

        public ChessPieceType PieceType
        {
            get { return mPieceType; }
        }

        public int PieceValue
        {
            get { return mPieceValue; }
        }

        public int CompareTo(PromotionPiece other)
        {
            return this.PieceValue.CompareTo(other.PieceValue);
        }
    }
    public class ChessViewModel: INotifyPropertyChanged, IGameViewModel
    {
        private ChessBoard mBoard;
        private ObservableCollection<ChessSquare> mSquares;
        private ObservableCollection <PromotionPiece> mPromotionPieces;
        public event EventHandler GameFinished;
        public event PropertyChangedEventHandler PropertyChanged;
        private const int MAX_AI_DEPTH = 4;
        private IGameAi mGameAi = new MinimaxAi(MAX_AI_DEPTH);
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ChessViewModel()
        {
            mBoard = new ChessBoard();

            mSquares = new ObservableCollection<ChessSquare>(
                BoardPosition.GetRectangularPositions(8, 8)
                .Select(pos => new ChessSquare()
                {
                    Position = pos,
                    Player = mBoard.GetPlayerAtPosition(pos),
                    Piece = mBoard.GetPieceAtPosition(pos)
                })
             );

            PossibleMoves = mBoard.GetPossibleMoves();
            mPromotionPieces = new ObservableCollection<PromotionPiece>();
            mPromotionPieces.Add(new PromotionPiece(ChessPieceType.Bishop, 3));
            mPromotionPieces.Add(new PromotionPiece(ChessPieceType.Knight, 3));
            mPromotionPieces.Add(new PromotionPiece(ChessPieceType.Rook, 5));
            mPromotionPieces.Add(new PromotionPiece(ChessPieceType.Queen, 9));
            
        }
        public NumberOfPlayers Players { get; set; }
        public void ApplyMove(BoardPosition startPos, BoardPosition endPos, ChessPieceType pieceType)
        {
        
            var possMove = mBoard.GetPossibleMoves().Where(m => startPos == m.StartPosition && endPos == m.EndPosition);
            if (possMove.Count()==1)
            {
                mBoard.ApplyMove(mBoard.GetPossibleMoves().Where(m => startPos == m.StartPosition && endPos == m.EndPosition).Single());
            }
            else//PROMOTION
            {
                mBoard.ApplyMove(mBoard.GetPossibleMoves().Where(m => startPos == m.StartPosition && endPos == m.EndPosition && pieceType==m.PromoteType).Single());
            }


            if (Players == NumberOfPlayers.One && !mBoard.IsFinished)
            {
                var bestMove = mGameAi.FindBestMove(mBoard);
                if (bestMove != null)
                {
                    mBoard.ApplyMove(bestMove as ChessMove);
                }
            }
            RebindState();

            if (mBoard.IsFinished)
            {
                GameFinished?.Invoke(this, new EventArgs());
            }
        }

        private void RebindState()
        {
            // Rebind the possible moves, now that the board has changed.
            PossibleMoves = mBoard.GetPossibleMoves();

            // Update the collection of squares by examining the new board state.
            var newSquares = BoardPosition.GetRectangularPositions(8, 8);
            int i = 0;
            foreach (var pos in newSquares)
            {
                mSquares[i].Player = mBoard.GetPlayerAtPosition(pos);
                mSquares[i].Position = pos;
                mSquares[i].Piece = mBoard.GetPieceAtPosition(pos);
                if (pos == mBoard.GetKingPosition(CurrentPlayer) && mBoard.IsCheck) {
                    mSquares[i].IsInCheck = true;
                    
                }
                else mSquares[i].IsInCheck = false;
                i++;
            }
            OnPropertyChanged(nameof(BoardAdvantage));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));
        }

        public bool IsPossibleStartPosition(BoardPosition startPos)
        {
            return PossibleMoves.Any(m => m.StartPosition == startPos);
        } 
        public bool IsPossibleEndPosition(BoardPosition startPos, BoardPosition endPos)
        {
            return PossibleMoves.Any(m => m.StartPosition == startPos && m.EndPosition == endPos);
            
        }
        public ObservableCollection<ChessSquare> Squares
        {
            get { return mSquares; }
        }

        public ObservableCollection<PromotionPiece> Promotions
        {
            get { return mPromotionPieces; }
        }
        public IEnumerable<ChessMove> PossibleMoves
        {
            get; private set;
        }

        public GameAdvantage CurrentAdvantage
        {
            get { return mBoard.CurrentAdvantage; }
        }

        public int CurrentPlayer
        {
            get { return mBoard.CurrentPlayer;  }
        }

        public bool CanUndo
        {
            get
            {
                if (mBoard.MoveHistory.Count > 0) return true;
                else return false;
            }
        }

        public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;

        public void UndoMove()
        {
            mBoard.UndoLastMove();
            //PossibleMoves = new HashSet<BoardPosition>(mBoard.GetPossibleMoves().Select(m => m.EndPosition));
            RebindState();
        }

        public bool IsInCheck
        {
            get { return mBoard.IsCheck; }
        }

        public bool IsCheckmate
        {
            get { return mBoard.IsCheckmate; }
        }

        public bool IsStalemate
        {
            get { return mBoard.IsStalemate; }
        }

        
    }
}