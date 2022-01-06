using System;
using System.Collections.Generic;

class Game
{
    Board board;
    List<Piece> mustTakePieces = new List<Piece>();
    bool whitesTurn;
    bool invalidMove;
    bool forgotTake;
    bool cantMove;
    string errorMessage;
    int whitesMoves;
    int blacksMoves;

    public Game()
    {
        board = new Board();
        whitesTurn = true;
        invalidMove = false;
        forgotTake = false;
        cantMove = true;
        whitesMoves = 0;
        blacksMoves = 0;
    }

    public Board getBoard()
    {
        return board;
    }

    public void play()
    {
        while (true)
        {
            checkHaveToJump();

            checkCanMove();

            if (endOfGame())
                break;

            turn();

            whitesTurn = !whitesTurn;
            cantMove = true;
            mustTakePieces = new List<Piece>();
        }
    }


    // private methods

    private void turn()
    {
        do
        {
            Console.Clear();
            board.printBoard();

            if (invalidMove || forgotTake)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMessage + '\n');
                Console.ResetColor();
                forgotTake = false;
            }

            int[] playerInput = printTurn();
            move(playerInput[0], playerInput[1], playerInput[2], playerInput[3]);
        } while (invalidMove);
    }

    private int[] printTurn()
    {
        if (whitesTurn)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Bily hrac je na tahu.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Cerny hrac je na tahu.");
        }
        Console.ResetColor();

        Console.WriteLine("Zadejte souradnice figurky, se kterou chcete hrat. Nejdrive pismeno, pak cislice.");
        ConsoleKeyInfo startChar = Console.ReadKey();
        ConsoleKeyInfo startInt = Console.ReadKey();
        Console.WriteLine("\nZadejte souradnice pole, kam chcete figurku presunout. Nejdrive pismeno, pak cislice.");
        ConsoleKeyInfo endChar = Console.ReadKey();
        ConsoleKeyInfo endInt = Console.ReadKey();

        char c = Char.ToUpper(startChar.KeyChar);
        char d = Char.ToUpper(endChar.KeyChar);

        Console.WriteLine();

        int[] results = new int[4];

        results[0] = 8 - (startInt.KeyChar - 48);
        results[1] = (int)c - 65;
        results[2] = 8 - (endInt.KeyChar - 48);
        results[3] = (int)d - 65;

        return results;
    }

    private void move(int startX, int startY, int endX, int endY)
    {
        try
        {
            Piece start = board.getPiece(startX, startY);
            Piece end = board.getPiece(endX, endY);

            Piece remove;
            remove = checkMove(start, end);

            makeMove(start, startX, startY, endX, endY, remove);

            if (mustTake())
                if (didntTake(board.getPiece(endX, endY), remove))
                {
                    forgotTake = true;
                    errorMessage = "Vynechan nutny skok, figurka " + mustTakePieces[0].getType().ToUserFriendlyString() + " byla odstranena!";
                    board.removePiece(mustTakePieces[0]);
                }

            updateMoves(start, remove);

            invalidMove = false;
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
            invalidMove = true;
        }
    }

    private void makeMove(Piece p, int startX, int startY, int endX, int endY, Piece remove)
    {
        p.setCoordinates(endX, endY);

        promoteIfPossible(p);

        board.setPiece(p);
        board.setPiece(new Piece(PieceType.none, startX, startY));

        if (remove.getType() != PieceType.none)
            board.removePiece(remove);
    }

    private Piece checkMove(Piece start, Piece end)
    {
        string text = "";

        if (start.isNone())
            text = "Na startovnim poli neni zadna figurka!";
        else if (!end.isNone())
            text = "Koncove pole je jiz obsazene figurkou!";
        else if (whitesTurn)
        {
            if (start.isBlack())
                text = "Bily hrac nemuze hrat s cernymi figurkami!";
        }
        else
        {
            if (start.isWhite())
                text = "Cerny hrac nemuze hrat s bilymi figurkami!";
        }

        if (text.Length > 0)
            throw new Exception(text);

        return checkMoveForPiece(start, end);
    }

    private Piece checkMoveForPiece(Piece start, Piece end)
    {
        if (start.isStone())
            return checkMoveStone(start, end);
        else
            return checkMoveDame(start, end);
    }

    private Piece checkMoveStone(Piece start, Piece end)
    {
        int changer = 1;
        if (start.isWhite())
            changer = -1;

        if (end.getX() == start.getX() + (1 * changer) && (end.getY() == start.getY() - 1 || end.getY() == start.getY() + 1))
            return new Piece(PieceType.none, 0, 0);
        else if (end.getX() == start.getX() + (2 * changer) && (end.getY() == start.getY() - 2 || end.getY() == start.getY() + 2))
        {
            int y;
            if (end.getY() < start.getY())
                y = end.getY() + 1;
            else
                y = end.getY() - 1;
            Piece beJumped = board.getPiece((end.getX() + start.getX()) / 2, y);

            if (start.isWhite() && beJumped.isBlack()) // white stone jumps over black piece
            {
                return beJumped;
            }
            else if (start.isBlack() && beJumped.isWhite()) // black stone jumps over white piece
            {
                return beJumped;
            }
        }

        throw new Exception("Neplatny pohyb s kamenem!");
    }

    private Piece checkMoveDame(Piece start, Piece end)
    {
        if (start.getX() + start.getY() != end.getX() + end.getY())
            if (start.getX() - start.getY() != end.getX() - end.getY())
                throw new Exception("Dama se muze pohybovat pouze diagonalne!");

        return checkDiogonal(start, end);
    }

    private Piece checkDiogonal(Piece start, Piece end)
    {
        bool jumped = false;
        Piece jumpedPiece = new Piece(PieceType.none, 0, 0);
        int iter = 0;
        int x = start.getX();
        int y = start.getY();

        if (start.getX() < end.getX())
        {
            if (y < end.getY())
            {
                while (iter == 0)
                {
                    x++;
                    y++;

                    iter = checkDiagonalBody(jumped, start, board.getPiece(x, y), end);

                    if (iter == 2)
                    {
                        jumpedPiece = board.getPiece(x, y);
                        iter = 0;
                        jumped = true;
                    }
                    else if (iter == 3)
                        return jumpedPiece;
                }
            }
            else
            {
                while (iter == 0)
                {
                    x++;
                    y--;

                    iter = checkDiagonalBody(jumped, start, board.getPiece(x, y), end);

                    if (iter == 2)
                    {
                        jumpedPiece = board.getPiece(x, y);
                        iter = 0;
                        jumped = true;
                    }
                    else if (iter == 3)
                        return jumpedPiece;
                }
            }
        }
        else
        {
            if (start.getY() < end.getY())
            {
                while (iter == 0)
                {
                    x--;
                    y++;

                    iter = checkDiagonalBody(jumped, start, board.getPiece(x, y), end);

                    if (iter == 2)
                    {
                        jumpedPiece = board.getPiece(x, y);
                        iter = 0;
                        jumped = true;
                    }
                    else if (iter == 3)
                        return jumpedPiece;
                }
            }
            else
            {
                while (iter == 0)
                {
                    x--;
                    y--;

                    iter = checkDiagonalBody(jumped, start, board.getPiece(x, y), end);

                    if (iter == 2)
                    {
                        jumpedPiece = board.getPiece(x, y);
                        iter = 0;
                        jumped = true;
                    }
                    else if (iter == 3)
                        return jumpedPiece;
                }
            }
        }
        return new Piece(PieceType.none, 0, 0);
    }

    private int checkDiagonalBody(bool jumped, Piece start, Piece current, Piece end)
    {
        if (jumped)
            if (current == end)
                return 3;
            else if (current.isNone())
                return 0;
            else
                throw new Exception("Dama nemuze preskakovat vice figurek na jeden skok!");

        if (current.getX() == end.getX())
            return 1;

        if (start.isWhite() && current.isWhite())
            throw new Exception("Bila dama nemuze skakat pres bile figurky!");
        else if (start.isBlack() && current.isBlack())
            throw new Exception("Cerna dama nemuze skakat pres cerne figurky!");
        else if (start.isWhite() && current.isBlack())
            return 2;
        else if (start.isBlack() && current.isWhite())
            return 2;

        return 0;
    }

    private void promoteIfPossible(Piece p)
    {
        if ((p.isWhite() && p.isStone() && p.getX() == 0) || (p.isBlack() && p.isStone() && p.getX() == 7))
            p.promote();
    }

    private void updateMoves(Piece moved, Piece remove)
    {
        if (whitesTurn)
        {
            if (moved.isStone())
                whitesMoves = 0;
            else if (remove.getType() != PieceType.none)
                whitesMoves = 0;
            else
                whitesMoves++;
        }
        else
        {
            if (moved.isStone())
                blacksMoves = 0;
            else if (remove.getType() != PieceType.none)
                blacksMoves = 0;
            else
                blacksMoves++;
        }
    }

    private void checkHaveToJump()
    {
        if (whitesTurn)
        {
            foreach (Piece p in board.getWhitePieces())
                if (p.isDame())
                    checkHaveToJumpForDame(p);

            if (mustTake())
                return;

            foreach (Piece p in board.getWhitePieces())
                if (p.isStone())
                    checkHaveToJumpForStone(p);
        }
        else
        {
            foreach (Piece p in board.getBlackPieces())
                if (p.isDame())
                    checkHaveToJumpForDame(p);

            if (mustTake())
                return;

            foreach (Piece p in board.getBlackPieces())
                if (p.isStone())
                    checkHaveToJumpForStone(p);
        }
    }

    private void checkHaveToJumpForDame(Piece dame)
    {
        int x = dame.getX() + 1;
        int y = dame.getY() + 1;
        bool enemyFound = false;

        while (x <= 7 && y <= 7)
        {
            int res = checkHaveToJumpForDameBody(dame, x, y, enemyFound);

            if (res == 0)
                return;
            else if (res == 1)
                break;
            else if (res == 2)
                enemyFound = true;

            x++;
            y++;
        }

        x = dame.getX() + 1;
        y = dame.getY() - 1;
        enemyFound = false;

        while (x <= 7 && y >= 0)
        {
            int res = checkHaveToJumpForDameBody(dame, x, y, enemyFound);

            if (res == 0)
                return;
            else if (res == 1)
                break;
            else if (res == 2)
                enemyFound = true;

            x++;
            y--;
        }

        x = dame.getX() - 1;
        y = dame.getY() + 1;
        enemyFound = false;

        while (x >= 0 && y <= 7)
        {
            int res = checkHaveToJumpForDameBody(dame, x, y, enemyFound);

            if (res == 0)
                return;
            else if (res == 1)
                break;
            else if (res == 2)
                enemyFound = true;

            x--;
            y++;
        }

        x = dame.getX() - 1;
        y = dame.getY() - 1;
        enemyFound = false;

        while (x >= 0 && y >= 0)
        {
            int res = checkHaveToJumpForDameBody(dame, x, y, enemyFound);

            if (res == 0)
                return;
            else if (res == 1)
                break;
            else if (res == 2)
                enemyFound = true;

            x--;
            y--;
        }

    }

    private int checkHaveToJumpForDameBody(Piece dame, int x, int y, bool enemyFound)
    {
        Piece p = board.getPiece(x, y);

        if (enemyFound && p.isNone())
        {
            mustTakePieces.Add(dame);
            return 0;
        }
        else if (enemyFound)
            return 1;

        if (dame.isWhite() && p.isWhite())
            return 1;
        else if (dame.isWhite() && p.isBlack())
            return 2;
        else if (dame.isBlack() && p.isBlack())
            return 1;
        else if (dame.isBlack() && p.isWhite())
            return 2;

        return 3;
    }

    private void checkHaveToJumpForStone(Piece stone)
    {
        if (stone.isWhite())
        {
            if (stone.getX() <= 1)
                return;

            if (stone.getY() - 1 > 0)
            {
                Piece left = board.getPiece(stone.getX() - 1, stone.getY() - 1);
                if (left.isBlack())
                {
                    if (board.getPiece(left.getX() - 1, left.getY() - 1).isNone())
                    {
                        mustTakePieces.Add(stone);
                        return;
                    }
                }
            }
            if (stone.getY() + 1 < 7)
            {
                Piece right = board.getPiece(stone.getX() - 1, stone.getY() + 1);

                if (right.isBlack())
                {
                    if (board.getPiece(right.getX() - 1, right.getY() + 1).isNone())
                    {
                        mustTakePieces.Add(stone);
                        return;
                    }
                }
            }
        }
        else
        {
            if (stone.getX() >= 6)
                return;
            try
            {
                Piece left = board.getPiece(stone.getX() + 1, stone.getY() - 1);
                Piece right = board.getPiece(stone.getX() + 1, stone.getY() + 1);
                if (left.isWhite())
                {
                    if (board.getPiece(left.getX() + 1, left.getY() - 1).isNone())
                    {
                        mustTakePieces.Add(stone);
                        return;
                    }
                }
                if (right.isWhite())
                {
                    if (board.getPiece(right.getX() + 1, right.getY() + 1).isNone())
                    {
                        mustTakePieces.Add(stone);
                        return;
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }

    private bool mustTake()
    {
        if (mustTakePieces.Count > 0)
            return true;
        return false;
    }

    private bool didntTake(Piece moved, Piece remove)
    {
        bool took = false;
        if (remove.getType() != PieceType.none)
            took = true;

        foreach (Piece p in mustTakePieces)
            if (moved == p && took)
                return false;
        return true;
    }

    private void checkCanMove()
    {
        if (mustTake())
        {
            cantMove = false;
            return;
        }

        if (whitesTurn)
            foreach (Piece p in board.getWhitePieces())
            {
                if (cantMove)
                    checkCanMoveForPiece(p);
                else
                    return;
            }
        else
            foreach (Piece p in board.getBlackPieces())
            {
                if (cantMove)
                    checkCanMoveForPiece(p);
                else
                    return;
            }
    }

    private void checkCanMoveForPiece(Piece p)
    {
        if (p.isStone())
            checkCanMoveForStone(p);
        else
            checkCanMoveForDame(p);
    }

    private void checkCanMoveForStone(Piece stone)
    {
        if (stone.isWhite())
        {
            if (stone.getY() - 1 >= 0)
            {
                Piece left = board.getPiece(stone.getX() - 1, stone.getY() - 1);
                if (left.isNone())
                    cantMove = false;
            }
            if (stone.getY() + 1 <= 7)
            {
                Piece right = board.getPiece(stone.getX() - 1, stone.getY() + 1);
                if (right.isNone())
                    cantMove = false;
            }
        }
        else
        {
            if (stone.getY() - 1 >= 0)
            {
                Piece left = board.getPiece(stone.getX() + 1, stone.getY() - 1);
                if (left.isNone())
                    cantMove = false;
            }
            if (stone.getY() + 1 <= 7)
            {
                Piece right = board.getPiece(stone.getX() + 1, stone.getY() + 1);
                if (right.isNone())
                    cantMove = false;
            }
        }
    }

    private void checkCanMoveForDame(Piece dame)
    {
        if (dame.getX() - 1 >= 0 && dame.getY() - 1 >= 0)
        {
            Piece topLeft = board.getPiece(dame.getX() - 1, dame.getY() - 1);
            if (topLeft.isNone())
            {
                cantMove = false;
                return;
            }
        }
        if (dame.getX() - 1 >= 0 && dame.getY() + 1 <= 7)
        {
            Piece topRight = board.getPiece(dame.getX() - 1, dame.getY() + 1);
            if (topRight.isNone())
            {
                cantMove = false;
                return;
            }
        }
        if (dame.getX() + 1 <= 7 && dame.getY() - 1 >= 0)
        {
            Piece bottomLeft = board.getPiece(dame.getX() + 1, dame.getY() - 1);
            if (bottomLeft.isNone())
            {
                cantMove = false;
                return;
            }
        }
        if (dame.getX() + 1 <= 7 && dame.getY() + 1 <= 7)
        {
            Piece bottomRight = board.getPiece(dame.getX() + 1, dame.getY() + 1);
            if (bottomRight.isNone())
            {
                cantMove = false;
                return;
            }
        }
    }

    private bool endOfGame()
    {
        if (board.getBlackPieces().Count == 0)
            return printEndGame("Bily hrac vyhral, cernemu hraci dosly figurky. Gratulujeme!");
        else if (board.getWhitePieces().Count == 0)
            return printEndGame("Cerny hrac vyhral, bilemu hraci dosly figurky. Gratulujeme!");
        else if (whitesMoves == 15)
            return printEndGame("Cerny hrac vyhral, bily hrac zopakoval 15 tahu s damou a nic neskocil. Gratulujeme!");
        else if (blacksMoves == 15)
            return printEndGame("Bily hrac vyhral, cerny hrac zopakoval 15 tahu s damou a nic neskocil. Gratulujeme!");
        else if (cantMove)
        {
            if (whitesTurn)
                return printEndGame("Cerny hrac vyhral, bily hrac nemuze hnout se zadnou figurkou. Gratulujeme!");
            else
                return printEndGame("Bily hrac vyhral, cerny hrac nemuze hnout se zadnou figurkou. Gratulujeme!");
        }

        return false;
    }

    private bool printEndGame(string message)
    {
        Console.Clear();
        board.printBoard();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
        return true;
    }
}
