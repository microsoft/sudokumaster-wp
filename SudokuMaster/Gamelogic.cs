/*
 * Copyright (c) 2011 Nokia Corporation.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuMaster
{
    /// <summary>
    /// Contains the logic for generating and managing puzzles
    /// </summary>
    public class GameLogic
    {
		public BoardModel Model { get; private set; }

        public const int BlockSize = 3;
		public const int BlocksPerSide = 3;
		public const int RowLength = BlockSize * BlocksPerSide;
		public const int ColumnLength = RowLength;
		public const int MaxEmptyCells = 45;

        protected int[] randOrder;
        protected bool solutionFound = false;
		private int[][] copyCells;
        protected Random randGen = new Random();

        public int EmptyCells { get; set; }
        public int PlayerMoves { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gridCells">9x9 array of Cells, the game board cells</param>
        public GameLogic()
        {
			Model = new BoardModel(ColumnLength, RowLength);
        }

        /// <summary>
        /// Tests rows for conflicting cells
        /// </summary>
        /// <param name="x">X coordinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <param name="value">Number to test</param>
        /// <param name="useCopy">Tells whether we use the main array or a copy of it</param>
        /// <returns>Conflicting point or null if no conflict</returns>
        private Point GetConflictsInRow(int x, int y, int value, bool useCopy)
        {
            for (int i = 0; i < RowLength; i++)
            {
				if (useCopy)
				{
                    if (i != x &&
						copyCells[i][y] != 0 &&
						copyCells[i][y] == value)
                    {
                        return new Point(i, y);
                    }
				}
				else
				{
					if (i != x &&
						Model.BoardNumbers[i][y].Value != 0 &&
						Model.BoardNumbers[i][y].Value == value)
					{
						return new Point(i, y);;
					}
				}
            }

            return null;
        }

        /// <summary>
        /// Tests columns for conflicting cells
        /// </summary>
        /// <param name="x">X coordinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <param name="value">Number to test</param>
        /// <param name="useCopy">Tells whether we use the main array or a copy of it</param>
		/// <returns>Conflicting point or null if no conflict</returns>
        private Point GetConflictsInColumn(int x, int y, int value, bool useCopy)
        {
            for (int i = 0; i < ColumnLength; i++)
            {
				if (useCopy)
				{
                    if (i != y &&
						copyCells[x][i] != 0 &&
						copyCells[x][i] == value)
                    {
                        return new Point(x, i);
                    }
				}
				else
				{
					if (i != y &&
						Model.BoardNumbers[x][i].Value != 0 &&
						Model.BoardNumbers[x][i].Value == value)
					{
						return new Point(x, i);
					}
				}
            }
            return null;
        }

        /// <summary>
        /// Tests blocks (3x3) for conflicting cells
        /// </summary>
        /// <param name="x">X coordinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <param name="value">Number to test</param>
        /// <param name="useCopy">Tells whether we use the main array or a copy of it</param>
		/// <returns>Conflicting point or null if no conflict</returns>
        private Point TestBlock(int x, int y, int value, bool useCopy)
        {
            int blocksFirstX = ((int)(x / 3)) * 3;
            int blocksFirstY = ((int)(y / 3)) * 3;
			for (int i = blocksFirstX; i < blocksFirstX + 3; i++)
			{
				for (int j = blocksFirstY; j < blocksFirstY + 3; j++)
				{
					if (useCopy)
					{
						if (copyCells[i][j] != 0)
							if (i != x && j != y && copyCells[i][j] == value)
							{
								return new Point(i, j);
							}
					}
					else
					{
						if (Model.BoardNumbers[i][j].Value != 0)
							if (i != x && j != y && Model.BoardNumbers[i][j].Value == value)
							{
								return new Point(i, j);
							}
					}
				}
			}
            return null;
        }

        /// <summary>
        /// Tries to set the given number to the given index.
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <param name="value">Number to set</param>
        /// <param name="useCopy">Tells whether we use the main array or a copy of it</param>
        /// <returns>true if it was possible, false otherwise.</returns>
        private bool SetNumber(int x, int y, int value, bool useCopy)
        {
            if (GetConflictsInRow(x, y, value, useCopy) == null &&
                GetConflictsInColumn(x, y, value, useCopy) == null &&
                TestBlock(x, y, value, useCopy) == null)
            {
				if (useCopy)
					copyCells[x][y] = value;
				else
					Model.BoardNumbers[x][y].Value = value;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Copies cell values from main array to the copy array
        /// </summary>
        private void MakeCopy()
        {
			copyCells = new int[RowLength][];

			for (int i = 0; i < RowLength; i++)
			{
				copyCells[i] = new int[ColumnLength];

				for (int j = 0; j < ColumnLength; j++)
					copyCells[i][j] = Model.BoardNumbers[i][j].Value;
			}
        }

        /// <summary>
        /// Checks whether the puzzle has an unique solution.
        /// The algorithm is mainly the same as is fillCell().
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        /// <returns>true if if it does, false otherwise.</returns>
        public bool CheckUniqueness(int x, int y)
        {
            if (y == RowLength)
            {
                if (solutionFound)
                {
                    solutionFound = false;
                    return true;
                }
                solutionFound = true;
                return false;
            }

            int nextX = x + 1, nextY = y;
            if (x == RowLength - 1)
            {
                nextX = 0;
                nextY = y + 1;
            }

			if (Model.BoardNumbers[x][y].Value != 0)
            {
                if (CheckUniqueness(nextX, nextY))
                    return true;
            }
            else
            {
                for (int i = 1; i <= 9; i++)
                    if (SetNumber(x, y, i, true))
                    {
                        if (CheckUniqueness(nextX, nextY))
                            return true;

						Model.BoardNumbers[x][y].Value = 0;
                    }
            }
            return false;
        }

        /// <summary>
        /// Removes numbers from the board until a desired puzzle is obtained.
        /// Checks after every deletion that the puzzle still has an unique solution.
        /// </summary>
        private void RemoveCells()
        {
            while (EmptyCells < MaxEmptyCells)
            {
                int randX = randGen.Next(ColumnLength);
                int randY = randGen.Next(RowLength);
				int temp = Model.BoardNumbers[randX][randY].Value;
                
				if (temp != 0)
                {
					Model.BoardNumbers[randX][randY].Value = 0;
                    MakeCopy();
                    
					if (CheckUniqueness(0, 0))
						Model.BoardNumbers[randX][randY].Value = temp;
                    else
                        EmptyCells++;
                }
            }
        }

        /// <summary>
        /// Generates a random full sudoku board by using recursive
        /// backtracking method.
        /// </summary>
        /// <param name="x">X coordinate on the grid</param>
        /// <param name="y">Y coordinate on the grid</param>
        private bool FillCell(int x, int y)
        {
            if (y == RowLength)
                return true;

            int nextX = x + 1, nextY = y;
            if (x == ColumnLength - 1)
            {
                nextX = 0;
                nextY = y + 1;
            }

            for (int i = 0; i < RowLength; i++)
            {
                if (SetNumber(x, y, randOrder[i], false) && FillCell(nextX, nextY))
                    return true;

				Model.BoardNumbers[x][y].Value = 0;
            }
            return false;
        }

        /// <summary>
        /// Creates an array with numbers 1-9 in it, ordered randomly.
        /// </summary>
        private void FillRandOrder()
        {
            randOrder = new int[RowLength];
            bool isSet = false;
            int rand, j;
            for (int i = 0; i < RowLength; i++)
            {
                while (!isSet)
                {
                    rand = randGen.Next(ColumnLength) + 1;
                    for (j = 0; j < ColumnLength; j++)
                        if (rand == randOrder[j])
                            break;
                    if (j == ColumnLength)
                    {
                        randOrder[i] = rand;
                        isSet = true;
                    }
                }
                isSet = false;
            }
        }

        /// <summary>
        /// Generates a new puzzle
        /// </summary>
        public void GeneratePuzzle()
        {
            for (int i = 0; i < RowLength; i++)
                for (int j = 0; j < ColumnLength; j++)
					Model.BoardNumbers[i][j].Value = 0;

            FillRandOrder();
            FillCell(0, 0);
            EmptyCells = 0;
            PlayerMoves = 0;
            RemoveCells();
            
			for (int i = 0; i < RowLength; i++)
            {
                for (int j = 0; j < ColumnLength; j++)
					Model.BoardNumbers[i][j].SetByGame = Model.BoardNumbers[i][j].Value != 0;
            }
        }

        /// <summary>
        /// Places the number selected by the player on the game board
        /// </summary>
        /// <param name="x">X coordinate to set</param>
        /// <param name="y">Y coordinate to set</param>
        /// <param name="value">Number to set</param>
        public List<Point> SetNumberByPlayer(int x, int y, int value)
        {
			var collisions = new List<Point>();
            
			var rowConflicts = GetConflictsInRow(x, y, value, false);
			var columnConflicts = GetConflictsInColumn(x, y, value, false);
			var blockConflicts = TestBlock(x, y, value, false);

			if (rowConflicts == null &&
				columnConflicts == null &&
				blockConflicts == null)
            {
				if (Model.BoardNumbers[x][y].Value != 0 && value == 0)
                    EmptyCells++;
				else if (Model.BoardNumbers[x][y].Value == 0 && value != 0)
                    EmptyCells--;

				Model.BoardNumbers[x][y].Value = value;
            }

            PlayerMoves++;

			if (rowConflicts != null)
				collisions.Add(rowConflicts);

			if (columnConflicts != null)
				collisions.Add(columnConflicts);

			if (blockConflicts != null)
				collisions.Add(blockConflicts);

			return collisions;
        }
    }
}
