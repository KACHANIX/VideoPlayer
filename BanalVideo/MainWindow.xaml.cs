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

        public DispatcherTimer Timer;
        public string SubPath;
        bool isPlaying = false;
        int index = 0;
        List<Subtitle> SubtitlesList;
        bool isDragging = false;
        public string currentFile = "";
        public bool isSubChosen;
        private bool isFullScreen = false;
        public MainWindow()
        {
            InitializeComponent();
            Volume.Minimum = 0;
            Volume.Maximum = 1;
            Volume.Value = 0.5;
            Screen.Volume = Volume.Value;
            Screen.Stretch = Stretch.Uniform;
            TimeLine.Minimum = 0;
            Screen.MediaOpened += Screen_MediaOpened;
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.F11)
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
        }

        private void LoadSubtitle(string path)
        {
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
                    SubtitlesList.Add(sub);
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
            (sender as UIElement).Opacity = 0.1;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                TimeLine.Value = Screen.Position.TotalSeconds;
            }
            Position.Text = Screen.Position.ToString().Remove(8);
        }

        private void Timer_Tick_With_Subs(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                TimeLine.Value = Screen.Position.TotalSeconds;
            }
            for (int i = 0; i < SubtitlesList.Count; i++)
            {
                if (Screen.Position >= SubtitlesList[i].Start && Screen.Position <= SubtitlesList[i].End)
                {
                    index = i;
                    break;
                }
            }

            if (index < SubtitlesList.Count)
            {
                if (Screen.Position >= SubtitlesList[index].Start && Screen.Position <= SubtitlesList[index].End)
                    Subtitle.Text = SubtitlesList[index].Text;
                else Subtitle.Text = "";
            }
            Position.Text = Screen.Position.ToString().Remove(8);
        }

        private void Screen_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeLine.Maximum = Screen.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Screen.Volume = (double)Volume.Value;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                Screen.Pause();
                try
                {
                    Timer.IsEnabled = false;
                }
                catch (NullReferenceException) { }
                isPlaying = false;
            }
            else
            {
                Screen.Play();
                try
                {
                    Timer.IsEnabled = true;
                }
                catch (NullReferenceException) { }
                isPlaying = true;
            }

        }

        private void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            Subtitle.Text = "";
            try
            {

                Timer.Tick -= Timer_Tick_With_Subs;

                Timer.Tick += Timer_Tick;
                SubtitlesList = null;

            }
            catch (Exception) { }
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
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
                
            };

            
            Timer.Tick -= Timer_Tick_With_Subs;
                        
            Timer.Tick += Timer_Tick;
            SubtitlesList = null;

            Timer.Start();
            Screen.Play();

            TimeLine.Opacity = 0.1;
            PlayButton.Opacity = 0.1;
            FileOpenButton.Opacity = 0.1;
            Volume.Opacity = 0.1;
            SubtitleButton.Opacity = 0.1;

            isPlaying = true;

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
        
        private void TimeLine_DragEnter(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void TimeLine_DragLeave(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            int SliderValue = (int)TimeLine.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, SliderValue);
            Screen.Position = ts;
        }

        private void SubtitleButton_Click(object sender, RoutedEventArgs e)
        {
            isSubChosen = false;
            SubtitleChoose subtitleChoose = new SubtitleChoose();
            subtitleChoose.ShowDialog();
            SubtitlesList = new List<Subtitle>();
            if (SubPath == "" || !isSubChosen)
                return;
            LoadSubtitle(SubPath);
            try
            {
                Timer.Tick -= Timer_Tick;
            }
            catch (System.NullReferenceException)
            {
                return;
            }
            Timer.Tick += Timer_Tick_With_Subs;
        }
    }
}
