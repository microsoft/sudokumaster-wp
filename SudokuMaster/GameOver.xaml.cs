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

namespace SudokuMaster
{
    /// <summary>
    /// The dialog displayed when the puzzle is solved.
    /// Contains puzzle solving time and moves, and a texbox for player's name.
    /// </summary>
    public partial class GameOver : UserControl
    {
        HighscoreItem score;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerScore">The score of the player. At least time and moves must be filled.</param>
        public GameOver(HighscoreItem playerScore)
        {
            InitializeComponent();
            
            // Start the fade in animation
            fadeInAnimation.Begin();

            // Show the position textblock and player name textbox only if the
            // score is good enough to make it to the list.
            score = playerScore;
            int position = Highscores.IsNewHighscore(score);
            score.Index = position;
            if (position > 0)
            {
                playerName.Visibility = Visibility.Visible;
                textBlockPlacement.Text = "Your placement is " + position.ToString();
            }
            else
            {
                playerName.Visibility = Visibility.Collapsed;
                ConfirmButton.Content = "Ok";
                textBlockPlacement.Text = "";
            }

            textBlockTime.Text = "Your time was " + score.Time.ToString();
        }

        /// <summary>
        /// Called when the player presses a key on the keyboard
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void PlayerName_KeyDown(object sender, KeyEventArgs e)
        {
            // Add the score to the list and start fading out when the enter
            // key is pressed
            if (e.Key == Key.Enter)
            {
                Focus();
            }
        }

        /// <summary>
        /// Called when the fade out animation is completed
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            // Remove this control from the parent UI element (game grid). Fade
            // out is just a visual effect, and this control would still exist
            // and receive events when it's not visible.
            (Parent as Panel).Children.Remove(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (score.Index > 0)
            {
                score.Name = playerName.Text;
                Highscores.AddNewHighscore(score);
                playerName.IsReadOnly = true;
            }
            fadeOutAnimation.Begin();
        }
    }
}
