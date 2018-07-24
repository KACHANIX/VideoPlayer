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
using System.Windows.Forms;

namespace BanalVideo
{
    /// <summary>
    /// Interaction logic for Browser.xaml
    /// </summary>
    public partial class Browser : Window
    {
        public Browser()
        {
            InitializeComponent();
            (wfhSample.Child as System.Windows.Forms.WebBrowser).ScriptErrorsSuppressed = true;
            string FileName = ((MainWindow)System.Windows.Application.Current.MainWindow).currentFile == "" ?
                "https://subscene.com/subtitles" :
                "https://" + $"subscene.com/subtitles/{((MainWindow)System.Windows.Application.Current.MainWindow).currentFile.ToLower().Replace(' ', '-')}/english";
            (wfhSample.Child as System.Windows.Forms.WebBrowser).Navigate(FileName);
        }
    }
}
