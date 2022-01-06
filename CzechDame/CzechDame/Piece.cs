using System;

public class Piece
{
    PieceType type;
    int x;
    int y;

    public Piece(PieceType type, int x, int y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }

    public PieceType getType()
    {
        return type;
    }

    public int getX()
    {
        return x;
    }

    public int getY()
    {
        return y;
    }

    public void setCoordinates(int x, int y)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7)
            throw new Exception("Zadane souradnice jsou za hranicemi hraci desky!");

        if (type != PieceType.none && isWhiteSquare(x, y))
            throw new Exception("Na bile pole nelze umistit zadnou figurku!");

        this.x = x;
        this.y = y;
    }

    public bool isWhite()
    {
        if (type == PieceType.stoneWhite || type == PieceType.dameWhite)
            return true;
        return false;
    }

    public bool isBlack()
    {
        if (type == PieceType.stoneBlack || type == PieceType.dameBlack)
            return true;
        return false;
    }

    public bool isNone()
    {
        if (type == PieceType.none)
            return true;
        return false;
    }

    public bool isStone()
    {
        if (type == PieceType.stoneWhite || type == PieceType.stoneBlack)
            return true;
        return false;
    }

    public bool isDame()
    {
        if (type == PieceType.dameWhite || type == PieceType.dameBlack)
            return true;
        return false;
    }

    public void promote()
    {
        if (type == PieceType.stoneWhite)
            type = PieceType.dameWhite;
        else
            type = PieceType.dameBlack;
    }



    // private methods
    private bool isWhiteSquare(int x, int y)
    {
        if ((x + y) % 2 == 0)
            return true;
        return false;
    }

}
