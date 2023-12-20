# ImageFun

## ImageFun1

This program generates something like line-by-line histograms from an image in any colors you choose. It will output multiple images based on how much of a color is in a particular section of the image.  

### Command Line Options
- **-i, --input** : The path to the input image.
- **-o, --ouput** : The output directory. Output files will be named with their color.
- **-c, --colors** : A list of colors (separated by spaces). Can be system color names or hex codes starting with `#`. ex: `-c white #ff4500 #87ceeb teal`
- **-m, --mode** : Either `divide` or `dotp`. Two different ways of calculating the amount of any given color in another. `divide` produces images that look better when layered with each other, `dotp` produces images that look better on their own. Default/invalid will use `divide`.
- **-f, --factor** : In `dotp` mode, this decimal number will adjust the curve. 1 is default, above 1 will lower the average amount of each color. (values <1 will probably crash, TODO fix that.)
- **-a, --alpha** : In `divide` mode, this flag will multiply the image's pixels' color by their opacity, making more tranparent pixels have less of an effect.
- **-w, --kernelWidth** : The width of the blocks. Higher numbers make the image more blurry. Default 1.
- **-h, --kernelHeight** : The height of the blocks. Lower numbers lose precision, while higher numbers lose resolution. Default 16.
