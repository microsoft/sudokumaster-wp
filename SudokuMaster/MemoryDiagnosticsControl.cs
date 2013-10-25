/*
 * Copyright (c) 2011 Nokia Corporation.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Phone.Info;

namespace SudokuMaster.Debug
{
    /// <summary>
    /// Helper class for showing current memory usage.
    /// Run only in debug mode.
    /// </summary>

    public static class MemoryDiagnosticsControl
    {
        static Popup _popup;
        static TextBlock _currentMemoryBlock;
        static DispatcherTimer _timer;
        static bool _forceGc;

        /// <summary>
        /// Show the memory counter
        /// </summary>
        /// <param name="forceGc">Whether or not to do automatic garbage collection each tick</param>

        public static void Start(bool forceGc)
        {
            _forceGc = forceGc;

            CreatePopup();
            CreateTimer();
            ShowPopup();
            StartTimer();
        }

        /// <summary>
        /// Stop the memory counter
        /// </summary>

        public static void Stop()
        {
            HidePopup();
            StopTimer();
        }

        /// <summary>
        /// Show the popup
        /// </summary>

        static void ShowPopup()
        {
            _popup.IsOpen = true;
        }

        static void StartTimer()
        {
            _timer.Start();
        }

        static void CreateTimer()
        {
            if (_timer != null)
                return;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _timer.Tick += TimerTick;
        }


        static void TimerTick(object sender, EventArgs e)
        {
            // call Garbage collector before getting memory usage
            if (_forceGc)
                GC.Collect();
            var mem = (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
            _currentMemoryBlock.Text = string.Format("{0:N}", mem / 1024);
        }

        static void CreatePopup()
        {

            if (_popup != null)

                return;
            _popup = new Popup();
            var fontSize = (double)Application.Current.Resources["PhoneFontSizeSmall"] - 2;
            var foreground = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Background = (Brush)Application.Current.Resources["PhoneSemitransparentBrush"] };
            _currentMemoryBlock = new TextBlock { Text = "---", FontSize = fontSize, Foreground = foreground };
            sp.Children.Add(new TextBlock { Text = "Mem(kB): ", FontSize = fontSize, Foreground = foreground });
            sp.Children.Add(_currentMemoryBlock);
            sp.RenderTransform = new CompositeTransform { Rotation = 90, TranslateX = 480, TranslateY = 420, CenterX = 0, CenterY = 0 };
            _popup.Child = sp;
        }

        static void StopTimer()
        {
            _timer.Stop();
        }

        static void HidePopup()
        {
            _popup.IsOpen = false;
        }
    }
}
