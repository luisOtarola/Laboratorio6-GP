using UnityEngine;

public class HillClimber
{
    private int gridSize = 9;
    private int[,] board;
    private bool[,] fixedCells;

    public HillClimber(int[,] initialBoard)
    {
        board = new int[gridSize, gridSize];
        fixedCells = new bool[gridSize, gridSize];

        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
            {
                board[r, c] = initialBoard[r, c];
                fixedCells[r, c] = initialBoard[r, c] != 0;
            }
    }

    // Evaluar fitness: número total de conflictos
    private int Fitness()
    {
        int conflicts = 0;

        // Filas
        for (int r = 0; r < gridSize; r++)
        {
            int[] count = new int[10];
            for (int c = 0; c < gridSize; c++) count[board[r, c]]++;
            for (int n = 1; n <= 9; n++) if (count[n] > 1) conflicts += count[n] - 1;
        }

        // Columnas
        for (int c = 0; c < gridSize; c++)
        {
            int[] count = new int[10];
            for (int r = 0; r < gridSize; r++) count[board[r, c]]++;
            for (int n = 1; n <= 9; n++) if (count[n] > 1) conflicts += count[n] - 1;
        }

        // Bloques 3x3
        for (int br = 0; br < gridSize; br += 3)
            for (int bc = 0; bc < gridSize; bc += 3)
            {
                int[] count = new int[10];
                for (int r = br; r < br + 3; r++)
                    for (int c = bc; c < bc + 3; c++) count[board[r, c]]++;
                for (int n = 1; n <= 9; n++) if (count[n] > 1) conflicts += count[n] - 1;
            }

        return conflicts;
    }

    // Inicializa celdas vacías con números aleatorios
    private void InitializeRandom()
    {
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                if (!fixedCells[r, c])
                    board[r, c] = Random.Range(1, 10);
    }

    // Intercambiar dos celdas no fijas para reducir conflictos
    private bool TryImprove()
    {
        bool improved = false;

        for (int br = 0; br < gridSize; br += 3)
            for (int bc = 0; bc < gridSize; bc += 3)
            {
                var freeCells = new System.Collections.Generic.List<(int,int)>();
                for (int r = br; r < br + 3; r++)
                    for (int c = bc; c < bc + 3; c++)
                        if (!fixedCells[r, c]) freeCells.Add((r,c));

                for (int i = 0; i < freeCells.Count; i++)
                    for (int j = i+1; j < freeCells.Count; j++)
                    {
                        var (r1,c1) = freeCells[i];
                        var (r2,c2) = freeCells[j];

                        int temp = board[r1,c1];
                        board[r1,c1] = board[r2,c2];
                        board[r2,c2] = temp;

                        if (Fitness() < Fitness())
                            improved = true;
                        else // revertir
                        {
                            temp = board[r1,c1];
                            board[r1,c1] = board[r2,c2];
                            board[r2,c2] = temp;
                        }
                    }
            }

        return improved;
    }

    public int[,] Solve()
    {
        InitializeRandom();

        bool improved;
        int maxIter = 1000; // evitar bucles infinitos
        int iter = 0;

        do
        {
            improved = TryImprove();
            iter++;
        } while (improved && iter < maxIter);

        return board;
    }
}
