/*
 * Copyright (c) 2011-2014 Microsoft Mobile.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using Microsoft.Phone.Controls;

namespace SudokuMaster
{
    /// <summary>
    /// The highscores page.
    /// The page contains just the title and a listbox. Loading and saving
    /// the scores is done with XmlSerializer.
    /// </summary>
    public partial class Highscores : PhoneApplicationPage
    {
        const string highscoresFilename = "highscores.xml";
        public static List<HighscoreItem> scores;

        /// <summary>
        /// Constructor
        /// Initializes the component and populates the listbox.
        /// </summary>
        public Highscores()
        {
            InitializeComponent();
            HighscoreList.ItemsSource = scores;
        }

        /// <summary>
        /// Loads the highscores from isolated storage
        /// </summary>
        static public void Load()
        {
            scores = new List<HighscoreItem>();
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

            // Create empty list if the highscores file does not exist.
            // This is needed when the application is started for the first time.
            if (!store.FileExists(highscoresFilename))
            {
                for (int i = 1; i <= 20; i++)
                {
                    scores.Add(new HighscoreItem(i, "Sudokumaster",
                        new TimeSpan(0, 59, 59), 100));
                }
                Save();
                return;
            }

            // Open the file and use XmlSerializer to deserialize the xml file into
            // a list of HighscoreItems.
			using (IsolatedStorageFileStream stream = store.OpenFile(highscoresFilename, FileMode.Open))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					XmlSerializer serializer = new XmlSerializer(scores.GetType());
					scores = (List<HighscoreItem>)serializer.Deserialize(reader);
				}
			}
        }

        /// <summary>
        /// Saves the highscores to isolated storage.
        /// </summary>
        static public void Save()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

            // Open the file and use XmlSerializer to serialize the list into the file
			using (IsolatedStorageFileStream stream = store.CreateFile(highscoresFilename))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					XmlSerializer serializer = new XmlSerializer(scores.GetType());
					serializer.Serialize(writer, scores);
                    writer.Flush();
				}
			}
        }

        /// <summary>
        /// Checks if given score is a new highscore
        /// </summary>
        /// <param name="score">Score to check. The score should contain at least the solving time and moves.</param>
        /// <returns>The position in highscore list, or zero if the score doesn't make it to the list</returns>
        static public int IsNewHighscore(HighscoreItem score)
        {
            foreach (HighscoreItem item in scores)
            {
                // Check the time, and if the times are the same, check the
                // moves needed to solve the puzzle
                if (score.Time < item.Time ||
                    (score.Time == item.Time && score.Moves < item.Moves))
                    return item.Index;
            }

            return 0;
        }

        /// <summary>
        /// Add a new score to highscore list
        /// </summary>
        /// <param name="score">Score to add. All members of the score should be filled.</param>
        static public void AddNewHighscore(HighscoreItem score)
        {
            // Insert the score into the list, remove weakest score from the list
            // and save the list.
            if (score.Index <= 0)
                return;
            scores.Insert(score.Index - 1, score);
            scores.RemoveAt(scores.Count - 1);
            for (int t = score.Index; t < scores.Count; t++)
                scores[t].Index++;
            Save();
        }    
    }
}
