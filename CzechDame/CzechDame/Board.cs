using System;
using System.Collections.Generic;

public class Board
{
    Piece[,] board;
    List<Piece> whitePieces = new List<Piece>(12);
    List<Piece> blackPieces = new List<Piece>(12);


    public Board()
    {
        board = new Piece[8, 8];

        for (int i = 0; i < board.GetLength(0); i++)
            for (int j = 0; j < board.GetLength(1); j++)
            {
                Piece p = setupBoard(i, j);
                board[i, j] = p;
                if (p.isWhite())
                    whitePieces.Add(p);
                else if (p.isBlack())
                    blackPieces.Add(p);
            }
    }

    public Piece[,] getBoard()
    {
        return board;
    }

    public Piece getPiece(int x, int y)
    {
        if (isOutOfBoard(x) || isOutOfBoard(y))
            throw new Exception("Zadane souradnice jsou za hranicemi hraci desky!");

        if (isWhiteSquare(x, y))
            throw new Exception("Nelze ziskat figurku z bileho pole!");

        return board[x,y];
    }

    public bool setPiece(Piece p)
    {
        board[p.getX(), p.getY()] = p;
        return true;
    }

    public List<Piece> getWhitePieces()
    {
        return whitePieces;
    }

    public List<Piece> getBlackPieces()
    {
        return blackPieces;
    }

    public void removePiece(Piece p)
    {
        Piece newPiece = new Piece(PieceType.none, p.getX(), p.getY());

        if (p.isWhite())
            whitePieces.Remove(p);
        else if (p.isBlack())
            blackPieces.Remove(p);

        setPiece(newPiece);
    }

    public void printBoard()
    {
        int newLine = 0, index = 8;
        bool first = true;
        Console.Title = "Česká dáma";
        Console.WriteLine("\n\n");

        foreach (Piece p in board)
        {
            if (first)
            {
                Console.Write("\t\t" + index + " ");
                index--;
                first = false;
            }


            if (p.isWhite())
                Console.ForegroundColor = ConsoleColor.White;
            else if (p.isNone())
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else
                Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write((char)p.getType() + " ");
            newLine++;
            if (newLine % 8 == 0)
            {
                Console.WriteLine();
                first = true;
                Console.ResetColor();
            }
        }
        Console.ResetColor();
        Console.WriteLine("\t\t  A B C D E F G H\n\n");
    }


    // private methods
    private Piece setupBoard(int row, int col)
    {
        if (row <= 2 && (row + col) % 2 != 0)
            return new Piece(PieceType.stoneBlack, row, col);
        else if (row >= 5 && (row + col) % 2 != 0)
            return new Piece(PieceType.stoneWhite, row, col);
        else
            return new Piece(PieceType.none, row, col);
    }

    private bool isOutOfBoard(int n)
    {
        if (n < 0 || n > 7)
            return true;
        return false;
    }

    private bool isWhiteSquare(int x, int y)
    {
        if ((x + y) % 2 == 0)
            return true;
        return false;
    }

}
