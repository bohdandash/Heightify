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
    }
}