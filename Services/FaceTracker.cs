using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Face_Processing.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;

namespace Face_Processing.Services
{
    public class FaceTracker
    {
        CascadeClassifier haarCascade;
        public FaceTracker()
        {
            haarCascade = new CascadeClassifier(Environment.CurrentDirectory + @"\HaarCascade\haarcascade_frontalface_alt2.xml");
        }

        public Rectangle[] ObjectExtractor(Mat image, double scaleFactor = 1.1, int minNeighbors = 10)
        {
            if (image == null)
            {
                throw new NullReferenceException();
            }
            UMat imageGrayScale = new UMat();
            CvInvoke.CvtColor(image, imageGrayScale, ColorConversion.Bgr2Gray);
            return haarCascade.DetectMultiScale(imageGrayScale, 1.1, minNeighbors: minNeighbors);
        }

        public IEnumerable<Image<TColor, byte>> GetImageObjects<TColor>(Mat imageMat, params Rectangle[] objectsRects) where TColor : struct, IColor
        {
            foreach (var _object in objectsRects)
            {                
                yield return imageMat.ToImage<TColor, byte>().Copy(_object).Resize(120, 120, Inter.Cubic);
            }
        }
        public void DrawRectangleOnImage(Mat image, cRectangle[] objectsRects, string[] labels)
        {
            for (int i = 0; i < objectsRects.Length; i++)
            {
                if (labels.Length > i && !string.IsNullOrEmpty(labels[i]))
                {
                    CvInvoke.PutText(image, labels[i], new Point(objectsRects[i].Rectangle.X, objectsRects[i].Rectangle.Y), FontFace.HersheyTriplex, 2, objectsRects[i].Color.MCvScalar, 3);
                }
                CvInvoke.Rectangle(image, objectsRects[i].Rectangle, objectsRects[i].Color.MCvScalar, 3, LineType.FourConnected);
            }
        }
    }
}