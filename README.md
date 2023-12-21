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

## Pixel Sorter 4

The latest and by far most powerful iteration of my pixel sorter.

### Command Line Options

- **-i, --input** : The path to the input image.
- **-o, --ouput** : The output directory. Output files will be named with their sorting string.
- **-m, --size-mode** : Width and height are interpreted as... 0: pixels; 1: divisions (Default: 0)
- **-w, --chunk-w** : The block width in pixels or divisions (see `size-mode`, Default: 1)
- **-h, --chunk-h** : The block height in pixels or divisions (see `size-mode`, Default: 16)
- **-r, --orientation** : The "up" direction of the sorted pixels... 0: horizontal; 1: vertical (Default: 0)
- **-s, --sort-stats** : A list of space-separated strings that describe how to sort the pixels. Each string will be output as its own image with the string as the name. Many strings may use a lot of memory and take a lot of time.

#### Sort Mode String

The sort mode string is made up of sorting rules separated by `-`. The pixels will first be sorted by the first sorting rule, then any pixels with the same values will be sorted according to the next sorting rule, and so on.  
Sorting rules have the following form:  
Either `hue`, `saturationhsl`, `saturationhsv`, `lightness`, `value`, `r`, `g`, `b`, a system color name, or a color hex starting with `#`.  
Optionally followed by `]` to signal ascending sort order or `[` for descending sort order. If ommitted, ascending will be used.  
Optionally, if a color or hex was used, followed by a `.0` or `.1` to change the way color difference/distance is calculated. Results may vary significantly or only slightly. Defaults to `.0` if ommitted.  

Example:  
```
-s r[-blue.1 hue value-teal[.0
```  
This will generate three images:  
One where the pixels have first been sorted descending according to their rgb R value, and then pixels with the same R value have been sorted (ascending) according to their closeness to the system color `blue`. It will be called `r[-blue.1.png`.  
One where the pixels have been sorted according to their hue, and pixels with the same hue are more-or-less randomly arranged next to each other. It will be called `hue.png`.  
One where the pixels have been sorted by value, and then pixels with the same value are sorted according to their closeness with the color `teal` in descending order. It will be called `value-teal[.0.png`.  

The `-w` and `-h` arguments will divide the image up into regions and sort the pixels within those regions. Try setting one to 1 pixel and the other to a larger number for a cool effect.
