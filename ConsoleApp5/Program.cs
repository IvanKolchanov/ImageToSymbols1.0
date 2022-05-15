using System;
using System.Drawing;

namespace ImageToSymbols
{
    class Program
    {
        private static int height;
        private static int width;
        private static double coefficient = 2;
        private static bool colorInversion = false;

        private static void normalizeImage(Bitmap image)
        {
            char[] gradient = new char[] {' ', '.', ':', '!', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' };
            //char[] gradient = new char[] { '.', ':', '~', '!', '=', 'v', '7', 'I', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' };
            long oneColor = 255 * 255 * 255 / gradient.Length;

            if (image.Width == height && image.Width == width)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {

                        Color pixelColor = image.GetPixel(i, j);
                        int pixelColorBrightness = pixelColor.R * pixelColor.B * pixelColor.G;
                        int gradientNum = (int)Math.Min(pixelColorBrightness / oneColor, gradient.Length - 1);
                        Console.Write(gradient[gradientNum] + "" + gradient[gradientNum]);
                    }
                }
            }
            else
            {
                double aspect = (double)width / height;
                double currentAscpect = (double)image.Width / image.Height;
                int h = image.Height, w = image.Width;
                if (aspect > currentAscpect)
                {
                    h = (int)(w / aspect);
                }else
                {
                    w = (int)(h * aspect);
                }
                int wAspect = w / width;
                int hAspect = h / height;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        long sum = 0;
                        for (int z = j * hAspect; z < hAspect * (j + 1); z++)
                        {
                            
                            for (int t = i * wAspect; t < wAspect * (i+1); t++)
                            {
                                Color pixel = image.GetPixel(t, z);
                                int pixelBrightness = (int)coefficient * pixel.R * pixel.G * pixel.B;
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
                    }
                }
            }     
        }

        static void Main(string[] args)
        {
            Console.Title = "Image to symbols";
            Console.SetWindowSize(224, 63);
            height = Console.WindowHeight;
            width = Console.WindowWidth / 2;
            while (true)
            {
                Console.WriteLine("Press the number of the command on your keyboard");
                Console.WriteLine("1. Transform an image to symbols");
                Console.WriteLine("2. Change the coefficient of transforming");
                Console.WriteLine("3. Change window's parameters");
                Console.WriteLine("4. Change color inversion");
                ConsoleKey input = Console.ReadKey(true).Key;
                switch (input)
                {
                    case ConsoleKey.D1:
                        Console.Clear();
                        Console.WriteLine("Enter the full path to the photo you want to transform");
                        String path = Console.ReadLine();
                        try
                        {
                            Bitmap image = new Bitmap(@path, true);
                            bool check = true;
                            while (check)
                            {
                                Console.Clear();
                                normalizeImage(image);
                                Console.WriteLine("Press A if you want to change the coefficient");
                                Console.WriteLine("Press ArrowRight or ArrowLeft to turn the picture 90 degrees in the chosen direction");
                                ConsoleKey key = Console.ReadKey(true).Key;
                                switch (key)
                                {
                                    case ConsoleKey.A:
                                        Console.Clear();
                                        Console.WriteLine("Enter the coefficient");
                                        coefficient = Convert.ToDouble(Console.ReadLine().Replace('.', ','));
                                        break;
                                    case ConsoleKey.LeftArrow:
                                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        break;
                                    case ConsoleKey.RightArrow:
                                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
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
                        Console.WriteLine("\n" + "If you want to leave it that way, press ESC");
                        Console.WriteLine("If not, press any button");
                        ConsoleKey input1 = Console.ReadKey(true).Key;
                        if (input1 == ConsoleKey.Escape) break;
                        colorInversion = !colorInversion;
                        break;
                }
                Console.Clear();
            }
        }
    }
}
