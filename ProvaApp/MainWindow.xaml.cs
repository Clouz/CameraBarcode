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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraBcLib;
using System.IO;

namespace ProvaApp
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Scansiona_Click(object sender, RoutedEventArgs e)
        {
            CameraBcLib.CameraBC bc = new CameraBC();
            bc.ShowDialog();
            
            if (bc.BcResult != null)
            {
                tipo.Text = bc.BcResult.BarcodeFormat;
                testo.Text = bc.BcResult.BarcodeTxt;
                imgPrew.Source = bc.BcResult.BitmapResult.ToBitmapImage();
            }
        }

        private void Scatta_Click(object sender, RoutedEventArgs e)
        {
            CameraBcLib.CameraBC bc = new CameraBC(LoadingMode:Mode.Photo);
            bc.ShowDialog();

            if (bc.BcResult != null)
            {
                imgPrew2.Source = bc.BcResult.BitmapResult.ToBitmapImage();
            }
        }
    }

    public static class BitmapHelper
    {
        public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }
    }


}
