using ConsoleExtender;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageToSymbols
{
    class Program
    {
        private static Dictionary<char, int> symbolLightness = new Dictionary<char, int>();
        private static Dictionary<int, char> lightnessToSymbol = new Dictionary<int, char>();
        private static int[] lightnessList;
        private static int height;
        private static int width;
        private static int drawMode = 4;
        private const int DRAW_SYMBOLS = 0, DRAW_IMAGE_COLOR = 1, DRAW_IMAGE_GREY = 2, DRAW_SYMBOLS_OLD_STYLE = 3, DRAW_IMAGE_NEW_GRADIENT = 4;
        private static double coefficient = 2;
        private static double fontAspect = 2.0;
        private static bool colorInversion = false;
        private static char[] gradient = new char[] { ' ', '.', ':', '!', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' }; //add '\u25A0' to add the whitebox into the list
        //private static char[] gradient = new char[] { ' ', '.', ':', '~', '!', '=', 'v', '7', 'I', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@', '\u25A0' };

        private static void setupGradient()
        {
            StreamReader sr = new StreamReader("C:\\Users\\kolch\\source\\repos\\w1ano\\ImageToSymbols1.0\\ConsoleApp5\\gradientSymbols.txt");
            String line = sr.ReadLine();
            symbolLightness.Add(' ', 0);
            symbolLightness.Add('\u25A0', 10000000);
            lightnessToSymbol.Add(0, ' ');
            lightnessToSymbol.Add(10000000, '\u25A0');
            while (line != null)
            {
                String[] a = line.Split(" ");
                symbolLightness.Add(a[0][0], Int32.Parse(a[1]));
                lightnessToSymbol.Add(Int32.Parse(a[1]), a[0][0]);
                line = sr.ReadLine();
            }
            int[] lightnessListInt = lightnessToSymbol.Keys.ToArray();
            lightnessList = lightnessListInt.OrderBy(x => x).ToArray();

            int topBorder = 999999;
            double aspect = (double)topBorder / lightnessList[lightnessList.Length - 2];
            char replaceVal = lightnessToSymbol[lightnessList[lightnessList.Length - 2]];
            lightnessToSymbol.Remove(lightnessList[lightnessList.Length - 2]);
            symbolLightness.Remove(replaceVal);
            lightnessList[lightnessList.Length - 2] = topBorder;
            lightnessToSymbol.Add(topBorder, replaceVal);
            symbolLightness.Add(replaceVal, topBorder);
            
            for (int i = 0; i < lightnessList.Length - 2; i++)
            {
                char a = lightnessToSymbol[lightnessList[i]];
                lightnessToSymbol.Remove(lightnessList[i]);
                symbolLightness.Remove(a);
                lightnessList[i] = (int)(lightnessList[i] * aspect);
                lightnessToSymbol[lightnessList[i]] = a;
                symbolLightness[a] = lightnessList[i];
            }

            //lightnessList.ToList().ForEach(i => Console.WriteLine(i));
            //symbolLightness.ToList().ForEach(i => Console.WriteLine(i));
        }

        private static char findSymbolByLightness(double L)
        {
            int integerL = (int)(L * 10000.0);
            int id = Array.BinarySearch(lightnessList, integerL);
            if (id < 0)
            {
                if (-id == lightnessList.Length - 1) id = -id;
                else id = -id - 2;
            }
            return lightnessToSymbol[lightnessList[id]];
        }

        private static double sRGBtoLin(double colorChannel)
        {
            if (colorChannel <= 0.04045) return colorChannel / 12.92;
            else return Math.Pow((colorChannel + 0.055) / 1.055, 2.4);
        }

        private static double findLightness(long R, long G, long B)
        {
            double vR = R / 255.0, vG = G / 255.0, vB = B / 255.0;
            double lR = sRGBtoLin(vR), lG = sRGBtoLin(vG), lB = sRGBtoLin(vB);
            double Y = lR * 0.2126 + lG * 0.7152 + lB * 0.0722;
            if (Y <= 0.008856) return Y * 903.3;
            else return Math.Pow(Y, 1.0 / 3.0) * 116 - 16;
        }

        private static void normalizeImage(Bitmap image) 
        {
            fontAspect = (double)ConsoleHelper.GetConsoleFontSize().Height / ConsoleHelper.GetConsoleFontSize().Width;
            if (ConsoleHelper.GetConsoleFontSize().Height == 5) fontAspect = 2;
            if (ConsoleHelper.GetConsoleFontSize().Height < 5) fontAspect = 2;

            double imageAspect = (double)image.Height / image.Width;
            if (imageAspect > 1 && image.Height > Console.LargestWindowHeight && image.Width > Console.LargestWindowWidth) //making all photos landscape, because the height of the monitor is more restrictive in amount of symbols than width
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                imageAspect = 1.0 / imageAspect;
            }else if (Console.LargestWindowWidth >= image.Width && Console.LargestWindowHeight >= image.Height)
            {
                Console.SetWindowSize(image.Width, (int)(image.Height / fontAspect));
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                return;
            }

            if (Console.LargestWindowWidth / fontAspect >= Console.LargestWindowHeight / imageAspect) //choosing width and height according to maximum quality
            {
                height = Math.Min(Console.LargestWindowHeight, image.Height);
                width = Math.Min((int)(height / imageAspect * fontAspect), image.Width);
            }
            else
            {
                width = Math.Min(Console.LargestWindowWidth, image.Width);
                height = Math.Min((int)Math.Round(width * imageAspect / fontAspect), image.Height);
            }
            Console.SetWindowSize(width, height); //setting up new window parameters
            Console.BufferWidth = Console.WindowWidth; //in case of user changing the buffer prior
            Console.BufferHeight = Console.WindowHeight + 10;
        }

        private static void convertImage(Bitmap image)
        {
            int wAspect = Math.Min(image.Width / width, 1); //getting width and height aspects for the conversion
            int hAspect = Math.Min(image.Height / height, 1);

            int biggerAspectNumW = image.Width - wAspect * width, smallerAspectNumW = width - biggerAspectNumW; //biggerAspectNumW and smallerAspectNumW
            int aspectNumMaxW = Math.Max(biggerAspectNumW, smallerAspectNumW), aspectNumMinW = biggerAspectNumW + smallerAspectNumW - aspectNumMaxW;
            int diffW, changeW;
            if (aspectNumMinW != 0)
            {
                changeW = aspectNumMaxW / aspectNumMinW + 1;
                diffW = changeW * aspectNumMinW;
            }else
            {
                diffW = -1; 
                changeW = 1;
            }
            
            if (biggerAspectNumW > smallerAspectNumW) wAspect++;

            int biggerAspectNumH = image.Height - hAspect * height, smallerAspectNumH = height - biggerAspectNumH;
            int aspectNumMaxH = Math.Max(biggerAspectNumH, smallerAspectNumH), aspectNumMinH = biggerAspectNumH + smallerAspectNumH - aspectNumMaxH;
            int changeH, diffH;
            if (aspectNumMinH != 0)
            {
                changeH = aspectNumMaxH / aspectNumMinH + 1;
                diffH = changeH * aspectNumMinH;
            }
            else
            {
                diffH = -1; changeH = 1;
            }
            
            
            if (biggerAspectNumH > smallerAspectNumH) hAspect++;
            
            int imageCordX = 0, imageCordY = 0;
            for (int j = 0; j < height; j++)
            {
                if (j % changeH == 0 && diffH > 0) { hAspect = biggerAspectNumH > smallerAspectNumH ? hAspect - 1 : hAspect + 1; diffH -= changeH; }
                for (int i = 0; i < width; i++)
                {
                    long sumR = 0, sumG = 0, sumB = 0;
                    if (i % changeW == 0 && diffW > 0) { wAspect = biggerAspectNumW > smallerAspectNumW ? wAspect - 1 : wAspect + 1; diffW -= changeW; }
                    for (int z = imageCordY; z < imageCordY + hAspect; z++)
                    {
                        for (int t = imageCordX; t < imageCordX + wAspect; t++)
                        {
                            Color pixel = image.GetPixel(t, z);
                            sumR += pixel.R;
                            sumG += pixel.G;
                            sumB += pixel.B;
                        }
                    }
                    sumR /= (wAspect * hAspect);
                    sumG /= (wAspect * hAspect);
                    sumB /= (wAspect * hAspect);
                    int avgSum;
                    switch (drawMode)
                    {
                        case DRAW_SYMBOLS:
                            avgSum = (int)(findLightness(sumR, sumG, sumB) * 255.0 / 100.0);
                            int gradientNum = (int)Math.Max(0, Math.Min(avgSum / 255.0 * (double)gradient.Length, gradient.Length - 1));
                            Console.Write(gradient[gradientNum]);
                            break;
                        case DRAW_IMAGE_GREY:
                            avgSum = (int)(findLightness(sumR, sumG, sumB) * 255.0 / 100.0);
                            Console.Write("\x1b[48;2;" + avgSum + ";" + avgSum + ";" + avgSum + "m");
                            Console.Write(" ");
                            break;
                        case DRAW_SYMBOLS_OLD_STYLE:
                            avgSum = (int)(sumR * sumG * sumB / 255.0 / 255.0 * coefficient);
                            Console.Write(gradient[(int)Math.Max(0, Math.Min(avgSum / 255.0 * (double)gradient.Length, gradient.Length - 1))]);
                            break;
                        case DRAW_IMAGE_COLOR:
                            Console.Write("\x1b[48;2;" + sumR + ";" + sumG + ";" + sumB + "m");
                            Console.Write(" ");
                            break;
                        case DRAW_IMAGE_NEW_GRADIENT:
                            double L = findLightness(sumR, sumG, sumB);
                            Console.Write(findSymbolByLightness(L));
                            break;

                    }

                    imageCordX += wAspect;
                    if (i % changeW == 0 && diffW >= 0) wAspect = biggerAspectNumW > smallerAspectNumW ? wAspect + 1 : wAspect - 1;
                    if (diffW == 0) { diffW = -1; }
                }
                diffW = changeW * aspectNumMinW;
                if (aspectNumMinW == 0) diffW = -1;
                imageCordX = 0;
                imageCordY += hAspect;
                if (j % changeH == 0 && diffH >= 0) hAspect = biggerAspectNumH > smallerAspectNumH ? hAspect + 1 : hAspect - 1;
                if (diffH == 0) diffH--;
            }
            Console.Write("\x1b[48;2;" + 0 + ";" + 0 + ";" + 0 + "m");
        }

        static void Main(string[] args)
        {
            Console.Title = "Image to symbols";
            height = Console.WindowHeight;
            width = Console.WindowWidth;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BufferHeight = Console.LargestWindowHeight + 10;
            ConsoleHelper.setupConsole();
            setupGradient();
            //Console.WriteLine(lightnessList.Length);
            //Console.WriteLine("\"" + lightnessToSymbol[lightnessList[0]] + "\"");
            //Console.WriteLine(findSymbolByLightness(1) + " " + findSymbolByLightness(10) + " " + findSymbolByLightness(100));

            bool programmRunning = true;
            while (programmRunning)
            {
                Console.WriteLine("Press the number of the command on your keyboard");
                Console.WriteLine("1. Transform an image to symbols");
                Console.WriteLine("2. Change the brightness coefficient");
                Console.WriteLine("3. Change color inversion");
                ConsoleKey input = Console.ReadKey(true).Key;
                switch (input)
                {
                    case ConsoleKey.D1:
                        Console.Clear();
                        Console.WriteLine("Enter the full path to the photo you want to transform");
                        String path = Console.ReadLine().Replace("\"", "");
                        try
                        {
                            Bitmap image = new Bitmap(@path, true);
                            bool check = true;
                            while (check)
                            {
                                Console.Clear();
                                normalizeImage(image);
                                convertImage(image);
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Press F if you want to changeW the coefficient");
                                
                                ConsoleKey key = Console.ReadKey(true).Key;
                                switch (key)
                                {
                                    case ConsoleKey.F:
                                        Console.Clear();
                                        Console.WriteLine("Enter the coefficient");
                                        coefficient = Convert.ToDouble(Console.ReadLine().Replace(',', '.'));
                                        break;
                                    default:
                                        check = false;
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadKey();
                        }
                        break;
                    case ConsoleKey.D2:
                        Console.WriteLine("Enter the coefficient (standart is 2)");
                        try
                        {
                            coefficient = Convert.ToDouble(Console.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadKey();
                        }
                        break;
                    case ConsoleKey.D3:
                        Console.Clear();
                        if (colorInversion)
                        {
                            Console.WriteLine("Color inversion is on");
                        }
                        else
                        {
                            Console.WriteLine("Color inversion is off");
                        }
                        Console.WriteLine("\nIf you want to leave it that way, press ESC");
                        Console.WriteLine("If not, press any button");
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape) break;
                        colorInversion = !colorInversion;
                        break;
                    case ConsoleKey.D4:
                        
                        break;
                    case ConsoleKey.Escape:
                        programmRunning = false;
                        break;

                }
                Console.Clear();
            }
        }
    }
}
