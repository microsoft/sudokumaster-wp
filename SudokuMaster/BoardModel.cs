/*
 * Copyright (c) 2011 Nokia Corporation.
 */


namespace SudokuMaster
{
	public class BoardModel
	{
		public BoardValue[][] BoardNumbers { get; private set; }

		public BoardModel(int ColumnLength, int RowLength)
		{
			BoardNumbers = new BoardValue[RowLength][];

			for (int x = 0; x < BoardNumbers.Length; x++)
				BoardNumbers[x] = new BoardValue[ColumnLength];

			// create the actual objects
			for (int x = 0; x < BoardNumbers.Length; x++)
			{
				for (int y = 0; y < BoardNumbers[x].Length; y++)
					BoardNumbers[x][y] = new BoardValue();
			}
		}
	}
}
