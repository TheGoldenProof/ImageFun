using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Sorter_4 {
    internal class PixelSorter4 {
        enum SortOrientation {
            Horizontal, Vertical
        }
        enum SortKey {
            Hue, SaturationHSL, SaturationHSV, Lightness, Value, R, G, B, Color
        }
        enum SortDirection {
            Descending, Ascending
        }
        enum DistanceMode {
            Euclidean, Angular
        }
        class SortInfo {
            public SortKey Key { get; set; }
            public SortDirection Direction { get; set; }
            public Color Color { get; set; }
            public int ColorIndex { get; set; }
            public DistanceMode DMode { get; set; }
            public string? Name { get; set; }
            public SortInfo? Next { get; set; }

            /*public SortInfo() {
                Key = SortKey.Hue;
                DMode = DistanceMode.Euclidean;
                Color = Color.Black;
                Next = null;
            }*/
        }

        struct Pixel {
            public (int x, int y) Coords { get; set; }
            public float Hue { get; set; }
            public float SaturationHSL { get; set; }
            public float SaturationHSV { get; set; }
            public float Lightness { get; set; }
            public float Value { get; set; }
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
            public List<float> Colors { get; }

            public Pixel() {
                Colors = new List<float>();
            }
        }

        #region Comparers
        class HueComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.Hue.CompareTo(y.Hue);
            }
        }
        class SaturationHSLComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.SaturationHSL.CompareTo(y.SaturationHSL);
            }
        }
        class SaturationHSVComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.SaturationHSV.CompareTo(y.SaturationHSV);
            }
        }
        class LightnessComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.Lightness.CompareTo(y.Lightness);
            }
        }
        class ValueComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.Value.CompareTo(y.Value);
            }
        }
        class RComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.R.CompareTo(y.R);
            }
        }
        class GComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.G.CompareTo(y.G);
            }
        }
        class BComparer : Comparer<Pixel> {
            public override int Compare(Pixel x, Pixel y) {
                return x.B.CompareTo(y.B);
            }
        }
        class ColorComparer : Comparer<Pixel> {
            int index = 0;
            public ColorComparer(int index) => this.index = index;

            public override int Compare(Pixel x, Pixel y) {
                return x.Colors[index].CompareTo(y.Colors[index]);
            }
        }
        #endregion

        #region Utils
        static Vector4 ColorToVec(Color color) {
            return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }
        static Color VecToColor(Vector4 vec) {
            return Color.FromArgb((int)(vec.W*255), (int)(vec.X * 255), (int)(vec.Y * 255), (int)(vec.Z * 255));
        }

        static float GetSaturationHSL(Color c) {
            float Xmax = Math.Max(c.R, Math.Max(c.G, c.B)) / 255.0f;
            float Xmin = Math.Min(c.R, Math.Min(c.G, c.B)) / 255.0f;
            float C = Xmax - Xmin;
            float L = (Xmax + Xmin) / 2;
            return L == 0 || L == 1 ? 0 : C / (1 - Math.Abs(2 * Xmax - C - 1));
        }

        static float GetSaturationHSV(Color c) {
            float Xmax = Math.Max(c.R, Math.Max(c.G, c.B)) / 255.0f;
            float Xmin = Math.Min(c.R, Math.Min(c.G, c.B)) / 255.0f;
            float C = Xmax - Xmin;
            return Xmax == 0 ? 0 : C / Xmax;
        }

        static float GetLightness(Color c) {
            float Xmax = Math.Max(c.R, Math.Max(c.G, c.B)) / 255.0f;
            float Xmin = Math.Min(c.R, Math.Min(c.G, c.B)) / 255.0f;
            return (Xmax + Xmin) / 2;
        }

        static float GetValue(Color c) {
            return Math.Max(c.R, Math.Max(c.G, c.B)) / 255.0f;
        }

        // This isn't actually the angular distance, its the projective size of one onto the other.
        static float GetAngularDistance(Vector4 color1, Vector4 color2) {
            Vector3 cpxna = new Vector3(color1.X, color1.Y, color1.Z);
            Vector3 other = new Vector3(color2.X, color2.Y, color2.Z);
            float dot = Vector3.Dot(other, cpxna);
            float len = Math.Max(other.LengthSquared(), cpxna.LengthSquared());
            return dot / len;
        }

        static float GetEuclideanDistance(Vector4 color1, Vector4 color2) {
            Vector3 cpxna = new Vector3(color1.X, color1.Y, color1.Z);
            Vector3 other = new Vector3(color2.X, color2.Y, color2.Z);
            return Vector3.DistanceSquared(other, cpxna);
        }
        #endregion

        static List<SortInfo> ParseSortInfo(IEnumerable<string> strings) {
            List<SortInfo> infos = new List<SortInfo>();

            foreach (string strImm in strings) {
                string str = strImm;
                bool addToPrev = false;

                while(true) {
                    SortInfo info = new SortInfo();
                    int cursor = 0; 
                    char c = str[cursor];

                    while(c != ']' && c != '[' && c != '.' && c != '-' && cursor < str.Length) {
                        cursor++;
                        if(cursor == str.Length) break;
                        c = str[cursor];
                    }

                    string mstr = str.Substring(0, cursor).ToLower();
                    if(mstr.Length == 0) break;
                    switch (mstr) {
                    case "hue": info.Key = SortKey.Hue; break;
                    case "saturationhsl": info.Key = SortKey.SaturationHSL; break;
                    case "saturationhsv": info.Key = SortKey.SaturationHSV; break;
                    case "lightness": info.Key = SortKey.Lightness; break;
                    case "value": info.Key = SortKey.Value; break;
                    case "r": info.Key = SortKey.R; break;
                    case "g": info.Key = SortKey.G; break;
                    case "b": info.Key = SortKey.B; break;
                    default:
                        info.Key = SortKey.Color;
                        Color bclr = Color.Black;
                        if(mstr.StartsWith("#")) {
                            bclr = Color.FromArgb(Convert.ToInt32(mstr[1..], 16));
                        } else {
                            bclr = Color.FromName(mstr);
                        }
                        info.Color = bclr;
                        break;
                    }

                    if (c == ']') {
                        info.Direction = SortDirection.Descending;
                        if (++cursor >= str.Length) goto routeInfo;
                        c = str[cursor];
                    } else if (c == '[') {
                        info.Direction = SortDirection.Ascending;
                        if(++cursor >= str.Length) goto routeInfo;
                        c = str[cursor];
                    }

                    if (c == '.') {
                        if(++cursor >= str.Length) goto routeInfo;
                        c = str[cursor];
                        if (c == '1') {
                            info.DMode = DistanceMode.Angular;
                        } else {
                            info.DMode = DistanceMode.Euclidean;
                        }
                    }

                    while (c != '-' && cursor < str.Length) {
                        cursor++;
                        if(cursor == str.Length) goto routeInfo;
                        c = str[cursor];
                    }

                    routeInfo:
                    if (addToPrev) {
                        SortInfo prev = infos.Last();
                        while (prev.Next != null) { prev = prev.Next; }
                        prev.Next = info;
                    } else {
                        info.Name = strImm;
                        infos.Add(info);
                    }

                    if (c ==  '-') {
                        addToPrev = true; 
                        if(++cursor >= str.Length) break;
                        str = str.Substring(cursor);
                    } else {
                        break;
                    }
                }
            }

            return infos;
        }

        public static void Run(Program.Options opts) {
            Bitmap imgIn = new Bitmap(opts.Input.FullName);
            if(!opts.Output.Exists) Directory.CreateDirectory(opts.Output.FullName);

            (int w, int h) calcSize = (0, 0);
            (int w, int h) extraSize = (0, 0);
            if (opts.SizeMode == 1) { //divisions
                calcSize.w = imgIn.Width / opts.ChunkW;
                calcSize.h = imgIn.Height / opts.ChunkH;
                extraSize.w = imgIn.Width % opts.ChunkW;
                extraSize.h = imgIn.Height % opts.ChunkH;
            } else /*if (opts.SizeMode == 0)*/ { // pixels
                calcSize.w = opts.ChunkW;
                calcSize.h = opts.ChunkH;
                extraSize.w = imgIn.Width % opts.ChunkW;
                extraSize.h = imgIn.Height % opts.ChunkH;
            }

            // figure out what stats to extract
            List<SortInfo> sortInfos = ParseSortInfo(opts.SortStats);
            List<SortKey> sortKeys = new List<SortKey>();
            List<(Vector4 v, DistanceMode m)> sortColors = new List<(Vector4, DistanceMode)>();
            foreach (SortInfo sortInfoImm in sortInfos) {
                SortInfo? sortInfo = sortInfoImm;
                while (sortInfo != null) {
                    if(sortInfo.Key != SortKey.Color && !sortKeys.Contains(sortInfo.Key)) {
                        sortKeys.Add(sortInfo.Key);
                    } else if(sortInfo.Key == SortKey.Color) {
                        (Vector4, DistanceMode) toAdd = (ColorToVec(sortInfo.Color), sortInfo.DMode);
                        int colorIndex = sortColors.FindIndex(e => e == toAdd);
                        if(colorIndex == -1) {
                            sortInfo.ColorIndex = sortColors.Count;
                            sortColors.Add(toAdd);
                        } else sortInfo.ColorIndex = colorIndex;
                    }
                    sortInfo = sortInfo.Next;
                }
            }

            Bitmap[] images = new Bitmap[sortInfos.Count];
            for(int i = 0; i < sortInfos.Count; i++)
                images[i] = new Bitmap(imgIn.Width, imgIn.Height);

            // extract the stats
            Pixel[,] pixels = new Pixel[imgIn.Width, imgIn.Height];
            for (int x = 0; x < imgIn.Width; x++) {
                for (int y = 0; y < imgIn.Height; y++) {
                    Color c = imgIn.GetPixel(x, y);
                    ref Pixel pix = ref pixels[x, y];
                    pix = new Pixel();
                    pix.Coords = (x, y);

                    foreach(SortKey key in sortKeys) {
                        switch(key) {
                        case SortKey.Hue: pix.Hue = c.GetHue(); break;
                        case SortKey.SaturationHSL: pix.SaturationHSL = GetSaturationHSL(c); break;
                        case SortKey.SaturationHSV: pix.SaturationHSV = GetSaturationHSV(c); break;
                        case SortKey.Lightness: pix.Lightness = GetLightness(c); break;
                        case SortKey.Value: pix.Value = GetValue(c); break;
                        case SortKey.R: pix.R = c.R; break;
                        case SortKey.G: pix.G = c.G; break;
                        case SortKey.B: pix.B = c.B; break;
                        }
                    }

                    if (sortColors.Count > 0) {
                        Vector4 vecc = ColorToVec(c);
                        foreach ((Vector4 v, DistanceMode m) sc in sortColors) {
                            pix.Colors.Add(sc.m switch {
                                DistanceMode.Angular => GetAngularDistance(sc.v, vecc),
                                _ => -GetEuclideanDistance(sc.v, vecc)
                            });
                        }
                    }
                }
            }

            void SubSort(List<Pixel> toSort, SortInfo info, int index, int count) {
                Comparer<Pixel> comparer = info.Key switch {
                    SortKey.SaturationHSL => new SaturationHSLComparer(),
                    SortKey.SaturationHSV => new SaturationHSVComparer(),
                    SortKey.Lightness => new LightnessComparer(),
                    SortKey.Value => new ValueComparer(),
                    SortKey.R => new RComparer(),
                    SortKey.G => new GComparer(),
                    SortKey.B => new BComparer(),
                    SortKey.Color => new ColorComparer(info.ColorIndex),
                    _ => new HueComparer()
                };

                toSort.Sort(index, count, comparer);
                if(info.Direction == SortDirection.Descending) 
                    toSort.Reverse(index, count);

                if (info.Next != null) {
                    int startI = index;
                    int scount = 0;
                    while (startI + scount < toSort.Count) {
                        Pixel pix = toSort[startI];
                        while(startI + scount < toSort.Count && comparer.Compare(pix, toSort[startI + scount]) == 0) scount++;
                        if(scount > 1) {
                            SubSort(toSort, info.Next, startI, scount);
                        }
                        startI += scount;
                        scount = 0;
                    }
                }
            }

            void SortBlock(int x, int y, int w, int h) {
                List<Pixel> toSort = new List<Pixel>(w * h);
                for (int dx = 0; dx < w; dx++)
                    for (int dy = 0; dy < h; dy++)
                        toSort.Add(pixels[x + dx, y + dy]);

                for (int i = 0; i < sortInfos.Count; i++) {
                    SortInfo info = sortInfos[i];
                    SubSort(toSort, info, 0, toSort.Count);

                    for(int dx = 0; dx < w; dx++) {
                        for(int dy = 0; dy < h; dy++) {
                            int index;
                            if (opts.Orientation == 1) { // vertical
                                index = dy + dx * h;
                            } else { // horizontal
                                index = dx + dy * w;
                            }
                            Pixel pix = toSort[index];
                            images[i].SetPixel(x + dx, y + dy, imgIn.GetPixel(pix.Coords.x, pix.Coords.y));
                        }
                    }
                }
            }

            // sort the whole kernels
            for (int x = 0; x < imgIn.Width - extraSize.w; x += calcSize.w) {
                for (int y = 0; y < imgIn.Height - extraSize.h; y += calcSize.h) {
                    SortBlock(x, y, calcSize.w, calcSize.h);
                }
            }

            // sort the kernels cut on the bottom
            for (int x = 0; x < imgIn.Width - extraSize.w; x += calcSize.w) {
                SortBlock(x, imgIn.Height - extraSize.h, calcSize.w, extraSize.h);
            }

            // sort the kernels cut on top
            for (int y = 0; y < imgIn.Height - extraSize.h; y += calcSize.h) {
                SortBlock(imgIn.Width - extraSize.w, y, extraSize.w, calcSize.h);
            }

            // sort the kernel cut on both
            SortBlock(imgIn.Width - extraSize.w, imgIn.Height - extraSize.h, extraSize.w, extraSize.h);

            // save the images
            for (int i = 0; i < images.Count(); i++) {
                images[i].Save(opts.Output.FullName + sortInfos[i].Name + ".png");
            }
        }
    }
}
