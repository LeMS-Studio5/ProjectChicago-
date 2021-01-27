﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace libProChic
{
  public  class MasterClass
    {
        private ConfigHelper con;
        private List<Color> colPallette = new List<Color>();
        private String root;
        public MasterClass()
        {
            root = Environment.ExpandEnvironmentVariables("%appdata%") + @"\LeMS\Update95\";
            if (Directory.Exists(root)) Directory.CreateDirectory(root);    //Creates GameDir if not already created
            con = new ConfigHelper(toSystemPath(@"C:\WINDOWS\WIN.ini"));//Environment.ExpandEnvironmentVariables(" % appdata % ") + @"\LeMS\Update95\C‰ƒ\WINDOWS
            ColourLoad();
        }
        private void ColourLoad()
        {
         
            colPallette.Clear();
            foreach (String col in File.ReadAllLines(toSystemPath(con.GetConfig("windows", "Color").Setting)))
            {
                colPallette.Add(convertColour(col,false));
            }
        }
        public ConfigHelper Config { get { return con; } }
        public Color convertColour(string rgbCode){
            return convertColour(rgbCode, true);
        }
        private Color convertColour(string rgbCode, bool fromPal){
                int[] col = rgbCode.Split(' ').Select(str => int.Parse(str)).ToArray();
                if (fromPal)
                    return FindClosestFromPallet(Color.FromArgb(col[0], col[1], col[2]));
                else
                    return Color.FromArgb(col[0], col[1], col[2]);
        }
        public Image prepareImage(Bitmap bmp, bool followPallet)
        {
            if (followPallet)
                return prepareImage(bmp);
            else
                return bmp;
        }
        private Image prepareImage(Bitmap bmp)
        {          
            return Process(bmp, Color.AliceBlue);
        }
        public Image prepareImage(string imageLocation)
        {
            if (!File.Exists(imageLocation))
                return new Bitmap(1, 1);
            FileStream fs = new System.IO.FileStream(imageLocation, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            BinaryReader Reader = new BinaryReader(fs);
            Bitmap bmpStream = new Bitmap(new MemoryStream(Reader.ReadBytes(System.Convert.ToInt32(fs.Length))));//System.Drawing.Image.FromStream(ImageStream));
            Bitmap bmp = bmpStream.Clone(new Rectangle(0, 0, bmpStream.Width, bmpStream.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb); // , col As Color, intBiPerPixel As Integer = 16 'FROM https://stackoverflow.com/questions/29585959/how-to-release-picture-from-picturebox-so-picture-file-may-be-deleted-in-vb-net and https://www.translatetheweb.com/?ref=TVert&from=&to=en&a=https://dotnet.currifex.org/dotnet/code/graphics/#ImageNoLock                                                                                                                                                     // Dim myEncoderParameters As EncoderParameters = New EncoderParameters(1), memoryStream = New MemoryStream()
            Reader.Close();          
            return prepareImage(bmp); // Image.FromStream(memoryStream)
        }
        public Color FindClosestFromPallet(Color col)
        {
            Color closeColour = col;
            double closeNum = double.MaxValue;
            if (col.A < 200)
                return closeColour;
            foreach (var colPal in colPallette)
            {
                if (ColourDistance(col, colPal) <= closeNum)
                {
                    closeNum = ColourDistance(col, colPal);
                    closeColour = colPal;
                }
            }
            // MessageBox.Show(closeColour.ToString)
            return closeColour;
        }
        public Color FindClosestFromPallet(ref Color col, ref Color AlphaColor)
        {
            Color closeColour = col;
            double closeNum = double.MaxValue;
            if (col.A < 200)
                return AlphaColor;
            foreach (Color colPal in colPallette)
            {
                if (ColourDistance(col, colPal) <= closeNum)
                {
                    closeNum = ColourDistance(col, colPal);
                    closeColour = colPal;
                }
            }
            // MessageBox.Show(closeColour.ToString)
            return closeColour;
        }
        private double ColourDistance(Color e1, Color e2)
        {
            long rmean = Convert.ToInt64((System.Convert.ToInt64(e1.R) + System.Convert.ToInt64(e2.R)) / (double)2);
            long r = System.Convert.ToInt64(e1.R) - System.Convert.ToInt64(e2.R);
            long g = System.Convert.ToInt64(e1.G) - System.Convert.ToInt64(e2.G);
            long b = System.Convert.ToInt64(e1.B) - System.Convert.ToInt64(e2.B);
            return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
        }
        public Bitmap Process(System.Drawing.Bitmap bmp){
            return Process(bmp, Color.Empty);
        }
        public Bitmap Process(System.Drawing.Bitmap bmp,  Color alpha){
            if ((bmp.PixelFormat != PixelFormat.Format24bppRgb && bmp.PixelFormat != PixelFormat.Format32bppArgb) || (bmp.Width < 1 || bmp.Height < 1)){ // <== A1
                return null;
            }
            int ww = bmp.Width / 8;
            int hh = bmp.Height / 8;
            using (FastBitmap fbitmap = new FastBitmap(bmp, 0, 0, bmp.Width - 0, bmp.Height - 0))
            { // <== A2
                unsafe
                {                                                                                                 // <== A3
                    byte* row = (byte*)fbitmap.Scan0, bb = row;                                                          // <== A4
                    for (int yy = 0; yy < fbitmap.Height; yy++, bb = (row += fbitmap.Stride))
                    {                     // <== A5
                        for (int xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {                         // <== A6
                            Color col = FindClosestFromPallet(Color.FromArgb(*(bb + 3), *(bb + 2), *(bb + 1), *(bb + 0)), alpha);
                            *(bb + 0) = col.B;                  // *(bb + 0) is B (Blue ) component of the pixel
                            *(bb + 1) = col.G;                  // *(bb + 1) is G (Green) component of the pixel
                            *(bb + 2) = col.R;               // *(bb + 2) is R (Red  ) component of the pixel
                            *(bb + 3) = col.A;           // *(bb + 3)  is A (Alpha) component of the pixel ( for 32bpp )
                                                         //    byte gray = (byte)((1140 * *(bb + 0) + 5870 * *(bb + 1) + 2989 * *(bb + 2)) / 10000);        // <== A7
                                                         //    *(bb + 0) = *(bb + 1) = *(bb + 2) = gray;
                        }
                    }
                }
                return fbitmap.Bitmap;
            }
        }
        private Color FindClosestFromPallet(Color col, Color AlphaColor){
            Color closeColour = col;
            Double closeNum = Double.MaxValue;
            if (AlphaColor.IsEmpty && col.A < 200) return closeColour;
            else if (col.A < 200) return AlphaColor;
            foreach (Color colPal in colPallette){
                Double testDistance = ColourDistance(col, colPal);
                if (testDistance <= closeNum){
                    closeNum = testDistance;
                    closeColour = colPal;
                }
            }
            return closeColour;
        }
        /// <summary>
        ///     ''' Returns Host Drive Path from a in-game path
        ///     ''' </summary>
        public string toSystemPath(string path)
        {
            char dirLetter = path.ToCharArray()[0];
            if (path == "") return "";
            if (char.IsLetter(path.First()) && path.Substring(1, 1) == ":")
                path = path.Substring(2);
            return root + dirLetter + "‰ƒ" + path;
        }
    }
    /// <summary>
    /// Fast access to bitmap data
    /// </summary>
    public class FastBitmap : IDisposable
    {
        //=========================================================================
        #region Constructors
        /// <summary>
        /// Create a new <c>FastBitamp</c>
        /// </summary>
        public FastBitmap(Bitmap bitmap, int xx, int yy, int width, int height)
        {
            this.Bitmap = bitmap;
            XX = xx;
            YY = yy;
            Width = width;
            Height = height;
            Data = this.Bitmap.LockBits(new Rectangle(xx, yy, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            PixelSize = FindPixelSize();
            Stride = Data.Stride;
            Scan0 = Data.Scan0;
        }

        /// <summary>
        /// Create a new <c>FastBitamp</c>
        /// <param
        /// </summary>
        public FastBitmap(Bitmap bitmap) : this(bitmap, 0, 0, bitmap.Width, bitmap.Height)
        {
        }
        #endregion

        #region IDisposable
        /// <inheritdoc />
        public void Dispose()
        {
            Bitmap.UnlockBits(Data);
        }
        #endregion

        #region Public members
        /// <summary>
        /// The bitmap
        /// </summary>
        public Bitmap Bitmap;
        /// <summary>
        /// The data of bitmap
        /// </summary>
        public readonly BitmapData Data;
        /// <summary>
        /// The pixel size
        /// </summary>
        public readonly int PixelSize;
        /// <summary>
        /// The left of rectangle proccesed
        /// </summary>
        public readonly int XX;
        /// <summary>
        /// The top of rectangle proccesed
        /// </summary>
        public readonly int YY;
        /// <summary>
        /// The width of rectangle proccesed
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// The height of rectangle proccesed
        /// </summary>
        public readonly int Height;
        /// <summary>
        /// the width of a single row
        /// </summary>
        public readonly int Stride;
        /// <summary>
        /// The first pixel location in rectangle proccesed
        /// </summary>
        public readonly IntPtr Scan0;

        #endregion

        #region private
        private int FindPixelSize()
        {
            if (Data.PixelFormat == PixelFormat.Format24bppRgb)
            {
                return 3;
            }
            if (Data.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return 4;
            }
            return 4;
        }
        #endregion
    }
}