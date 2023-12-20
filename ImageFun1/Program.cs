using CommandLine;

namespace ImageFun1 {
    internal static class Program {
        public class Options {
            private readonly FileInfo input;
            private readonly FileInfo output;
            private readonly IEnumerable<string> colors;
            private readonly float factor;
            private readonly string mode;
            private readonly bool alpha;
            private readonly int kernelWidth;
            private readonly int kernelHeight;

            public Options(FileInfo input, FileInfo output, IEnumerable<string> colors, float factor, string mode, bool alpha, int kernelWidth, int kernelHeight) {
                this.input = input;
                this.output = output;
                this.colors = colors;
                this.factor = factor;
                this.mode = mode;
                this.alpha = alpha;
                this.kernelWidth = kernelWidth;
                this.kernelHeight = kernelHeight;
            }

            [Option('i', "input", Required=true)]
            public FileInfo Input { get { return input; } }

            [Option('o', "output", Required=true)] 
            public FileInfo Output { get { return output; } }

            [Option('c', "colors", Default="white", HelpText ="A list of Microsoft color names (case-insensitive) or hex codes (starting with #)")]
            public IEnumerable<string> Colors { get {  return colors; } }

            [Option('f', "factor", Default=1.0f)]
            public float Factor { get { return factor; } }

            [Option('m', "mode", Default ="divide", HelpText ="Different calculation modes: divide, dotp")]
            public string Mode { get { return mode; } }

            [Option('a', "alpha", Default=false)]
            public bool Alpha { get { return alpha; } }

            [Option('w', "kernel-width", Default=1)]
            public int KernelWidth { get { return kernelWidth; } }
            [Option('h', "kernel-height", Default=16)]
            public int KernelHeight { get {  return kernelHeight; } }
        }

        static int Main(string[] args) {
            return CommandLine.Parser.Default.ParseArguments<Options>(args).MapResult((opts) => RunOptions(opts), (errors) => HandleParseError(errors));
        }

        static int RunOptions(Options opts) {
            ImageFun1.Run(opts);
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