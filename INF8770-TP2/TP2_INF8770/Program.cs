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
            decoupage8x8(imageBM);
        }

        public static Bitmap rgb2ycbcr(Bitmap bmp, byte[,] yData, byte[,] bData, byte[,] rData)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            yData = new byte[width, height];                   
            bData = new byte[width/2, height/2];                   
            rData = new byte[width/2, height/2];                   

            //Convert to YCbCr
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float blue = bmp.GetPixel(x, y).B;
                    float green = bmp.GetPixel(x, y).G; 
                    float red = bmp.GetPixel(x, y).R;

                    double Y = (0.299 * red) + (0.587 * green) + (0.114 * blue);
                    double Cb = 128 + 0.564 * (blue - Y);
                    double Cr = 128 + 0.713 * (red - Y);

                    yData[x, y] = (byte)Y;

                    if(y % 2 == 0 && x % 2 == 0) 
                    {
                        int suby = y/2;
                        int subx = x/2; 

                        bData[subx, suby] = (byte)Cb;
                        rData[subx, suby] = (byte)Cr;
                    }

                    Color nouvelleCouleur = Color.FromArgb(yData[x, y], bData[x, y], rData[x, y]);
                    newBitmap.SetPixel(x, y, nouvelleCouleur);
                }
            }
            newBitmap.Save("mybmp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            return newBitmap;
        }

        public static Bitmap ycbcr2rgb(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
            byte[,] rData = new byte[width, height];
            byte[,] gData = new byte[width, height];
            byte[,] bData = new byte[width, height];

            //Convert to RGB
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float Y = bmp.GetPixel(x, y).R;
                    float Cb = bmp.GetPixel(x, y).G;
                    float Cr = bmp.GetPixel(x, y).B;

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
            newBitmap.Save("test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            return newBitmap;
        }

        public static List<byte[,]> decoupage8x8(byte[,] data)
        {
            byte[,] block;
            List<byte[,]> tab8x8 = new List<byte[,]>();
            for (int i = 0; i < data.GetLength(1); i = i + 8)
            {
                for (int j = 0; j < data.GetLength(0); j = j + 8)
                {
                    block = new byte[8, 8];
                    for (int x = j, positionx = 0; x < (j + 8); x++, positionx++)
                    {
                        for (int y = i, positiony = 0; y < (i + 8); y++, positiony++)
                        {
                            block[positiony, positionx] = data[x, y];
                            Console.WriteLine(block[positiony, positionx]);
                        }

                    }
                    tab8x8.Add(block);

                }
            }
            return tab8x8;
        }

        public void DCT(List<Color[,]> tab8x8)
        {
            int n = 8;
            double w, z;
            for (int u = 0; u < n; u++)
            {
                for (int v = 0; v < n; v++)
                {
                    if (u == 1){ w = Math.Sqrt(1.0 / n); }
                    else{ w = Math.Sqrt(2.0 / n); }
                    if (v == 1) { z = Math.Sqrt(1.0 / n); }
                    else { z = Math.Sqrt(2.0 / n); }
                    double factor = w * z;

                    double sum = 0.0;
                    for (int x = 1; x <= n; ++x)
                    {
                        for (int y = 1; y <= n; ++y)
                        {
                            //int value = valeur du pixel a la position x y;

                            double insideCos1 = (Math.PI * (2 * (x - 1) + 1) * (u - 1)) / (2 * n);
                            double insideCos2 = (Math.PI * (2 * (y - 1) + 1) * (v - 1)) / (2 * n);
                            //sum += value * Math.Cos(insideCos1) * Math.Cos(insideCos2);
                        }
                    }

                    double dct_transform = factor * sum;

                    //créer nouveau bloc 8x8 avec les fréquences obtenu
                    //oOutput(u - 1, v - 1) = dct_transform;
                }
            }
        }
    }
}
