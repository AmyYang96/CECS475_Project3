using System;
using System.Text;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.View;

namespace Cecs475.BoardGames.Chess.View
{
    /// <summary>
    /// A chess game view for string-based console input and output.
    /// </summary>
    public class ChessConsoleView : IConsoleView
    {
        private static char[] LABELS = { '.', 'P', 'R', 'N', 'B', 'Q', 'K' };

        // Public methods.
        public string BoardToString(ChessBoard board)
        {
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < ChessBoard.BoardSize; i++)
            {
                str.Append(8 - i);
                str.Append(" ");
                for (int j = 0; j < ChessBoard.BoardSize; j++)
                {
                    var space = board.GetPieceAtPosition(new BoardPosition(i, j));
                    if (space.PieceType == ChessPieceType.Empty)
                        str.Append(". ");
                    else if (space.Player == 1)
                        str.Append($"{LABELS[(int)space.PieceType]} ");
                    else
                        str.Append($"{char.ToLower(LABELS[(int)space.PieceType])} ");
                }
                str.AppendLine();
            }
            str.AppendLine("  a b c d e f g h");
            return str.ToString();
        }

        /// <summary>
        /// Converts the given ChessMove to a string representation in the form
        /// "(start, end)", where start and end are board positions in algebraic
        /// notation (e.g., "a5").
        /// 
        /// If this move is a pawn promotion move, the selected promotion piece 
        /// must also be in parentheses after the end position, as in 
        /// "(a7, a8, Queen)".
        /// </summary>
        public string MoveToString(ChessMove move)
        {
            if (move.PromoteType == ChessPieceType.Empty)
            {
                return PositionToString(move.StartPosition) + ", " + PositionToString(move.EndPosition);
            }
            else
            {
                return PositionToString(move.StartPosition) + ", " + PositionToString(move.EndPosition) + ", " + PieceToString(move.PromoteType);
            }
        }

        public string PieceToString(ChessPieceType pieceType)
        {
            string type = "";

            switch (pieceType)
            {
                case ChessPieceType.Bishop:
                    type = "Bishop";
                    break;
                case ChessPieceType.Knight:
                    type = "Knight";
                    break;
                case ChessPieceType.Rook:
                    type = "Rook";
                    break;
                case ChessPieceType.Queen:
                    type = "Queen";
                    break;
            }
            return type;

        }
        public string PlayerToString(int player)
        {
            return player == 1 ? "White" : "Black";
        }

        /// <summary>
        /// Converts a string representation of a move into a ChessMove object.
        /// Must work with any string representation created by MoveToString.
        /// </summary>
        public ChessMove ParseMove(string moveText)
        {
            // BoardPosition start = ParsePosition(moveText.Substring(0,2));
            // BoardPosition end = ParsePosition(moveText.Substring(4,2));
            moveText = moveText.Replace(" ", "");

            if (moveText.StartsWith("(") && moveText.EndsWith(")"))
            {
                moveText = moveText.Replace("(", "");
                moveText = moveText.Replace(")", "");
            }
            else if (moveText.StartsWith("("))
            {
                moveText = moveText.Replace("(", "");
            }
            string[] movesArr = moveText.Split(',');
            BoardPosition start = ParsePosition(movesArr[0]);
            BoardPosition end = ParsePosition(movesArr[1]);

            if (movesArr.Length > 2)
            {
                String moveType = movesArr[2];
                moveType.ToLower();
                ChessMove move = new ChessMove(start, end, ChessMoveType.Normal);

                switch (moveType)
                {
                    case "queen":
                        move = new ChessMove(start, end, ChessPieceType.Queen);
                        break;
                    case "rook":
                        move = new ChessMove(start, end, ChessPieceType.Rook);
                        break;
                    case "bishop":
                        move = new ChessMove(start, end, ChessPieceType.Bishop);
                        break;
                    case "knight":
                        move = new ChessMove(start, end, ChessPieceType.Knight);
                        break;
                    case "castlequeenside":
                        move = new ChessMove(start, end, ChessMoveType.CastleQueenSide);
                        break;
                    case "castlekingside":
                        move = new ChessMove(start, end, ChessMoveType.CastleKingSide);
                        break;
                    case "enpassant":
                        move = new ChessMove(start, end, ChessMoveType.EnPassant);
                        break;
                    case "Queen":
                        move = new ChessMove(start, end, ChessPieceType.Queen);
                        break;
                    case "Rook":
                        move = new ChessMove(start, end, ChessPieceType.Rook);
                        break;
                    case "Bishop":
                        move = new ChessMove(start, end, ChessPieceType.Bishop);
                        break;
                    case "Knight":
                        move = new ChessMove(start, end, ChessPieceType.Knight);
                        break;
                    case "Castlequeenside":
                        move = new ChessMove(start, end, ChessMoveType.CastleQueenSide);
                        break;
                    case "Castlekingside":
                        move = new ChessMove(start, end, ChessMoveType.CastleKingSide);
                        break;
                    case "Enpassant":
                        move = new ChessMove(start, end, ChessMoveType.EnPassant);
                        break;
                }
                return move;
            }
            else if (moveText.Equals("e1,g1") || moveText.Equals("e8,g8"))
            {
                return new ChessMove(start, end, ChessMoveType.CastleKingSide);
            }
            else if (moveText.Equals("e1,c1") || moveText.Equals("e8,c8"))
            {
                return new ChessMove(start, end, ChessMoveType.CastleQueenSide);
            }
            else
            {
                return new ChessMove(start, end, ChessMoveType.Normal);
            }

        }

        public static BoardPosition ParsePosition(string pos)
        {
            return new BoardPosition(8 - (pos[1] - '0'), pos[0] - 'a');
        }

        public static string PositionToString(BoardPosition pos)
        {
            return $"{(char)(pos.Col + 'a')}{8 - pos.Row}";
        }

        #region Explicit interface implementations
        // Explicit method implementations. Do not modify these.
        string IConsoleView.BoardToString(IGameBoard board)
        {
            return BoardToString(board as ChessBoard);
        }

        string IConsoleView.MoveToString(IGameMove move)
        {
            return MoveToString(move as ChessMove);
        }

        IGameMove IConsoleView.ParseMove(string moveText)
        {
            return ParseMove(moveText);
        }
        #endregion
    }
}
