# ImageToSymbols1.0
##Main goal of the program:
  * converting an image into symbols as close as possible to the initial image
  * adapting the program for different fonts, sizes of console and photo orientation

##Added features:
  * drawing the image in RGB color format
  * drawing the image in black and white format using 256 shades of grey
  * inverting the images brightness (for symbols and black&white) and colors for color mode
  * changing the orientation of the image
  * adding a custom gradient for the output (any special symbols, lower and upper letters of english alphabet, numbers and '\u25A0' (white square)
  * changing the lower and upper borders for ' ' and '\u25A0' (white box) respectively

##How does the gradient work:
  1. I analyzed all standart special symbols - ~!@#$%^&*()_+-=[]{}|\'";:/?.>,<` , numbers and lowercase and uppercase english alphabet - 94 symbols, using a program I wrote
  2. For determing the brightness of the symbol I found the average of Lightness of its pixels (to calculate lightness I transformed RGB values to linear and used recommened formulas:
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/60fc68ec-0ae4-4096-8cb8-163cdcf820d1)
  3. The perceived lightness is determined on the scale of 0.0 to 100.0. I took it with the precision of .4 decimal places and multiplied by 10000, because it is easier to save integer values
  4. The lightness values of symbols are uploaded to the program using a .txt file, which can be gotten for any given number of symbols if you are willing to run the program with another set
  5. The values of symbol were from around 50000 (5 out of 100 for ') to around 400000 (40 out of 100 for @). Therefore, during the start of the program the values are scaled depending on top and lower border for '\u25A0' (white square) and ' ' respectively
  6. Then depending on the size of the image the program changes the size of the console for the one with best resolution
  7. Then the program calculates average lightness for group's of pixels each representing one symbol and depending on the lightness the most appropriate symbol is chosen

!!! Feature/bug/strange behaviour of windows is that on some devices it is possible to set the font to umimanagably small values like 1 pixel wide and 2 pixels high, even though through consoles setting the smallest possible is 5 to 10 pixels (it says that the actual values are 2 to 5, but it is proven wrong if you take a photo of the console)!!!  <br>
!!! Using the feature you can make the font very small just by rescaling the console window with your fingers on a touchsreen or on a touchpad, allowing the user to output images in almost original quality!!!

##Instructions for work are here and in the program itself:

  ###Main menu: <br>
  1. Transform an image to symbols <br>
  2. Change the brightness coefficient <br>
  3. Change color inversion <br>
  4. Add custom gradient <br>
  5. Set up upper border for '\u25A0' symbol <br>
  6. Set up lower border for ' ' symbol <br>
  7. Read the instruction <br>
  8. Return to default settings <br>
     
  ###Output menu: <br><br>
    The instructions for the main menu are quite clear, but there are a few hidden commands for 1. menu screen <br><br>
    * After submitting a photo for the convertion while viewing it is possible to change the image in a few ways: <br>
    * pressing F allows the user to change the brightness of the image <br>
    * pressing I allows the user to change the output into inverse mode (inverse brightness, color) <br>
    * pressing 1 puts the conversion into symbol mode (default mode) <br>
    * pressing 2 puts the conversion into color mode <br>
    * pressing 3 puts the conversion into black and white color mode <br>
    * pressing 4 puts the conversion into custom gradient symbol mode (only works if you set it up in main menu) <br>
    * pressing RightArrow (->) turns the image 90 degrees clockwise <br>
    * pressing LeftArrow (<-) turns the image 90 degrees counter-clockwise <br>
    * pressing 0 puts the conversion into old symbol mode, used in the original program <br>
    The quality of the image is dependent on the size of the font (the smaller the font, the better the image) <br>
    You can make the font smaller by right clicking on the console (smallest font would be 5 pixels wide) <br>
    For some laptop users it is possible to make it even smaller (1 pixel wide) by resizing the window with fingers/touchpad <br>

##Examples: <br>
  Original image: <br>
  ![4](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/fa3b00b5-87a3-477a-9f23-414548ab41fe)
  
  Image to symbols (font - 5 by 10 pixels, image - 576x286 symbols): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/65cb45f4-41e2-42ee-b836-e2bedecc064a)
  
  Image to color (font - 5 by 10 pixels, image - 576x286 symbols): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/09eb9fb7-3edf-42e4-8bc7-b9ece60e6328)
  
  Image to black&white (font - 5 by 10 pixels, image - 576x286 symbols): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/5cedfb48-edd6-4afb-b37a-5a02a4cd58eb)
  
  Image to symbols, custom gradient (font - 5 by 10 pixels, image - 576x286 symbols, gradient - "abc123)!-"): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/f201ad8c-deb4-4afc-a5b5-0fcf6523962e)
  
  Image to color (font - 1 by 2 pixels, image - 761x378 (original resolution)): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/3b356ea8-8fdb-4882-b912-da548a533a69)
  
  Image to symbols with inversion (font - 1 by 2 pixels, image - 761x378): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/a020f659-96bc-4d02-8f07-2fe22d2d9b54)
  
  Image to color with inversion (font - 1 by 2 pixels, image - 761x378): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/c0fd7f12-0f3e-4f7a-8eac-bb597b450089)
  
  Image to color with lightness coefficient changed to 1.1 (def - 1.0) (font - 1 by 2 pixels, image - 761x378): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/ee3c0f27-4225-41bc-b7fa-60e27bcee9ed)
  
  Image to color with lightness coefficient changed to 1.1 (def - 1.0) and inversion (font - 1 by 2 pixels, image - 761x378): <br>
  ![image](https://github.com/IvanKolchanov/ImageToSymbols1.0/assets/83294629/57b16fee-e132-4a44-b2d3-b87e638c5b6b)
  







