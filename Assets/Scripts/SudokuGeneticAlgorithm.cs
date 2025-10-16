using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class SudokuGeneticAlgorithm : MonoBehaviour
{
    public SudokuGridManager gridManager;
    public int populationSize = 20;   // total tableros
    public int parentsCount = 10;     // tableros seleccionados
    public int generations = 10;      // número de iteraciones
    public PopulationDisplay populationUI; // UI de lista de individuos

    private List<int[,]> population = new List<int[,]>();
    private List<float> fitnesses = new List<float>();
    private int[,] bestBoard;
    private int[,] worstBoard;

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

        // espera hasta que SudokuGridManager haya creado la grilla
        yield return new WaitUntil(() => gridManager.IsReady);
        RunGA();
    }

    public void RunGA()
    {
        // 1️⃣ Generar población inicial
        GenerateInitialPopulation();

        for (int gen = 0; gen < generations; gen++)
        {
            Debug.Log($"Generación {gen + 1}");

            // 2️⃣ Evaluar fitness
            fitnesses = population.Select(p => CalculateFitness(p)).ToList();

            // 2b️ Mostrar población en UI
            if (populationUI != null)
                populationUI.ShowPopulation(population, fitnesses);

            // Guardar mejor y peor tablero
            int bestIndex = fitnesses.IndexOf(fitnesses.Max());
            int worstIndex = fitnesses.IndexOf(fitnesses.Min());
            bestBoard = CloneBoard(population[bestIndex]);
            worstBoard = CloneBoard(population[worstIndex]);

            Debug.Log($"🏆 Mejor fitness: {fitnesses[bestIndex]} | Peor fitness: {fitnesses[worstIndex]}");

            // 3️⃣ Selección
            List<int[,]> parents = SelectTopParents(population, fitnesses, parentsCount);

            // 4️⃣ Mutación para crear descendencia
            List<int[,]> offspring = new List<int[,]>();
            foreach (var parent in parents)
            {
                int[,] child = SudokuMutation.MutateInsertAndRemove(parent, gridManager);
                offspring.Add(child);
            }

            // 5️⃣ Reemplazo: padres + hijos
            population = new List<int[,]>();
            population.AddRange(parents);
            population.AddRange(offspring);
        }

        // Mostrar el mejor tablero final en la UI
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(bestBoard);
    }

    void GenerateInitialPopulation()
    {
        population.Clear();
        for (int i = 0; i < populationSize; i++)
        {
            int[,] board = new int[9, 9];
            gridManager.FillBoard(board);          // tablero completo válido
            RemoveNumbersForDifficulty(board);     // deja solo algunos según dificultad
            population.Add(board);
        }
    }

    void RemoveNumbersForDifficulty(int[,] board)
    {
        int numbersToKeep = 28; // media dificultad
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

        // Aplicar Naked y Hidden Singles hasta que no haya progreso
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

        // Fitness alto = tablero más difícil (más celdas sin resolver)
        int emptyCells = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (tempBoard[r, c] == 0)
                    emptyCells++;

        return emptyCells;
    }

    List<int[,]> SelectTopParents(List<int[,]> pop, List<float> fitnesses, int count)
    {
        return pop.Zip(fitnesses, (p, f) => new { board = p, fitness = f })
                  .OrderByDescending(x => x.fitness)
                  .Take(count)
                  .Select(x => x.board)
                  .ToList();
    }

    // === Mostrar mejor / peor tablero ===
    public void ShowBestBoard()
    {
        if (bestBoard == null) return;
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(bestBoard);
        Debug.Log($"Mostrando mejor tablero (fitness {fitnesses.Max()})");
    }

    public void ShowWorstBoard()
    {
        if (worstBoard == null) return;
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(worstBoard);
        Debug.Log($"Mostrando peor tablero (fitness {fitnesses.Min()})");
    }

    // === Copiar tablero 9x9 ===
    private int[,] CloneBoard(int[,] original)
    {
        int[,] copy = new int[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                copy[r, c] = original[r, c];
        return copy;
    }
    public void GenerateNewPopulation()
    {
        gridManager.ClearGrid(); // limpia la UI
        RunGA();
    }
}
