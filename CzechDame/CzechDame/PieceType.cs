public enum PieceType
{
    none = '.',
    stoneBlack = 'b',
    stoneWhite = 'w',
    dameBlack = 'B',
    dameWhite = 'W'
}

public static class StaticMethods
{
    public static string ToUserFriendlyString(this PieceType piece)
    {
        switch (piece)
        {
            case PieceType.stoneWhite:
                return "bily kamen";
            case PieceType.stoneBlack:
                return "cerny kamen";
            case PieceType.dameWhite:
                return "bila dama";
            case PieceType.dameBlack:
                return "cerna dama";
            default:
                return "prazdne pole";
        }
    }
}