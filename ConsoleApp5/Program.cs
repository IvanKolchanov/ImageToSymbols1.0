using System;
using System.Drawing;

namespace ImageToSymbols
{
    class Program
    {
        private static int height;
        private static int width;
        private static bool bestFit = true;
        private static double coefficient = 2;
        private static bool colorInversion = false;
        private static char[] gradient = new char[] { ' ', '.', ':', '!', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' }; //add '\u25A0' to add the whitebox into the list
        //char[] gradient = new char[] { '.', ':', '~', '!', '=', 'v', '7', 'I', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' };
        private static void normalizeImage(Bitmap image)
        {
            long oneColor = 255 * 255 * 255 / gradient.Length;
            int wAspect = 1, hAspect = 1;
            Console.BufferHeight = Console.LargestWindowHeight + 10;
            int wX = -1, wY = -1, hX = -1, hY = -1;
            if (bestFit)
            {
                double imageAspect = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
                if (imageAspect > 1) {
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    imageAspect = 1.0 / imageAspect;
                }
                if (Console.LargestWindowWidth / 2.0 >= Console.LargestWindowHeight / imageAspect)
                {
                    height = Console.LargestWindowHeight;
                    width = (int)(height / imageAspect);
                    
                }else
                {
                    width = (int)(Console.LargestWindowWidth / 2.0);
                    height = (int)(width * imageAspect / 2.0);
                }
                Console.SetWindowSize(width * 2, height);
                Console.BufferWidth = Console.WindowWidth;
                wAspect = image.Width / width;
                hAspect = image.Height / height;
                wY = image.Width - wAspect * width; wX = width - wY;
                hY = image.Height - hAspect * height; hX = height - hY;
                Console.WriteLine(width + " " + height + " " + wAspect + " " + hAspect + " " + wX + " " + hX);
            }
            else
            {
                double aspect = (double)width / height;
                double currentAscpect = (double)image.Width / image.Height;
                int h = image.Height, w = image.Width;
                if (aspect > currentAscpect)
                {
                    h = (int)(w / aspect);
                }
                else
                {
                    w = (int)(h * aspect);
                }
                wAspect = w / width;
                hAspect = h / height;
            }
            int imageCordX = 0, imageCordY = 0;
            for (int j = 0; j < height; j++)
            {
                if (bestFit && j >= hX) hAspect++;
                for (int i = 0; i < width; i++)
                {
                    long sum = 0;
                    if (bestFit && i >= wX) wAspect++;
                    for (int z = imageCordY; z < imageCordY + hAspect; z++)
                    {
                        for (int t = imageCordX; t < imageCordX + wAspect; t++)
                        {
                            Color pixel = image.GetPixel(t, z);
                            int pixelBrightness = (int)(coefficient * (double)pixel.R * pixel.G * pixel.B);
                            sum += pixelBrightness;
                        }
                    }
                    sum = sum / (wAspect * hAspect);
                    int gradientNum = (int)Math.Max(0, Math.Min(sum / oneColor, gradient.Length - 1));
                    if (!colorInversion)
                    {
                        Console.Write(gradient[gradientNum] + "" + gradient[gradientNum]);
                    }
                    else
                    {
                        Console.Write(gradient[gradient.Length - 1 - gradientNum] + "" + gradient[gradient.Length - 1 - gradientNum]);
                    }
                    imageCordX += wAspect;
                    if (bestFit && i >= wX) wAspect--;
                }
                imageCordX = 0;
                imageCordY += hAspect;
                if (bestFit && j >= hX) hAspect--;
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Image to symbols";
            //Console.SetWindowSize(Math.Min(270, Console.LargestWindowWidth), Math.Min(135, Console.LargestWindowHeight));
            height = Console.WindowHeight;
            width = Console.WindowWidth / 2;
            while (true)
            {
                Console.WriteLine("Press the number of the command on your keyboard");
                Console.WriteLine("1. Transform an image to symbols");
                Console.WriteLine("2. Change the coefficient of transforming");
                Console.WriteLine("3. Change window's parameters");
                Console.WriteLine("4. Change color inversion");
                Console.WriteLine("5. Change best fit");
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
                                Console.WriteLine("Press F if you want to change the coefficient");
                                if (!bestFit) Console.WriteLine("Press ArrowRight or ArrowLeft to turn the picture 90 degrees in the chosen direction");
                                ConsoleKey key = Console.ReadKey(true).Key;
                                switch (key)
                                {
                                    case ConsoleKey.F:
                                        Console.Clear();
                                        Console.WriteLine("Enter the coefficient");
                                        coefficient = Convert.ToDouble(Console.ReadLine().Replace(',', '.'));
                                        break;
                                    case ConsoleKey.LeftArrow:
                                        if (!bestFit) image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        break;
                                    case ConsoleKey.RightArrow:
                                        if (!bestFit) image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                        break;
                                    default:
                                        check = false;
                                        break;
                                }
                            }
                        }catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadKey();
                        }
                        break;
                    case ConsoleKey.D2:
                        Console.WriteLine("Enter the coefficient");
                        try
                        {
                            coefficient = Convert.ToDouble(Console.ReadLine());
                        }catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadKey();
                        }
                        break;
                    case ConsoleKey.D3:
                        Console.Clear();
                        Console.WriteLine("Enter new width and new height in the separate lines");
                        try
                        {
                            int width1 = Math.Min(Math.Max(60, Convert.ToInt32(Console.ReadLine())), 224);
                            int height1 = Math.Min(Math.Max(30, Convert.ToInt32(Console.ReadLine())), 63);
                            Console.SetWindowSize(width1, height1);
                            Console.BufferWidth = width1;
                            height = height1;
                            width = width1 / 2;
                            Console.WriteLine("Press any key to continue");
                        }
                        catch (Exception e) {
                            Console.WriteLine(e.Message);
                        }
                        Console.ReadKey();
                        break;
                    case ConsoleKey.D4:
                        Console.Clear();
                        if (colorInversion)
                        {
                            Console.WriteLine("Color inversion is on");
                        }else
                        {
                            Console.WriteLine("Color inversion is off");
                        }
                        Console.WriteLine("\nIf you want to leave it that way, press ESC");
                        Console.WriteLine("If not, press any button");
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape) break;
                        colorInversion = !colorInversion;
                        break;
                    case ConsoleKey.D5:
                        Console.Clear();
                        if (bestFit) Console.WriteLine("Best fit is turned on");
                        else Console.WriteLine("Best fit is turned off");
                        Console.WriteLine("\nIf you want to leave it that way, press ESC");
                        Console.WriteLine("If not, press any button");
                        if(Console.ReadKey(true).Key == ConsoleKey.Escape) break;
                        bestFit = !bestFit;
                        break;

                }
                Console.Clear();
            }
        }
    }
}
