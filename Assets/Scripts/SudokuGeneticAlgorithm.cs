using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using TMPro;

public class SudokuGeneticAlgorithm : MonoBehaviour
{
    public SudokuGridManager gridManager;
    public int populationSize = 20;   // total tableros
    public int parentsCount = 10;     // padres seleccionados
    public PopulationDisplay populationUI; // UI de lista de individuos
    public TMP_Text fitnessAverageText;

    private List<int[,]> population = new List<int[,]>();
    private List<float> fitnesses = new List<float>();
    private int[,] bestBoard;
    private int[,] worstBoard;
    
    public HillClimbing hillClimbing; // 🔹 Arriba en la clase, referencia desde el inspector

    void Start()
    {
        StartCoroutine(RunGAWhenReady());
    }

    IEnumerator RunGAWhenReady()
    {
        if (gridManager == null)
        {
            Debug.LogError("gridManager no asignado en SudokuGeneticAlgorithm.");
            yield break;
        }

        yield return new WaitUntil(() => gridManager.IsReady);

        // Generar población inicial solo una vez
        GenerateInitialPopulation();
        EvaluatePopulation();
        ShowPopulationUI();
    }

    void GenerateInitialPopulation()
    {
        population.Clear();
        int numbersToKeep = GetNumbersToKeepByDifficulty();
        for (int i = 0; i < populationSize; i++)
        {
            int[,] board = new int[9, 9];
            gridManager.FillBoard(board);          // tablero completo válido
            RemoveNumbersForDifficulty(board);     // dejar algunos según dificultad
            population.Add(board);
        }
    }

    private int GetNumbersToKeepByDifficulty()
    {
        switch (gridManager.currentDifficulty)
        {
            case SudokuGridManager.Difficulty.Easy: return 30;
            case SudokuGridManager.Difficulty.Medium: return 25;
            case SudokuGridManager.Difficulty.Hard: return 20;
            default: return 28;
        }
    }

    void RemoveNumbersForDifficulty(int[,] board)
    {
        int numbersToKeep = GetNumbersToKeepByDifficulty(); 
        int totalCells = 9 * 9;
        int toRemove = totalCells - numbersToKeep;

        while (toRemove > 0)
        {
            int r = Random.Range(0, 9);
            int c = Random.Range(0, 9);
            if (board[r, c] != 0)
            {
                board[r, c] = 0;
                toRemove--;
            }
        }
    }

    float CalculateFitness(int[,] board)
    {
        int[,] tempBoard = (int[,])board.Clone();
        bool progress = true;

        // Naked y Hidden Singles
        while (progress)
        {
            progress = false;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (tempBoard[r, c] == 0)
                    {
                        List<int> candidates = new List<int>();
                        for (int n = 1; n <= 9; n++)
                            if (gridManager.IsValidPlacement(tempBoard, n, r, c))
                                candidates.Add(n);

                        if (candidates.Count == 1)
                        {
                            tempBoard[r, c] = candidates[0];
                            progress = true;
                        }
                    }
                }
            }
        }

        // Fitness alto = más difícil (más celdas sin resolver)
        int emptyCells = 0;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (tempBoard[r, c] == 0)
                {
                    for (int n = 1; n <= 9; n++)
                        if (gridManager.IsValidPlacement(tempBoard, n, r, c))
                            emptyCells++;
                }
            }
        }

        return emptyCells;
    }

    void EvaluatePopulation()
    {
        fitnesses = population.Select(p => CalculateFitness(p)).ToList();

        int bestIndex = fitnesses.IndexOf(fitnesses.Max());
        int worstIndex = fitnesses.IndexOf(fitnesses.Min());
        bestBoard = CloneBoard(population[bestIndex]);
        worstBoard = CloneBoard(population[worstIndex]);

        float averageFitness = fitnesses.Average();
        if (fitnessAverageText != null)
            fitnessAverageText.text = $"Fitness Promedio: {averageFitness:F2}";

        Debug.Log($"🏆 Mejor fitness: {fitnesses[bestIndex]} | Peor fitness: {fitnesses[worstIndex]} | Prom: {averageFitness:F2}");
    }

    void ShowPopulationUI()
    {
        if (populationUI != null)
            populationUI.ShowPopulation(population, fitnesses);
    }

    // Clonar tablero 9x9
    int[,] CloneBoard(int[,] original)
    {
        int[,] copy = new int[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                copy[r, c] = original[r, c];
        return copy;
    }

    // === Ejecutar Nueva Generación ===
    public void GenerateNewGeneration_WorstParents()
    {
        int keep = Mathf.Clamp(parentsCount, 1, populationSize / 2);

        // 1️⃣ Seleccionar los 10 peores padres (fitness más bajo)
        var indexed = population
            .Select((board, idx) => new { board, fitness = fitnesses[idx] })
            .OrderBy(x => x.fitness) // ascendente = peor primero
            .Take(keep)
            .Select(x => CloneBoard(x.board))
            .ToList();

        List<int[,]> offspring = new List<int[,]>();
        foreach (var parent in indexed)
        {
            offspring.Add(SudokuMutation.MutateInsertAndRemove(parent, gridManager));
        }

        // Nueva población = padres + descendientes
        population = new List<int[,]>();
        population.AddRange(indexed); // padres
        population.AddRange(offspring); // hijos

        EvaluatePopulation();
        ShowPopulationUI();
    }

    // función para generar con los mejores padres
    public void GenerateNewGeneration_BestParents()
    {
        int keep = Mathf.Clamp(parentsCount, 1, populationSize / 2);

        // 1️⃣ Seleccionar los 10 mejores padres (fitness más alto)
        var indexed = population
            .Select((board, idx) => new { board, fitness = fitnesses[idx] })
            .OrderByDescending(x => x.fitness) // descendente = mejor primero
            .Take(keep)
            .Select(x => CloneBoard(x.board))
            .ToList();

        // 2️⃣ Crear descendencia mutando a los mejores padres
        List<int[,]> offspring = new List<int[,]>();
        foreach (var parent in indexed)
        {
            offspring.Add(SudokuMutation.MutateInsertAndRemove(parent, gridManager));
        }

        // 3️⃣ Nueva población = padres + descendientes
        population = new List<int[,]>();
        population.AddRange(indexed); // padres
        population.AddRange(offspring); // hijos

        EvaluatePopulation();
        ShowPopulationUI();
    }

    // Genera 10 generaciones usando los 10 peores padres
    public void Generate10Generations_WorstParents()
    {
        StartCoroutine(GenerateMultipleGenerations(false, 10));
    }

    // Genera 10 generaciones usando los 10 mejores padres
    public void Generate10Generations_BestParents()
    {
        StartCoroutine(GenerateMultipleGenerations(true, 10));
    }

    // Método que aplica varias generaciones
    private IEnumerator GenerateMultipleGenerations(bool useBestParents, int times)
    {
        for (int i = 0; i < times; i++)
        {
            if (useBestParents)
                GenerateNewGeneration_BestParents();
            else
                GenerateNewGeneration_WorstParents();

            // Espera un frame para que Unity actualice la UI
            yield return null;
        }
    }
    

    // Mostrar mejor / peor tablero en UI
    public void ShowBestBoard()
    {
        if (bestBoard == null) return;
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(bestBoard);
        
        if (hillClimbing != null)
        hillClimbing.RefreshBoardFromGrid(); // 🔹 sincroniza HillClimbing
    }

    public void ShowWorstBoard()
    {
        if (worstBoard == null) return;
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(worstBoard);

        if (hillClimbing != null)
        hillClimbing.RefreshBoardFromGrid(); // 🔹 sincroniza HillClimbing
    }

    public void SetDifficultyEasy()
    {
        gridManager.currentDifficulty = SudokuGridManager.Difficulty.Easy;
        gridManager.GeneratePuzzleByDifficulty();  // ← actualiza UI del grid
        GenerateInitialPopulation();
        EvaluatePopulation();
        ShowPopulationUI();

        if (hillClimbing != null)
        hillClimbing.RefreshBoardFromGrid(); // 🔹 sincroniza HillClimbing
    }

    public void SetDifficultyMedium()
    {
        gridManager.currentDifficulty = SudokuGridManager.Difficulty.Medium;
        gridManager.GeneratePuzzleByDifficulty();  // ← actualiza UI del grid
        GenerateInitialPopulation();
        EvaluatePopulation();
        ShowPopulationUI();
        
        if (hillClimbing != null)
        hillClimbing.RefreshBoardFromGrid(); // 🔹 sincroniza HillClimbing
    }

    public void SetDifficultyHard()
    {
        gridManager.currentDifficulty = SudokuGridManager.Difficulty.Hard;
        gridManager.GeneratePuzzleByDifficulty();  // ← actualiza UI del grid
        GenerateInitialPopulation();
        EvaluatePopulation();
        ShowPopulationUI();
        
        if (hillClimbing != null)
        hillClimbing.RefreshBoardFromGrid(); // 🔹 sincroniza HillClimbing
    }
}
