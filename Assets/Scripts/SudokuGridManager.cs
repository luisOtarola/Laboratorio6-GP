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
        GeneratePuzzleByDifficulty(); // coloca números iniciales según dificultad
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
                if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int val) && val == number) return false;
            }
            if (i != row)
            {
                string t2 = grid[i, col].text;
                if (!string.IsNullOrEmpty(t2) && int.TryParse(t2, out int val2) && val2 == number) return false;
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
                    if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int val) && val == number) return false;
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

    // ===== HILL CLIMBING =====
    public void SolveWithHillClimbing()
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

    public void UpdateBoardVisual(int[,] board)
    {
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                grid[r, c].text = board[r, c] == 0 ? "" : board[r, c].ToString();
    }

    // ===== GENERACIÓN ALEATORIA =====
    public void GenerateRandomPuzzleUnvalidated(int numbersToPlace = 15)
    {
        int[,] board = new int[gridSize, gridSize];
        int placed = 0;
        while (placed < numbersToPlace)
        {
            int row = Random.Range(0, gridSize);
            int col = Random.Range(0, gridSize);
            int num = Random.Range(1, 10);

            if (board[row, col] == 0)
            {
                board[row, col] = num;
                placed++;
            }
        }

        FillInitialBoard(board);
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
                    bg.color = Color.white; // color por defecto
            }
        }
    }
    // ===== DIFICULTAD =====
    public void GeneratePuzzleByDifficulty()
    {
        ClearGrid();
        int numbersToPlace = 15;

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                numbersToPlace = 25;
                break;
            case Difficulty.Medium:
                numbersToPlace = 20;
                break;
            case Difficulty.Hard:
                numbersToPlace = 15;
                break;
        }

        GenerateRandomPuzzleUnvalidated(numbersToPlace);
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
