/*
 * Copyright (c) 2011-2014 Microsoft Mobile.
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SudokuMaster
{
    /// <summary>
    /// Represents a single cell on the board grid
    /// </summary>
    public partial class Cell : UserControl
    {
		// dependency properties so that we can use databinding in our custom control
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(Cell), new PropertyMetadata(OnValueChanged));
		public static readonly DependencyProperty SetByGameProperty = DependencyProperty.Register("SetByGame", typeof(bool), typeof(Cell), new PropertyMetadata(true, OnSetByGameChanged)); 
																					 
		// static brushes - optimization so that we don't create them every single time
		private static SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
		private static SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

		#region properties
		public bool SetByGame
		{
			get { return (bool)GetValue(SetByGameProperty); }
			set { SetValue(SetByGameProperty, value); }
		}

		public int Value
		{
			get { return (int)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public bool IsPlayerSettable
		{
			get { return !SetByGame; }
		} 
		#endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="panel">Parent, or owner of this cell</param>
        public Cell()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when manipulation of the cell is completed.
        /// It's fired when user takes the finger off the screen,
        /// even if the fingers have drifted away from the control.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void UserControl_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // Start the fade out animation which returns the cell's color back to original.
            fadeOutAnimation.Begin();

            if (!IsPlayerSettable)
				SoundHelper.PlaySound(SoundHelper.SoundType.CellSelectedSound);
			else
                SoundHelper.PlaySound(SoundHelper.SoundType.NumberChosenSound);
        }

        /// <summary>
        /// Called when player touches the cell.
        /// Starts the fade in animation which gradually makes the cell red.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            fadeInAnimation.Begin();
        }

		/// <summary>
		/// Event fired when our SetByGame dependecy property change
		/// </summary>
		private static void OnSetByGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var cell = d as Cell;
			var value = (bool)e.NewValue;

			cell.textValue.Foreground = value ? blackBrush : whiteBrush;
		}

		/// <summary>
		/// Blink the cell
		/// </summary>
		public void Blink()
		{
			blinkAnimation.Begin();
		}
		
		/// <summary>
		/// Event fired when our Value dependecy property change
		/// </summary>
		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var cell = d as Cell;
			var value = (int)e.NewValue;

			if (value == 0)
				cell.textValue.Text = "";
			else
				cell.textValue.Text = value.ToString();
		}
	}
}
