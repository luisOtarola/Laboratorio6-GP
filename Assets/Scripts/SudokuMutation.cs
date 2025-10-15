using System.Collections.Generic;
using UnityEngine;

public static class SudokuMutation
{
    // Mutación: elimina 3 números existentes y agrega 3 nuevos válidos
    public static int[,] MutateInsertAndRemove(int[,] parentBoard, SudokuGridManager gridManager)
    {
        int[,] board = (int[,])parentBoard.Clone();

        // 1️⃣ Eliminar 3 números aleatorios
        List<(int, int)> filledCells = new List<(int, int)>();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] != 0) filledCells.Add((r, c));

        for (int i = 0; i < 3 && filledCells.Count > 0; i++)
        {
            int index = Random.Range(0, filledCells.Count);
            var cell = filledCells[index];
            board[cell.Item1, cell.Item2] = 0;
            filledCells.RemoveAt(index);
        }

        // 2️⃣ Insertar 3 números aleatorios válidos
        List<(int, int)> emptyCells = new List<(int, int)>();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] == 0) emptyCells.Add((r, c));

        for (int i = 0; i < 3 && emptyCells.Count > 0; i++)
        {
            int index = Random.Range(0, emptyCells.Count);
            var cell = emptyCells[index];

            // Encontrar un número válido aleatorio
            List<int> validNumbers = new List<int>();
            for (int n = 1; n <= 9; n++)
                if (gridManager.IsValidPlacement(board, n, cell.Item1, cell.Item2))
                    validNumbers.Add(n);

            if (validNumbers.Count > 0)
            {
                int numIndex = Random.Range(0, validNumbers.Count);
                board[cell.Item1, cell.Item2] = validNumbers[numIndex];
            }

            emptyCells.RemoveAt(index);
        }

        return board;
    }
}
