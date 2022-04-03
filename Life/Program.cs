using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(Parameters parameters)
        {
            CellSize = parameters.cellSize;

            Cells = new Cell[parameters.width / parameters.cellSize, parameters.height / parameters.cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(parameters.liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }

    public class Parameters
    {
        struct Data
        {
            public int height { get; set; }
            public int width { get; set; }
            public int cellSize { get; set; }
            public double liveDensity { get; set; }
        }

        Data newdata;
        public int height
        {
            get { return newdata.height; }
        }
        public int width
        {
            get { return newdata.width; }
        }
        public int cellSize
        {
            get { return newdata.cellSize; }
        }
        public double liveDensity
        {
            get { return newdata.liveDensity; }
        }
        public void LoadParameters(string jsonPath)
        {
            newdata = JsonConvert.DeserializeObject<Data>(File.ReadAllText(jsonPath));
        }

        public void LoadFile(Board board, string filePath)
        {
            string state = File.ReadAllText(filePath);

            for (int i = 0; i < state.Length; i++)
            {

            }
        }
    }

    class Program
    {
        static Board board;
        static private void Reset()
        {
            Parameters parameters = new Parameters();
            parameters.LoadParameters(@"Parameters.json");

            board = new Board(parameters);
            //LoadFromFile(board, "F:\\new.txt");
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static void Main(string[] args)
        {
            int counter = 0;

            Reset();
            while (true)
            {
                Console.Clear();
                Render();
                board.Advance();
                counter++;
                Thread.Sleep(1000);

                //if (counter == 20)
                //{
                //    SaveFile(board, ".\\Colonies\\colony5.txt");
                //}
            }
        }

        static void SaveFile(Board board, string filePath)
        {
            string state = string.Empty;

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    bool cellIsAlive = board.Cells[col, row].IsAlive;

                    if (cellIsAlive) state += '*';
                    else state += ' ';
                }
                state += '\n';
            }

            File.WriteAllText(filePath, state);
        }

        static void LoadFromFile(Board board, string filePath)
        {
            string state = File.ReadAllText(filePath);

            int col = 0, row = 0;
            bool toFill = true;

            foreach (char c in state)
            {
                if (row >= board.Rows) return;
                if (col >= board.Columns) toFill = false;

                if (c == '\n')
                {
                    row++;
                    col = -1;
                    toFill = true;
                }
                else if (c == ' ' && toFill)
                {
                    board.Cells[col, row].IsAlive = false;
                }
                else if (toFill)
                {
                    board.Cells[col, row].IsAlive = true;
                }
                col++;
            }
        }
    }
}