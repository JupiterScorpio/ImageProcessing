﻿using System;
using System.Threading.Tasks;
using System.Linq;

namespace UAM.PTO
{
    public static class Filter
    {
        #region public functons

        public static PNM ApplyPointProcessing(this PNM oldImage, Func<byte, byte, byte, Pixel> filter)
        {
            PNM newImage = new PNM(oldImage.Width, oldImage.Height);
            byte r, g, b;
            int size = oldImage.Width * oldImage.Height;
            for (int i = 0; i < size; i++)
            {
                oldImage.GetPixel(i, out r, out g, out b);
                Pixel pixel = filter(r, g, b);
                newImage.SetPixel(i, pixel.Red, pixel.Green, pixel.Blue);
            };
            return newImage;
        }

        public static PNM ApplyConvolutionMatrix(this PNM image, float[] matrix, float weight, float shift)
        {
            int length = (int)Math.Sqrt(matrix.Length);
            if (Math.Pow(length, 2) != matrix.Length || (length / 2) * 2 == length)
                throw new ArgumentException("matrix");
            PNM newImage = PNM.Copy(image);
            int padding = length / 2;
            Pad(newImage, padding);
            newImage = ApplyConvolutionMatrixCore(newImage, matrix, length, weight, shift);
            Trim(newImage, padding);
            return newImage;
        }

        public static PNM ApplyPixelFunction(this PNM image, int matrixLength, Func<PNM, int, Pixel> func)
        {
            PNM newImage = PNM.Copy(image);
            int padding = matrixLength / 2;
            Pad(newImage, padding);
            newImage = ApplyConvolutionFunctionCore(newImage, matrixLength, func);
            Trim(newImage, padding);
            return newImage;
        }

        public static PNM ApplyHeightMapFunction(this PNM image, int matrixLength, Func<int, int, float[], int, Pixel> func)
        {
            PNM newImage = PNM.Copy(image);
            int padding = matrixLength / 2;
            Pad(newImage, padding);
            int newImageSize = newImage.Width * newImage.Height;
            float[] heightmap = new float[newImageSize];
            byte r,g,b;
            for (int i = 0; i < newImageSize; i++)
            {
                newImage.GetPixel(i, out r, out g, out b);
                heightmap[i] = PNM.RGBToLuminosity(r, g, b) / 255f;
            }
            newImage = ApplyHeightMapFunctionCore(newImage.Width, newImage.Height, heightmap, matrixLength, func);
            Trim(newImage, padding);
            return newImage;
        }

        //this should be rewritten to ApplyConvolutionFunction
        public static PNM ApplyGradientEdgesDetection(this PNM image)
        {
            PNM workImage = PNM.Copy(image);
            Pad(workImage, 1);
            Tuple<float[], float[], float[]> xraster = ApplyConvolutionUnbound(workImage, new float[] {-1, 0, 1,
                                                                                                       -1, 0, 1,
                                                                                                       -1, 0, 1}, 3);
            Tuple<float[], float[], float[]> yraster = ApplyConvolutionUnbound(workImage, new float[] { 1,  1,  1,
                                                                                                        0,  0,  0,
                                                                                                       -1, -1, -1}, 3);
            PNM newImage = new PNM(image.Width, image.Height);
            int size = image.Width * image.Height;
            Parallel.For(0, size, i =>
            {
                byte r = Coerce(Math.Sqrt(Math.Pow(xraster.Item1[i], 2) + Math.Pow(yraster.Item1[i], 2)));
                byte g = Coerce(Math.Sqrt(Math.Pow(xraster.Item2[i], 2) + Math.Pow(yraster.Item2[i], 2)));
                byte b = Coerce(Math.Sqrt(Math.Pow(xraster.Item3[i], 2) + Math.Pow(yraster.Item3[i], 2)));
                newImage.SetPixel(i, r, g, b);
            });
            return newImage;
        }

        #endregion

        internal static Tuple<float[], float[], float[]> ApplyConvolutionUnbound(PNM image, float[] matrix, int matrixLength)
        {
            int padding = matrixLength / 2;
            int oldHeight = image.Height - (padding * 2);
            int oldWidth = image.Width - (padding * 2);
            Tuple<float[], float[], float[]> rasters = Tuple.Create(new float[oldHeight * oldWidth],
                                                                    new float[oldHeight * oldWidth],
                                                                    new float[oldHeight * oldWidth]);
            int maxHeight = image.Height - padding;
            int maxWidth = image.Width - padding;
            Parallel.For(padding, maxHeight, i =>
            {
                int index = (i - padding) * oldWidth;
                for (int j = padding; j < maxWidth; j++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;
                    // current index position
                    int position = i * image.Width + j;
                    for (int m = 0; m < matrixLength; m++)
                    {
                        for (int n = 0; n < matrixLength; n++)
                        {
                            byte r, g, b;
                            image.GetPixel(position - ((padding - m) * image.Width) - (padding - n), out r, out g, out b);
                            float coeff = matrix[(m * matrixLength) + n];
                            sumR += r * coeff;
                            sumG += g * coeff;
                            sumB += b * coeff;
                        }
                    }
                    rasters.Item1[index] = sumR;
                    rasters.Item2[index] = sumG;
                    rasters.Item3[index] = sumB;
                    index++;
                }
            });
            return rasters;
        }

        private static PNM ApplyHeightMapFunctionCore(int imgWidth, int imgHeight, float[] heightmap, int matrixLength, Func<int, int, float[], int, Pixel> func)
        {
            PNM newImage = new PNM(imgWidth, imgHeight);
            int padding = matrixLength / 2;
            int maxHeight = imgHeight - padding;
            int maxWidth = imgWidth - padding;
            int width = imgWidth;
            Parallel.For(padding, maxHeight, i =>
            {
                for (int j = padding; j < maxWidth; j++)
                {
                    // current index position
                    int position = i * width + j;
                    newImage.SetPixel(position, func(imgWidth, imgHeight, heightmap, position));
                }
            });
            return newImage;
        }

        // poor man's pixel shader
        private static PNM ApplyConvolutionFunctionCore(PNM image, int matrixLength, Func<PNM, int, Pixel> func)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            int padding = matrixLength / 2;
            int maxHeight = newImage.Height - padding;
            int maxWidth = newImage.Width - padding;
            int width = newImage.Width;
            Parallel.For(padding, maxHeight, i =>
            {
                for (int j = padding; j < maxWidth; j++)
                {
                    // current index position
                    int position = i * width + j;
                    newImage.SetPixel(position, func(image, position));
                }
            });
            return newImage;
        }

        internal static T[] PadWithZeros<T>(T[] array, int width, int height, int widthPadding, int heightPadding) where T : struct
        {
            int newHeight = height + (2 * heightPadding);
            int newWidth = width + (2 * widthPadding);
            T[] newRaster = new T[newHeight * newWidth];
            // skip black rows at the top
            int start = heightPadding * newWidth;
            int oldSize = width * height;
            // copy rows
            for (int i_new = start, i_old = 0; i_old < oldSize; i_new += newWidth, i_old += width)
            {
                Array.Copy(array, i_old, newRaster, i_new + widthPadding, width);
            }
            return newRaster;
        }

        internal static void Pad(PNM image, int padding)
        {
            image.raster = PadWithZeros(image.raster, image.Width* 3, image.Height, padding * 3, padding);
            image.Width += 2 * padding;
            image.Height += 2 * padding;
            PadCorners(image, padding);
            PadBorders(image, padding);
        }

        private static void PadBorders(PNM image, int padding)
        {
            // copy top and bottom
            for(int i = 0;i < padding; i++)
            {
                Buffer.BlockCopy(image.raster, 3 * (padding * image.Width + padding), image.raster, 3 * (i * image.Width + padding), (image.Width - padding * 2) * 3);
                Buffer.BlockCopy(image.raster, 3 * ((image.Height - padding - 1) * image.Width + padding), image.raster, 3 * ((i + image.Height - padding) * image.Width + padding), (image.Width - padding * 2) * 3);
            }
            // pad right and left
            for (int i = 0; i < image.Height - (2 * padding); i++)
            {
                byte r, g, b;
                
                image.GetPixel(padding, padding + i, out r, out g, out b);
                for (int j = 0; j < padding; j++)
                {
                    image.SetPixel(j, padding + i, r, g, b);
                } 
                
                image.GetPixel(image.Width - padding - 1, padding + i, out r, out g, out b);
                for (int j = 0; j < padding; j++)
                {
                    image.SetPixel(image.Width - j - 1, padding + i, r, g, b);
                }
            }
        }

        private static void PadCorners(PNM image, int padding)
        {
            byte r, g, b;
            // pad top left
            image.GetPixel(padding, padding, out r, out g, out b);
            for (int i = 0; i < padding; i++)
            {
                for (int j = 0; j < padding; j++)
                {
                    image.SetPixel(i, j, r, g, b);
                }
            }
            // pad top right
            image.GetPixel(image.Width - padding - 1, padding, out r, out g, out b);
            for (int i = image.Width - padding; i < image.Width; i++)
            {
                for (int j = 0; j < padding; j++)
                {
                    image.SetPixel(i, j, r, g, b);
                }
            }
            // pad bottom left
            image.GetPixel(padding, image.Height - padding - 1, out r, out g, out b);
            for (int i = 0; i < padding; i++)
            {
                for (int j = image.Height - padding; j < image.Height; j++)
                {
                    image.SetPixel(i, j, r, g, b);
                }
            }
            // pad bottom right
            image.GetPixel(image.Width - padding - 1, image.Height - padding - 1, out r, out g, out b);
            for (int i = image.Width - padding; i < image.Width; i++)
            {
                for (int j = image.Height - padding; j < image.Height; j++)
                {
                    image.SetPixel(i, j, r, g, b);
                }
            }
        }

        internal static T[] Trim<T>(T[] array, int width, int height, int widthPadding, int heightPadding) where T : struct
        {
            int newHeight = height - (2 * heightPadding);
            int newWidth = width - (2 * widthPadding);
            int newSize = newHeight * newWidth;
            int oldSize = height * width;
            T[] newRaster = new T[newSize];
            int start = heightPadding * width;
            for (int i_old = start, i_new = 0; i_new < newSize; i_old += width, i_new += newWidth)
            {
                Array.Copy(array, i_old + widthPadding, newRaster, i_new, newWidth);
            }
            return newRaster;
        }

        internal static void Trim(PNM image, int padding)
        {
            image.raster = Trim(image.raster, image.Width * 3, image.Height, padding * 3, padding);
            image.Width -= 2 * padding;
            image.Height -= 2 * padding;
        }

        internal static byte Coerce(float f)
        {
            if (f <= 0)
                return 0;
            else if (f >= 255)
                return 255;
            else
                return Convert.ToByte(f);
        }

        internal static byte Coerce(double f)
        {
            if (f <= 0)
                return 0;
            else if (f >= 255)
                return 255;
            else
                return Convert.ToByte(f);
        }

        private static PNM ApplyConvolutionMatrixCore(PNM image, float[] matrix, int matrixLength, float weight, float shift)
        {
            PNM newImage = new PNM(image.Width, image.Height);
            int padding = matrixLength / 2;
            int maxHeight = newImage.Height - padding;
            int maxWidth = newImage.Width - padding;
            int width = newImage.Width;
            Parallel.For(padding, maxHeight, i =>
            {
                for (int j = padding; j < maxWidth; j++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;
                    // current index position
                    int position = i * width + j;
                    for (int m = 0; m < matrixLength; m++)
                    {
                        for (int n = 0; n < matrixLength; n++)
                        {
                            byte r, g, b;
                            image.GetPixel(position - ((padding - m) * width) - (padding - n), out r, out g, out b);
                            float coeff = matrix[(m * matrixLength) + n];
                            sumR += (r * coeff * weight);
                            sumG += (g * coeff * weight);
                            sumB += (b * coeff * weight);
                        }
                    }
                    newImage.SetPixel(position, Coerce(sumR + shift), Coerce(sumG + shift), Coerce(sumB + shift));
                }
            });
            return newImage;
        }

        // calculate relative position
        internal static int RelativeIndex(int width, int position, int x, int y)
        {
            return position + (y * width) + x;
        }

        internal static double Module(double x, double y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }
    }
}