/*
 * Copyright (c) 2011-2014 Microsoft Mobile.
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace SudokuMaster
{
	public class SoundHelper
	{
        public enum SoundType
        {
            GameEndSound,
            CellSelectedSound,
            NumberChosenSound
        }

        private static SoundEffect gameEndEffect = LoadSound("sounds/60443__jobro__tada1.wav");
        private static SoundEffect cellSelectedEffect = LoadSound("sounds/7040__yawfle__050816_chair_04.wav");
        private static SoundEffect numberChosenEffect = LoadSound("sounds/7043__yawfle__050816_chair_07.wav");

        private static SoundEffect LoadSound(string path)
        {
            using (var stream = TitleContainer.OpenStream(path))
            {
                return SoundEffect.FromStream(stream);
            }
        }

		/// <summary>
		/// Playes given sound file
		/// </summary>
		/// <param name="soundFile"> Path and name of a wav file to be played</param>
		static public void PlaySound(SoundType type)
		{
            //using (var stream = TitleContainer.OpenStream(soundFile))
            //{
            //    var effect = SoundEffect.FromStream(stream);
            //    FrameworkDispatcher.Update();
            //    effect.Play();
            //}
            FrameworkDispatcher.Update();
            switch (type)
            {
                case SoundType.CellSelectedSound:
                    cellSelectedEffect.Play();
                    break;
                case SoundType.GameEndSound:
                    gameEndEffect.Play();
                    break;
                case SoundType.NumberChosenSound:
                    numberChosenEffect.Play();
                    break;
                    
            }
		}
	}
}
