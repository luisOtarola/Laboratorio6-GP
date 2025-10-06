using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniSudokuGridManager : MonoBehaviour
{
    [Header("Prefab and Grid")]
    public GameObject cellPrefab; 
    public int gridSize = 4; // Mini Sudoku 4x4

    private TMP_InputField[,] grid; 

    void Start()
    {
        GenerateGrid();
        GenerateRandomPuzzleUnvalidated(6); // coloca 6 números aleatorios
    }

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

        if (!int.TryParse(value, out int number) || number < 1 || number > gridSize)
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
            bg.color = valid ? Color.white : new Color(1f, 0.4f, 0.4f); // rojo claro si inválido
    }

    bool IsValidPlacement(int number, int row, int col)
    {
        // Revisar filas y columnas
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

        // Revisar bloque 2x2
        int blockSize = 2;
        int startRow = (row / blockSize) * blockSize;
        int startCol = (col / blockSize) * blockSize;
        for (int r = startRow; r < startRow + blockSize; r++)
        {
            for (int c = startCol; c < startCol + blockSize; c++)
            {
                if (r == row && c == col) continue;
                string t = grid[r, c].text;
                if (!string.IsNullOrEmpty(t) && int.TryParse(t, out int val) && val == number) return false;
            }
        }

        return true;
    }

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

    // ---------------- Genera tablero aleatorio ----------------
    public void GenerateRandomPuzzleUnvalidated(int numbersToPlace = 6)
    {
        int[,] board = new int[gridSize, gridSize];
        int placed = 0;
        while (placed < numbersToPlace)
        {
            int row = Random.Range(0, gridSize);
            int col = Random.Range(0, gridSize);
            int num = Random.Range(1, gridSize + 1);

            if (board[row, col] == 0)
            {
                board[row, col] = num;
                placed++;
            }
        }

        FillInitialBoard(board);
    }

    // ---------------- Hill Climbing ----------------
    public void SolveWithHillClimbing()
    {
        int[,] currentBoard = GetCurrentBoard();
        MiniHillClimber climber = new MiniHillClimber(currentBoard);
        int[,] solved = climber.Solve();

        // Actualizar UI
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                grid[r, c].text = solved[r, c].ToString();
    }
}