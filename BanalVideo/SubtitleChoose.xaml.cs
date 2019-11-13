using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BanalVideo
{
    public partial class SubtitleChoose : Window
    {
        public SubtitleChoose()
        {
            InitializeComponent();
        }

        private void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            Browser browser = new Browser();
            browser.ShowDialog();
            SelectAndPassBack();
            this.Close();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAndPassBack();
            this.Close();
        }
        private void SelectAndPassBack()
        {
            string path = "";
            OpenFileDialog FileToOpen = new OpenFileDialog
            {
                Filter = "Субтитры (*.srt)|*.srt"
            };
            FileToOpen.ShowDialog();
            path = FileToOpen.FileName;
            if (path == "")
                this.Close();

            ((MainWindow)Application.Current.MainWindow).subPath = path;
            ((MainWindow)Application.Current.MainWindow).isSubChosen = true;
        }
    }
}
