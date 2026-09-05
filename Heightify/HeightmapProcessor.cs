using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Heightify
{
    public class HeightmapProcessor
    {
        private readonly Func<double, double> _function;

        public HeightmapProcessor(Func<double, double> function)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
        }

        public unsafe Bitmap ConvertToHeightMap(Bitmap inputImage)
        {
            if (inputImage == null) throw new ArgumentNullException(nameof(inputImage));

            int width = inputImage.Width;
            int height = inputImage.Height;

            Bitmap heightMap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData srcData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = heightMap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* srcScan0 = (byte*)srcData.Scan0.ToPointer();
                byte* dstScan0 = (byte*)dstData.Scan0.ToPointer();

                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcScan0 + (y * srcStride);
                    byte* dstRow = dstScan0 + (y * dstStride);

                    for (int x = 0; x < width; x++)
                    {
                        int offset = x * 3;
                        byte b = srcRow[offset];
                        byte g = srcRow[offset + 1];
                        byte r = srcRow[offset + 2];

                        double gray = (r * 0.3) + (g * 0.59) + (b * 0.11);
                        double normalizedInput = gray / 255.0;

                        double heightValue = _function(normalizedInput);
                        heightValue = Math.Max(0.0, Math.Min(1.0, heightValue));
                        byte finalIntensity = (byte)(heightValue * 255.0);

                        dstRow[offset] = finalIntensity;
                        dstRow[offset + 1] = finalIntensity;
                        dstRow[offset + 2] = finalIntensity;
                    }
                }
            }
            finally
            {
                inputImage.UnlockBits(srcData);
                heightMap.UnlockBits(dstData);
            }

            return heightMap;
        }

        public unsafe Bitmap ConvertNormalToHeightMap(Bitmap sourceImage)
        {
            if (sourceImage == null)
            {
                throw new ArgumentNullException(nameof(sourceImage));
            }

            int width = sourceImage.Width;
            int height = sourceImage.Height;

            Bitmap heightMap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData srcData = sourceImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = heightMap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* srcScan0 = (byte*)srcData.Scan0.ToPointer();
                byte* dstScan0 = (byte*)dstData.Scan0.ToPointer();

                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcScan0 + (y * srcStride);
                    byte* dstRow = dstScan0 + (y * dstStride);

                    for (int x = 0; x < width; x++)
                    {
                        int offset = x * 3;

                        // 24bppRgb buffer: [0]=Blue, [1]=Green, [2]=Red
                        float b = srcRow[offset];
                        float g = srcRow[offset + 1];
                        float r = srcRow[offset + 2];

                        // unpack color components from [0, 255] to vector [-1.0, 1.0]
                        float nx = (r / 255.0f) * 2.0f - 1.0f;
                        float ny = (g / 255.0f) * 2.0f - 1.0f;
                        float nz = (b / 255.0f) * 2.0f - 1.0f;

                        // normalize normal vector
                        float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                        if (length > 1e-6f)
                        {
                            nx /= length;
                        }

                        // map normalized curvature/height value back to grayscale range [0, 255]
                        int intensity = (int)((nx + 1.0f) * 127.5f);
                        byte finalValue = (byte)Math.Max(0, Math.Min(255, intensity));

                        dstRow[offset] = finalValue;
                        dstRow[offset + 1] = finalValue;
                        dstRow[offset + 2] = finalValue;
                    }
                }
            }
            finally
            {
                sourceImage.UnlockBits(srcData);
                heightMap.UnlockBits(dstData);
            }

            return heightMap;
        }

        public Bitmap MixChannelsToHeightmap(string imagePath, double redWeight, double greenWeight, double blueWeight)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));

            using (Mat source = Cv2.ImRead(imagePath, ImreadModes.Color))
            {
                if (source.Empty() || source.Channels() != 3)
                {
                    throw forensicsInvalidOperationException("Image must be a valid 3-channel color texture.");
                }

                // split image into BGR planes
                Mat[] channels = Cv2.Split(source);

                try
                {
                    // OpenCV order: [0] = Blue, [1] = Green, [2] = Red
                    double[] weights = { blueWeight, greenWeight, redWeight };

                    using (Mat weightedSum = new Mat(source.Size(), MatType.CV_32FC1))
                    using (Mat tempConverted = new Mat())
                    using (Mat normalizedResult = new Mat())
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            channels[i].ConvertTo(tempConverted, MatType.CV_32FC1);
                            Cv2.ScaleAdd(tempConverted, weights[i], i == 0 ? new Mat(source.Size(), MatType.CV_32FC1, Scalar.All(0)) : weightedSum, weightedSum);
                        }

                        // normalize values across 0-255 range
                        Cv2.Normalize(weightedSum, normalizedResult, 0, 255, NormTypes.MinMax);
                        normalizedResult.ConvertTo(normalizedResult, MatType.CV_8UC1);

                        // in-memory conversion to Bitmap 
                        return BitmapConverter.ToBitmap(normalizedResult);
                    }
                }
                finally
                {
                    // disposal
                    for (int i = 0; i < channels.Length; i++)
                    {
                        channels[i]?.Dispose();
                    }
                }
            }
        }

        private static InvalidOperationException forensicsInvalidOperationException(string message) => new InvalidOperationException(message);

    }
}