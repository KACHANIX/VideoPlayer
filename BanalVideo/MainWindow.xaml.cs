using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text;
using System.Collections.Generic;
using System.Windows.Input;

namespace BanalVideo
{
    public partial class MainWindow : Window
    {

        public string subPath;
        public string currentFile;
        public bool isSubChosen;

        DispatcherTimer timer;
        List<Subtitle> subList;
        int subIndex = 0;

        bool isDragging = false;
        bool isFullScreen = false;
        bool isPlaying = false;
        public MainWindow()
        {
            InitializeComponent();

            TimeLine.MouseEnter += Mouse_Enter;
            PlayButton.MouseEnter += Mouse_Enter;
            FileOpenButton.MouseEnter += Mouse_Enter;
            Volume.MouseEnter += Mouse_Enter;
            SubtitleButton.MouseEnter += Mouse_Enter;

            TimeLine.MouseLeave += Mouse_Leave;
            PlayButton.MouseLeave += Mouse_Leave;
            FileOpenButton.MouseLeave += Mouse_Leave;
            Volume.MouseLeave += Mouse_Leave;
            SubtitleButton.MouseLeave += Mouse_Leave;
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (isPlaying)
                {
                    Screen.Pause();
                    try
                    {
                        timer.IsEnabled = false;
                    }
                    catch (NullReferenceException) { }
                    isPlaying = false;
                }
                else
                {
                    Screen.Play();
                    try
                    {
                        timer.IsEnabled = true;
                    }
                    catch (NullReferenceException) { }
                    isPlaying = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                Volume.Value -= 0.05;
            }
            else if (e.Key == Key.Up)
            {
                Volume.Value += 0.05;
            }
            else if (e.Key == Key.Left)
            {
                try
                {
                    Screen.Position = new TimeSpan(0, 0, ((int)TimeLine.Value - 5));
                }
                catch (ArgumentOutOfRangeException) { }
            }
            else if (e.Key == Key.Right)
            {
                try
                {
                    Screen.Position = new TimeSpan(0, 0, 0, ((int)TimeLine.Value + 5));
                }
                catch (ArgumentOutOfRangeException) { }
            }
            else if (e.Key == Key.F11)
            {
                if (!isFullScreen)
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    isFullScreen = true;
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    isFullScreen = false;
                }
            }
            else if (e.Key == Key.Escape && isFullScreen)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                isFullScreen = false;
            }

            e.Handled = true;
        }

        private void LoadSubtitle(string path)
        {
            subIndex = 0;
            int i = 0;
            using (StreamReader sr = new StreamReader(path, Encoding.Default, true))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string tempstr = null;
                    if (i == 0)
                        tempstr = sr.ReadLine();
                    else tempstr = line;
                    Subtitle sub = new Subtitle
                    {
                        index = i
                    };
                    string s = tempstr.Substring(0, tempstr.IndexOf(" "));
                    sub.Start = TimeSpan.Parse(s.Replace(',', '.'));
                    s = tempstr.Substring(tempstr.IndexOf(">") + 1).Trim();
                    sub.End = TimeSpan.Parse(s.Replace(',', '.'));

                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s == (i + 2).ToString())
                            break;
                        sub.Text += s;
                    }
                    subList.Add(sub);
                    i++;
                }
            }
        }

        private void Mouse_Enter(object sender, MouseEventArgs e)
        {
            (sender as UIElement).Opacity = 1;
        }

        private void Mouse_Leave(object sender, MouseEventArgs e)
        {
            (sender as UIElement).Opacity = 0.2;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                TimeLine.Value = Screen.Position.TotalSeconds;
            }
            try
            {
                Position.Text = Screen.Position.ToString().Remove(8);
            }
            catch (Exception) { }
        }

        private void Timer_Tick_With_Subs(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                TimeLine.Value = Screen.Position.TotalSeconds;
            }
            for (int i = 0; i < subList.Count; i++)
            {
                if (Screen.Position >= subList[i].Start && Screen.Position <= subList[i].End)
                {
                    subIndex = i;
                    break;
                }
            }

            if (subIndex < subList.Count)
            {
                if (Screen.Position >= subList[subIndex].Start && Screen.Position <= subList[subIndex].End)
                {
                    Subtitle.Text = subList[subIndex].Text;
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
            catch (Exception) { }
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
            if (isPlaying)
            {
                Screen.Pause();
                try
                {
                    timer.IsEnabled = false;
                }
                catch (NullReferenceException) { MessageBox.Show("asd"); }
                isPlaying = false;
            }
            else
            {
                Screen.Play();
                try
                {
                    timer.IsEnabled = true;
                }
                catch (NullReferenceException) { MessageBox.Show("asd"); }
                isPlaying = true;
            }
        }

        private void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            Subtitle.Text = "";

            //Check if the video isn't chosen yet
            try
            {
                timer.Tick -= Timer_Tick_With_Subs;
            }
            catch (NullReferenceException) { }

            string path = "";
            OpenFileDialog FileToOpen = new OpenFileDialog();
            FileToOpen.Filter = "Видео (*.MP4;*.AVI;*.MKV)|*.MP4;*.AVI;*.MKV";
            FileToOpen.CheckFileExists = true;
            FileToOpen.ShowDialog();
            path = FileToOpen.FileName;
            if (path == "")
                return;
            currentFile = Path.GetFileName(path).Split('.')[0];
            Screen.Source = new Uri(path, UriKind.Absolute);
            TimeLine.Value = 0;

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
            Screen.Play();
            isPlaying = true;

            subList = null;

        }

        private void TimeLine_DragEnter(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void TimeLine_DragLeave(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            int SliderValue = (int)TimeLine.Value;
            TimeSpan newPosition = new TimeSpan(0, 0, SliderValue);
            Screen.Position = newPosition;
        }

        private void SubtitleButton_Click(object sender, RoutedEventArgs e)
        {
            isSubChosen = false;
            SubtitleChoose subtitleChoose = new SubtitleChoose();
            subtitleChoose.ShowDialog();
            subList = new List<Subtitle>();
            if (subPath == "" || !isSubChosen)
                return;
            LoadSubtitle(subPath);

            //Check if the video isn't chosen yet
            try
            {
                timer.Tick -= Timer_Tick;
            }
            catch (NullReferenceException)
            {
                return;
            }
            timer.Tick += Timer_Tick_With_Subs;
        }
    }
}
