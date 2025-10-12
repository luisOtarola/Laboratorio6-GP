using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
public class SudokuSolverHillClimbing
{
    private int gridSize = 9;
    private int[,] board;
    private Action<int, int, int> onCellChanged; // fila, columna, nuevo valor

    public SudokuSolverHillClimbing(int[,] initialBoard, Action<int, int, int> onCellChangedCallback = null)
    {
        gridSize = initialBoard.GetLength(0);
        board = (int[,])initialBoard.Clone();
        onCellChanged = onCellChangedCallback;
    }

    public int[,] Solve(int maxIterations = 10000)
    {
        List<(int r, int c)> filledCells = new List<(int, int)>();
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                if (board[r, c] != 0)
                    filledCells.Add((r, c));

        int fitness = CalculateConflicts(board);
        int iterations = 0;

        while (fitness > 0 && iterations < maxIterations)
        {
            iterations++;

            // Elegir una celda ocupada aleatoria
            var cell = filledCells[Random.Range(0, filledCells.Count)];
            int oldValue = board[cell.r, cell.c];

            // Probar cambiar el número a otro distinto
            int newValue = Random.Range(1, 10);
            while (newValue == oldValue)
                newValue = Random.Range(1, 10);

            board[cell.r, cell.c] = newValue;

            int newFitness = CalculateConflicts(board);

            if (newFitness < fitness)
            {
                // Cambio aceptado: actualizar fitness y notificar al manager
                fitness = newFitness;
                onCellChanged?.Invoke(cell.r, cell.c, newValue); // Solo estas celdas cambian de color
            }
            else
            {
                // Cambio rechazado: revertir
                board[cell.r, cell.c] = oldValue;
                // NO llamar al callback
            }
        }

        Debug.Log("Hill Climbing terminado en " + iterations + " iteraciones. Conflictos = " + fitness);
        return board;
    }

    private int CalculateConflicts(int[,] board)
    {
        int conflicts = 0;

        // Filas
        for (int r = 0; r < gridSize; r++)
        {
            int[] count = new int[10];
            for (int c = 0; c < gridSize; c++)
                if (board[r, c] != 0) count[board[r, c]]++;
            for (int i = 1; i <= 9; i++)
                if (count[i] > 1) conflicts += count[i] - 1;
        }

        // Columnas
        for (int c = 0; c < gridSize; c++)
        {
            int[] count = new int[10];
            for (int r = 0; r < gridSize; r++)
                if (board[r, c] != 0) count[board[r, c]]++;
            for (int i = 1; i <= 9; i++)
                if (count[i] > 1) conflicts += count[i] - 1;
        }

        // Bloques 3x3
        for (int br = 0; br < gridSize; br += 3)
            for (int bc = 0; bc < gridSize; bc += 3)
            {
                int[] count = new int[10];
                for (int r = br; r < br + 3; r++)
                    for (int c = bc; c < bc + 3; c++)
                        if (board[r, c] != 0) count[board[r, c]]++;
                for (int i = 1; i <= 9; i++)
                    if (count[i] > 1) conflicts += count[i] - 1;
            }

        return conflicts;
    }
}
