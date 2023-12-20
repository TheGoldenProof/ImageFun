using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageFun1 {
    internal class ImageFun1 {
        private static Vector4 ColorToVec(Color color) {
            return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        private static List<Vector4> ParseColors(IEnumerable<string> strs, out List<string> names) {
            List<Vector4> ret = new List<Vector4>();
            names = new List<string>();
            if (strs.Count() == 0) {
                ret.Add(Vector4.One);
                names.Add("White");
            }
            foreach (string sclr in strs) {
                Color bclr = Color.Transparent;
                if (sclr.StartsWith("#")) {
                    bclr = Color.FromArgb(Convert.ToInt32(sclr[1..], 16));
                } else {
                    bclr = Color.FromName(sclr);
                }
                if(bclr != Color.Transparent || bclr != Color.Black) {
                    ret.Add(ColorToVec(bclr));
                    names.Add(sclr);
                }
            }
            return ret;
        }

        public static void Run(Program.Options opts) {
            int kernelArea = opts.KernelWidth * opts.KernelHeight;
            List<string> colorNames;
            List<Vector4> colors = ParseColors(opts.Colors, out colorNames);
            int mode = opts.Mode switch {
                "dotp" => 1,
                _ => 0
            };
            Bitmap imgIn = new Bitmap(opts.Input.FullName);
            if(imgIn.Width < opts.KernelWidth) {
                Console.WriteLine("Error: Image width is smaller than kernel width.");
                return;
            } else if(imgIn.Height < opts.KernelHeight) {
                Console.WriteLine("Error: Image height is smaller than kernel height.");
                return;
            }

            Vector4[,] pixelsIn = new Vector4[imgIn.Width, imgIn.Height];
            for(int x = 0; x < imgIn.Width; x++)
                for(int y = 0; y < imgIn.Height; y++)
                    pixelsIn[x, y] = ColorToVec(imgIn.GetPixel(x, y));

            Bitmap[] images = new Bitmap[colors.Count];
            for (int i = 0; i < colors.Count; i++)
                    images[i] = new Bitmap(imgIn.Width, imgIn.Height);

            float[] counts = new float[colors.Count];
            Vector4 sum = Vector4.Zero;
            for(int x = 0; x < imgIn.Width - opts.KernelWidth; x += opts.KernelWidth) {
                for(int y = 0; y < imgIn.Height - opts.KernelHeight; y += opts.KernelHeight) {

                    Array.Clear(counts, 0, counts.Length);
                    sum = Vector4.Zero;

                    for(int kx = 0; kx < opts.KernelWidth; kx++) {
                        for(int ky = 0; ky < opts.KernelHeight; ky++) {
                            Vector4 cpx = pixelsIn[x + kx, y + ky];

                            if (mode == 0) {
                                sum += cpx * (opts.Alpha ? cpx.W : 1.0f);
                            } else if (mode == 1) {
                                for(int i = 0; i < colors.Count; i++) {
                                    Vector3 cpxna = new Vector3(cpx.X, cpx.Y, cpx.Z);
                                    Vector3 other = new Vector3(colors[i].X, colors[i].Y, colors[i].Z);
                                    float dot = Vector3.Dot(other, cpxna);
                                    float len = Math.Max(other.LengthSquared(), cpxna.LengthSquared());
                                    float acc = dot / len;
                                    counts[i] += float.Pow(acc, opts.Factor);
                                }
                            }

                        }
                    }

                    for (int i = 0; i < colors.Count; i++) {
                        Color c = Color.FromArgb(
                            255,
                            (byte)(255 * colors[i].X),
                            (byte)(255 * colors[i].Y),
                            (byte)(255 * colors[i].Z));

                        if (mode == 0) {
                            float max = Math.Max(colors[i].X, Math.Max(colors[i].Y, colors[i].Z)) / opts.KernelWidth;
                            Vector4 quo = sum / colors[i];
                            float count = Math.Min(quo.X, Math.Min(quo.Y, quo.Z));

                            for(int ky = 0; ky < count * max; ky++) {
                                for(int kx = 0; kx < opts.KernelWidth; kx++) {
                                    images[i].SetPixel(x + kx, y + opts.KernelHeight - ky, c);
                                }
                            }
                        } else if (mode == 1) {
                            for(int ky = 0; ky < counts[i] / opts.KernelWidth; ky++) {
                                for(int kx = 0; kx < opts.KernelWidth; kx++) {
                                    images[i].SetPixel(x + kx, y + opts.KernelHeight - ky, c);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < colors.Count; i++) {
                images[i].Save(opts.Output.FullName + colorNames[i] + ".png");
            }
        }
    }
}
