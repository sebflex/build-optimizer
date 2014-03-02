using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POESKillTree
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public bool IsIndeterminate
        {
            get { return progressBar1.IsIndeterminate; }
            set { progressBar1.IsIndeterminate = true; }
        }

        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void ShutDown()
        {
            if (this.Dispatcher.CheckAccess())
                this.Close();
            else
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Close();
                }));
        }
    }
}
