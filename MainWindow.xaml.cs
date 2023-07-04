using System;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Media;
using System.Reflection;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Timer = System.Timers.Timer;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Management;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Interop;
using Emgu.CV.Cuda;
using Emgu.CV.Face;
using Emgu.CV.Util;
using System.Linq;
using Face_Processing.Extensions;
using System.Text.RegularExpressions;
using Face_Processing.Services;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using static Emgu.CV.Face.FaceRecognizer;

namespace Face_Processing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture capture;
        private EigenFaceRecognizer recognizer;
        private FaceTracker faceTracker;
        public MainWindow()
        {
            InitializeComponent();
            faceTracker = new FaceTracker();
        }

        void Image_Grabbed(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            capture.Retrieve(frame);
            var objectsRect = faceTracker.ObjectExtractor(frame);
            List<string> labels = new List<string>();
            var rgb = new Rgb(0, 0, 225);
            foreach (var _object in faceTracker.GetImageObjects<Rgb>(frame, objectsRect))
            {
                var predicatedObject = new PredictionResult();
                try
                {
                    _object._EqualizeHist();
                    predicatedObject = recognizer.Predict(_object.Convert<Gray, byte>().Mat);
                    if (predicatedObject.Label != -1 && predicatedObject.Distance <= 2000)
                    {
                        Task.Run(() => frame.Save($"C:\\Users\\7\\Desktop\\Log\\{predicatedObject.Label}_{DateTime.Now.ToString("yyyy_MM_dddd_HH_mm_ss")}.jpg"));
                        rgb = new Rgb(0, 255, 0);
                        labels.Add(predicatedObject.Label.ToString() + $" <--> {predicatedObject.Distance}");
                    }
                    else
                        labels.Add("Unknown");
                }
                catch (Exception ee)
                {
                    labels.Add("Unknown");
                }
            }
            faceTracker.DrawRectangleOnImage(frame, objectsRect, labels.ToArray());

            this.Dispatcher.Invoke(new Action(() =>
            {
                image1.Source = frame.ToBitmap().ToImageSource();
            }));
            frame.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            recognizer.Write("trainedData.yaml");
        }

        public Task TrainRecognizer()
        {
            if (File.Exists(Environment.CurrentDirectory + @"\trainedData.yaml"))
            {
                recognizer = new EigenFaceRecognizer();
                recognizer.Read(Environment.CurrentDirectory + @"\trainedData.yaml");
            }
            else
            {
                string searchFolder = @"C:\Users\7\Desktop\images\";
                var filters = new string[] { "jpg", "jpeg", "png" };
                var files = PublicFunctions.GetFilesFrom(searchFolder, filters, false);
                recognizer = new EigenFaceRecognizer(files.Length);
                FaceTracker faceTracker = new FaceTracker();
                VectorOfMat imageList = new VectorOfMat();
                VectorOfInt labelList = new VectorOfInt();
                foreach (string file in files)
                {
                    var image = CvInvoke.Imread(file);
                    if (image != null)
                    {
                        var objectsRect = faceTracker.ObjectExtractor(image);
                        var face = faceTracker.GetImageObjects<Gray>(image, objectsRect).FirstOrDefault();
                        face._EqualizeHist();
                        imageList.Push(face.Mat);
                    }
                    if (int.TryParse(file.Remove(0, file.LastIndexOf('\\') + 1).Split(".")[0].Replace("_", ""), out int PersonId))
                    {
                        labelList.Push(new[] { PersonId });
                    }
                }
                recognizer.Train(imageList, labelList);
            }
            return Task.CompletedTask;
        }

        public static List<string> GetAllConnectedCameras()
        {
            var cameraNames = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraNames.Add(device["Caption"].ToString());
                }
            }

            return cameraNames;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await TrainRecognizer();
            //init the camera
            capture = new VideoCapture(0);
            capture.ImageGrabbed += Image_Grabbed;
            capture.Start();
        }
    }
}
