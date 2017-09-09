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

namespace CameraBcLib
{
    /// <summary>
    /// Logica di interazione per ResolutionList.xaml
    /// </summary>
    public partial class ResolutionList : Window
    {
        public ResolutionSingle result;

        public ResolutionList(ResolutionData data, ResolutionSingle currentRes)
        {
            result = currentRes;
            InitializeComponent();
            Lista.ItemsSource = data.ResList;
        }

        private void Lisa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            result = (ResolutionSingle)Lista.SelectedItem;
            this.Close();
        }

    }
}
