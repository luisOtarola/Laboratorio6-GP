using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SudokuGridManager : MonoBehaviour
{
    [Header("Prefab and Grid")]
    public GameObject cellPrefab;
    public int gridSize = 9;

    private TMP_InputField[,] grid;
    public bool IsReady => grid != null;

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty = Difficulty.Easy;

    void Start()
    {
        GenerateGrid();
        GeneratePuzzleByDifficulty();
    }

    // ================== GENERACIÓN DE GRID ==================
    void GenerateGrid()
    {
        grid = new TMP_InputField[gridSize, gridSize];

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                GameObject cellObj = Instantiate(cellPrefab, transform);
                TMP_InputField input = cellObj.GetComponent<TMP_InputField>();

                int rr = r;
                int cc = c;

                input.characterLimit = 1;
                input.contentType = TMP_InputField.ContentType.IntegerNumber;
                input.lineType = TMP_InputField.LineType.SingleLine;
                input.text = "";

                input.onEndEdit.AddListener((string val) => OnCellEndEdit(input, rr, cc));

                grid[r, c] = input;
            }
        }
    }

    // ================== VALIDACIÓN DE CELDA ==================
    void OnCellEndEdit(TMP_InputField input, int row, int col)
    {
        string value = input.text;

        if (string.IsNullOrEmpty(value))
        {
            UpdateCellVisual(input, true);
            return;
        }

        if (!int.TryParse(value, out int number) || number < 1 || number > 9)
        {
            input.text = "";
            UpdateCellVisual(input, true);
            return;
        }

        bool valid = IsValidPlacement(GetCurrentBoard(), number, row, col);
        UpdateCellVisual(input, valid);
    }

    void UpdateCellVisual(TMP_InputField input, bool valid)
    {
        Image bg = input.GetComponent<Image>();
        if (bg != null)
            bg.color = valid ? Color.white : new Color(1f, 0.4f, 0.4f);
    }

    // ================== VALIDACIÓN DE NÚMEROS ==================
    public bool IsValidPlacement(int[,] board, int number, int row, int col)
    {
        for (int i = 0; i < gridSize; i++)
        {
            if (board[row, i] == number && i != col) return false;
            if (board[i, col] == number && i != row) return false;
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if ((r != row || c != col) && board[r, c] == number)
                    return false;
            }
        }
        return true;
    }

    // ================== COLOREAR CONFLICTOS ==================
    public void HighlightConflicts()
    {
        if (grid == null)
        {
            Debug.LogWarning("HighlightConflicts: grid no inicializado.");
            return;
        }
        int[,] board = GetCurrentBoard();

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                string t = grid[r, c].text;
                if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int number))
                {
                    bool valid = IsValidPlacement(board, number, r, c);
                    Image bg = grid[r, c].GetComponent<Image>();
                    if (bg != null)
                        bg.color = valid ? Color.white : Color.yellow;
                }
            }
        }
    }

    // ================== HILL CLIMBING (RESOLVER) ==================
    public void SolveWithHillClimbing()
    {
        if (grid == null)
        {
            Debug.LogWarning("SolveWithHillClimbing: grid no inicializado.");
            return;
        }
        int[,] currentBoard = GetCurrentBoard();
        SudokuSolverHillClimbing solver = new SudokuSolverHillClimbing(
            currentBoard,
            (r, c, value) =>
            {
                grid[r, c].text = value.ToString();
                Image bg = grid[r, c].GetComponent<Image>();
                if (bg != null)
                    bg.color = new Color(0.4f, 0.6f, 1f);
            }
        );
        solver.Solve();
    }

    // ================== MÉTODOS DE TABLERO ==================
    public void FillInitialBoard(int[,] board)
    {
        if (grid == null)
        {
            Debug.LogWarning("FillInitialBoard: grid no inicializado todavía.");
            return;
       }
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (board[r, c] != 0)
                {
                    grid[r, c].text = board[r, c].ToString();
                    grid[r, c].interactable = false;
                    Image bg = grid[r, c].GetComponent<Image>();
                    if (bg != null)
                        bg.color = new Color(0.9f, 0.9f, 0.9f);
                }
            }
        }
    }

    public int[,] GetCurrentBoard()
    {
        int[,] board = new int[gridSize, gridSize];
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                string t = grid[r, c].text;
                board[r, c] = string.IsNullOrEmpty(t) ? 0 : int.Parse(t);
            }
        }
        return board;
    }

    public void ClearGrid()
    {
        if (grid == null) return; // proteger contra NullReference si se llama antes de GenerateGrid

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                grid[r, c].text = "";
                grid[r, c].interactable = true;
                Image bg = grid[r, c].GetComponent<Image>();
                if (bg != null)
                    bg.color = Color.white;
            }
        }
    }

    // ================== GENERADOR VÁLIDO (BACKWARD FROM GOAL) ==================
    public void GeneratePuzzleByDifficulty()
    {
        ClearGrid();
        int numbersToKeep = 30;

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                numbersToKeep = 35;
                break;
            case Difficulty.Medium:
                numbersToKeep = 28;
                break;
            case Difficulty.Hard:
                numbersToKeep = 22;
                break;
        }

        int[,] board = new int[gridSize, gridSize];
        FillBoard(board);              // Sudoku completo válido
        RemoveNumbers(board, numbersToKeep); // Quita según dificultad
        FillInitialBoard(board);       // Muestra en la UI
    }

    // Genera un Sudoku completo con backtracking
    public bool FillBoard(int[,] board)
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (board[row, col] == 0)
                {
                    List<int> numbers = new List<int>();
                    for (int n = 1; n <= 9; n++) numbers.Add(n);
                    Shuffle(numbers);

                    foreach (int num in numbers)
                    {
                        if (IsValidPlacement(board, num, row, col))
                        {
                            board[row, col] = num;
                            if (FillBoard(board)) return true;
                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    // Mezclar lista (aleatorio)
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    // Quitar números según dificultad (manteniendo validez básica)
    void RemoveNumbers(int[,] board, int numbersToKeep)
    {
        int totalCells = gridSize * gridSize;
        int toRemove = totalCells - numbersToKeep;

        while (toRemove > 0)
        {
            int row = Random.Range(0, gridSize);
            int col = Random.Range(0, gridSize);

            if (board[row, col] != 0)
            {
                int backup = board[row, col];
                board[row, col] = 0;

                // Si quieres asegurar una sola solución, podrías agregar verificación aquí
                toRemove--;
            }
        }
    }

    // ================== CAMBIO DE DIFICULTAD ==================
    public void SetDifficultyEasy()
    {
        currentDifficulty = Difficulty.Easy;
        GeneratePuzzleByDifficulty();
    }

    public void SetDifficultyMedium()
    {
        currentDifficulty = Difficulty.Medium;
        GeneratePuzzleByDifficulty();
    }

    public void SetDifficultyHard()
    {
        currentDifficulty = Difficulty.Hard;
        GeneratePuzzleByDifficulty();
    }
}
