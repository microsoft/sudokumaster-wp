/*
 * Copyright (c) 2011-2014 Microsoft Mobile.
 */

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace SudokuMaster
{
    /// <summary>
    /// Main page of the application, the game itself
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Possible game states; game not started yet, ongoing and game over
        /// </summary>
        enum GameState
        {
            NotStarted = 0,
            Ongoing,
            GameOver
        };

        const String gameStateFile = "gamestate.dat";
        private GameLogic game;
        private GameState gameState = GameState.NotStarted;
        private DispatcherTimer gameTimer;
        private DateTime gameStartTime;
        private DateTime gamePausedTime;
        private TimeSpan gameTimeElapsed;
		private Cell[][] cells;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // Initialize the grid and instantiate the logic
            game = new GameLogic();
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += StatusTimerTick;

            gamePausedTime = new DateTime();

			cells = CreateGrid();

            // For tombstoning; listen for deactivated event and restore state
            // if the application was deactivated earlier.
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(App_Deactivated);
            RestoreState();
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="r">Event arguments</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (gameState == GameState.Ongoing && gamePausedTime > gameStartTime)
            {
                gameStartTime += DateTime.Now - gamePausedTime;
                UpdateStatus();
            }           
        }

        /// <summary>
        /// Event handler for the highscores -button.
        /// Navigates to the highscores page.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void HighscoresButton_Click(object sender, EventArgs e)
        {
            gamePausedTime = DateTime.Now;

            NavigationService.Navigate(new Uri("/HighscoresPage.xaml",
                UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Event handler for the new game -button
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void NewGameButton_Click(object sender, EventArgs e)
        {
            if (gameState == GameState.Ongoing)
            {
                MessageBoxResult result = MessageBox.Show("Do you really want to start new game? \nAny game progress will be lost.", "", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    NewGame();
                }
            }
            else
                NewGame();
        }


        /// <summary>
        /// Event handler for the status timer
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        private void StatusTimerTick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        /// <summary>
        /// Generates a new puzzle and starts the game
        /// </summary>
        private void NewGame()
        {
            // Close the GameOver dialog if it was still active
            GameOver gameOver = LayoutRoot.Children.OfType<GameOver>().SingleOrDefault();
            LayoutRoot.Children.Remove(gameOver);

			numberSelection.Visibility = System.Windows.Visibility.Collapsed;

			// Display wait note (spinning circle)
			waitIndicator.Visibility = System.Windows.Visibility.Visible;
			waitIndicator.StartSpin();

			// Disable databinding while generating puzzle
			DataContext = null;

			// Puzzle generation takes couple of seconds, do it in another thread
			ThreadPool.QueueUserWorkItem(dummy =>
				{
					// generating puzzle doesn't touch UI so it can run on another thread
					game.GeneratePuzzle();

					// switching to UI thread to modify UI components
					Deployment.Current.Dispatcher.BeginInvoke(() =>
						{
							DataContext = game.Model; // let's turn on databinding again
							gameTimer.Start();
							gameStartTime = DateTime.Now;
							gameState = GameState.Ongoing;
							UpdateStatus();
							waitIndicator.Visibility = System.Windows.Visibility.Collapsed;
							waitIndicator.StopSpin();
						});
				});
        }

        /// <summary>
        /// Updates status to UI; player moves, empty cells and game time
        /// </summary>
        private void UpdateStatus()
        {
            gameTimeElapsed = DateTime.Now - gameStartTime;

            GameTime.Text  = String.Format("{0:D1}:{1:D2}:{2:D2}",
                gameTimeElapsed.Hours, gameTimeElapsed.Minutes, gameTimeElapsed.Seconds);

            Empty.Text = game.EmptyCells.ToString();
            Moves.Text = game.PlayerMoves.ToString();

        }

        /// <summary>
        /// Ends current game. Called when all the cells are filled.
        /// </summary>
        private void GameEnds()
        {
            gameTimer.Stop();

            // Blink all cells and prevent the player from modifying the cells
            for (int row = 0; row < GameLogic.RowLength; row++)
            {
                for (int col = 0; col < GameLogic.ColumnLength; col++)
                {
					game.Model.BoardNumbers[row][col].SetByGame = true; // to block the user input
                    cells[row][col].Blink();
                }
            }

            // Display the score with GameOver dialog
            HighscoreItem score = new HighscoreItem();
            score.Time = new TimeSpan(gameTimeElapsed.Days, gameTimeElapsed.Hours,
                gameTimeElapsed.Minutes, gameTimeElapsed.Seconds, 0);
            score.Moves = game.PlayerMoves;


			//TODO: move this to XAML
            GameOver gameOver = new GameOver(score);
            // Main page is divided into 2x3 grid. Make sure the row and column
            // properties are set properly (position 0,0 with span 2,3) to make
            // the dialog visible anywhere on the page.
            gameOver.SetValue(Grid.RowSpanProperty, 3);
            gameOver.SetValue(Grid.ColumnSpanProperty, 2);
            gameOver.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Center);
            gameOver.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            gameOver.SetValue(MarginProperty, new Thickness(10, 0, 0, 0));

            LayoutRoot.Children.Add(gameOver);
            gameState = GameState.GameOver;

			SoundHelper.PlaySound(SoundHelper.SoundType.GameEndSound);
        }

        /// <summary>
        /// Creates the grid cells and populates the board grid with the cells
        /// </summary>
        /// <returns>9x9 array of empty cells</returns>
		private Cell[][] CreateGrid()
		{
			var darkImage = new BitmapImage(new Uri("/gfx/darkGridItem.png", UriKind.Relative));
			var lightImage = new BitmapImage(new Uri("/gfx/lightGridItem.png", UriKind.Relative));

			bool lightCell = false;
			Cell[][] cells = new Cell[GameLogic.RowLength][];

			for (int row = 0; row < GameLogic.RowLength; row++)
			{
				cells[row] = new Cell[GameLogic.ColumnLength];

				if (row % GameLogic.BlockSize != 0)
					lightCell = !lightCell;

				for (int col = 0; col < GameLogic.ColumnLength; col++)
				{
                    // switch image type (light or dark) after each 3 cells in row
					if (col % GameLogic.BlockSize == 0)
						lightCell = !lightCell;

					Cell c = new Cell();
					c.SetValue(Grid.RowProperty, row);
					c.SetValue(Grid.ColumnProperty, col);
					c.BackgroundImage.Source = lightCell ? lightImage : darkImage;
                   
                    // install event handler
					c.MouseLeftButtonDown += new MouseButtonEventHandler(OnCellTouched);

                    // set binding to proper BoardValue from BoardViewModel
					Binding b = new Binding(string.Format("BoardNumbers[{0}][{1}].Value", row, col));
					c.SetBinding(Cell.ValueProperty, b);

					Binding b2 = new Binding(string.Format("BoardNumbers[{0}][{1}].SetByGame", row, col)); 
					c.SetBinding(Cell.SetByGameProperty, b2);

					cells[row][col] = c;
					BoardGrid.Children.Add(c);
				}
			}

			return cells;
		}

		private void OnCellTouched(object sender, MouseButtonEventArgs e)
		{
			Cell cell = sender as Cell;

			if (!cell.IsPlayerSettable)
				return;

			// This lambda experssion will allow us to have access to destination cell in a clean way
			numberSelection.OnSelectedNumber = (selectedNumber => OnNumberChoosen(cell, selectedNumber));

			// Place the dialog above the cell, but make sure the dialog fits on the screen.
			numberSelection.KeyboardMargin = GetPositionForCell(cell);
			
			// Change the visibility + start fade in animation
			numberSelection.ShowKeyboard();
		}

		/// <summary>
		/// Helper method to get the absolute position with respect to the screen borders
		/// </summary>
		private Thickness GetPositionForCell(Cell cell)
		{
			var pos = new System.Windows.Point(cell.ActualWidth / 2 - numberSelection.KeyboardSize.Width / 2, cell.ActualHeight / 2 - numberSelection.KeyboardSize.Height / 2);
			pos = cell.TransformToVisual(LayoutRoot).Transform(pos);

			if (pos.X < 0)
				pos.X = 0;
			else if (pos.X > LayoutRoot.ActualWidth - numberSelection.KeyboardSize.Width)
				pos.X = LayoutRoot.ActualWidth - numberSelection.KeyboardSize.Width;

			if (pos.Y < 0)
				pos.Y = 0;
			else if (pos.Y > LayoutRoot.ActualHeight - numberSelection.KeyboardSize.Height)
				pos.Y = LayoutRoot.ActualHeight - numberSelection.KeyboardSize.Height;

			return new Thickness(pos.X, pos.Y, 0, 0);
		}

		/// <summary>
		/// Action triggered when user selected a number
		/// </summary>
		private void OnNumberChoosen(Cell sender, int number)
		{
			var conflictingCells = game.SetNumberByPlayer((int)sender.GetValue(Grid.RowProperty),
														  (int)sender.GetValue(Grid.ColumnProperty), 
														  number);

			foreach (var point in conflictingCells)
				cells[point.X][point.Y].Blink();

            SoundHelper.PlaySound(SoundHelper.SoundType.CellSelectedSound);
            
            if (gameState != GameState.NotStarted && game.EmptyCells == 0)
                GameEnds();
		}

        /// <summary>
        /// Event handler for application deactivation.
        /// Stores the current game state into a file.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="r">Event arguments.</param>
        void App_Deactivated(object sender, DeactivatedEventArgs e)
        {
            StoreState();
        }

        /// <summary>
        /// Reads the game state from a file and continues the game from where
        /// it was left.
        /// </summary>
        private void RestoreState()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(gameStateFile))
                return;

			int emptyCells = 0;
			using (IsolatedStorageFileStream stream = store.OpenFile(gameStateFile, FileMode.Open))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					// Read the state and stats
					gameState = (GameState)reader.ReadInt32();
					game.PlayerMoves = reader.ReadInt32();
					gameTimeElapsed = new TimeSpan(reader.ReadInt64());
					gameStartTime = DateTime.Now - gameTimeElapsed;

					// Read contents of the cells
					for (int row = 0; row < GameLogic.RowLength; row++)
					{
						for (int col = 0; col < GameLogic.ColumnLength; col++)
						{
							int value = reader.ReadInt32();
							game.Model.BoardNumbers[row][col].Value = value;
							game.Model.BoardNumbers[row][col].SetByGame = reader.ReadBoolean();

							if (value == 0)
								emptyCells++;
						}
					}
				}
			}

            store.DeleteFile(gameStateFile);

            if (gameState == GameState.Ongoing)
            {
                game.EmptyCells = emptyCells;
                gameTimer.Start();
            }
            else
            {
                game.EmptyCells = 0;
            }

			DataContext = game.Model;
            UpdateStatus();
        }

        /// <summary>
        /// Stores current game state to a file
        /// </summary>
        private void StoreState()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

			using (IsolatedStorageFileStream stream = store.CreateFile(gameStateFile))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{

			        writer.Write((Int32)gameState);
					writer.Write(game.PlayerMoves);
					writer.Write((Int64)gameTimeElapsed.Ticks);

					// Contents of the cells
					for (int row = 0; row < GameLogic.RowLength; row++)
					{
						for (int col = 0; col < GameLogic.ColumnLength; col++)
						{
							writer.Write(game.Model.BoardNumbers[row][col].Value);
							writer.Write(game.Model.BoardNumbers[row][col].SetByGame);
						}
					}
				}
			}
        }

        /// <summary>
        /// Event handler for orientation changes.
        /// Repositions UI elements depending on the orientation.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="r">Event arguments</param>
        private void PhoneApplicationPage_OrientationChanged(object sender,OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Landscape ||
                e.Orientation == PageOrientation.LandscapeLeft ||
                e.Orientation == PageOrientation.LandscapeRight)
            {
                Logo.SetValue(Grid.RowProperty, 1);
                Logo.SetValue(Grid.ColumnSpanProperty, 1);

                BoardGrid.SetValue(Grid.RowProperty, 0);
                BoardGrid.SetValue(Grid.ColumnProperty, 1);
                BoardGrid.SetValue(Grid.RowSpanProperty, 3);
                BoardGrid.SetValue(Grid.ColumnSpanProperty, 2);

                waitIndicator.SetValue(Grid.RowProperty, 0);
                waitIndicator.SetValue(Grid.ColumnProperty, 1);
                waitIndicator.SetValue(Grid.RowSpanProperty, 3);
                waitIndicator.SetValue(Grid.ColumnSpanProperty, 2);

                Statistics.SetValue(Grid.RowProperty, 1);
                Statistics.SetValue(Grid.RowSpanProperty, 2);
                Statistics.SetValue(Grid.ColumnSpanProperty, 1);

                if(e.Orientation == PageOrientation.LandscapeLeft)
                    LayoutRoot.Margin = new Thickness(0 ,0 ,72 ,0);
                if (e.Orientation == PageOrientation.LandscapeRight)
                    LayoutRoot.Margin = new Thickness(72, 0, 0, 0);

                LayoutRoot.RowDefinitions[0].Height = new GridLength(90);
                LayoutRoot.RowDefinitions[1].Height = new GridLength(90);

                for (int t = 0; t < Statistics.ColumnDefinitions.Count; t++)
                    Statistics.ColumnDefinitions[t].Width = new GridLength(0);

                Statistics.ColumnDefinitions[0].Width = new GridLength(10);
                Statistics.ColumnDefinitions[1].Width = new GridLength(35, GridUnitType.Star);
                Statistics.ColumnDefinitions[2].Width = new GridLength(65, GridUnitType.Star);

                Statistics.RowDefinitions[0].Height = new GridLength(10);
                Statistics.RowDefinitions[1].Height = new GridLength(100, GridUnitType.Star);
                Statistics.RowDefinitions[2].Height = new GridLength(100, GridUnitType.Star);
                Statistics.RowDefinitions[3].Height = new GridLength(100, GridUnitType.Star);
                Statistics.RowDefinitions[4].Height = new GridLength(10);

                Statistics.Height = 192;

                MovesImage.SetValue(Grid.ColumnProperty, 1);
                MovesImage.SetValue(Grid.RowProperty, 1);

                EmptyImage.SetValue(Grid.ColumnProperty, 1);
                EmptyImage.SetValue(Grid.RowProperty, 2);

                GameTimeImage.SetValue(Grid.ColumnProperty, 1);
                GameTimeImage.SetValue(Grid.RowProperty, 3);

                Moves.SetValue(Grid.ColumnProperty, 2);
                Moves.SetValue(Grid.RowProperty, 1);

                Empty.SetValue(Grid.ColumnProperty, 2);
                Empty.SetValue(Grid.RowProperty, 2);

                GameTime.SetValue(Grid.ColumnProperty, 2);
                GameTime.SetValue(Grid.RowProperty, 3);
            }
            else
            {
                Logo.SetValue(Grid.RowProperty, 0);
                Logo.SetValue(Grid.ColumnSpanProperty, 2);

                BoardGrid.SetValue(Grid.RowProperty, 1);
                BoardGrid.SetValue(Grid.ColumnProperty, 0);
                BoardGrid.SetValue(Grid.RowSpanProperty, 1);
                BoardGrid.SetValue(Grid.ColumnSpanProperty, 2);

                waitIndicator.SetValue(Grid.RowProperty, 1);
                waitIndicator.SetValue(Grid.ColumnProperty, 0);
                waitIndicator.SetValue(Grid.RowSpanProperty, 1);
                waitIndicator.SetValue(Grid.ColumnSpanProperty, 2);

                Statistics.SetValue(Grid.RowProperty, 3);
                Statistics.SetValue(Grid.RowSpanProperty, 1);
                Statistics.SetValue(Grid.ColumnSpanProperty, 2);

                LayoutRoot.Margin = new Thickness(0, 0, 0, 72);
                LayoutRoot.RowDefinitions[0].Height = new GridLength(120);
                LayoutRoot.RowDefinitions[1].Height = new GridLength(460);

                for (int t = 0; t < Statistics.RowDefinitions.Count; t++)
                    Statistics.RowDefinitions[t].Height = new GridLength(0);

                Statistics.ColumnDefinitions[0].Width = new GridLength(18);
                Statistics.ColumnDefinitions[1].Width = new GridLength(60, GridUnitType.Star);
                Statistics.ColumnDefinitions[2].Width = new GridLength(75, GridUnitType.Star);
                Statistics.ColumnDefinitions[3].Width = new GridLength(60, GridUnitType.Star);
                Statistics.ColumnDefinitions[4].Width = new GridLength(75, GridUnitType.Star);
                Statistics.ColumnDefinitions[5].Width = new GridLength(60, GridUnitType.Star);
                Statistics.ColumnDefinitions[6].Width = new GridLength(90, GridUnitType.Star);
                Statistics.ColumnDefinitions[7].Width = new GridLength(18);

                Statistics.RowDefinitions[0].Height = new GridLength(100, GridUnitType.Star);

                Statistics.Height = 64;
                
                MovesImage.SetValue(Grid.ColumnProperty, 1);
                MovesImage.SetValue(Grid.RowProperty, 0);

                EmptyImage.SetValue(Grid.ColumnProperty, 3);
                EmptyImage.SetValue(Grid.RowProperty, 0);

                GameTimeImage.SetValue(Grid.ColumnProperty, 5);
                GameTimeImage.SetValue(Grid.RowProperty, 0);

                Moves.SetValue(Grid.ColumnProperty, 2);
                Moves.SetValue(Grid.RowProperty, 0);

                Empty.SetValue(Grid.ColumnProperty, 4);
                Empty.SetValue(Grid.RowProperty, 0);

                GameTime.SetValue(Grid.ColumnProperty, 6);
                GameTime.SetValue(Grid.RowProperty, 0);
            }
        }
    }
}
