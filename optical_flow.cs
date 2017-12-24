using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace OpticalFlowTracking
{
    class Program
    {

        static void Main(string[] args)
        {
            Mat flow, cflow, gray, prevgray, img_bgr;
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            prevgray = new Mat();

            VideoCapture cap = new VideoCapture();
            cap.Open(0);
            int sleepTime = (int)Math.Round(1000 / cap.Fps);

            using (Window window = new Window("capture"))
            using (Mat frame = new Mat()) // Frame image buffer
            {
                while (true)
                {
                    cap.Read(frame); 
                    if (frame.Empty())
                        break;
                    gray = new Mat();
                    flow = new Mat();
                    cflow = new Mat();
                    img_bgr = new Mat();
                    Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                    if (prevgray.Empty())
                        prevgray = gray;
                    else {
                        Cv2.CalcOpticalFlowFarneback(prevgray, gray, flow, 0.5, 5, 16, 3, 5, 1.2, OpticalFlowFlags.FarnebackGaussian);
                        Cv2.CvtColor(prevgray, cflow, ColorConversionCodes.GRAY2BGR);
                        drawOptFlowMap(ref flow, ref cflow, 1.5, 16, new Scalar(0, 0, 255));
                        drawHsv(flow, out img_bgr);
                        Mat gray_bgr = new Mat();
                        gray_bgr = Mat.Zeros(frame.Rows, frame.Cols, MatType.CV_8UC1);
                        Cv2.CvtColor(img_bgr, gray_bgr, ColorConversionCodes.BGR2GRAY);
                        Cv2.Normalize(gray_bgr, gray_bgr, 0, 255, NormTypes.MinMax, MatType.CV_8UC1);
                        Cv2.Blur(gray_bgr, gray_bgr, new Size(3, 3));

                        // Detect edges using Threshold
                        Mat img_thresh = new Mat();
                        img_thresh = Mat.Zeros(frame.Rows, frame.Cols, MatType.CV_8UC1);
                        Cv2.Threshold(gray_bgr, img_thresh, 155, 255, ThresholdTypes.BinaryInv);
                        Cv2.FindContours(img_thresh, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
             
                        if (contours.Length == 0)
                        {
                            throw new NotSupportedException("Couldn't find any object in the image.");
                        }

                        for (int i = 0; i < contours.Length; i++) {
                            Rect box = Cv2.BoundingRect(contours[i]);
                            if (box.Width > 50 && box.Height > 50 && box.Width < 900 && box.Height < 680)
                            {
                                Cv2.Rectangle(frame,
                                    box.TopLeft, box.BottomRight,
                                    new Scalar(0, 255, 0), 4);
                            }
                        }
                        window.Image = frame;
                        Char c = (Char)Cv2.WaitKey(1);
                        if (c == 27) break;
                        Swap<Mat>(ref gray, ref prevgray);
                    }
                }
            }
        }


        private static void drawOptFlowMap(ref Mat flow, ref Mat cflowmap, double scale, int step, Scalar color){
            for (int y = 0; y < cflowmap.Rows; y += step)
                for (int x = 0; x < cflowmap.Cols; x += step) {
                    Point2f fxy = flow.At<Point2f>(y, x) * scale;
                    Cv2.Line(cflowmap, new Point(x, y), new Point(Math.Round(x + fxy.X), Math.Round(y + fxy.Y)), color);
                    Cv2.Circle(cflowmap, new Point(x, y), 2, color, -1);
                }
        }

        private static void drawHsv(Mat flow, out Mat bgr)
        {
            Mat[] xy = new Mat[2];
            Cv2.Split(flow, out xy);
            Mat magnitude = new Mat();
            Mat angle = new Mat();
            Cv2.CartToPolar(xy[0], xy[1], magnitude, angle, true);
            //translate magnitude to range [0;1]
            double mag_max, mag_min;
            Cv2.MinMaxLoc(magnitude, out mag_min, out mag_max);
            magnitude.ConvertTo(
                magnitude,    // output matrix
                -1,           // type of the ouput matrix, if negative same type as input matrix
                1.0 / mag_max // scaling factor
             );

            //build hsv image
            Mat[] _hsv = new Mat[3];
            Mat hsv = new Mat();
            bgr = new Mat();
            _hsv[0] = angle;
            _hsv[1] = magnitude;
            _hsv[2] = Mat.Ones(angle.Size(), MatType.CV_32F);
            Cv2.Merge(_hsv, hsv);
            Cv2.CvtColor(hsv, bgr, ColorConversionCodes.HSV2BGR);
        }
        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

    }
}
