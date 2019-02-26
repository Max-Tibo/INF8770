using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace TP2_INF8770
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap imageBM = new Bitmap("mario.png");
            Bitmap nouvelleimage = rgb2ycbcr(imageBM);
            ycbcr2rgb(nouvelleimage);
        }

        public static Bitmap rgb2ycbcr(Bitmap bmp)
        {

            int width = bmp.Width;
            int height = bmp.Height;
            byte[,] yData = new byte[width, height];                   
            byte[,] bData = new byte[width, height];                   
            byte[,] rData = new byte[width, height];                   

            unsafe
            {
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                int heightInPixels = bitmapData.Height;
                int widthInBytes = width * 3;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
                //Convert to YCbCr
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        int xPor3 = x*4;
                        float blue = currentLine[xPor3++];
                        float green = currentLine[xPor3++];
                        float red = currentLine[xPor3];

                        double Y = (0.299 * red) + (0.587 * green) + (0.114 * blue);
                        double Cb = 128 + 0.564 * (blue - Y);
                        double Cr = 128 + 0.713 * (red - Y);

                        yData[x, y] = (byte)Y;
                        bData[x, y] = (byte)Cb;
                        rData[x, y] = (byte)Cr;

                        Color nouvelleCouleur = Color.FromArgb(yData[x, y], bData[x, y], rData[x, y]);
                        newBitmap.SetPixel(x, y, nouvelleCouleur);
                    }
                }
                bmp.UnlockBits(bitmapData);
                newBitmap.Save("mybmp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                return newBitmap;
            }
        }

        public static Bitmap ycbcr2rgb(Bitmap bmp)
        {

            int width = bmp.Width;
            int height = bmp.Height;
            byte[,] rData = new byte[width, height];                   
            byte[,] gData = new byte[width, height];                   
            byte[,] bData = new byte[width, height];                   

            unsafe
            {
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                int heightInPixels = bitmapData.Height;
                int widthInBytes = width * 3;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
                //Convert to YCbCr
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        int xPor4 = x * 4;
                        float Y = currentLine[xPor4++];
                        float Cb = currentLine[xPor4++];
                        float Cr = currentLine[xPor4];


                        double red = Y + 1.403 * (Cr - 128);
                        double green = Y - 0.714 * (Cr - 128) - 0.344 * (Cb - 128);
                        double blue = Y + 1.773 * (Cb - 128);

                        rData[x, y] = (byte)red;
                        gData[x, y] = (byte)green;
                        bData[x, y] = (byte)blue;

                        Color nouvelleCouleur = Color.FromArgb(rData[x, y], gData[x, y], bData[x, y]);
                        newBitmap.SetPixel(x, y, nouvelleCouleur);
                    }
                }
                bmp.UnlockBits(bitmapData);
                newBitmap.Save("retour.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                return newBitmap;
            }
        }

        public void decoupage8x8(Bitmap bitmap)
        {

        }

        public void DCT(Bitmap bitmap)
        {

        }
    }
}
