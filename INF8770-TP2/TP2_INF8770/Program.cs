using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace TP2_INF8770
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap imageBM = new Bitmap("test2.png");
            int width = imageBM.Width;
            int height = imageBM.Height;
            int initialWeigth = width * height * 8 * 3;

            // Création des tableaux 2D qui vont contenir les informations de Y Cb Cr
            byte[,] yData = new byte[width, height];
            byte[,] bData = new byte[width/2, height/2];
            byte[,] rData = new byte[width/2, height/2];

            //juste pour test
            byte[,] redData = new byte[width, height];
            byte[,] greenData = new byte[width, height];
            byte[,] blueData = new byte[width, height];

            // Applique la conversion et division des résultats de chaques composantes dans leur tableau respectif
            Bitmap nouvelleimage = rgb2ycbcr(imageBM, yData, bData, rData);

            ycbcr2rgb(yData, bData, rData);

            // Création des listes qui vont contenir tous les tableaux 8x8 de chaque élément Y Cb Cr
            List<byte[,]> yBlocks = new List<byte[,]>();
            List<byte[,]> bBlocks = new List<byte[,]>();
            List<byte[,]> rBlocks = new List<byte[,]>();

            // Remplissage des listes
            yBlocks = decoupage8x8(yData);
            bBlocks = decoupage8x8(bData);
            rBlocks = decoupage8x8(rData);

            // Merge toutes les listes de blocs 8x8 ensemble en vue de la dct
            List<byte[,]> blocks = new List<byte[,]>();
            blocks.AddRange(yBlocks);
            blocks.AddRange(bBlocks);
            blocks.AddRange(rBlocks);

            // Applique la dct
            List<int[,]> DCTBlocks = new List<int[,]>();
            DCTBlocks = DCT(blocks);

            Quantification(DCTBlocks);

            // Lecture en diagonal
            List<int> listData2compress = new List<int>();
            listData2compress = zigZagMatrix(DCTBlocks, 8, 8);

            // Compression par Huffman et RLE
            String string2compress_huff = String.Join(" ", listData2compress);
            HuffmanTree huffmanTree = new HuffmanTree();
            huffmanTree.Build(string2compress_huff);
            BitArray compressedString_huff = huffmanTree.Encode(string2compress_huff);
            StringBuilder string2compress_rle = new StringBuilder();
            foreach(var b in compressedString_huff){
                string2compress_rle.Append((bool)b ? "1" : "0");
            }
            String string2compressRLE = string2compress_rle.ToString();
            String compressedString_rle = Transform.RunLengthEncode(string2compressRLE);

            // Calcul taux de compression
            int[] compressedString2calculate = compressedString_rle.Split('/').Select(n => Convert.ToInt32(n)).ToArray();
            String bitString = "";
            for(int i = 0; i < compressedString2calculate.Length; i++)
            {
                var binary = Convert.ToString(compressedString2calculate[i], 2);
                bitString += binary;
            }
            float compressionRatio = (float)1 - ((float)bitString.Length / (float)initialWeigth);
            Console.WriteLine(compressionRatio);

            // Decodage
            String decompressedString_rle = Transform.RunLengthDecode(compressedString_rle);
            var string2decompress = new BitArray(decompressedString_rle.Select(c => c == '1').ToArray());
            String decompressedString_huff = huffmanTree.Decode(string2decompress);
            int[] icolorValues = decompressedString_huff.Split(' ').Select(n => Convert.ToInt32(n)).ToArray();
            List<int> lcolorValues = icolorValues.OfType<int>().ToList();

            List<int[,]> inverseQUNTBlocks = new List<int[,]>();
            inverseQUNTBlocks = zigZagMatrixBuilder(lcolorValues, 8, 8);
            QuantificationInverse(inverseQUNTBlocks);

            List<byte[,]> inverseDCTBlocks = new List<byte[,]>();
            inverseDCTBlocks = inverseDCT(inverseQUNTBlocks);

            // Listes qui contiennent tous les listes de tableaux 8x8 de chaque élément Y Cb Cr
            List<byte[,]> inverseYBlocks = new List<byte[,]>();
            List<byte[,]> inverseBBlocks = new List<byte[,]>();
            List<byte[,]> inverseRBlocks = new List<byte[,]>();

            for(int i = 0; i < yBlocks.Capacity; i++)
            {
                inverseYBlocks.Add(inverseDCTBlocks[i]);
            }
            for (int j = yBlocks.Capacity; j < yBlocks.Capacity + bBlocks.Capacity; j++)
            {
                inverseBBlocks.Add(inverseDCTBlocks[j]);
            }
            for (int k = yBlocks.Capacity + bBlocks.Capacity; k < yBlocks.Capacity + bBlocks.Capacity + rBlocks.Capacity; k++)
            {
                inverseRBlocks.Add(inverseDCTBlocks[k]);
            }

            // Création des tableaux 2D qui vont contenir les informations inverses de Y Cb Cr
            byte[,] inverseYData = inversedecoupage8x8(inverseYBlocks);
            byte[,] inverseBData = inversedecoupage8x8(inverseBBlocks);
            byte[,] inverseRData = inversedecoupage8x8(inverseRBlocks);

            ycbcr2rgb(inverseYData, inverseBData, inverseRData);
            Console.ReadKey();
        }

        public static Bitmap rgb2ycbcr(Bitmap bmp, byte[,] yData, byte[,] bData, byte[,] rData)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap ycbcrBitmap = new Bitmap(bmp.Width, bmp.Height);

            byte tempCb = 0;
            byte tempCr = 0;
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
                    /*bData[x, y] = (byte)Cb;
                    rData[x, y] = (byte)Cr;*/
                    Color nouvelleCouleur = new Color();

                    if (y % 2 == 0 && x % 2 == 0)
                    {
                        int suby = y / 2;
                        int subx = x / 2;

                        bData[subx, suby] = (byte)Cb;
                        tempCb = (byte)Cb;
                        rData[subx, suby] = (byte)Cr;
                        tempCr = (byte)Cr;
                        nouvelleCouleur = Color.FromArgb(yData[x, y], bData[subx, suby], rData[subx, suby]);
                    }
                    else
                    {
                        nouvelleCouleur = Color.FromArgb(yData[x, y], tempCb, tempCr);
                    }
                    /*nouvelleCouleur = Color.FromArgb(yData[x, y], bData[x, y], rData[x, y]);*/
                    ycbcrBitmap.SetPixel(x, y, nouvelleCouleur);
                }
            }
            ycbcrBitmap.Save("mybmp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            return ycbcrBitmap;
        }

        public static Bitmap ycbcr2rgb(byte[,] yData, byte[,] bData, byte[,] rData)
        {
            int width = yData.GetLength(0);
            int height = yData.GetLength(1);
            Bitmap rgbBitmap = new Bitmap(width, height);
            byte[,] redData = new byte[width, height];
            byte[,] greenData = new byte[width, height];
            byte[,] blueData = new byte[width, height];

            float Y = 0;
            float Cb = 0;
            float Cr = 0;
            //Convert to RGB
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Y = (float)yData[x, y];
                    /*Cb = (float)bData[x, y];
                    Cr = (float)rData[x, y];*/
                    if (y % 2 == 0 && x % 2 == 0)
                    {
                        int suby = y / 2;
                        int subx = x / 2;
                        Cb = (float)bData[subx, suby];
                        Cr = (float)rData[subx, suby];
                    }
                    double red = Y + 1.403 * (Cr - 128);
                    double green = Y - 0.714 * (Cr - 128) - 0.344 * (Cb - 128);
                    double blue = Y + 1.773 * (Cb - 128);

                    redData[x, y] = (byte)red;
                    greenData[x, y] = (byte)green;
                    blueData[x, y] = (byte)blue;

                    Color nouvelleCouleur = Color.FromArgb(redData[x, y], greenData[x, y], blueData[x, y]);
                    rgbBitmap.SetPixel(x, y, nouvelleCouleur);
                }
            }
            rgbBitmap.Save("test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            return rgbBitmap;
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
                        }
                    }
                    tab8x8.Add(block);
                }
            }
            return tab8x8;
        }

        public static byte[,] inversedecoupage8x8(List<byte[,]> data)
        {
            int nbOfBlocks = data.Capacity;
            int size = (int)Math.Sqrt(nbOfBlocks) * 8;
            byte[,] block = new byte[size,size];
            int index = 0;
            int level = (int)Math.Sqrt(nbOfBlocks);
            byte[,] tmpblock;
            for (int i = 0; i < level; i++)
            {
                for (int j = 0; j < level; j++)
                {
                    tmpblock = data[index];
                    index++;
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            block[(j * 8) + y, (i * 8) + x] = tmpblock[x, y];
                        }
                    }
                }
            }
            return block;
        }


        public static List<int[,]> DCT(List<byte[,]> tab8x8)
        {
            List<int[,]> ListTabFrequence = new List<int[,]>();
            for (int i = 0; i < tab8x8.Count; i++) {
                int[,] tabFrequence = new int[8, 8];
                int n = 8;
                double w, z;

                for (int u = 0; u < n; u++)
                {
                    for (int v = 0; v < n; v++)
                    {
                        if (u == 1) { w = Math.Sqrt(1.0 / n); }
                        else { w = Math.Sqrt(2.0 / n); }
                        if (v == 1) { z = Math.Sqrt(1.0 / n); }
                        else { z = Math.Sqrt(2.0 / n); }
                        double factor = w * z;

                        double sum = 0.0;
                        for (int x = 0; x < n; x++)
                        {
                            for (int y = 0; y < n; y++)
                            {
                                double value = tab8x8[i][x,y];

                                double insideCos1 = (Math.PI * ((float)2 * x + (float)1) * u) / ((float)2 * n);
                                double insideCos2 = (Math.PI * ((float)2 * y + (float)1) * v) / ((float)2 * n);
                                sum += value * Math.Cos(insideCos1) * Math.Cos(insideCos2);
                            }
                        }

                        double dct_transform = factor * sum;

                        tabFrequence[u,v] = (int)dct_transform;
                    }
                }
                ListTabFrequence.Add(tabFrequence);
            }
            return ListTabFrequence;
        }

        public static List<byte[,]> inverseDCT(List<int[,]> tab8x8)
        {
            List<byte[,]> ListTabFrequence = new List<byte[,]>();
            for (int i = 0; i < tab8x8.Count; i++)
            {
                byte[,] tabFrequence = new byte[8, 8];
                int n = 8;
                double w, z;

                for (int x = 0; x < n; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        double sum = 0.0;
                        for (int u = 0; u < n; u++)
                        {
                            for (int v = 0; v < n; v++)
                            {
                                if (u == 1) { w = Math.Sqrt(1.0 / n); }
                                else { w = Math.Sqrt(2.0 / n); }
                                if (v == 1) { z = Math.Sqrt(1.0 / n); }
                                else { z = Math.Sqrt(2.0 / n); }
                                double factor = w * z;
                                double value = tab8x8[i][u, v];

                                double insideCos1 = (Math.PI * ((float)2 * x + (float)1) * u) / ((float)2 * n);
                                double insideCos2 = (Math.PI * ((float)2 * y + (float)1) * v) / ((float)2 * n);
                                sum += factor * value * Math.Cos(insideCos1) * Math.Cos(insideCos2);
                            }
                        }

                        double dct_transform = sum;

                        tabFrequence[x, y] = (byte)dct_transform;
                    }
                }
                ListTabFrequence.Add(tabFrequence);
            }
            return ListTabFrequence;
        }

        public static void Quantification(List<int[,]> listFrequence8x8)
        {
            /*int[,] matriceQuantification = new int[,] { { 16, 11, 10, 16, 24, 40, 51, 61 },
                                                        { 12, 12, 14, 19, 26, 58, 60, 55 }, 
                                                        { 14, 13, 16, 24, 40, 57, 69, 56 }, 
                                                        { 14, 17, 22, 29, 51, 87, 80, 62 },
                                                        { 18, 22, 37, 56, 68, 109, 103, 77 },
                                                        { 24, 35, 55, 64, 81, 104, 113, 92 },
                                                        { 49, 64, 78, 87, 103, 121, 120, 101 },
                                                        { 72, 92, 95, 98, 112, 100, 103, 99 },};*/

            int[,] matriceQuantification = new int[,] { { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },};

            /*int[,] matriceQuantification = new int[,] { { 13, 23, 33, 43, 53, 63, 73, 83 },
                                                        { 23, 33, 43, 53, 63, 73, 83, 93 },
                                                        { 33, 43, 53, 63, 73, 83, 93, 103 },
                                                        { 43, 53, 63, 73, 83, 93, 103, 113 },
                                                        { 53, 63, 73, 83, 93, 103, 113, 123 },
                                                        { 63, 73, 83, 93, 103, 113, 123, 133 },
                                                        { 73, 83, 93, 103, 113, 123, 133, 143 },
                                                        { 83, 93, 103, 113, 123, 133, 143, 153 },};*/

            for (int i = 0; i < listFrequence8x8.Count; i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        listFrequence8x8[i][x, y] = listFrequence8x8[i][x, y] / matriceQuantification[x, y];
                    }
                }
            }
        }

        public static void QuantificationInverse(List<int[,]> listFrequence8x8)
        {
            /*int[,] matriceQuantification = new int[,] { { 16, 11, 10, 16, 24, 40, 51, 61 },
                                                        { 12, 12, 14, 19, 26, 58, 60, 55 },
                                                        { 14, 13, 16, 24, 40, 57, 69, 56 },
                                                        { 14, 17, 22, 29, 51, 87, 80, 62 },
                                                        { 18, 22, 37, 56, 68, 109, 103, 77 },
                                                        { 24, 35, 55, 64, 81, 104, 113, 92 },
                                                        { 49, 64, 78, 87, 103, 121, 120, 101 },
                                                        { 72, 92, 95, 98, 112, 100, 103, 99 },};*/

            int[,] matriceQuantification = new int[,] { { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },
                                                        { 1, 1, 1, 1, 1, 1, 1, 1 },};

            /*int[,] matriceQuantification = new int[,] { { 13, 23, 33, 43, 53, 63, 73, 83 },
                                                        { 23, 33, 43, 53, 63, 73, 83, 93 },
                                                        { 33, 43, 53, 63, 73, 83, 93, 103 },
                                                        { 43, 53, 63, 73, 83, 93, 103, 113 },
                                                        { 53, 63, 73, 83, 93, 103, 113, 123 },
                                                        { 63, 73, 83, 93, 103, 113, 123, 133 },
                                                        { 73, 83, 93, 103, 113, 123, 133, 143 },
                                                        { 83, 93, 103, 113, 123, 133, 143, 153 },};*/

            for (int i = 0; i < listFrequence8x8.Count; i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        listFrequence8x8[i][x, y] = listFrequence8x8[i][x, y] * matriceQuantification[x, y];
                    }
                }
            }
        }

        // Utility function to read matrix in zig-zag form 
        // https://www.geeksforgeeks.org/print-matrix-zag-zag-fashion/
        static List<int> zigZagMatrix(List<int[,]> list, int n, int m)
        {
            List<int> listData = new List<int>();

            for (int index = 0; index < list.Count; index++) {
                int[,] arr = list[index];
                int row = 0, col = 0;

                // Boolean variable that will 
                // true if we need to increment 
                // 'row' valueotherwise false- 
                // if increment 'col' value 
                bool row_inc = false;

                // Print matrix of lower half 
                // zig-zag pattern 
                int mn = Math.Min(m, n);
                for (int len = 1; len <= mn; ++len)
                {
                    for (int i = 0; i < len; ++i)
                    {

                        listData.Add(arr[row, col]);

                        if (i + 1 == len)
                            break;

                        // If row_increment value is true 
                        // increment row and decrement col 
                        // else decrement row and increment 
                        // col 
                        if (row_inc)
                        {
                            ++row;
                            --col;
                        }
                        else
                        {
                            --row;
                            ++col;
                        }
                    }

                    if (len == mn)
                        break;

                    // Update row or col valaue 
                    // according to the last 
                    // increment 
                    if (row_inc)
                    {
                        ++row;
                        row_inc = false;
                    }
                    else
                    {
                        ++col;
                        row_inc = true;
                    }
                }

                // Update the indexes of row 
                // and col variable 
                if (row == 0)
                {
                    if (col == m - 1)
                        ++row;
                    else
                        ++col;
                    row_inc = true;
                }
                else
                {
                    if (row == n - 1)
                        ++col;
                    else
                        ++row;
                    row_inc = false;
                }

                // Print the next half 
                // zig-zag pattern 
                int MAX = Math.Max(m, n) - 1;
                for (int len, diag = MAX; diag > 0; --diag)
                {

                    if (diag > mn)
                        len = mn;
                    else
                        len = diag;

                    for (int i = 0; i < len; ++i)
                    {
                        listData.Add(arr[row, col]);

                        if (i + 1 == len)
                            break;

                        // Update row or col vlaue 
                        // according to the last 
                        // increment 
                        if (row_inc)
                        {
                            ++row;
                            --col;
                        }
                        else
                        {
                            ++col;
                            --row;
                        }
                    }

                    // Update the indexes of 
                    // row and col variable 
                    if (row == 0 || col == m - 1)
                    {
                        if (col == m - 1)
                            ++row;
                        else
                            ++col;

                        row_inc = true;
                    }

                    else if (col == 0 || row == n - 1)
                    {
                        if (row == n - 1)
                            ++col;
                        else
                            ++row;

                        row_inc = false;
                    }
                }
            }
            return listData;
        }

        static List<int[,]> zigZagMatrixBuilder(List<int> list, int n, int m)
        {
            List<int[,]> listData = new List<int[,]>();

            for (int index = 0; index < list.Count; index += (m*n)) {
                int[,] arr = new int[8,8];
                int row = 0, col = 0;
                int reverseIndex = index;
                // Boolean variable that will 
                // true if we need to increment 
                // 'row' valueotherwise false- 
                // if increment 'col' value 
                bool row_inc = false;

                // Print matrix of lower half 
                // zig-zag pattern 
                int mn = Math.Min(m, n);
                for (int len = 1; len <= mn; ++len)
                {
                    for (int i = 0; i < len; ++i)
                    {

                        arr[row, col] = list[reverseIndex];
                        reverseIndex++;

                        if (i + 1 == len)
                            break;

                        // If row_increment value is true 
                        // increment row and decrement col 
                        // else decrement row and increment 
                        // col 
                        if (row_inc)
                        {
                            ++row;
                            --col;
                        }
                        else
                        {
                            --row;
                            ++col;
                        }
                    }

                    if (len == mn)
                        break;

                    // Update row or col valaue 
                    // according to the last 
                    // increment 
                    if (row_inc)
                    {
                        ++row;
                        row_inc = false;
                    }
                    else
                    {
                        ++col;
                        row_inc = true;
                    }
                }

                // Update the indexes of row 
                // and col variable 
                if (row == 0)
                {
                    if (col == m - 1)
                        ++row;
                    else
                        ++col;
                    row_inc = true;
                }
                else
                {
                    if (row == n - 1)
                        ++col;
                    else
                        ++row;
                    row_inc = false;
                }

                // Print the next half 
                // zig-zag pattern 
                int MAX = Math.Max(m, n) - 1;
                for (int len, diag = MAX; diag > 0; --diag)
                {

                    if (diag > mn)
                        len = mn;
                    else
                        len = diag;

                    for (int i = 0; i < len; ++i)
                    {
                        arr[row, col] = list[reverseIndex];
                        reverseIndex++;

                        if (i + 1 == len)
                            break;

                        // Update row or col vlaue 
                        // according to the last 
                        // increment 
                        if (row_inc)
                        {
                            ++row;
                            --col;
                        }
                        else
                        {
                            ++col;
                            --row;
                        }
                    }

                    // Update the indexes of 
                    // row and col variable 
                    if (row == 0 || col == m - 1)
                    {
                        if (col == m - 1)
                            ++row;
                        else
                            ++col;

                        row_inc = true;
                    }

                    else if (col == 0 || row == n - 1)
                    {
                        if (row == n - 1)
                            ++col;
                        else
                            ++row;

                        row_inc = false;
                    }
                }
                listData.Add(arr);
            }
            return listData;
        }
    }

    

    // Ajout des classes et méthodes nécessaire pour la compression de huffman:
    // https://www.csharpstar.com/csharp-huffman-coding-using-dictionary/
    public class Node
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public Node Right { get; set; }
        public Node Left { get; set; }

        public List<bool> Traverse(char symbol, List<bool> data)
        {
            // Leaf
            if (Right == null && Left == null)
            {
                if (symbol.Equals(this.Symbol))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null)
                {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(symbol, leftPath);
                }

                if (Right != null)
                {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(symbol, rightPath);
                }

                if (left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }
    }

    public class HuffmanTree
    {
        private List<Node> nodes = new List<Node>();
        public Node Root { get; set; }
        public Dictionary<char, int> Frequencies = new Dictionary<char, int>();

        public void Build(string source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (!Frequencies.ContainsKey(source[i]))
                {
                    Frequencies.Add(source[i], 0);
                }

                Frequencies[source[i]]++;
            }

            foreach (KeyValuePair<char, int> symbol in Frequencies)
            {
                nodes.Add(new Node() { Symbol = symbol.Key, Frequency = symbol.Value });
            }

            while (nodes.Count > 1)
            {
                List<Node> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList<Node>();

                if (orderedNodes.Count >= 2)
                {
                    // Take first two items
                    List<Node> taken = orderedNodes.Take(2).ToList<Node>();

                    // Create a parent node by combining the frequencies
                    Node parent = new Node()
                    {
                        Symbol = '*',
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                this.Root = nodes.FirstOrDefault();

            }

        }

        public BitArray Encode(string source)
        {
            List<bool> encodedSource = new List<bool>();

            for (int i = 0; i < source.Length; i++)
            {
                List<bool> encodedSymbol = this.Root.Traverse(source[i], new List<bool>());
                encodedSource.AddRange(encodedSymbol);
            }

            BitArray bits = new BitArray(encodedSource.ToArray());

            return bits;
        }

        public string Decode(BitArray bits)
        {
            Node current = this.Root;
            string decoded = "";

            foreach (bool bit in bits)
            {
                if (bit)
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }

                if (IsLeaf(current))
                {
                    decoded += current.Symbol;
                    current = this.Root;
                }
            }

            return decoded;
        }

        public bool IsLeaf(Node node)
        {
            return (node.Left == null && node.Right == null);
        }

    }

    // Ajout des classes et méthodes nécessaire pour la compression RLE:
    // https://gist.github.com/lsauer/3744846 && http://en.wikipedia.org/wiki/Run-length_encoding
    public class Transform
    {
        public const char EOF = '\u007F';
        public const char ESCAPE = '/';
        public static string RunLengthEncode(string s)
        {
            
            try
            {
                string srle = string.Empty;
                int ccnt = 1; //char counter
                for (int i = 0; i < s.Length - 1; i++)
                {
                    if (s[i] != s[i + 1] || i == s.Length - 2) //..a break in character repetition or the end of the string
                    {
                        if (s[i] == s[i + 1] && i == s.Length - 2) //end of string condition
                            ccnt++;
                        srle += ccnt + ("1234567890".Contains(s[i]) ? "" + ESCAPE : "") + s[i]; //escape digits
                        if (s[i] != s[i + 1] && i == s.Length - 2) //end of string condition
                            srle += ("1234567890".Contains(s[i + 1]) ? "1" + ESCAPE : "") + s[i + 1];
                        ccnt = 1; //reset char repetition counter
                    }
                    else
                    {
                        ccnt++;
                    }

                }
                return srle;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in RLE:" + e.Message);
                return null;
            }
        }
        public static string RunLengthDecode(string s)
        {
            try
            {
                string dsrle = string.Empty
                        , ccnt = string.Empty; //char counter
                for (int i = 0; i < s.Length; i++)
                {
                    if ("1234567890".Contains(s[i])) //extract repetition counter
                    {
                        ccnt += s[i];
                    }
                    else
                    {
                        if (s[i] == ESCAPE)
                        {
                            i++;
                        }
                        dsrle += new String(s[i], int.Parse(ccnt));
                        ccnt = "";
                    }

                }
                return dsrle;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in RLD:" + e.Message);
                return null;
            }
        }
    }
}
