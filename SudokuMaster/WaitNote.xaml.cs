/*
 * Copyright (c) 2011-2014 Microsoft Mobile.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Globalization;

namespace SudokuMaster
{
    /// <summary>
    /// Wait note, spinning circle animation
    /// </summary>
    public partial class WaitNote : UserControl
    {
        /// <summary>
        /// Constructor
        /// Starts the spin animation immediately.
        /// </summary>
        public WaitNote()
        {
            InitializeComponent();
        }


		public void StartSpin()
		{
			spinAnimation.Begin();
		}

		public void StopSpin()
		{
			spinAnimation.Stop();
		}
    }
}
