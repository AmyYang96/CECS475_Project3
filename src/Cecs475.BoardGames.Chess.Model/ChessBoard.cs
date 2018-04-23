using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model
{
    /// <summary>
    /// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
    /// by which player's pieces.
    /// </summary>
    public class ChessBoard : IGameBoard
    {

        private enum PieceValue
        {
            Pawn = 1,
            Knight = 3,
            Bishop = 3,
            Rook = 5,
            Queen = 9

        }
        #region Member fields.

        private int WhitePoints = 0, BlackPoints = 0;
        // The history of moves applied to the board.
        private List<ChessMove> mMoveHistory = new List<ChessMove>();

        public const int BoardSize = 8;

        // TODO: create bitboards for each player's pieces. Use the ulong type.
        private ulong mWhitePawns = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000;
        private ulong mWhiteRooks = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_10000001;
        private ulong mWhiteKnights = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000010;
        private ulong mWhiteBishops = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00100100;
        private ulong mWhiteQueen = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00010000;
        private ulong mWhiteKing = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000;

        private ulong mBlackPawns = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackRooks = 0b10000001_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackKnights = 0b01000010_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackBishops = 0b00100100_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackQueen = 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackKing = 0b00001000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;


        // TODO: Add a means of tracking miscellaneous board state, like captured pieces and the 50-move rule.

        private int mDrawCounter;

        // TODO: add a field for tracking the current player and the board advantage.		
        private int mCurrentPlayer = 1;

        #endregion

        #region Properties.
        // TODO: implement these properties.
        // You can choose to use auto properties, computed properties, or normal properties 
        // using a private field to back the property.

        // You can add set bodies if you think that is appropriate, as long as you justify
        // the access level (public, private).

        public bool IsFinished { get { return IsCheckmate || IsStalemate || IsDraw; } }
        public int CurrentPlayer { get { return mCurrentPlayer; } }

        public GameAdvantage CurrentAdvantage { get { return CalculateGameAdvantage(); } }

        public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

        // TODO: implement IsCheck, IsCheckmate, IsStalemate
        public bool IsCheck
        {
            get
            {
                if (KingInDanger && CanMove) return true;
                else return false;
            }

        }

        public bool IsCheckmate
        {
            get
            {
                if (KingInDanger && !CanMove) return true;
                else return false;
            }
        }

        public bool IsStalemate
        {
            get
            {
                if (!CanMove && !KingInDanger) return true;
                else return false;
            }
        }

        private bool KingInDanger
        {
            get
            {
                if (GetDirectionOfCheck(CurrentPlayer, GetKingPosition(CurrentPlayer)).Count > 0 ||
                    GetKnightCheckPositions(CurrentPlayer, GetKingPosition(CurrentPlayer)).Count > 0
                ) return true;
                else return false;
            }
        }

        private bool CanMove
        {
            get
            {
                if (GetPossibleMoves().Count() == 0) return false;
                else return true;
            }
        }

        private const int DRAW_TURNS = 100;
        public bool IsDraw
        {
            get { return mDrawCounter >= DRAW_TURNS; }
        }

        /// <summary>
        /// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move, and resets to 0
        /// for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
        /// </summary>
        public int DrawCounter
        {
            get { return mDrawCounter; }
        }
        #endregion


        private int ReturnValuePoint(ChessPieceType pieceType)
        {
            int value = 0;

            switch (pieceType)
            {
                case ChessPieceType.Pawn: value = (int)PieceValue.Pawn; break;
                case ChessPieceType.Bishop: value = (int)PieceValue.Bishop; break;
                case ChessPieceType.Knight: value = (int)PieceValue.Knight; break;
                case ChessPieceType.Rook: value = (int)PieceValue.Rook; break;
                case ChessPieceType.Queen: value = (int)PieceValue.Queen; break;
            }

            return value;
        }

        private GameAdvantage CalculateGameAdvantage()
        {

            if (WhitePoints > BlackPoints)
            {
                return new GameAdvantage(1, WhitePoints - BlackPoints);
            }
            else if (WhitePoints < BlackPoints)
            {
                return new GameAdvantage(2, BlackPoints - WhitePoints);
            }
            else
            {
                return new GameAdvantage(0, 0);
            }
        }

        private void SetNewBoardAdvantage()
        {
            foreach (BoardPosition position in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                if (GetPlayerAtPosition(position) == 1)
                {
                    WhitePoints += ReturnValuePoint(GetPieceAtPosition(position).PieceType);
                }
                else
                {
                    BlackPoints += ReturnValuePoint(GetPieceAtPosition(position).PieceType);
                }
            }
        }

        #region Public methods.


        public bool ForseeCheck(BoardPosition positionToMoveFrom)
        {
            bool causeCheck = false;

            ChessPiece pieceToMove = GetPieceAtPosition(positionToMoveFrom);
            RemovePieceAtPosition(positionToMoveFrom, pieceToMove);
            List<BoardPosition> b = (List<BoardPosition>)GetPositionsOfPiece(ChessPieceType.King, pieceToMove.Player);

            if (b.Count != 0)
            {
                if (GetDirectionOfCheck(pieceToMove.Player, GetKingPosition(pieceToMove.Player)).Count > 0)
                {
                    causeCheck = true;
                }
            }
            SetPieceAtPosition(positionToMoveFrom, pieceToMove);
            return causeCheck;

        }
        public IEnumerable<ChessMove> GetPossibleMoves()
        {
            List<ChessMove> possibleMoves = new List<ChessMove>();
            IEnumerable<BoardPosition> attackedPositions = new List<BoardPosition>();

            foreach (BoardPosition position in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                int player = CurrentPlayer;

                if (GetPieceAtPosition(position).Player != player)
                {
                    continue;
                }
                if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.Pawn))
                {
                    attackedPositions = GetPossiblePawnMoveEndPositions(player, position);
                }
                else if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.Knight))
                {
                    attackedPositions = GetKnightAttackedPositions(player, position);
                }
                else if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.King))
                {
                    attackedPositions = GetKingPossibleKingMoveEndPositions(player, position);
                }
                else if (!GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.Empty))
                {
                    attackedPositions = GetOtherAttackedPositions(player, position, true); //boolean for possible moves
                }
                else
                {
                    attackedPositions = new List<BoardPosition>();
                }
                possibleMoves.AddRange(AddPossibleMoves(player, position, attackedPositions));

            }

            possibleMoves = GetPromotionMoves(possibleMoves);
            possibleMoves = GetEnPassantMoves(possibleMoves);

            if (TwoKingsOnly || IsDraw) return new List<ChessMove>();
            else return possibleMoves;

        }

        private List<ChessMove> GetEnPassantMoves(List<ChessMove> possibleMoves)
        {
            if (GameHasStarted)
            {
                List<ChessMove> enPassantMoves = new List<ChessMove>();
                enPassantMoves.AddRange(GetEnPassantAttackMoves(CurrentPlayer));
                foreach (ChessMove m in enPassantMoves)
                {
                    if (possibleMoves.Contains(new ChessMove(m.StartPosition, m.EndPosition, ChessMoveType.Normal)))
                    {
                        possibleMoves.Remove(new ChessMove(m.StartPosition, m.EndPosition, ChessMoveType.Normal));
                    }
                }
                possibleMoves.AddRange(enPassantMoves);
            }
            return possibleMoves;
        }

        private bool TwoKingsOnly
        {
            get
            {
                if (
                    mBlackBishops == 0 && mBlackKnights == 0 && mBlackRooks == 0 && mBlackPawns == 0 && mBlackQueen == 0 &&
                    mWhiteBishops == 0 && mWhiteKnights == 0 && mWhiteRooks == 0 && mWhitePawns == 0 && mWhiteQueen == 0
                ) return true;
                else return false;
            }
        }

        private bool GameHasStarted
        {
            get
            {
                return MoveHistory.Count() > 0;
            }
        }

        private List<ChessMove> GetPromotionMoves(List<ChessMove> possibleMoves)
        {
            List<ChessMove> promotionMoves = new List<ChessMove>();
            foreach (ChessMove move in possibleMoves)
            {
                if (move.StartPosition.Row == 1 && move.Player == 1 &&
                GetPieceAtPosition(move.StartPosition).PieceType == ChessPieceType.Pawn)
                {//black's side
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Queen));
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Rook));
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Bishop));
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Knight));
                    //ossibleMoves.Remove(move);
                }
                else if (move.StartPosition.Row == 6 && move.Player == 2 &&
                GetPieceAtPosition(move.StartPosition).PieceType == ChessPieceType.Pawn)
                {//white's side
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Queen));
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Rook));
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Bishop));
                    promotionMoves.Add(new ChessMove(move.StartPosition, move.EndPosition, ChessPieceType.Knight));
                }
            }

            foreach (ChessMove m in promotionMoves)
            {
                if (possibleMoves.Contains(new ChessMove(m.StartPosition, m.EndPosition, ChessMoveType.Normal)))
                {
                    possibleMoves.Remove(new ChessMove(m.StartPosition, m.EndPosition, ChessMoveType.Normal));
                }
            }
            possibleMoves.AddRange(promotionMoves);
            return possibleMoves;
        }

        public string printBoard()
        {
            string str = "";
            int i = 0;
            foreach (BoardPosition position in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                ChessPieceType type = GetPieceAtPosition(position).PieceType;
                if (type == ChessPieceType.Pawn) str += "P ";
                else if (type == ChessPieceType.Bishop) str += "B ";
                else if (type == ChessPieceType.Rook) str += "R ";
                else if (type == ChessPieceType.Knight) str += "N ";
                else if (type == ChessPieceType.King) str += "K ";
                else if (type == ChessPieceType.Queen) str += "Q ";
                else if (type == ChessPieceType.Empty) str += "- ";
                else str += "X ";
                i++;
                if (i % 8 == 0)
                {
                    i = 0;
                    str += " || ";
                }

            }
            return str;
        }

        public Boolean CantMove(int player, BoardPosition position)
        {
            BoardPosition kingPosition = GetPositionsOfPiece(ChessPieceType.King, player).First();
            int opponent = (player == 1) ? 2 : 1;
            if (!PositionIsThreatened(position, opponent))
            {
                return true;
            }
            else if (!PositionIsThreatened(kingPosition, opponent))
            {
                return true;
            }
            else if (!BlockingDanger(position, kingPosition))
            {
                return true;
            }
            return false;
        }

        public Boolean BlockingDanger(BoardPosition blockingPosition, BoardPosition vulnerablePosition)
        {

            if (blockingPosition.Row == vulnerablePosition.Row)
            {
                for (int i = Math.Min(blockingPosition.Col, vulnerablePosition.Col); i < Math.Max(blockingPosition.Col, vulnerablePosition.Col); i++)
                {
                    if (!PositionIsEmpty(new BoardPosition(blockingPosition.Row, i)))
                    {
                        return false;
                    }
                }
            }
            else if (blockingPosition.Col == vulnerablePosition.Col)
            {
                for (int i = Math.Min(blockingPosition.Row, vulnerablePosition.Row); i < Math.Max(blockingPosition.Row, vulnerablePosition.Row); i++)
                {
                    if (!PositionIsEmpty(new BoardPosition(i, blockingPosition.Col)))
                    {
                        return false;
                    }
                }
            }
            else if (Math.Abs(blockingPosition.Col - vulnerablePosition.Col) == Math.Abs(blockingPosition.Row - vulnerablePosition.Row))
            {
                BoardDirection direction = new BoardDirection();
                if (vulnerablePosition.Col < blockingPosition.Col && vulnerablePosition.Row < blockingPosition.Row)
                {
                    direction = new BoardDirection(1, 1);
                }
                else if (vulnerablePosition.Col < blockingPosition.Col && vulnerablePosition.Row > blockingPosition.Row)
                {
                    direction = new BoardDirection(1, -1);
                }
                else if (vulnerablePosition.Col > blockingPosition.Col && vulnerablePosition.Row < blockingPosition.Row)
                {
                    direction = new BoardDirection(-1, 1);
                }
                else if (vulnerablePosition.Col > blockingPosition.Col && vulnerablePosition.Row > blockingPosition.Row)
                {
                    direction = new BoardDirection(-1, -1);
                }
                for (int i = 1; i < Math.Abs(blockingPosition.Col - vulnerablePosition.Col) - 1; i++)
                {
                    if (!PositionIsEmpty(new BoardPosition(vulnerablePosition.Col + i * direction.ColDelta, vulnerablePosition.Row + i * direction.RowDelta)))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public string GetPossibleMovesString()
        {
            string str = "";

            foreach (ChessMove move in GetPossibleMoves())
            {
                str += "start ";
                str += move.StartPosition;
                str += " end ";
                str += move.EndPosition;
                str += ", ";
            }

            return str;
        }

        public string GetDirectionFromKingString(int player, BoardPosition position)
        {
            BoardDirection dir = GetDirectionFromKing(player, position);
            return "(" + dir.RowDelta + ", " + dir.ColDelta + ")";

        }

        public BoardDirection GetDirectionFromKing(int player, BoardPosition position)
        {
            BoardPosition kingPosition = GetKingPosition(player);
            int rowDelta = position.Row - kingPosition.Row;
            int colDelta = position.Col - kingPosition.Col;

            sbyte row;
            sbyte col;

            if (rowDelta == 0)
            {
                row = 0;
                if (colDelta > 0) col = 1;
                else col = -1;
            }
            else if (colDelta == 0)
            {
                col = 0;
                if (rowDelta > 0) row = 1;
                else row = -1;
            }
            else if (Math.Abs(rowDelta) == Math.Abs(colDelta))
            {
                if (rowDelta > 0) row = 1;
                else row = -1;
                if (colDelta > 0) col = 1;
                else col = -1;
            }
            else
            {
                row = 0;
                col = 0;
            }

            return new BoardDirection(row, col);
        }

        public string GetPossiblePawnMoveEndPositionsString()
        {
            string str = "";
            List<ChessMove> possibleMoves = new List<ChessMove>();

            foreach (BoardPosition position in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                int player = mCurrentPlayer;
                List<BoardPosition> attackedPositions = new List<BoardPosition>();

                // if(CantMove(player, position)) {
                // 	continue;
                // }
                if (GetPieceAtPosition(position).Player != player)
                {
                    continue;
                }
                if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.Pawn))
                {
                    attackedPositions = GetPossiblePawnMoveEndPositions(player, position);
                }
                possibleMoves.AddRange(AddPossibleMoves(player, position, attackedPositions));
            }

            foreach (ChessMove move in possibleMoves)
            {
                str += "(";
                str += move.StartPosition;
                str += ", ";
                str += move.EndPosition;
                str += ") ... ";
            }

            return str;
        }

        public List<ChessMove> GetEnPassantAttackMoves(int byPlayer)
        {

            var lastMove = MoveHistory[MoveHistory.Count - 1];
            List<ChessMove> enPassantMoves = new List<ChessMove>();
            //check 2 moves
            if (GetPieceAtPosition(lastMove.EndPosition).PieceType == ChessPieceType.Pawn && HasMovedTwoSpaces(lastMove) == true)
            {

                var leftOfPawn = lastMove.EndPosition.Translate(0, -1);
                var rightOfPawn = lastMove.EndPosition.Translate(0, 1);
                var threateningPositionsForBlackKing = ThreateningPositionsForKing(GetKingPosition(2), 2);
                var threateningPositionsForWhiteKing = ThreateningPositionsForKing(GetKingPosition(1), 1);

                if (PositionInBounds(leftOfPawn) == true && GetPieceAtPosition(leftOfPawn).PieceType == ChessPieceType.Pawn &&
                GetPieceAtPosition(leftOfPawn).Player == GetOpponent(lastMove.Player))
                {//en passant
                    if (GetOpponent(lastMove.Player) == 1)
                    {//white en passant 
                        if (GetPieceAtPosition(lastMove.EndPosition.Translate(-1, 0)).PieceType == ChessPieceType.Empty
                            && ForseeCheck(leftOfPawn) == false)
                        {
                            enPassantMoves.Add(new ChessMove(leftOfPawn, lastMove.EndPosition.Translate(-1, 0), ChessMoveType.EnPassant));
                        }
                    }
                    else
                    {
                        if (GetPieceAtPosition(lastMove.EndPosition.Translate(1, 0)).PieceType == ChessPieceType.Empty
                            && ForseeCheck(leftOfPawn) == false)
                        {
                            enPassantMoves.Add(new ChessMove(leftOfPawn, lastMove.EndPosition.Translate(1, 0), ChessMoveType.EnPassant));
                        }
                    }
                }
                if (PositionInBounds(rightOfPawn) == true && GetPieceAtPosition(rightOfPawn).PieceType == ChessPieceType.Pawn &&
                GetPieceAtPosition(rightOfPawn).Player == GetOpponent(lastMove.Player))
                {//en passant from right
                    if (GetOpponent(lastMove.Player) == 1)
                    {//white en passant 
                        if (GetPieceAtPosition(lastMove.EndPosition.Translate(-1, 0)).PieceType == ChessPieceType.Empty
                            && ForseeCheck(rightOfPawn) == false)
                        {
                            enPassantMoves.Add(new ChessMove(rightOfPawn, lastMove.EndPosition.Translate(-1, 0), ChessMoveType.EnPassant));
                        }
                    }
                    else
                    {
                        if (GetPieceAtPosition(lastMove.EndPosition.Translate(1, 0)).PieceType == ChessPieceType.Empty
                            && ForseeCheck(rightOfPawn) == false)
                        {
                            enPassantMoves.Add(new ChessMove(rightOfPawn, lastMove.EndPosition.Translate(1, 0), ChessMoveType.EnPassant));
                        }
                    }
                }
            }
            return enPassantMoves;

        }

        public bool HasMovedTwoSpaces(ChessMove m)
        {
            if (m.StartPosition == m.EndPosition.Translate(-2, 0)
                || m.StartPosition == m.EndPosition.Translate(2, 0))
            {
                return true;
            }
            else return false;
        }
        public IEnumerable<BoardPosition> GetEnPassantAttackPositions(int byPlayer)
        {
            if (MoveHistory.Count() > 0)
            {
                var enPassantMoves = GetEnPassantAttackMoves(1);
                return enPassantMoves.Select(m => m.EndPosition);
            }
            else
            {
                return null;
            }
        }

        public List<BoardPosition> GetPossiblePawnMoveEndPositions(int byPlayer, BoardPosition position)
        {
            List<BoardPosition> possiblePawnEndPositions = new List<BoardPosition>();
            List<BoardPosition> attackedPawnPositions = new List<BoardPosition>();

            attackedPawnPositions = GetPawnAttackedPositions(byPlayer, position);
            foreach (BoardPosition boardPosition in attackedPawnPositions)
            {
                if (GetPlayerAtPosition(boardPosition).Equals(GetOpponent(byPlayer)))
                {
                    possiblePawnEndPositions.Add(boardPosition);
                }
            }
            if (byPlayer == 2)
            {
                if (position.Row == 1 &&
                    GetPieceAtPosition(new BoardPosition(position.Row + 2, position.Col)).PieceType.Equals(ChessPieceType.Empty) &&
                    GetPieceAtPosition(new BoardPosition(position.Row + 1, position.Col)).PieceType.Equals(ChessPieceType.Empty)
                )
                {
                    possiblePawnEndPositions.Add(new BoardPosition(position.Row + 2, position.Col));
                }
                if (GetPieceAtPosition(new BoardPosition(position.Row + 1, position.Col)).PieceType.Equals(ChessPieceType.Empty))
                {
                    possiblePawnEndPositions.Add(new BoardPosition(position.Row + 1, position.Col));
                }
            }
            else
            {
                if (position.Row == 6 &&
                    GetPieceAtPosition(new BoardPosition(position.Row - 2, position.Col)).PieceType.Equals(ChessPieceType.Empty) &&
                    GetPieceAtPosition(new BoardPosition(position.Row - 1, position.Col)).PieceType.Equals(ChessPieceType.Empty)
                )
                {
                    possiblePawnEndPositions.Add(new BoardPosition(position.Row - 2, position.Col));
                }
                if (GetPieceAtPosition(new BoardPosition(position.Row - 1, position.Col)).PieceType.Equals(ChessPieceType.Empty))
                {
                    possiblePawnEndPositions.Add(new BoardPosition(position.Row - 1, position.Col));
                }
            }
            return possiblePawnEndPositions;
        }

        public IEnumerable<BoardPosition> GetKingPossibleKingMoveEndPositions(int byPlayer, BoardPosition position)
        {
            IEnumerable<BoardPosition> possibleKingEndPositions = new List<BoardPosition>();
            IEnumerable<BoardPosition> impossibleKingEndPositions = new List<BoardPosition>();
            IEnumerable<BoardPosition> attackedKingPositions = new List<BoardPosition>();
            IEnumerable<BoardPosition> attackedOponentPositions = new List<BoardPosition>();

            attackedKingPositions = GetOtherAttackedPositions(byPlayer, position, true);
            attackedOponentPositions.Union(GetAttackedPositions((byPlayer == 1) ? 2 : 1));
            impossibleKingEndPositions = attackedKingPositions.Intersect(attackedOponentPositions);
            possibleKingEndPositions = attackedKingPositions.Except(impossibleKingEndPositions);

            return possibleKingEndPositions;

        }

        public BoardPosition GetKingPosition(int player)
        {
            return GetPositionsOfPiece(ChessPieceType.King, player).First();
        }

        public IEnumerable<ChessMove> AddPossibleMoves(int byPlayer, BoardPosition position, IEnumerable<BoardPosition> attackedPositions)
        {
            List<ChessMove> possibleMoves = new List<ChessMove>();

            foreach (BoardPosition attackedPosition in attackedPositions)
            {
                ChessMove move = new ChessMove(position, attackedPosition);
                move.Player = byPlayer;
                bool canAdd = false;

                if (GetDirectionOfCheck(mCurrentPlayer, GetKingPosition(mCurrentPlayer)).Count > 0 ||
                     GetKnightCheckPositions(mCurrentPlayer, GetKingPosition(mCurrentPlayer)).Count > 0)
                {
                    List<BoardDirection> directionsOfCheck = GetDirectionOfCheck(byPlayer, GetKingPosition(mCurrentPlayer));
                    BoardPosition kingPosition = GetPositionsOfPiece(ChessPieceType.King, byPlayer).First();
                    foreach (BoardDirection directionOfCheck in directionsOfCheck)
                    {
                        int col = move.EndPosition.Col - kingPosition.Col;
                        int row = move.EndPosition.Row - kingPosition.Row;
                        if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.King))
                        {
                            if (((directionOfCheck.RowDelta == 0 && row != 0) || (directionOfCheck.ColDelta == 0 && col != 0)))
                            {
                                canAdd = true;
                            }
                            if ((directionOfCheck.RowDelta != 0 && directionOfCheck.ColDelta != 0 && row != col))
                            {
                                canAdd = true;
                            }
                            if (GetDirectionOfCheck(mCurrentPlayer, move.EndPosition).Count == 0)
                            {
                                canAdd = true;
                            }
                        }
                        else
                        {
                            // if(((directionOfCheck.RowDelta == 0 && row == 0 ) || (directionOfCheck.ColDelta == 0 && col == 0))) {
                            // 	if((directionOfCheck.RowDelta > 0 && row <= kingPosition.Row) || (directionOfCheck.ColDelta > 0 && col <= kingPosition.Col)) {
                            // 		canAdd = true;
                            // 	}
                            // 	if((directionOfCheck.RowDelta < 0 && row <= kingPosition.Row) || (directionOfCheck.ColDelta < 0 && col <= kingPosition.Col)) {
                            // 		canAdd = true;
                            // 	}

                            // }
                            // if((directionOfCheck.RowDelta != 0 && directionOfCheck.ColDelta != 0 && Math.Abs(row) == Math.Abs(col))) {
                            // 	canAdd = true;
                            // } 

                            //if(directionOfCheck.RowDelta != 0 && directionOfCheck.ColDelta != 0) {
                            BoardDirection direction = GetDirectionFromKing(byPlayer, move.EndPosition);
                            BoardPosition threateningPiece = GetThreateningPiece(kingPosition, directionOfCheck, byPlayer);
                            if (direction.Equals(directionOfCheck))
                            {
                                if (GetDistance(kingPosition, move.EndPosition) < GetDistance(kingPosition, threateningPiece))
                                {
                                    canAdd = true;
                                }
                            }
                            //}

                            if (move.EndPosition.Equals(GetThreateningPiece(kingPosition, directionOfCheck, byPlayer)))
                            {
                                canAdd = true;
                            }


                        }


                    }

                    if (GetKnightCheckPositions(byPlayer, kingPosition).Count == 1)
                    {
                        foreach (BoardPosition knightPosition in GetPositionsOfPiece(ChessPieceType.Knight, GetOpponent(byPlayer)))
                        {
                            if (GetKnightAttackedPositions(GetOpponent(byPlayer), knightPosition).Contains(kingPosition))
                            {
                                if (move.EndPosition == knightPosition)
                                {
                                    canAdd = true;
                                    break;
                                }
                            }
                        }
                    }


                }
                else canAdd = true;

                if (GetPieceAtPosition(position).PieceType == ChessPieceType.King && GetDirectionOfCheck(mCurrentPlayer, move.EndPosition).Count > 0)
                {
                    canAdd = false;
                }

                if (GetPieceAtPosition(position).PieceType == ChessPieceType.King && GetKnightCheckPositions(byPlayer, move.EndPosition).Count > 0)
                {
                    canAdd = false;
                }

                // if(GetKnightCheckPositions(byPlayer, GetKingPosition(byPlayer)).Count != 0 ) {
                // 	if(! GetAvoidKnightCheckEndPositions(move, byPlayer).Contains(move.EndPosition)) {
                // 		canAdd = false;
                // 	}
                // }

                if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.King))
                {
                    if (GetDistance(move.EndPosition, GetKingPosition(GetOpponent(byPlayer))) <= Math.Sqrt(2))
                    {
                        canAdd = false;
                    }
                }
                else
                {//} if(GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.Pawn)) {
                    ChessPiece p = GetPieceAtPosition(position);
                    RemovePieceAtPosition(position, p);
                    SetPieceAtPosition(attackedPosition, p);
                    if (GetDirectionOfCheck(byPlayer, GetKingPosition(byPlayer)).Count > 0)
                    {
                        foreach (BoardDirection direction in GetDirectionOfCheck(byPlayer, GetKingPosition(byPlayer)))
                        {
                            if (!attackedPosition.Equals(GetThreateningPiece(GetKingPosition(byPlayer), direction, byPlayer)))
                            {
                                canAdd = false;
                            }
                        }
                    }
                    RemovePieceAtPosition(attackedPosition, p);
                    SetPieceAtPosition(position, p);
                }

                if (GetPieceAtPosition(move.EndPosition).Player == byPlayer) canAdd = false;


                if (canAdd) possibleMoves.Add(move);

            }


            //add possible castle moves
            bool kingSideCastle = false;
            bool queenSideCastle = false;
            if (GetPieceAtPosition(position).PieceType.Equals(ChessPieceType.King))
            {
                kingSideCastle = CanCastle(byPlayer)[0];
                queenSideCastle = CanCastle(byPlayer)[1];
            }

            if (kingSideCastle && !(GetDirectionOfCheck(mCurrentPlayer, GetKingPosition(mCurrentPlayer)).Count > 0 ||
                     GetKnightCheckPositions(mCurrentPlayer, GetKingPosition(mCurrentPlayer)).Count > 0))
            {
                if (byPlayer == 1)
                {
                    possibleMoves.Add(new ChessMove(GetKingPosition(byPlayer), new BoardPosition(7, 6), ChessMoveType.CastleKingSide));
                }
                else
                {
                    possibleMoves.Add(new ChessMove(GetKingPosition(byPlayer), new BoardPosition(0, 6), ChessMoveType.CastleKingSide));
                }
            }

            if (queenSideCastle && !(GetDirectionOfCheck(mCurrentPlayer, GetKingPosition(mCurrentPlayer)).Count > 0 ||
                     GetKnightCheckPositions(mCurrentPlayer, GetKingPosition(mCurrentPlayer)).Count > 0))
            {
                if (byPlayer == 1)
                {
                    possibleMoves.Add(new ChessMove(GetKingPosition(byPlayer), new BoardPosition(7, 2), ChessMoveType.CastleQueenSide));
                }
                else
                {
                    possibleMoves.Add(new ChessMove(GetKingPosition(byPlayer), new BoardPosition(0, 2), ChessMoveType.CastleQueenSide));
                }

            }

            return possibleMoves;
        }

        public BoardPosition GetThreateningPiece(BoardPosition kingPosition, BoardDirection direction, int player)
        {

            BoardPosition currentPosition = kingPosition;
            while (GetPlayerAtPosition(currentPosition) != GetOpponent(player))
            {
                currentPosition = new BoardPosition(currentPosition.Row + direction.RowDelta, currentPosition.Col + direction.ColDelta);
            }

            return currentPosition;
        }

        public List<BoardPosition> GetAvoidKnightCheckEndPositions(ChessMove move, int player)
        {
            List<BoardPosition> positions = new List<BoardPosition>();

            if (move.StartPosition.Equals(GetKingPosition(player)))
            {
                if (GetKnightAttackedPositions(player, move.EndPosition).Count == 0 &&
                    GetDirectionOfCheck(player, move.EndPosition).Count == 0
                )
                {
                    positions.Add(move.EndPosition);
                }
            }
            else
            {
                if (GetKnightAttackedPositions(player, GetKingPosition(player)).Count == 1)
                {
                    if (GetKnightAttackedPositions(player, GetKingPosition(player)).First().Equals(move.EndPosition))
                    {
                        positions.Add(move.EndPosition);
                    }
                }
            }

            return positions;
        }

        public List<BoardPosition> GetKnightCheckPositions(int player, BoardPosition kingPosition)
        {
            int opponent = GetOpponent(player);
            List<BoardPosition> knightCheckPositions = new List<BoardPosition>();
            IEnumerable<BoardPosition> knightAttackedPositions = new List<BoardPosition>();
            IEnumerable<BoardPosition> knightPositions = GetPositionsOfPiece(ChessPieceType.Knight, opponent);

            foreach (BoardPosition knightPosition in knightPositions)
            {
                knightAttackedPositions = GetKnightAttackedPositions(opponent, knightPosition);
                foreach (BoardPosition knightAttackedPosition in knightAttackedPositions)
                {
                    if (knightAttackedPosition == kingPosition)
                    {
                        knightCheckPositions.Add(knightPosition);
                    }
                }
            }

            return knightCheckPositions;
        }

        public BoardPosition GetKingStartPosition(int player)
        {
            if (player == 1)
            {
                return new BoardPosition(7, 4);
            }
            else
            {
                return new BoardPosition(0, 4);
            }
        }

        public List<Boolean> CanCastle(int byPlayer)
        {
            bool kingSideCastle = true;
            bool queenSideCastle = true;
            List<Boolean> castles = new List<Boolean>();

            if (!CheckHasMoved(GetKingStartPosition(byPlayer)))
            {
                foreach (BoardPosition rookPosition in GetPositionsOfPiece(ChessPieceType.Rook, byPlayer))
                {
                    if (
                        !(byPlayer == 1 && rookPosition.Row == 7 && rookPosition.Col == 0) &&
                        !(byPlayer == 1 && rookPosition.Row == 7 && rookPosition.Col == 7) &&
                        !(byPlayer == 2 && rookPosition.Row == 0 && rookPosition.Col == 0) &&
                        !(byPlayer == 2 && rookPosition.Row == 0 && rookPosition.Col == 7)

                    )
                    {
                        continue;
                    }
                    if (!CheckHasMoved(rookPosition))
                    {
                        if (rookPosition.Col < GetKingPosition(byPlayer).Col)
                        {
                            for (int i = 2; i < 4; i++)
                            {
                                if (
                                    !GetPieceAtPosition(new BoardPosition(GetKingPosition(byPlayer).Row, i)).PieceType.Equals(ChessPieceType.Empty) ||
                                    GetDirectionOfCheck(byPlayer, new BoardPosition(GetKingPosition(byPlayer).Row, i)).Count != 0 ||
                                    GetKnightCheckPositions(byPlayer, new BoardPosition(GetKingPosition(byPlayer).Row, i)).Count != 0
                                )
                                {
                                    queenSideCastle = false;
                                }
                            }
                        }
                        if (rookPosition.Col > GetKingPosition(byPlayer).Col)
                        {
                            for (int i = 5; i < 7; i++)
                            {
                                if (
                                    !GetPieceAtPosition(new BoardPosition(GetKingPosition(byPlayer).Row, i)).PieceType.Equals(ChessPieceType.Empty) ||
                                    GetDirectionOfCheck(byPlayer, new BoardPosition(GetKingPosition(byPlayer).Row, i)).Count != 0 ||
                                    GetKnightCheckPositions(byPlayer, new BoardPosition(GetKingPosition(byPlayer).Row, i)).Count != 0
                                )
                                {
                                    kingSideCastle = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rookPosition.Col < GetKingPosition(byPlayer).Col)
                        {
                            queenSideCastle = false;
                        }
                        if (rookPosition.Col > GetKingPosition(byPlayer).Col)
                        {
                            kingSideCastle = false;
                        }
                    }
                }
            }
            else
            {
                kingSideCastle = false;
                queenSideCastle = false;
            }

            if (byPlayer == 1)
            {
                if (!GetPieceAtPosition(new BoardPosition(7, 0)).PieceType.Equals(ChessPieceType.Rook))
                {
                    queenSideCastle = false;
                }
                if (!GetPieceAtPosition(new BoardPosition(7, 7)).PieceType.Equals(ChessPieceType.Rook))
                {
                    kingSideCastle = false;
                }
                if (GetDirectionOfCheck(byPlayer, new BoardPosition(7, 2)).Count != 0)
                {
                    queenSideCastle = false;
                }
                if (GetDirectionOfCheck(byPlayer, new BoardPosition(7, 6)).Count != 0)
                {
                    kingSideCastle = false;
                }
                if (GetKnightCheckPositions(byPlayer, new BoardPosition(7, 2)).Count != 0)
                {
                    queenSideCastle = false;
                }
                if (GetKnightCheckPositions(byPlayer, new BoardPosition(7, 6)).Count != 0)
                {
                    kingSideCastle = false;
                }
            }
            else if (byPlayer == 2)
            {
                if (!GetPieceAtPosition(new BoardPosition(0, 0)).PieceType.Equals(ChessPieceType.Rook))
                {
                    queenSideCastle = false;
                }
                if (!GetPieceAtPosition(new BoardPosition(0, 7)).PieceType.Equals(ChessPieceType.Rook))
                {
                    kingSideCastle = false;
                }
                if (GetDirectionOfCheck(byPlayer, new BoardPosition(0, 2)).Count != 0)
                {
                    queenSideCastle = false;
                }
                if (GetDirectionOfCheck(byPlayer, new BoardPosition(0, 6)).Count != 0)
                {
                    kingSideCastle = false;
                }
                if (GetKnightCheckPositions(byPlayer, new BoardPosition(0, 2)).Count != 0)
                {
                    queenSideCastle = false;
                }
                if (GetKnightCheckPositions(byPlayer, new BoardPosition(0, 6)).Count != 0)
                {
                    kingSideCastle = false;
                }
            }
            castles.Add(kingSideCastle);
            castles.Add(queenSideCastle);
            return castles;
        }

        public int GetOpponent(int player)
        {
            return (player == 1) ? 2 : 1;
        }

        public string GetDirectionOfCheckString(int player, BoardPosition kingPosition)
        {
            string str = "";

            foreach (BoardDirection pos in GetDirectionOfCheck(player, kingPosition))
            {
                str += "(";
                str += pos.RowDelta;
                str += ", ";
                str += pos.ColDelta;
                str += ") ... ";
            }

            return str;
        }

        public double GetDistance(BoardPosition a, BoardPosition b)
        {
            double row = a.Row - b.Row;
            double col = a.Col - b.Col;
            return Math.Sqrt(Math.Pow(row, 2) + Math.Pow(col, 2));
        }

        public List<BoardPosition> ThreateningPositionsForKing(BoardPosition kingPosition, int player)
        {
            List<BoardPosition> threateningPositions = new List<BoardPosition>();
            foreach (BoardDirection cardinalDirection in BoardDirection.CardinalDirections)
            {
                if (PositionInBounds(new BoardPosition(kingPosition.Row + cardinalDirection.RowDelta, kingPosition.Col + cardinalDirection.ColDelta)) == true)
                    threateningPositions.Add(new BoardPosition(kingPosition.Row + cardinalDirection.RowDelta, kingPosition.Col + cardinalDirection.ColDelta));
            }
            return threateningPositions;
        }

        public List<BoardDirection> GetDirectionOfCheck(int player, BoardPosition kingPosition)
        {
            List<BoardDirection> directionsOfCheck = new List<BoardDirection>();
            //BoardPosition kingPosition = GetPositionsOfPiece(ChessPieceType.King, player).First();
            IEnumerable<BoardDirection> cardinalDirections = BoardDirection.CardinalDirections;


            foreach (BoardDirection cardinalDirection in cardinalDirections)
            {
                int step = 1;
                BoardPosition currentPosition = kingPosition;
                currentPosition = new BoardPosition(currentPosition.Row + (cardinalDirection.RowDelta), currentPosition.Col + (cardinalDirection.ColDelta));
                bool found = false;

                while (PositionInBounds(currentPosition) && !found)
                {
                    ChessPiece pieceAtPosition = GetPieceAtPosition(currentPosition);
                    //if(pieceAtPosition.Player == player) goto endofloop; // is this breaking out of inner or outter loop?
                    if (GetPieceAtPosition(currentPosition).Player != player || (GetPieceAtPosition(currentPosition).PieceType.Equals(ChessPieceType.King) && GetPieceAtPosition(currentPosition).Player == player))
                    {

                        if (
                            pieceAtPosition.Player == GetOpponent(player) &&
                            cardinalDirection.RowDelta != 0 && cardinalDirection.ColDelta != 0
                        )
                        {
                            if (pieceAtPosition.PieceType.Equals(ChessPieceType.Bishop) ||
                                pieceAtPosition.PieceType.Equals(ChessPieceType.Queen)
                            )
                            {
                                directionsOfCheck.Add(cardinalDirection);
                            }
                            if (pieceAtPosition.PieceType.Equals(ChessPieceType.Pawn) && step == 1)
                            {
                                directionsOfCheck.Add(cardinalDirection);
                            }
                        }
                        if (
                            pieceAtPosition.Player == GetOpponent(player) &&
                            (cardinalDirection.RowDelta == 0 || cardinalDirection.ColDelta == 0)
                        )
                        {
                            if (pieceAtPosition.PieceType.Equals(ChessPieceType.Rook) ||
                                pieceAtPosition.PieceType.Equals(ChessPieceType.Queen)
                            )
                            {
                                directionsOfCheck.Add(cardinalDirection);
                            }
                        }
                        if (pieceAtPosition.Player == GetOpponent(player)) found = true;
                        step++;
                        currentPosition = new BoardPosition(currentPosition.Row + (cardinalDirection.RowDelta), currentPosition.Col + (cardinalDirection.ColDelta));

                    }
                    else
                    {
                        found = true;
                        //currentPosition = new BoardPosition(9, 9);
                    }
                }

            }
            return directionsOfCheck;
        }

        public void ApplyMove(ChessMove m)
        {
            // STRONG RECOMMENDATION: any mutation to the board state should be run
            // through the method SetPieceAtPosition.

            // if(MoveHistory.Count>0){
            // 	var enPassantMoves = GetEnPassantAttackMoves(1);
            // 	foreach(ChessMove move in enPassantMoves){
            // 		if(m.StartPosition==move.StartPosition && m.EndPosition==move.EndPosition){
            // 			break;
            // 		}
            // 	}
            // 	m = new ChessMove(m.StartPosition, m.EndPosition, ChessMoveType.EnPassant);
            // }
            if (GetPossibleMoves().Contains(m))
            {
                m.Player = CurrentPlayer;
                BoardPosition startingPosition = m.StartPosition;
                BoardPosition endingPosition = m.EndPosition;
                ChessPiece pieceToMove = GetPieceAtPosition(startingPosition);
                ChessMoveType moveType = m.MoveType;

                switch (moveType)
                {
                    case ChessMoveType.Normal:
                        if (GetPieceAtPosition(endingPosition).PieceType == ChessPieceType.Empty) //end square is empty
                        {
                            RemovePieceAtPosition(startingPosition, pieceToMove);
                            SetPieceAtPosition(endingPosition, pieceToMove);
                            if (pieceToMove.PieceType != ChessPieceType.Pawn)
                            {//reset counter if pawn is moved
                                mDrawCounter++;
                            }
                            else
                            {
                                mDrawCounter = 0;
                            }
                        }
                        else
                        {//capture
                            m = CapturePiece(m);
                        }
                        break;
                    case ChessMoveType.EnPassant:
                        m = CapturePiece(m);
                        break;
                    case ChessMoveType.PawnPromote:

                        if (GetPieceAtPosition(endingPosition).PieceType != ChessPieceType.Empty)
                        { //if there's a capture

                            m = CapturePiece(m);
                        }

                        if (GetPieceAtPosition(startingPosition).PieceType != ChessPieceType.Empty)
                        {
                            RemovePieceAtPosition(startingPosition, GetPieceAtPosition(startingPosition));
                        }
                        RemovePieceAtPosition(endingPosition, GetPieceAtPosition(endingPosition));
                        SetPieceAtPosition(endingPosition, new ChessPiece(m.PromoteType, m.Player));
                        if (CurrentPlayer == 1)
                        {
                            WhitePoints += (ReturnValuePoint(m.PromoteType) - 1);
                        }
                        else
                        {
                            BlackPoints += (ReturnValuePoint(m.PromoteType) - 1);
                        }
                        break;
                    case ChessMoveType.CastleKingSide:
                        if (GetPieceAtPosition(startingPosition.Translate(0, 3)).PieceType == ChessPieceType.Rook
                            && GetPieceAtPosition(startingPosition.Translate(0, 1)).PieceType == ChessPieceType.Empty
                            && GetPieceAtPosition(startingPosition.Translate(0, 2)).PieceType == ChessPieceType.Empty)
                        {

                            var rook = GetPieceAtPosition(startingPosition.Translate(0, 3));
                            RemovePieceAtPosition(startingPosition.Translate(0, 3), rook);//remove rook from its pos
                            SetPieceAtPosition(startingPosition.Translate(0, 1), rook);

                            //move king
                            RemovePieceAtPosition(startingPosition, pieceToMove);
                            SetPieceAtPosition(endingPosition, pieceToMove);
                        }
                        break;
                    case ChessMoveType.CastleQueenSide:
                        if (GetPieceAtPosition(startingPosition.Translate(0, -4)).PieceType == ChessPieceType.Rook
                            && GetPieceAtPosition(startingPosition.Translate(0, -1)).PieceType == ChessPieceType.Empty
                            && GetPieceAtPosition(startingPosition.Translate(0, -2)).PieceType == ChessPieceType.Empty
                            && GetPieceAtPosition(startingPosition.Translate(0, -3)).PieceType == ChessPieceType.Empty)
                        {

                            var rook = GetPieceAtPosition(startingPosition.Translate(0, -4));
                            RemovePieceAtPosition(startingPosition.Translate(0, -4), rook);//remove rook from its pos
                            SetPieceAtPosition(startingPosition.Translate(0, -1), rook);

                            //move king
                            RemovePieceAtPosition(startingPosition, pieceToMove);
                            SetPieceAtPosition(endingPosition, pieceToMove);

                        }
                        break;
                }

                if (moveType == ChessMoveType.PawnPromote) mDrawCounter = 0;
                m.DrawCount = mDrawCounter;
                mMoveHistory.Add(m);
                if (mCurrentPlayer == 1)
                {
                    mCurrentPlayer = 2;
                }
                else
                {
                    mCurrentPlayer = 1;
                }
            }
            else
            {
                throw new ArgumentException("Invalid move!");
            }
        }

        public bool CheckHasMoved(BoardPosition startingPosition)
        {
            foreach (ChessMove move in mMoveHistory)
            {
                if (move.StartPosition.Equals(startingPosition))
                {
                    return true;
                }
            }
            return false;
        }


        private ChessMove CapturePiece(ChessMove m)
        {
            BoardPosition startingPosition = m.StartPosition;
            BoardPosition endingPosition = m.EndPosition;
            ChessPiece pieceToMove = GetPieceAtPosition(startingPosition);
            ChessPiece pieceToCapture = GetPieceAtPosition(endingPosition);

            if (m.MoveType == ChessMoveType.EnPassant)
            {
                if (CurrentPlayer == 1)
                {//white
                    pieceToCapture = GetPieceAtPosition(endingPosition.Translate(1, 0));//enemy pawn is down
                    RemovePieceAtPosition(endingPosition.Translate(1, 0), pieceToCapture);
                }
                else
                {
                    pieceToCapture = GetPieceAtPosition(endingPosition.Translate(-1, 0));//enemy pawn is  up
                    RemovePieceAtPosition(endingPosition.Translate(-1, 0), pieceToCapture);
                }
            }
            m.IsCapturing = true;
            m.PieceCaptured = pieceToCapture;
            mDrawCounter = 0;//reset

            RemovePieceAtPosition(endingPosition, pieceToCapture);//capture;			
            RemovePieceAtPosition(startingPosition, pieceToMove);

            SetPieceAtPosition(endingPosition, pieceToMove);
            if (m.Player == 1)
            {//white
                WhitePoints += ReturnValuePoint(pieceToCapture.PieceType);
                //BlackPoints -= ReturnValuePoint(pieceToCapture.PieceType);
            }
            else
            {
                //WhitePoints -= ReturnValuePoint(pieceToCapture.PieceType);
                BlackPoints += ReturnValuePoint(pieceToCapture.PieceType);
            }
            return m;
        }

        private void UndoLastMoveCapture(ChessMove moveToUndo)
        {
            BoardPosition startingPosition = moveToUndo.StartPosition;
            BoardPosition endingPosition = moveToUndo.EndPosition;
            ChessPiece pieceToMove = GetPieceAtPosition(endingPosition);
            ChessPiece capturedPiece = moveToUndo.PieceCaptured;

            //remove piece at current pos and move back
            RemovePieceAtPosition(endingPosition, pieceToMove);
            SetPieceAtPosition(startingPosition, pieceToMove);

            //put back capture piece

            if (moveToUndo.MoveType == ChessMoveType.EnPassant)
            {
                if (moveToUndo.Player == 1)
                {//white
                    SetPieceAtPosition(endingPosition.Translate(1, 0), capturedPiece);
                }
                else
                {//black
                    SetPieceAtPosition(endingPosition.Translate(-1, 0), capturedPiece);
                }
            }
            else
            {//undo regular capture
                SetPieceAtPosition(endingPosition, capturedPiece);
            }
            //undo draw count
            if (mMoveHistory.Count > 0)
            {
                mDrawCounter = mMoveHistory[mMoveHistory.Count - 1].DrawCount;//look back 
            }
            //undo advantange
            if (moveToUndo.Player == 1)
            {
                WhitePoints -= ReturnValuePoint(capturedPiece.PieceType);
                //BlackPoints =+ ReturnValuePoint(capturedPiece.PieceType);
            }
            else
            {
                //WhitePoints =+ ReturnValuePoint(capturedPiece.PieceType);
                BlackPoints -= ReturnValuePoint(capturedPiece.PieceType);
            }
        }
        public void UndoLastMove()
        {
            if (mMoveHistory.Count > 0 && mMoveHistory[mMoveHistory.Count - 1].Player != CurrentPlayer)
            {
                ChessMove moveToUndo = mMoveHistory[mMoveHistory.Count - 1];
                mMoveHistory.RemoveAt(mMoveHistory.Count - 1);//remove move
                BoardPosition startingPosition = moveToUndo.StartPosition;
                BoardPosition endingPosition = moveToUndo.EndPosition;
                ChessPiece pieceToMove = GetPieceAtPosition(endingPosition);
                bool captured = moveToUndo.IsCapturing;

                switch (moveToUndo.MoveType)
                {
                    case ChessMoveType.Normal:
                        if (captured == false) //not capture anything
                        {
                            RemovePieceAtPosition(endingPosition, pieceToMove);
                            SetPieceAtPosition(startingPosition, pieceToMove);
                            if (pieceToMove.PieceType == ChessPieceType.Pawn)
                            {//reset counter if pawn is moved back
                                if (mMoveHistory.Count > 0)
                                {
                                    mDrawCounter = mMoveHistory[mMoveHistory.Count - 1].DrawCount;//look back 
                                }
                            }
                            else mDrawCounter--;
                        }
                        else
                        {//capture
                            UndoLastMoveCapture(moveToUndo);
                        }
                        break;

                    case ChessMoveType.EnPassant:
                        UndoLastMoveCapture(moveToUndo);
                        break;

                    case ChessMoveType.PawnPromote:
                        RemovePieceAtPosition(endingPosition, GetPieceAtPosition(endingPosition));
                        SetPieceAtPosition(startingPosition, new ChessPiece(ChessPieceType.Pawn, pieceToMove.Player));
                        if (moveToUndo.IsCapturing == true)
                        { //if there's a capture
                            SetPieceAtPosition(endingPosition, moveToUndo.PieceCaptured);

                            if (mMoveHistory.Count > 0)
                                mDrawCounter = mMoveHistory[mMoveHistory.Count - 1].DrawCount;
                            if (moveToUndo.Player == 1)
                            {
                                WhitePoints -= ReturnValuePoint(moveToUndo.PieceCaptured.PieceType);
                            }
                            else
                            {
                                BlackPoints -= ReturnValuePoint(moveToUndo.PieceCaptured.PieceType);
                            }
                        }
                        if (moveToUndo.Player == 1)
                        {
                            WhitePoints -= (ReturnValuePoint(moveToUndo.PromoteType) - 1);
                        }
                        else
                        {
                            BlackPoints -= (ReturnValuePoint(moveToUndo.PromoteType) - 1);
                        }
                        if (mMoveHistory.Count > 0) mDrawCounter = mMoveHistory[mMoveHistory.Count - 1].DrawCount;
                        break;

                    case ChessMoveType.CastleQueenSide:
                        //move king back
                        SetPieceAtPosition(startingPosition, pieceToMove);
                        RemovePieceAtPosition(endingPosition, pieceToMove);

                        //move rook back
                        var rookKingSide = GetPieceAtPosition(endingPosition.Translate(0, 1));
                        RemovePieceAtPosition(endingPosition.Translate(0, 1), rookKingSide);
                        SetPieceAtPosition(endingPosition.Translate(0, -2), rookKingSide);
                        break;
                    case ChessMoveType.CastleKingSide:
                        //move king back
                        SetPieceAtPosition(startingPosition, pieceToMove);
                        RemovePieceAtPosition(endingPosition, pieceToMove);

                        //move rook back
                        var rook = GetPieceAtPosition(endingPosition.Translate(0, -1));
                        RemovePieceAtPosition(endingPosition.Translate(0, -1), rook);
                        SetPieceAtPosition(endingPosition.Translate(0, 1), rook);
                        break;
                }

                if (CurrentPlayer == 1)
                {
                    mCurrentPlayer = 2;
                }
                else
                {
                    mCurrentPlayer = 1;
                }

            }
            else
            {
                throw new InvalidOperationException("No moves to be undone");
                throw new InvalidOperationException("Not your turn");
            }
        }

        /// <summary>
        /// Returns whatever chess piece is occupying the given position.
        /// </summary>
        public ChessPiece GetPieceAtPosition(BoardPosition position)
        {

            // Get the bit position corresponding to this BoardPosition object.
            int index = GetBitIndexForPosition(position);
            // Create a bitmask with a 1 in the bit position for the calculated index.
            ulong mask = 1UL << index;

            var whitePawn = mask & mWhitePawns;
            if (whitePawn != 0)
            {
                return new ChessPiece(ChessPieceType.Pawn, 1);
            }

            var whiteRook = mask & mWhiteRooks;
            if (whiteRook != 0)
            {
                return new ChessPiece(ChessPieceType.Rook, 1);
            }

            var whiteKnight = mask & mWhiteKnights;
            if (whiteKnight != 0)
            {
                return new ChessPiece(ChessPieceType.Knight, 1);
            }

            var whiteBishop = mask & mWhiteBishops;
            if (whiteBishop != 0)
            {
                return new ChessPiece(ChessPieceType.Bishop, 1);
            }

            var whiteQueen = mask & mWhiteQueen;
            if (whiteQueen != 0)
            {
                return new ChessPiece(ChessPieceType.Queen, 1);
            }

            var whiteKing = mask & mWhiteKing;
            if (whiteKing != 0)
            {
                return new ChessPiece(ChessPieceType.King, 1);
            }

            var blackPawn = mask & mBlackPawns;
            if (blackPawn != 0)
            {
                return new ChessPiece(ChessPieceType.Pawn, 2);
            }

            var blackRook = mask & mBlackRooks;
            if (blackRook != 0)
            {
                return new ChessPiece(ChessPieceType.Rook, 2);
            }

            var blackKnight = mask & mBlackKnights;
            if (blackKnight != 0)
            {
                return new ChessPiece(ChessPieceType.Knight, 2);
            }

            var blackBishop = mask & mBlackBishops;
            if (blackBishop != 0)
            {
                return new ChessPiece(ChessPieceType.Bishop, 2);
            }

            var blackQueen = mask & mBlackQueen;
            if (blackQueen != 0)
            {
                return new ChessPiece(ChessPieceType.Queen, 2);
            }

            var blackKing = mask & mBlackKing;
            if (blackKing != 0)
            {
                return new ChessPiece(ChessPieceType.King, 2);
            }

            return new ChessPiece(ChessPieceType.Empty, 0);
        }

        /// <summary>
        /// Returns the bit index corresponding to the given BoardPosition, with the LSB being index 0
        /// and the MSB being index 63.
        /// </summary>
        private static int GetBitIndexForPosition(BoardPosition boardPosition)
        {
            return (63 - (boardPosition.Row * 8 + boardPosition.Col));
        }

        /// <summary>
        /// Returns whatever player is occupying the given position.
        /// </summary>
        public int GetPlayerAtPosition(BoardPosition position)
        {
            // MAY HAVE TO CHANGE TO HANDLE -1 AND 1 INSTEAD OF 1 AND 2
            return GetPieceAtPosition(position).Player;
        }

        /// <summary>
        /// Returns true if the given position on the board is empty.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEmpty(BoardPosition pos)
        {
            if (GetPieceAtPosition(pos).PieceType == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the given position contains a piece that is the enemy of the given player.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEnemy(BoardPosition pos, int player)
        {
            if (GetPlayerAtPosition(pos) != player)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the given position is in the bounds of the board.
        /// </summary>
        public static bool PositionInBounds(BoardPosition pos)
        {
            const int MIN = 0;
            if (pos.Col >= MIN && pos.Col < BoardSize && pos.Row >= MIN && pos.Row < BoardSize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns all board positions where the given piece can be found.
        /// </summary>
        public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player)
        {
            List<BoardPosition> pieces = new List<BoardPosition>();
            // var positionsOfPiece = BoardPosition.GetRectangularPositions(BoardSize, BoardSize).Where(
            // 	position => piece == GetPieceAtPosition(position).PieceType && 
            // 	player == GetPieceAtPosition(position).Player);
            // return  positionsOfPiece;

            foreach (BoardPosition position in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                ChessPiece thisPosition = GetPieceAtPosition(position);
                if (thisPosition.PieceType == piece && thisPosition.Player == player)
                {
                    pieces.Add(position);
                }
            }

            return pieces;
        }

        /// <summary>
        /// Returns true if the given player's pieces are attacking the given position.
        /// </summary>
        public bool PositionIsThreatened(BoardPosition position, int byPlayer)
        {
            if (GetAttackedPositions(byPlayer).Contains(position))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string GetAttackedPositionsBlackString()
        {
            string str = "";
            foreach (BoardPosition bp in GetAttackedPositions(2))
            {
                str += "(";
                str += bp.Row;
                str += ", ";
                str += bp.Col;
                str += ") ... ";
            }
            return str;
        }

        public string GetAttackedPositionsWhiteString()
        {
            string str = "";
            foreach (BoardPosition bp in GetAttackedPositions(1))
            {
                str += "(";
                str += bp.Row;
                str += ", ";
                str += bp.Col;
                str += ") ... ";
            }
            return str;
        }

        public string GetAttackedWhiteKingPositions()
        {
            string str = "";

            foreach (BoardPosition move in GetOtherAttackedPositions(1, GetKingPosition(1), true))
            {
                str += "(";
                str += move.Row;
                str += ", ";
                str += move.Col;
                str += ") ... ";
            }

            return str;
        }
        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given player.
        /// </summary>
        public ISet<BoardPosition> GetAttackedPositions(int byPlayer)
        {
            var positions = new HashSet<BoardPosition>();

            foreach (BoardPosition position in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                if (GetPlayerAtPosition(position) != byPlayer)
                {
                    continue;
                }
                if (GetPieceAtPosition(position).PieceType == ChessPieceType.Pawn)
                {
                    positions.UnionWith(GetPawnAttackedPositions(byPlayer, position));
                }
                else if (GetPieceAtPosition(position).PieceType == ChessPieceType.Knight)
                {
                    positions.UnionWith(GetKnightAttackedPositions(byPlayer, position));
                }
                else if (GetPieceAtPosition(position).PieceType != ChessPieceType.Empty)
                {
                    positions.UnionWith(GetOtherAttackedPositions(byPlayer, position, false));
                }
            }

            return positions;
        }
        #endregion

        #region Private methods.

        private List<BoardPosition> GetPawnAttackedPositions(int byPlayer, BoardPosition position)
        {
            var possibleAttackedPositions = new List<BoardPosition>();
            var attackedPositions = new List<BoardPosition>();

            if (byPlayer == 2)
            {
                possibleAttackedPositions.Add(new BoardPosition(position.Row + 1, position.Col + 1));
                possibleAttackedPositions.Add(new BoardPosition(position.Row + 1, position.Col - 1));
            }
            else
            {
                possibleAttackedPositions.Add(new BoardPosition(position.Row - 1, position.Col + 1));
                possibleAttackedPositions.Add(new BoardPosition(position.Row - 1, position.Col - 1));
            }


            foreach (BoardPosition possibleAttackedPosition in possibleAttackedPositions)
            {
                if (PositionInBounds(possibleAttackedPosition) && GetPlayerAtPosition(possibleAttackedPosition) != byPlayer)
                {
                    attackedPositions.Add(possibleAttackedPosition);
                }
            }

            if (MoveHistory.Count() > 0)
                attackedPositions.AddRange(GetEnPassantAttackPositions(byPlayer));
            return attackedPositions;
        }

        private List<BoardPosition> GetKnightAttackedPositions(int byPlayer, BoardPosition position)
        {
            var possibleAttackedPositions = new List<BoardPosition>();
            var attackedPositions = new List<BoardPosition>();

            possibleAttackedPositions.Add(new BoardPosition(position.Row + 2, position.Col - 1));
            possibleAttackedPositions.Add(new BoardPosition(position.Row + 2, position.Col + 1));
            possibleAttackedPositions.Add(new BoardPosition(position.Row + 1, position.Col - 2));
            possibleAttackedPositions.Add(new BoardPosition(position.Row + 1, position.Col + 2));
            possibleAttackedPositions.Add(new BoardPosition(position.Row - 2, position.Col - 1));
            possibleAttackedPositions.Add(new BoardPosition(position.Row - 2, position.Col + 1));
            possibleAttackedPositions.Add(new BoardPosition(position.Row - 1, position.Col - 2));
            possibleAttackedPositions.Add(new BoardPosition(position.Row - 1, position.Col + 2));

            foreach (BoardPosition possibleAttackedPosition in possibleAttackedPositions)
            {
                if (PositionInBounds(possibleAttackedPosition) && GetPlayerAtPosition(possibleAttackedPosition) != byPlayer)
                {
                    attackedPositions.Add(possibleAttackedPosition);
                }
            }

            return attackedPositions;
        }

        private List<BoardPosition> GetOtherAttackedPositions(int byPlayer, BoardPosition position, Boolean isPossibleMoves)
        {

            int magnitude = 0; // 1 for king, 8 for others
            List<BoardDirection> directions = new List<BoardDirection>();

            if (GetPieceAtPosition(position).PieceType == ChessPieceType.King)
            {
                magnitude = 1;
                foreach (BoardDirection direction in BoardDirection.CardinalDirections)
                {
                    directions.Add(direction);
                }
            }
            else if (GetPieceAtPosition(position).PieceType == ChessPieceType.Queen)
            {
                magnitude = 8;
                foreach (BoardDirection direction in BoardDirection.CardinalDirections)
                {
                    directions.Add(direction);
                }
            }
            else if (GetPieceAtPosition(position).PieceType == ChessPieceType.Bishop)
            {
                magnitude = 8;
                directions.Add(new BoardDirection(1, -1));
                directions.Add(new BoardDirection(1, 1));
                directions.Add(new BoardDirection(-1, -1));
                directions.Add(new BoardDirection(-1, 1));
            }
            else if (GetPieceAtPosition(position).PieceType == ChessPieceType.Rook)
            {
                magnitude = 8;
                directions.Add(new BoardDirection(1, 0));
                directions.Add(new BoardDirection(-1, 0));
                directions.Add(new BoardDirection(0, -1));
                directions.Add(new BoardDirection(0, 1));
            }
            return GetOtherAttackedPositions(byPlayer, position, magnitude, directions, isPossibleMoves);
        }

        private List<BoardPosition> GetOtherAttackedPositions(int byPlayer, BoardPosition position, int magnitude, List<BoardDirection> directions, Boolean isPossibleMoves)
        {
            var possibleAttackedPositions = new List<BoardPosition>();
            var attackedPositions = new List<BoardPosition>();

            foreach (BoardDirection direction in directions)
            {
                for (int i = 1; i <= magnitude; i++)
                {
                    BoardPosition possibleAttackedPosition = new BoardPosition(position.Row + (direction.RowDelta * i), position.Col + (direction.ColDelta * i));
                    possibleAttackedPositions.Add(possibleAttackedPosition);
                    if (isPossibleMoves && GetPieceAtPosition(possibleAttackedPosition).PieceType != ChessPieceType.Empty)
                    {
                        break;
                    }
                    if (GetPlayerAtPosition(possibleAttackedPosition) == GetOpponent(byPlayer))
                    {
                        break;
                    }
                }
            }

            foreach (BoardPosition possibleAttackedPosition in possibleAttackedPositions)
            {
                if (PositionInBounds(possibleAttackedPosition) && GetPlayerAtPosition(possibleAttackedPosition) != byPlayer)
                {
                    attackedPositions.Add(possibleAttackedPosition);
                }
            }
            return attackedPositions;
        }

        /// <summary>
        /// Mutates the board state so that the given piece is at the given position.
        /// </summary>
        private void SetPieceAtPosition(BoardPosition position, ChessPiece piece)
        {
            int index = GetBitIndexForPosition(position);
            ulong mask = 1UL << index;

            if (piece.Player == 1)
            { //white
                switch (piece.PieceType)
                {
                    case ChessPieceType.Pawn:
                        mWhitePawns |= mask;
                        break;
                    case ChessPieceType.Rook:
                        mWhiteRooks |= mask;
                        break;
                    case ChessPieceType.Knight:
                        mWhiteKnights |= mask;
                        break;
                    case ChessPieceType.Bishop:
                        mWhiteBishops |= mask;
                        break;
                    case ChessPieceType.Queen:
                        mWhiteQueen |= mask;
                        break;
                    case ChessPieceType.King:
                        mWhiteKing |= mask;
                        break;
                }
            }
            else
            { //black

                // // Construct a bitmask for the bit position corresponding to the BoardPosition.
                // int index = GetBitIndexForPosition(position);
                // ulong mask = 1UL << index;

                // // To set a particular player at a given position, we must bitwise OR the mask
                // // into the player's bitboard, and then remove that mask from the other player's
                // // bitboard. 
                // if (player == 1) {
                // 	mBlackPieces |= mask;

                // 	// ANDing with the NOT of a bitmask wipes that bit from the bitboard.
                // 	mWhitePieces &= ~mask;
                // }
                // else if (player == 2) {
                // 	mWhitePieces |= mask;
                // 	mBlackPieces &= ~mask;
                // }
                // else {
                // 	mBlackPieces &= ~mask;
                // 	mWhitePieces &= ~mask;
                // }


                switch (piece.PieceType)
                {
                    case ChessPieceType.Pawn:
                        mBlackPawns |= mask;
                        break;
                    case ChessPieceType.Rook:
                        mBlackRooks |= mask;
                        break;
                    case ChessPieceType.Knight:
                        mBlackKnights |= mask;
                        break;
                    case ChessPieceType.Bishop:
                        mBlackBishops |= mask;
                        break;
                    case ChessPieceType.Queen:
                        mBlackQueen |= mask;
                        break;
                    case ChessPieceType.King:
                        mBlackKing |= mask;
                        break;
                }
            }
        }

        public void RemovePieceAtPosition(BoardPosition position, ChessPiece piece)
        {
            int index = GetBitIndexForPosition(position);
            ulong mask = 1UL << index;

            if (piece.Player == 1)
            { //white
                mWhitePawns &= ~mask;
                mWhiteRooks &= ~mask;
                mWhiteKnights &= ~mask;
                mWhiteBishops &= ~mask;
                mWhiteQueen &= ~mask;
                mWhiteKing &= ~mask;
            }
            else
            {
                mBlackPawns &= ~mask;
                mBlackRooks &= ~mask;
                mBlackKnights &= ~mask;
                mBlackBishops &= ~mask;
                mBlackQueen &= ~mask;
                mBlackKing &= ~mask;
            }

        }
        #endregion

        #region Explicit IGameBoard implementations.
        IEnumerable<IGameMove> IGameBoard.GetPossibleMoves()
        {
            return GetPossibleMoves();
        }
        void IGameBoard.ApplyMove(IGameMove m)
        {
            ApplyMove(m as ChessMove);
        }
        IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;
        #endregion

        // You may or may not need to add code to this constructor.
        public ChessBoard()
        {

        }

        public ChessBoard(bool emptyBoard)
        {
            if (emptyBoard)
            {
                mWhitePawns = 0;
                mWhiteBishops = 0;
                mWhiteKnights = 0;
                mWhiteRooks = 0;
                mWhiteQueen = 0;
                mWhiteKing = 0;

                mBlackPawns = 0;
                mBlackBishops = 0;
                mBlackKnights = 0;
                mBlackRooks = 0;
                mBlackQueen = 0;
                mBlackKing = 0;
            }
        }

        public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
            : this(true)
        {
            var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
            var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
            if (king1.Count() != 1 || king2.Count() != 1)
            {
                throw new ArgumentException("A chess board must have a single king for each player");
            }

            foreach (var position in BoardPosition.GetRectangularPositions(8, 8))
            {
                SetPieceAtPosition(position, ChessPiece.Empty);
            }

            int[] values = { 0, 0 };
            foreach (var pos in startingPositions)
            {
                SetPieceAtPosition(pos.Item1, pos.Item2);
                // TODO: you must calculate the overall advantage for this board, in terms of the pieces
                // that the board has started with. "pos.Item2" will give you the chess piece being placed
                // on this particular position.
            }
            mDrawCounter = 0;
            // WhitePoints=0;
            // BlackPoints=0;
            SetNewBoardAdvantage();
        }
    }
}
