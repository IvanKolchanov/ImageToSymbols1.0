using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageToSymbols
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);

        private static int height;
        private static int width;
        private static double coefficient = 2;
        private static bool colorInversion = false;
        //private static char[] gradient = new char[] { ' ', '.', ':', '!', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@' }; //add '\u25A0' to add the whitebox into the list
        private static char[] gradient = new char[] { ' ', '.', ':', '~', '!', '=', 'v', '7', 'I', '/', 'r', '(', 'l', '1', 'Z', '4', 'H', '9', 'W', '8', '$', '@', '\u25A0' };
        
        private static void normalizeImage(Bitmap image)
        {
            double imageAspect = (double)image.Height / image.Width;
            if (imageAspect > 1) //making all photos landscape, because the height of the monitor is more restrictive in amount of symbols than width
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                imageAspect = 1.0 / imageAspect;
            }

            if (Console.LargestWindowWidth / 2.0 >= Console.LargestWindowHeight / imageAspect) //choosing width and height according to maximum quality
            {
                height = Console.LargestWindowHeight;
                width = (int)(height / imageAspect) * 2;

            }
            else
            {
                width = Console.LargestWindowWidth / 2 * 2;
                height = (int)(width * imageAspect / 2);
            }

            Console.SetWindowSize(width, height); //setting up new window parameters
            Console.BufferWidth = Console.WindowWidth; //in case of user changing the buffer prior
            Console.BufferHeight = Console.WindowHeight + 50;
        }

        private static void convertImage(Bitmap image)
        {
            int wAspect = image.Width / width; //getting width and height aspects for the conversion
            int hAspect = image.Height / height;
            int biggerAspectNumW = image.Width - wAspect * width, smallerAspectNumW = width - biggerAspectNumW, diffW = Math.Min(biggerAspectNumW, smallerAspectNumW) * 2; //biggerAspectNumW and smallerAspectNumW
            
            bool change = false;
            int hY = image.Height - hAspect * height, hX = height - hY;
            //Console.WriteLine(wAspect + " " + smallerAspectNumW + " " + biggerAspectNumW + " " + diffW);
            int imageCordX = 0, imageCordY = 0;
            long oneColor = 255 * 255 * 255 / gradient.Length;
            for (int j = 0; j < height; j++)
            {
                if (j >= hX) hAspect++;
                for (int i = 0; i < width; i++)
                {
                    long sum = 0;
                    if (change && diffW > 0) { wAspect++; diffW -= 2; }
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
                    if (colorInversion) gradientNum = gradient.Length - gradientNum - 1;
                    Console.Write(gradient[gradientNum]);

                    imageCordX += wAspect;
                    if (change && diffW >= 0) wAspect--;
                    change = !change;
                    if (diffW == 0) { wAspect = biggerAspectNumW > smallerAspectNumW ? wAspect + 1 : wAspect; diffW = -1; }
                }
                diffW = Math.Min(biggerAspectNumW, smallerAspectNumW) * 2;
                wAspect = biggerAspectNumW > smallerAspectNumW ? wAspect - 1 : wAspect;
                imageCordX = 0;
                imageCordY += hAspect;
                if (j >= hX) hAspect--;
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Image to symbols";
            height = Console.WindowHeight;
            width = Console.WindowWidth;
            Console.BufferHeight = Console.LargestWindowHeight + 10;

            

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
                                Console.WriteLine("Press F if you want to change the coefficient");
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
