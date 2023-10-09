using ConsoleExtender;
using System;
using System.Collections;
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
        private const int DRAW_SYMBOLS = 0, DRAW_IMAGE_COLOR = 1, DRAW_IMAGE_GREY = 2, DRAW_SYMBOLS_OLD_STYLE = 3, DRAW_IMAGE_NEW_GRADIENT = 4, DRAW_CUSTOM_GRADIENT = 5;
        private static double coefficient = 1;
        private static double fontAspect = 2.0;
        private static bool colorInversion = false;
        private static char[] gradient = new char[] { ' ', '.', ':', '!', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' }; //add '\u25A0' to add the whitebox into the list
        //private static char[] gradient = new char[] { ' ', '.', ':', '~', '!', '=', 'v', '7', 'I', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@', '\u25A0' };
        private static char[] customGradient;
        private static int[] customGradientLightness, customScaledLightness;

        private static void setupGradient()
        {
            StreamReader sr = new StreamReader("C:\\Users\\kolch\\source\\repos\\w1ano\\ImageToSymbols1.0\\ConsoleApp5\\gradientSymbols.txt");
            String line = sr.ReadLine();
            symbolLightness.Add(' ', 0);
            symbolLightness.Add('\u25A0', 1000000);
            lightnessToSymbol.Add(0, ' ');
            lightnessToSymbol.Add(1000000, '\u25A0');
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

        private static char findSymbolByLightnessCustom(double L)
        {
            int integerL = (int)(L * 10000.0);
            int id = Array.BinarySearch(customScaledLightness, integerL);
            if (id < 0)
            {
                if (-id == customScaledLightness.Length - 1) id = -id;
                else id = -id - 2;
            }
            return lightnessToSymbol[customGradientLightness[Math.Min(Math.Max(id, 0), customGradientLightness.Length - 1)]];
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

            if (Console.LargestWindowWidth >= image.Width && Console.LargestWindowHeight >= image.Height / fontAspect)
            {
                Console.SetWindowSize(image.Width, (int)(image.Height / fontAspect));
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                return;
            }

            double imageAspect = (double)image.Height / image.Width;

            if (Console.LargestWindowWidth >= image.Width)
            {
                height = Console.LargestWindowHeight;
                width = (int)(height / imageAspect * fontAspect);
                Console.SetWindowSize(width, height);
                return;
            }

            if (Console.LargestWindowHeight >= image.Height / fontAspect) {
                width = Console.LargestWindowWidth;
                height = (int)(width * imageAspect / fontAspect);
                Console.SetWindowSize(width, height);
                return;
            }

            if (Console.LargestWindowWidth / fontAspect >= Console.LargestWindowHeight / imageAspect) //choosing width and height according to maximum quality
            {
                height = Console.LargestWindowHeight;
                width = (int)(height / imageAspect * fontAspect);
            }
            else
            {
                width = Console.LargestWindowWidth;
                height = (int)(width * imageAspect / fontAspect);
            }
            Console.SetWindowSize(width, height); //setting up new window parameters
            Console.BufferWidth = Console.WindowWidth; //in case of user changing the buffer prior
            Console.BufferHeight = Console.WindowHeight + 10;
        }

        private static void convertImage(Bitmap image)
        {
            Console.BufferWidth = width;
            int wAspect = Math.Max(image.Width / width, 1); //getting width and height aspects for the conversion
            int hAspect = Math.Max(image.Height / height, 1);

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
                            if (colorInversion) gradientNum = gradient.Length - gradientNum - 1;
                            Console.Write(gradient[gradientNum]);
                            break;
                        case DRAW_IMAGE_GREY:
                            avgSum = (int)(findLightness(sumR, sumG, sumB) * 255.0 / 100.0);
                            if (colorInversion) avgSum = 255 - avgSum;
                            Console.Write("\x1b[48;2;" + avgSum + ";" + avgSum + ";" + avgSum + "m");
                            Console.Write(" ");
                            break;
                        case DRAW_SYMBOLS_OLD_STYLE:
                            avgSum = (int)(sumR * sumG * sumB / 255.0 / 255.0 * coefficient);
                            if (colorInversion) avgSum = 255 - avgSum;
                            Console.Write(gradient[(int)Math.Max(0, Math.Min(avgSum / 255.0 * (double)gradient.Length, gradient.Length - 1))]);
                            break;
                        case DRAW_IMAGE_COLOR:
                            if (colorInversion) { sumR = 255 - sumR; sumG = 255 - sumG; sumB = 255 - sumB; }
                            Console.Write("\x1b[48;2;" + sumR + ";" + sumG + ";" + sumB + "m");
                            Console.Write(" ");
                            break;
                        case DRAW_IMAGE_NEW_GRADIENT:
                            double L = findLightness(sumR, sumG, sumB);
                            if (colorInversion) L = 100 - L;
                            Console.Write(findSymbolByLightness(L));
                            break;
                        case DRAW_CUSTOM_GRADIENT:
                            L = findLightness(sumR, sumG, sumB);
                            if (colorInversion) L = 100 - L;
                            Console.Write(findSymbolByLightnessCustom(L));
                            break;

                    }

                    imageCordX += wAspect;
                    if (i % changeW == 0 && diffW >= 0) wAspect = biggerAspectNumW > smallerAspectNumW ? wAspect + 1 : wAspect - 1;
                    if (diffW == 0) { diffW = -1; }
                }
                Console.Write("\n");
                diffW = changeW * aspectNumMinW;
                if (aspectNumMinW == 0) diffW = -1;
                imageCordX = 0;
                imageCordY += hAspect;
                if (j % changeH == 0 && diffH >= 0) hAspect = biggerAspectNumH > smallerAspectNumH ? hAspect + 1 : hAspect - 1;
                if (diffH == 0) diffH--;
            }
            Console.Write("\x1b[48;2;" + 0 + ";" + 0 + ";" + 0 + "m");
            Console.Write("");
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

            bool programmRunning = true;
            while (programmRunning)
            {
                Console.WriteLine("Press the number of the command on your keyboard");
                Console.WriteLine("1. Transform an image to symbols");
                Console.WriteLine("2. Change the brightness coefficient");
                Console.WriteLine("3. Change color inversion");
                Console.WriteLine("4. Add custom gradient");
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
                            bool imageConvertRunning = true;
                            while (imageConvertRunning)
                            {
                                Console.Clear();
                                normalizeImage(image);
                                String[] pathArray = path.Split("\\");
                                Console.Title = "Image to symbols: " + pathArray[pathArray.Length - 1] + " - " + width + "x" + (int)(height * fontAspect);
                                convertImage(image);
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                                //Console.WriteLine("Press F if you want to changeW the coefficient");
                                
                                ConsoleKey key = Console.ReadKey(true).Key;
                                switch (key)
                                {
                                    case ConsoleKey.F:
                                        Console.Clear();
                                        Console.WriteLine("Enter the coefficient");
                                        coefficient = Convert.ToDouble(Console.ReadLine().Replace(',', '.'));
                                        break;
                                    case ConsoleKey.LeftArrow:
                                        image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                                    case ConsoleKey.RightArrow:
                                        image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                                    case ConsoleKey.I:
                                        colorInversion = !colorInversion; break;
                                    case ConsoleKey.D1:
                                        drawMode = DRAW_IMAGE_NEW_GRADIENT; break;
                                    case ConsoleKey.D2:
                                        drawMode = DRAW_IMAGE_COLOR; break;
                                    case ConsoleKey.D3:
                                        drawMode = DRAW_IMAGE_GREY; break;
                                    case ConsoleKey.D4:
                                        drawMode = DRAW_CUSTOM_GRADIENT; break;
                                    default:
                                        Console.Title = "Image to symbols";
                                        imageConvertRunning = false;
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
                        try { coefficient = Convert.ToDouble(Console.ReadLine()); }
                        catch (Exception e) { Console.WriteLine(e.Message); Console.ReadKey(); }
                        break;
                    case ConsoleKey.D3:
                        Console.Clear();
                        colorInversion = !colorInversion;
                        if (colorInversion) Console.WriteLine("Color inversion is on");
                        else Console.WriteLine("Color inversion is off");
                        Console.ReadKey();
                        break;
                    case ConsoleKey.D4:
                        Console.Clear();
                        Console.WriteLine("Enter custom gradient as a string of symbols of english alphabet (capital or lower) and default special symbols");
                        String inputString = Console.ReadLine();
                        customGradient = inputString.ToCharArray();
                        List<int> listCustomGradient = new List<int>();
                        for (int i = 0; i < customGradient.Length; i++)
                        {
                            listCustomGradient.Add(symbolLightness[customGradient[i]]);
                        }
                        customGradientLightness = listCustomGradient.ToArray();
                        customGradientLightness = customGradientLightness.OrderBy(i => i).ToArray();
                        double aspect = 999999 / customGradientLightness[customGradientLightness.Length - 1];
                        customScaledLightness = new int[customGradientLightness.Length];
                        for (int i = 0; i < customGradientLightness.Length; i++) customScaledLightness[i] = (int)(customGradientLightness[i] * aspect);
                        customGradientLightness.ToList().ForEach(i => Console.WriteLine(lightnessToSymbol[i] + " " + i));
                        Console.ReadKey();
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
