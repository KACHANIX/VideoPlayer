using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace BanalVideo
{
    public partial class MainWindow : Window
    {
        private string _subPath;
        private bool _isSubChosen;
        private DispatcherTimer _timer;
        private List<Subtitle> _subList;
        private int _subIndex;
        private bool _isDragging;
        private bool _isFullScreen;
        private bool _isPlaying;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space when _isPlaying:
                    Screen.Pause();
                    try
                    {
                        _timer.IsEnabled = false;
                    }
                    catch (NullReferenceException)
                    {
                    }

                    _isPlaying = false;
                    break;

                case Key.Space:
                    Screen.Play();
                    try
                    {
                        _timer.IsEnabled = true;
                    }
                    catch (NullReferenceException)
                    {
                    }

                    _isPlaying = true;
                    break;

                case Key.Down:
                    Volume.Value -= 0.05;
                    break;

                case Key.Up:
                    Volume.Value += 0.05;
                    break;

                case Key.Left:
                    try
                    {
                        Screen.Position = new TimeSpan(0, 0, ((int) TimeLine.Value - 5));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    break;

                case Key.Right:
                    try
                    {
                        Screen.Position = new TimeSpan(0, 0, 0, ((int) TimeLine.Value + 5));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    break;

                case Key.F11:
                    ChangeWindowState();
                    break;

                case Key.Escape when _isFullScreen:
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    _isFullScreen = false;

                    break;
            }

            e.Handled = true;
        }

        private void LoadSubtitle(string path)
        {
            _subIndex = 0;
            int i = 0;
            using (var sr = new StreamReader(path, Encoding.Default, true))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var temp = i == 0 ? sr.ReadLine() : line;
                    Subtitle sub = new Subtitle
                    {
                        index = i
                    };
                    var s = temp?.Substring(0, temp.IndexOf(" ", StringComparison.Ordinal));
                    sub.Start = TimeSpan.Parse(s.Replace(',', '.'));
                    s = temp.Substring(temp.IndexOf(">", StringComparison.Ordinal) + 1).Trim();
                    sub.End = TimeSpan.Parse(s.Replace(',', '.'));

                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s == (i + 2).ToString())
                            break;
                        sub.Text += s;
                    }

                    _subList.Add(sub);
                    i++;
                }
            }
        }

        private void Mouse_Enter(object sender, MouseEventArgs e)
        {
            ((UIElement) sender).Opacity = 1;
        }

        private void Mouse_Leave(object sender, MouseEventArgs e)
        {
            ((UIElement) sender).Opacity = 0.2;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isDragging)
            {
                TimeLine.Value = Screen.Position.TotalSeconds;
            }

            try
            {
                Position.Text = Screen.Position.ToString().Remove(8);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void Timer_Tick_With_Subs(object sender, EventArgs e)
        {
            if (!_isDragging)
            {
                TimeLine.Value = Screen.Position.TotalSeconds;
            }

            for (var i = 0; i < _subList.Count; i++)
            {
                if (Screen.Position < _subList[i].Start || Screen.Position > _subList[i].End) continue;
                _subIndex = i;
                break;
            }

            if (_subIndex < _subList.Count)
            {
                if (Screen.Position >= _subList[_subIndex].Start && Screen.Position <= _subList[_subIndex].End)
                {
                    Subtitle.Text = _subList[_subIndex].Text;
                }
                else
                {
                    Subtitle.Text = "";
                }
            }

            try
            {
                Position.Text = Screen.Position.ToString().Remove(8);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void Screen_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeLine.Maximum = Screen.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Screen.Volume = Volume.Value;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                Screen.Pause();
                try
                {
                    _timer.IsEnabled = false;
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("OOPS!");
                }

                _isPlaying = false;
            }
            else
            {
                Screen.Play();
                try
                {
                    _timer.IsEnabled = true;
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("OOPS");
                }

                _isPlaying = true;
            }
        }

        private void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            Subtitle.Text = "";

            //Check if the video isn't chosen yet
            try
            {
                _timer.Tick -= Timer_Tick_With_Subs;
            }
            catch (NullReferenceException)
            {
                // Ignore
            }

            var fileToOpen = new OpenFileDialog
            {
                Filter = "Видео (*.MP4;*.AVI;*.MKV)|*.MP4;*.AVI;*.MKV", CheckFileExists = true
            };
            fileToOpen.ShowDialog();
            var path = fileToOpen.FileName;
            if (path == "")
                return;
            Screen.Source = new Uri(path, UriKind.Absolute);
            TimeLine.Value = 0;

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += Timer_Tick;
            _timer.Start();
            Screen.Play();
            _isPlaying = true;

            _subList = null;
        }

        private void TimeLine_DragEnter(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void TimeLine_DragLeave(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
            var sliderValue = (int) TimeLine.Value;
            var newPosition = new TimeSpan(0, 0, sliderValue);
            Screen.Position = newPosition;
        }

        private void SubtitleButton_Click(object sender, RoutedEventArgs e)
        {
            _isSubChosen = false;
            var fileToOpen = new OpenFileDialog
            {
                Filter = "Субтитры (*.srt)|*.srt"
            };
            fileToOpen.ShowDialog();
            var path = fileToOpen.FileName;
            if (path == "")
                return;
            _subPath = path;
            _isSubChosen = true;
            _subList = new List<Subtitle>();

            if (_subPath == "" || !_isSubChosen)
                return;
            LoadSubtitle(_subPath);

            //Check if the video isn't chosen yet
            try
            {
                _timer.Tick -= Timer_Tick;
            }
            catch (NullReferenceException)
            {
                return;
            }

            _timer.Tick += Timer_Tick_With_Subs;
        }

        private void Screen_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Volume.Value += 0.05;
            else
                Volume.Value -= 0.05;
        }

        private void Screen_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeWindowState();
        }

        private void ChangeWindowState()
        {
            if (!_isFullScreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                _isFullScreen = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                _isFullScreen = false;
            }
        }
    }
}