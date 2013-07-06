using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;

namespace MedLeap
{
    public partial class MainWindow : Window
    {
        VideoCaptureDevice _localWebCam;
        private FilterInfoCollection _localWebCamsCollection;
        private string _filterSwitchState;
        private bool _drawBlobState;
        private bool _refineBlobState;

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //put screen into memory
            System.Drawing.Image imgforms = (Bitmap)eventArgs.Frame.Clone();
            var ms = new MemoryStream();
            imgforms.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);

            //new bitmap
            var bip = new Bitmap(ms);
            
            //filter
            var filter = new ColorFiltering();
            switch (_filterSwitchState)
            {
                case "filter1" :
                    filter.Red = new IntRange(0, 255);
                    filter.Green = new IntRange(0, 100);
                    filter.Blue = new IntRange(0, 100);
                    break;
                case "filter2":
                    filter.Red = new IntRange(0, 255);
                    filter.Green = new IntRange(0, 100);
                    filter.Blue = new IntRange(0, 100);
                    break;
                case "none":
                    filter.Red = new IntRange(0, 255);
                    filter.Green = new IntRange(0, 255);
                    filter.Blue = new IntRange(0, 255);
                    break;
            }
            filter.ApplyInPlace(bip);
            var ms2 = new MemoryStream();
            bip.Save(ms2, ImageFormat.Bmp);

            //blobs
            BlobCounterBase bc = new BlobCounter();
            bc.FilterBlobs = true;
            bc.MinWidth = 10;
            bc.MinHeight = 10;
            bc.ObjectsOrder = ObjectsOrder.Size;
            bc.ProcessImage(bip);
            Rectangle[] rects = bc.GetObjectsRectangles();

            var ms3 = new MemoryStream();

            if (_drawBlobState)
            {
                using (var bitmap = new Bitmap(500, 500))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(bip, new Rectangle(0, 0, 500, 500), new Rectangle(0, 0, 500, 500), GraphicsUnit.Pixel);
                        foreach (Rectangle rect in rects)
                        {
                            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 0), 2))
                            {
                                canvas.DrawRectangle(pen, rect);
                            }
                        }
                        ms3.Seek(0, SeekOrigin.Begin);
                        bitmap.Save(ms3, ImageFormat.Bmp);
                    }
                }
            }

            //read mem into bitmap for feed
            var bi = new BitmapImage();
            bi.BeginInit();
            ms2.Seek(0, SeekOrigin.Begin);
            if (_drawBlobState)
            {
                bi.StreamSource = ms3;
            }
            else
            {
                bi.StreamSource = ms2;
            }
            bi.EndInit();
            bi.Freeze();

            //var ms3 = new MemoryStream();
            //bipOverlay.Save(ms3, ImageFormat.Bmp);

            ////read mem into bitmap for overlay
            //var biOverlay = new BitmapImage();
            //biOverlay.BeginInit();
            //ms3.Seek(0, SeekOrigin.Begin);
            //biOverlay.StreamSource = ms3;
            //biOverlay.Freeze();

            //update screen
            Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    Viewer1.Source = bi;
                    BlobCount.Text = rects.Length.ToString();
                    Frame.Text = DateTime.Now.Ticks.ToString();
                    UpdateStatus("Pushed Frame at " + DateTime.Now.Millisecond.ToString());
                }));
            
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _localWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _localWebCam = new VideoCaptureDevice(_localWebCamsCollection[1].MonikerString);
            _localWebCam.NewFrame += Cam_NewFrame;

            UpdateStatus("Trying to start LocalWebCam");
            _localWebCam.Start();
            UpdateStatus("LocalWebCam started!");

        }
        void UpdateStatus(string statusString)
        {
            Status.Text = statusString;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton li = (sender as RadioButton);
            _filterSwitchState = li.Content.ToString();
        }

        private void DrawBlob_OnClick(object sender, RoutedEventArgs e)
        {
            _drawBlobState = !_drawBlobState;
        }

        private void Refineblob_OnClick(object sender, RoutedEventArgs e)
        {
            _refineBlobState = !_refineBlobState;
        }
    }
}
