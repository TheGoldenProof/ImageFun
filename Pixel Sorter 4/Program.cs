using CommandLine;

namespace Pixel_Sorter_4 {
    internal class Program {
        public class Options {
            private readonly FileInfo input;
            private readonly FileInfo output;
            private readonly int sideMode;
            private readonly int chunkW;
            private readonly int chunkH;
            private readonly IEnumerable<string> sortStats;
            private readonly int orientation;

            public Options(FileInfo input, FileInfo output, int sizeMode, int chunkW, int chunkH, IEnumerable<string> sortStats, int orientation) {
                this.input = input;
                this.output = output;
                this.sideMode = sizeMode;
                this.chunkW = chunkW;
                this.chunkH = chunkH;
                this.sortStats = sortStats;
                this.orientation = orientation;
            }

            [Option('i', "input", Required = true)]
            public FileInfo Input { get { return input; } }
            [Option('o', "output", Required = true, HelpText = "Must be a directory (ending with a slash)")]
            public FileInfo Output { get { return output; } }

            [Option('m', "size-mode", Default = 0, HelpText = "width and height are interpreted as... 0: pixels; 1: divisions")]
            public int SizeMode { get { return this.sideMode; } }

            [Option('w', "chunk-w", Default = 1, HelpText = "Kernel width in pixels or divisions (see size-mode)")]
            public int ChunkW { get { return chunkW; } }
            [Option('h', "chunk-h", Default = 16, HelpText = "Kernel width in pixels or divisions (see size-mode)")]
            public int ChunkH { get { return chunkH; } }

            [Option('s', "sort-stats", Default = "hue", HelpText = "A list of things to sort by. Enter 'hue', 'saturationhsl', 'saturationhsv', 'brightness', 'value', 'r', 'g', 'b', a system color, or a hex code starting with #. Secondary sorts can be used like so: 'r-#345cdf' will sort pixels with the same r-value by the closeness to #345cdf.")]
            public IEnumerable<string> SortStats { get { return sortStats; } }

            [Option('r', "orientation", Default = 0, HelpText = "The orientation of the sorted pixels... 0: horizontal; 1: vertical")]
            public int Orientation { get { return orientation; } }
        }
        static int Main(string[] args) {
            return CommandLine.Parser.Default.ParseArguments<Options>(args).MapResult((opts) => RunOptions(opts), (errors) => HandleParseError(errors));
        }

        static int RunOptions(Options opts) {
            PixelSorter4.Run(opts);
            return 0;
        }

        static int HandleParseError(IEnumerable<Error> errors) {
            var result = -2;
            Console.WriteLine("errors {0}", errors.Count());
            if(errors.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                result = -1;
            Console.WriteLine("Exit code {0}", result);
            return result;
        }
    }
}
