using UnityEngine;
using System.Collections.Generic;

public class MiniHillClimber
{
    private int gridSize = 4; // Mini Sudoku 4x4
    private int[,] board;
    private bool[,] fixedCells;

    public MiniHillClimber(int[,] initialBoard)
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

    // Fitness = número total de conflictos
    private int Fitness()
    {
        int conflicts = 0;

        // Filas
        for (int r = 0; r < gridSize; r++)
        {
            int[] count = new int[gridSize + 1];
            for (int c = 0; c < gridSize; c++) count[board[r, c]]++;
            for (int n = 1; n <= gridSize; n++) if (count[n] > 1) conflicts += count[n] - 1;
        }

        // Columnas
        for (int c = 0; c < gridSize; c++)
        {
            int[] count = new int[gridSize + 1];
            for (int r = 0; r < gridSize; r++) count[board[r, c]]++;
            for (int n = 1; n <= gridSize; n++) if (count[n] > 1) conflicts += count[n] - 1;
        }

        // Bloques 2x2
        for (int br = 0; br < gridSize; br += 2)
            for (int bc = 0; bc < gridSize; bc += 2)
            {
                int[] count = new int[gridSize + 1];
                for (int r = br; r < br + 2; r++)
                    for (int c = bc; c < bc + 2; c++) count[board[r, c]]++;
                for (int n = 1; n <= gridSize; n++) if (count[n] > 1) conflicts += count[n] - 1;
            }

        return conflicts;
    }

    // Inicializa celdas vacías con números aleatorios
    private void InitializeRandom()
    {
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                if (!fixedCells[r, c])
                    board[r, c] = Random.Range(1, gridSize + 1);
    }

    // Intenta mejorar el tablero intercambiando números dentro de un bloque 2x2
    private bool TryImprove()
    {
        bool improved = false;

        for (int br = 0; br < gridSize; br += 2)
            for (int bc = 0; bc < gridSize; bc += 2)
            {
                List<(int, int)> freeCells = new List<(int, int)>();
                for (int r = br; r < br + 2; r++)
                    for (int c = bc; c < bc + 2; c++)
                        if (!fixedCells[r, c]) freeCells.Add((r, c));

                for (int i = 0; i < freeCells.Count; i++)
                    for (int j = i + 1; j < freeCells.Count; j++)
                    {
                        var (r1, c1) = freeCells[i];
                        var (r2, c2) = freeCells[j];

                        int temp = board[r1, c1];
                        board[r1, c1] = board[r2, c2];
                        board[r2, c2] = temp;

                        if (Fitness() < Fitness())
                            improved = true;
                        else
                        {
                            // revertir
                            board[r2, c2] = board[r1, c1];
                            board[r1, c1] = temp;
                        }
                    }
            }

        return improved;
    }

    // Resolver usando Hill Climbing
    public int[,] Solve()
    {
        InitializeRandom();

        bool improved;
        int maxIter = 500;
        int iter = 0;

        do
        {
            improved = TryImprove();
            iter++;
        } while (improved && iter < maxIter);

        return board;
    }
}
