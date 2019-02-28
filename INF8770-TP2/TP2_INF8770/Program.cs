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
            Bitmap imageBM = new Bitmap("mario.png");
            int width = imageBM.Width;
            int height = imageBM.Height;

            // Création des tableaux 2D qui vont contenir les informations de Y Cb Cr
            byte[,] yData = new byte[width, height]; 
            byte[,] bData = new byte[width/2, height/2];
            byte[,] rData = new byte[width/2, height/2];

            // Applique la conversion et division des résultats de chaques composantes dans leur tableau respectif
            Bitmap nouvelleimage = rgb2ycbcr(imageBM, yData, bData, rData);
            ycbcr2rgb(nouvelleimage);

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
            DCT(blocks);
        }

        public static Bitmap rgb2ycbcr(Bitmap bmp, byte[,] yData, byte[,] bData, byte[,] rData)
        {
            int width = bmp.Width;
            int height = bmp.Height;                   

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

        public void DCT(List<byte[,]> tab8x8)
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
        public const char ESCAPE = '\\';

        public static string RunLengthEncode(string s)
        {
            try
            {
                string srle = string.Empty;
                int ccnt = 1; //char counter
                for (int i = 0; i < s.Length-1; i++ )
                {
                    if (s[i] != s[i + 1] || i == s.Length-2 ) //..a break in character repetition or the end of the string
                    {
                        if (s[i] == s[i + 1] && i == s.Length - 2) //end of string condition
                            ccnt++;
                        srle += ccnt + ("1234567890".Contains(s[i]) ? ""+ESCAPE : "") + s[i]; //escape digits
                        if (s[i] != s[i + 1] && i == s.Length - 2) //end of string condition
                            srle += ("1234567890".Contains(s[i + 1]) ? "1" + ESCAPE : "") + s[i + 1]; 
                        ccnt = 1; //reset char repetition counter
                    }
                    else {
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
            try {
                string  dsrle = string.Empty
                        , ccnt = string.Empty; //char counter
                for (int i = 0; i < s.Length; i++) {
                    if ( "1234567890".Contains(s[i]) ) //extract repetition counter
                    {
                        ccnt += s[i];
                    }
                    else {
                        if (s[i] == ESCAPE) {
                            i++;
                        }
                        dsrle += new String(s[i], int.Parse(ccnt));
                        ccnt = "";
                    }

                }
                return dsrle;
            }
            catch (Exception e) {
                Console.WriteLine("Exception in RLD:" + e.Message);
                return null;
            }
        }
    }
}
