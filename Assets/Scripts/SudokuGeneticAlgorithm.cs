using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SudokuGeneticAlgorithm : MonoBehaviour
{
    [Header("Referencias")]
    public SudokuGridManager gridManager;      // Tu script de Sudoku
    public PopulationDisplay populationUI;     // UI para mostrar la población

    [Header("Parámetros GA")]
    public int populationSize = 20;   // total tableros
    public int parentsCount = 10;     // tableros seleccionados
    public int generations = 10;      // número de iteraciones

    private List<int[,]> population = new List<int[,]>(); // población actual

    void Start()
    {
        // Intentar encontrar el SudokuGridManager si no está asignado
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<SudokuGridManager>();
            if (gridManager == null)
            {
                Debug.LogError("No se pudo encontrar SudokuGridManager en la escena.");
                return;
            }
        }
        
        // Esperar un frame para asegurar que gridManager esté completamente inicializado
        Invoke("RunGA", 0.1f);
    }

    public void RunGA()
    {
        if (gridManager == null)
        {
            Debug.LogError("SudokuGridManager es null. No se puede ejecutar el algoritmo genético.");
            return;
        }

        // 1️⃣ Generar población inicial
        GenerateInitialPopulation();

        for (int gen = 0; gen < generations; gen++)
        {
            Debug.Log($"Generación {gen + 1}");

            // 2️⃣ Evaluar fitness
            List<float> fitnesses = population.Select(p => CalculateFitness(p)).ToList();

            // Mostrar en UI
            if (populationUI != null)
                populationUI.ShowPopulation(population, fitnesses);

            // 3️⃣ Selección de los mejores padres
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

            // Mostrar mejor fitness en consola
            int[,] best = population.OrderByDescending(p => CalculateFitness(p)).First();
            float bestFitness = CalculateFitness(best);
            Debug.Log($"Mejor fitness: {bestFitness}");
        }

        // Mostrar el mejor tablero final en la UI del Sudoku
        int[,] finalBest = population.OrderByDescending(p => CalculateFitness(p)).First();
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(finalBest);
    }

    // ================== Población Inicial ==================
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

    // ================== Fitness ==================
    float CalculateFitness(int[,] board)
    {
        // Fitness basado en dificultad usando Naked Single
        int unresolved = 0;
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

                    if (candidates.Count != 1) unresolved++;
                }
            }
        }
        return unresolved; // mayor valor = más difícil
    }

    // ================== Selección ==================
    List<int[,]> SelectTopParents(List<int[,]> pop, List<float> fitnesses, int count)
    {
        return pop.Zip(fitnesses, (p, f) => new { board = p, fitness = f })
                  .OrderByDescending(x => x.fitness)
                  .Take(count)
                  .Select(x => x.board)
                  .ToList();
    }
}
