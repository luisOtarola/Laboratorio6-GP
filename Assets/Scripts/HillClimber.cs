using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HillClimbing : MonoBehaviour
{
    public SudokuGridManager gridManager;
    public TMP_Text fitnessText;

    private int[,] board;
    private List<(int, int)> candidateCells;
    private int currentIndex = 0;
    private bool initialized = false;

    void Start()
    {
        StartCoroutine(WaitForGridReady());
    }

    private System.Collections.IEnumerator WaitForGridReady()
    {
        yield return new WaitUntil(() => gridManager != null && gridManager.IsReady);

        InitializeBoard();
        initialized = true;
        UpdateFitnessUI();
    }

    private void InitializeBoard()
    {
        board = gridManager.GetCurrentBoard();
        if (board == null)
        {
            Debug.LogError("❌ HillClimbing: No se pudo obtener el tablero desde el gridManager.");
            return;
        }

        // ✅ Ahora analizamos TODAS las celdas (rellenas y vacías)
        candidateCells = GetAllCells(board);
        currentIndex = 0;

        Debug.Log($"✅ HillClimbing inicializado con {candidateCells.Count} celdas a analizar.");
    }

    public void StepHillClimbing()
    {
        if (!initialized)
        {
            Debug.LogWarning("⚠️ HillClimbing aún no está inicializado.");
            return;
        }

        if (candidateCells == null || candidateCells.Count == 0)
        {
            Debug.LogWarning("⚠️ No hay celdas disponibles para analizar.");
            return;
        }

        if (currentIndex >= candidateCells.Count)
        {
            Debug.Log("✅ Hill Climbing completado: no quedan celdas para mejorar.");
            return;
        }

        var cell = candidateCells[currentIndex];
        int row = cell.Item1;
        int col = cell.Item2;

        float currentFitness = CalculateFitness(board);
        bool improved = false;

        var neighbors = GetNeighbors(row, col);
        foreach (var n in neighbors)
        {
            int nr = n.Item1;
            int nc = n.Item2;

            int originalValue = board[row, col];
            int neighborValue = board[nr, nc];

            // ✅ Solo hacer swap si ambos números pueden ir legalmente en la celda opuesta
            bool canPlaceOriginal = (originalValue == 0) || gridManager.IsValidPlacement(board, originalValue, nr, nc);
            bool canPlaceNeighbor = (neighborValue == 0) || gridManager.IsValidPlacement(board, neighborValue, row, col);

            if (canPlaceOriginal && canPlaceNeighbor)
            {
                SwapCells(row, col, nr, nc);
                float newFitness = CalculateFitness(board);

                if (newFitness < currentFitness)
                {
                    improved = true;
                    Debug.Log($"✨ Mejora encontrada: ({row},{col}) ↔ ({nr},{nc}) | {currentFitness} → {newFitness}");
                    currentFitness = newFitness;
                    break;
                }
                else
                {
                    SwapCells(row, col, nr, nc); // revertir si no mejora
                }
            }
        }

        if (!improved)
            currentIndex++;

        // Actualizar visualmente
        gridManager.ClearGrid();
        gridManager.FillInitialBoard(board);
        UpdateFitnessUI();
    }

    public void StepHillClimbingReverse()
    {
        if (!initialized)
        {
            Debug.LogWarning("⚠️ HillClimbing aún no está inicializado.");
            return;
        }

        if (candidateCells == null || candidateCells.Count == 0)
        {
            Debug.LogWarning("⚠️ No hay celdas disponibles para analizar.");
            return;
        }

        if (currentIndex >= candidateCells.Count)
        {
            Debug.Log("✅ Hill Climbing completado: no quedan celdas para analizar.");
            return;
        }

        var cell = candidateCells[currentIndex];
        int row = cell.Item1;
        int col = cell.Item2;

        float currentFitness = CalculateFitness(board);
        bool worsened = false;

        var neighbors = GetNeighbors(row, col);
        foreach (var n in neighbors)
        {
            int nr = n.Item1;
            int nc = n.Item2;

            int originalValue = board[row, col];
            int neighborValue = board[nr, nc];

            // ✅ Solo hacer swap si ambos números pueden ir legalmente en la celda opuesta
            bool canPlaceOriginal = (originalValue == 0) || gridManager.IsValidPlacement(board, originalValue, nr, nc);
            bool canPlaceNeighbor = (neighborValue == 0) || gridManager.IsValidPlacement(board, neighborValue, row, col);

            if (canPlaceOriginal && canPlaceNeighbor)
            {
                SwapCells(row, col, nr, nc);
                float newFitness = CalculateFitness(board);

                if (newFitness > currentFitness)
                {
                    worsened = true;
                    Debug.Log($"⚠️ Peor opción elegida: ({row},{col}) ↔ ({nr},{nc}) | {currentFitness} → {newFitness}");
                    currentFitness = newFitness;
                    break;
                }
                else
                {
                    SwapCells(row, col, nr, nc); // revertir si no empeora
                }
            }
        }

        if (!worsened)
            currentIndex++;

        gridManager.ClearGrid();
        gridManager.FillInitialBoard(board);
        UpdateFitnessUI();
    }

    // 🔹 Ejecuta todo el hill climbing (mejorando) en todas las celdas
    public void FullHillClimbing()
    {
        if (!initialized) return;
        Debug.Log("🧩 Iniciando Hill Climbing completo (mejorar)");
        for (int i = 0; i < candidateCells.Count; i++)
        {
            currentIndex = i;
            StepHillClimbing();
        }
        Debug.Log("✅ Hill Climbing completo finalizado.");
    }

    // 🔹 Ejecuta todo el hill climbing inverso (empeorando) en todas las celdas
    public void FullHillClimbingReverse()
    {
        if (!initialized) return;
        Debug.Log("🧩 Iniciando Hill Climbing completo inverso (empeorar)");
        for (int i = 0; i < candidateCells.Count; i++)
        {
            currentIndex = i;
            StepHillClimbingReverse();
        }
        Debug.Log("✅ Hill Climbing inverso completo finalizado.");
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

    List<(int, int)> GetAllCells(int[,] b)
    {
        List<(int, int)> cells = new List<(int, int)>();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cells.Add((r, c));
        return cells;
    }

    List<(int, int)> GetNeighbors(int r, int c)
    {
        List<(int, int)> neighbors = new List<(int, int)>();
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr;
                int nc = c + dc;
                if (nr >= 0 && nr < 9 && nc >= 0 && nc < 9)
                    neighbors.Add((nr, nc));
            }
        }
        return neighbors;
    }

    void SwapCells(int r1, int c1, int r2, int c2)
    {
        int temp = board[r1, c1];
        board[r1, c1] = board[r2, c2];
        board[r2, c2] = temp;
    }

    void UpdateFitnessUI()
    {
        if (fitnessText != null)
        {
            float f = CalculateFitness(board);
            fitnessText.text = $"Fitness Actual: {f:F2}";
        }
        else
        {
            Debug.LogWarning(" No se asignó un TMP_Text a HillClimbing.");
        }
    }

    public void RefreshBoardFromGrid()
    {
        board = gridManager.GetCurrentBoard();
        if (board == null)
        {
            Debug.LogError(" HillClimbing: No se pudo obtener el nuevo tablero.");
            return;
        }

        candidateCells = GetAllCells(board);
        currentIndex = 0;
        initialized = true;
        UpdateFitnessUI();

        Debug.Log(" HillClimbing: tablero actualizado tras cambio de dificultad.");
    }
}
