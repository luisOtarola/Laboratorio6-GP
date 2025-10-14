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

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty = Difficulty.Easy;

    void Start()
    {
        GenerateGrid();
        GeneratePuzzleByDifficulty();
        SudokuMapSelector selector = GetComponent<SudokuMapSelector>();

        // 1. Generar 20 mapas completos
        List<int[,]> allMaps = selector.GenerateMaps(20);

        // 2. Seleccionar los 10 mejores según número de impares
        List<int[,]> bestMaps = selector.SelectBestMaps(allMaps, 10);

        Debug.Log("Mejores mapas seleccionados: " + bestMaps.Count);
    }

    // ===== GENERACIÓN DE GRID =====
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

    // ===== VALIDACIÓN DE INPUT =====
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

        bool valid = IsValidPlacement(number, row, col);
        UpdateCellVisual(input, valid);
    }

    void UpdateCellVisual(TMP_InputField input, bool valid)
    {
        Image bg = input.GetComponent<Image>();
        if (bg != null)
            bg.color = valid ? Color.white : new Color(1f, 0.4f, 0.4f); // rojo si hay conflicto
    }

    bool IsValidPlacement(int number, int row, int col)
    {
        for (int i = 0; i < gridSize; i++)
        {
            if (i != col)
            {
                string t = grid[row, i].text;
                if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int val) && val == number)
                    return false;
            }
            if (i != row)
            {
                string t2 = grid[i, col].text;
                if (!string.IsNullOrEmpty(t2) && int.TryParse(t2, out int val2) && val2 == number)
                    return false;
            }
        }

        int blockSize = 3;
        int startRow = (row / blockSize) * blockSize;
        int startCol = (col / blockSize) * blockSize;
        for (int r = startRow; r < startRow + blockSize; r++)
            for (int c = startCol; c < startCol + blockSize; c++)
                if (!(r == row && c == col))
                {
                    string t = grid[r, c].text;
                    if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int val) && val == number)
                        return false;
                }

        return true;
    }

    // ===== RESALTAR CONFLICTOS =====
    public void HighlightConflicts()
    {
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                string t = grid[r, c].text;
                if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int number))
                {
                    bool valid = IsValidPlacement(number, r, c);
                    Image bg = grid[r, c].GetComponent<Image>();
                    if (bg != null)
                        bg.color = valid ? Color.white : Color.yellow; // amarillo para conflicto
                }
            }
        }
    }

    // ===== GENERACIÓN DE PUZZLE VÁLIDO =====
    public void GenerateValidSudokuPuzzle(int numbersToRemove)
    {
        int[,] board = new int[gridSize, gridSize];
        SolveSudoku(board); // genera una solución completa válida

        // Quitar números según dificultad
        int removed = 0;
        while (removed < numbersToRemove)
        {
            int row = Random.Range(0, gridSize);
            int col = Random.Range(0, gridSize);
            if (board[row, col] != 0)
            {
                board[row, col] = 0;
                removed++;
            }
        }

        FillInitialBoard(board);
    }

    // ===== SOLVER DE SUDOKU (BACKTRACKING) =====
    public bool SolveSudoku(int[,] board)
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
                        if (IsValidSudokuPlacement(board, num, row, col))
                        {
                            board[row, col] = num;
                            if (SolveSudoku(board))
                                return true;
                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true; // completado
    }

    bool IsValidSudokuPlacement(int[,] board, int number, int row, int col)
    {
        for (int i = 0; i < gridSize; i++)
        {
            if (board[row, i] == number) return false;
            if (board[i, col] == number) return false;
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (board[r, c] == number) return false;

        return true;
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ===== MÉTODOS DE TABLERO =====
    public void FillInitialBoard(int[,] board)
    {
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (board[r, c] != 0)
                {
                    grid[r, c].text = board[r, c].ToString();
                    grid[r, c].interactable = false;
                    Image bg = grid[r, c].GetComponent<Image>();
                    if (bg != null) bg.color = new Color(0.9f, 0.9f, 0.9f);
                }
                else
                {
                    grid[r, c].text = "";
                    grid[r, c].interactable = true;
                    Image bg = grid[r, c].GetComponent<Image>();
                    if (bg != null) bg.color = Color.white;
                }
            }
        }
    }

    public int[,] GetCurrentBoard()
    {
        int[,] board = new int[gridSize, gridSize];
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
            {
                string t = grid[r, c].text;
                board[r, c] = string.IsNullOrEmpty(t) ? 0 : int.Parse(t);
            }
        return board;
    }

    public void ClearGrid()
    {
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

    // ===== HILL CLIMBING =====
    /*public void SolveWithHillClimbing()
    {
        int[,] currentBoard = GetCurrentBoard();
        SudokuSolverHillClimbing solver = new SudokuSolverHillClimbing(
            currentBoard,
            (r, c, value) => // callback solo para celdas modificadas
            {
                grid[r, c].text = value.ToString();
                Image bg = grid[r, c].GetComponent<Image>();
                if (bg != null)
                    bg.color = new Color(0.4f, 0.6f, 1f); // azul claro
            }
        );

        solver.Solve();
    }*/

    // ===== DIFICULTAD =====
    public void GeneratePuzzleByDifficulty()
    {
        ClearGrid();

        int numbersToRemove = 45; // por defecto

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                numbersToRemove = gridSize * gridSize - 40; // 40 numeros visibles
                break;
            case Difficulty.Medium:
                numbersToRemove = gridSize * gridSize - 30; // 30 numeros visibles
                break;
            case Difficulty.Hard:
                numbersToRemove = gridSize * gridSize - 18; // 18 numeros visibles
                break;
        }

        GenerateValidSudokuPuzzle(numbersToRemove);
    }

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
