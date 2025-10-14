using System.Collections.Generic;
using UnityEngine;

public class SudokuMapSelector : MonoBehaviour
{
    public int gridSize = 9;

    // Genera 20 mapas usando tu backtracking
    public List<int[,]> GenerateMaps(int numberOfMaps)
    {
        List<int[,]> maps = new List<int[,]>();

        for (int i = 0; i < numberOfMaps; i++)
        {
            int[,] board = new int[gridSize, gridSize];
            SudokuGridManager sudoku = GetComponent<SudokuGridManager>();
            sudoku.SolveSudoku(board); // genera un tablero completo
            maps.Add(board);
        }

        return maps;
    }

    // Calcula fitness: cantidad de números impares
    int CalculateFitness(int[,] board)
    {
        int fitness = 0;
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (board[r, c] % 2 == 1) fitness++;
            }
        }
        return fitness;
    }

    // Selecciona los mejores mapas según fitness
    public List<int[,]> SelectBestMaps(List<int[,]> maps, int numberToSelect)
    {
        // Crear lista de tuplas (mapa, fitness)
        List<(int[,] board, int fitness)> scoredMaps = new List<(int[,], int)>();
        foreach (var map in maps)
        {
            int fit = CalculateFitness(map);
            scoredMaps.Add((map, fit));
        }

        // Ordenar descendente por fitness
        scoredMaps.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        // Tomar los mejores
        List<int[,]> bestMaps = new List<int[,]>();
        for (int i = 0; i < numberToSelect && i < scoredMaps.Count; i++)
        {
            bestMaps.Add(scoredMaps[i].board);
        }

        return bestMaps;
    }
}
