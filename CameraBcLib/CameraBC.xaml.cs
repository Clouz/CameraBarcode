using System;
using System.Windows;
using System.IO;
using Camera_NET;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using ZXing;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;

namespace CameraBcLib
{
    /// <summary>
    /// Logica di interazione per CameraBC.xaml
    /// </summary>
    public partial class CameraBC : Window
    {
        private int selectedCamera = 0;
        /// <summary>
        /// Return the selected camera number
        /// </summary>
        public int SelectedCamera {
            get
            {
                return selectedCamera;
            }
            private set
            {
                if (value < _CameraChoice.Devices.Count)
                    selectedCamera = value;
                else
                    selectedCamera = 0;
            }
        }

        /// <summary>
        /// Return the selected webcam resolution
        /// </summary>
        public ResolutionSingle selectedResolution { get; private set; }

        /// <summary>
        /// Return the loaded camera mode
        /// </summary>
        public Mode LoadingMode { get; private set; }

        /// <summary>
        /// Return the scanning frequancy
        /// </summary>
        public TimeSpan TimeScan { get; private set; }

        /// <summary>
        /// Return the result of scanning/photo
        /// </summary>
        public BarcodeResult BcResult;

        private CameraChoice _CameraChoice;
        private Camera_NET.ResolutionList resolutions;
        private Bitmap bitmap = null;
        private IBarcodeReader reader = new BarcodeReader();
        private List<string> res = new List<string>();
        private DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
        ResolutionData resData;


        /// <summary>
        /// Show a camera scanner / photo window
        /// </summary>
        /// <param name="SelectedCamera">Select the default camera</param>
        /// <param name="TimeScanMilliseconds">Select the scanning frequancy</param>
        /// <param name="LoadingMode">Select if is required to scan or to take a photo</param>
        public CameraBC(int SelectedCamera = 0, int TimeScanMilliseconds = 300, Mode LoadingMode = Mode.Barcode)
        {
            InitializeComponent();

            _CameraChoice = new CameraChoice();
            _CameraChoice.UpdateDeviceList();

            if (_CameraChoice.Devices.Count > 0)
            {
                this.SelectedCamera = SelectedCamera;
                var camera_moniker = _CameraChoice.Devices[this.SelectedCamera].Mon;
                cameraControl.CameraControl.SetCamera(camera_moniker, null);

                initResolution();
                cameraControl.CameraControl.SetCamera(camera_moniker, resolutions[selectedResolution.ID]);
            }

            this.TimeScan = TimeSpan.FromMilliseconds(TimeScanMilliseconds);
            this.LoadingMode = LoadingMode;

            if (this.LoadingMode == Mode.Photo)
            {
                CameraStop.Visibility = Visibility.Collapsed;
                CameraScatta.Content = "Scatta";
            }
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            if (_CameraChoice.Devices.Count == 0)
            {
                MessageBox.Show("No Camera Detected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            cameraControl.CameraControl.CloseCamera();
        }

        private void CameraScatta_Click(object sender, RoutedEventArgs e)
        {
            if (this.LoadingMode == Mode.Photo)
            {
                bitmap = cameraControl.CameraControl.SnapshotSourceImage();
                BcResult = new BarcodeResult
                {
                    BitmapResult = bitmap
                };
                this.Close();
            }
            else
            {
                initTimer();
            }
        }

        private void initTimer()
        {
            updateTimer.Tick += new EventHandler(scattaN);
            updateTimer.Interval = this.TimeScan;
            updateTimer.Start();
        }

        private void scattaN(Object info, EventArgs e)
        {
            bitmap = cameraControl.CameraControl.SnapshotSourceImage();

            var barcodeBitmap = bitmap;
            var result = reader.Decode(bitmap);

            if (result != null)
            {
                BcResult = new BarcodeResult
                {
                    BitmapResult = bitmap,
                    BarcodeFormat = result.BarcodeFormat.ToString(),
                    BarcodeTxt = result.Text,
                };

                updateTimer.Stop();

                StatusBar.Text = "Barcode Found!";
                CameraScatta.IsEnabled = true;

                this.Close();
            }
            else
            {
                StatusBar.Text = "Scanning...";
                CameraScatta.IsEnabled = false;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            updateTimer.Stop();
            StatusBar.Text = "Stop scanning";
            CameraScatta.IsEnabled = true;
            CameraStop.IsEnabled = false;
            this.Close();
        }

        private void AvanzaCamera_Click(object sender, RoutedEventArgs e)
        {
            if (updateTimer.IsEnabled == true )
            {
                updateTimer.Stop();
                CameraScatta.IsEnabled = true;
                CameraStop.IsEnabled = false;
            }

            SelectedCamera++;
            cameraControl.CameraControl.SetCamera(_CameraChoice.Devices[selectedCamera].Mon, null);

            initResolution();
            cameraControl.CameraControl.SetCamera(_CameraChoice.Devices[selectedCamera].Mon, resolutions[selectedResolution.ID]);
            StatusBar.Text = selectedResolution.Name;

        }

        private void CambiaRisoluzione_Click(object sender, RoutedEventArgs e)
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
                CameraScatta.IsEnabled = true;
                CameraStop.IsEnabled = false;
            }

            ResolutionList winResList = new ResolutionList(resData, selectedResolution);
            winResList.ShowDialog();
            selectedResolution = winResList.result;

            cameraControl.CameraControl.SetCamera(_CameraChoice.Devices[selectedCamera].Mon, resolutions[selectedResolution.ID]);
            StatusBar.Text = selectedResolution.Name;

        }

        private void initResolution()
        {
            resolutions = Camera.GetResolutionList(cameraControl.CameraControl.Moniker);
            resData = new ResolutionData(resolutions);
            selectedResolution = resData.ResList[resData.ResList.Count - 1];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }
    }

    public enum Mode
    {
        Barcode,
        Photo
    }

    public class ResolutionData
    {
        public List<ResolutionSingle> ResList { get; set; } = new List<ResolutionSingle>();

        public ResolutionData( Camera_NET.ResolutionList res)
        {
            for (int i = 0; i < res.Count; i++)
            {
                ResList.Add(new ResolutionSingle() {
                    ID = i,
                    Height = res[i].Height,
                    Width = res[i].Width,
                    Name = res[i].ToString()
                });
            }

            ResList.Sort( (a, b) => (a.Product.CompareTo(b.Product)));
            
        }
    }

    public class ResolutionSingle
    {
        public int ID { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Name { get; set; }
        private int product;
        public int Product
        {
            set
            {
                product = Height * Width;
            }

            get
            {
                product = Height * Width;
                return product;
            }
        }
    }
    

    public class BarcodeResult
    {
        public Bitmap BitmapResult { get; set; }
        public string BarcodeFormat { get; set; }
        public string BarcodeTxt { get; set; }
    }
}
