using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SudokuMaster
{
	public class BoardValue	 : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _value;

        /// <summary>
        /// Value representing number in particular cell
        /// </summary>
		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
				NotifyPropertyChanged("Value");
			}
		}

		private bool _setByGame;

		/// <summary>
		/// Flag indicating if the number was set before game started, or by user
		/// </summary>
        public bool SetByGame
		{
			get
			{
				return _setByGame;
			}
			set
			{
				_setByGame = value;
				NotifyPropertyChanged("SetByGame");
			}
		}

		private void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
