/*
 * Copyright (c) 2011 Nokia Corporation.
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

namespace SudokuMaster
{
    /// <summary>
    /// Number selection control which is shown when the player touches a cell on the game grid
    /// </summary>
    public partial class NumberSelection : UserControl
    {
		private SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
		private SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);

		#region properties
		public Thickness KeyboardMargin
		{
			get { return keyboardGrid.Margin; }
			set { keyboardGrid.Margin = value; }
		}

		public Size KeyboardSize
		{
			get { return new Size(keyboardGrid.Width, keyboardGrid.Height); }
		}

		public Action OnClickOutside { get; set; }
		public Action<int> OnSelectedNumber { get; set; }
		#endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cell">The cell select by the player for manipulation</param>
        public NumberSelection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the player touches the screen outside of this control
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        public void OnClickBackground(object sender, MouseButtonEventArgs e)
        {
            // Do not receive these events anymore and start fading out this control
            fadeOutAnimation.Begin();

			if (OnClickOutside != null)
				OnClickOutside();
        }

		public void ShowKeyboard()
		{
			fadeInAnimation.Begin();
			Visibility = System.Windows.Visibility.Visible;
		}
        
        /// <summary>
        /// Called when the fade out animation is completed
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
			this.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Called when the player touches any one of the buttons
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
		private void Button_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
		{
			// Invert the colors of touched text block and its backgroud
			(sender as Button).Background = whiteBrush;
		}
        
        /// <summary>
        /// Called when the player is not touching the button anymore
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void Button_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // Invert the colors of touched text block and its backgroud
            fadeOutAnimation.Begin();
            SoundHelper.PlaySound(SoundHelper.SoundType.NumberChosenSound);

            // Get the value of the text block player pressed and pass it to
            // the game logic. The cell will be cleared if the value is zero.
            int val = 0;
            Int32.TryParse((sender as Button).Content.ToString(), out val);

			if (OnSelectedNumber != null)
				OnSelectedNumber(val);
        }
    }
}
