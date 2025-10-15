using System.Collections.Generic;

public static class SudokuSolver
{
    // Cuenta celdas no resueltas por Naked Single
    public static int CountUnresolvedNakedSingle(int[,] board, SudokuGridManager gridManager)
    {
        int count = 0;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (board[r, c] == 0)
                {
                    List<int> candidates = new List<int>();
                    for (int n = 1; n <= 9; n++)
                        if (gridManager.IsValidPlacement(board, n, r, c))
                            candidates.Add(n);

                    if (candidates.Count != 1) count++;
                }
            }
        }
        return count;
    }

    // Similar se puede agregar Hidden Single si quieres más realismo
}
