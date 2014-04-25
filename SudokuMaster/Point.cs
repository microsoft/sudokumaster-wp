/*
 * Copyright (c) 2011-2014 Microsoft Mobile.
 */

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SudokuMaster
{
	public class Point
	{
		public int X;
		public int Y;

		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}
}
