using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Foundation
{
    public class ImageUtility
    {

        public static Icon IconFromImage(Image img)
        {
            var ms = new System.IO.MemoryStream();
            var bw = new System.IO.BinaryWriter(ms);
            // Header
            bw.Write((short)0);   // 0 : reserved
            bw.Write((short)1);   // 2 : 1=ico, 2=cur
            bw.Write((short)1);   // 4 : number of images
                                  // Image directory
            var w = img.Width;
            if (w >= 256) w = 0;
            bw.Write((byte)w);    // 0 : width of image
            var h = img.Height;
            if (h >= 256) h = 0;
            bw.Write((byte)h);    // 1 : height of image
            bw.Write((byte)0);    // 2 : number of colors in palette
            bw.Write((byte)0);    // 3 : reserved
            bw.Write((short)0);   // 4 : number of color planes
            bw.Write((short)0);   // 6 : bits per pixel
            var sizeHere = ms.Position;
            bw.Write((int)0);     // 8 : image size
            var start = (int)ms.Position + 4;
            bw.Write(start);      // 12: offset of image data
                                  // Image data
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var imageSize = (int)ms.Position - start;
            ms.Seek(sizeHere, System.IO.SeekOrigin.Begin);
            bw.Write(imageSize);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            // And load it
            return new Icon(ms);
        }


        public static byte[] IconToBytes(Icon icon)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                return ms.ToArray();
            }
        }


        public static Icon BytesToIcon(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return new Icon(ms);
            }
        }



        public static byte[] ConvertImageToByteArray(Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }
    }


    /// <summary>
    /// Adapted from this gist: https://gist.github.com/darkfall/1656050
    /// Provides helper methods for imaging
    /// </summary>
    public static class ImagingHelper
    {


        public static System.Drawing.Bitmap ReplaceTransparency(string file, System.Drawing.Color background)
        {
            return ReplaceTransparency(System.Drawing.Image.FromFile(file), background);
        }

        public static System.Drawing.Bitmap ReplaceTransparency(System.Drawing.Image image, System.Drawing.Color background)
        {
            return ReplaceTransparency((System.Drawing.Bitmap)image, background);
        }


        public static System.Drawing.Bitmap ReplaceTransparency(Bitmap bmp1, Color target)
        {
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2))
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }

        // This clips off the right and bottom edges...
        //public static System.Drawing.Bitmap ReplaceTransparency(System.Drawing.Bitmap bitmap, System.Drawing.Color background)
        //{
        //    /* Important: you have to set the PixelFormat to remove the alpha channel.
        //     * Otherwise you'll still have a transparent image - just without transparent areas */
        //    // var result = new System.Drawing.Bitmap(bitmap.Size.Width, bitmap.Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    var result = new System.Drawing.Bitmap(bitmap.Size.Width, bitmap.Size.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        //    var g = System.Drawing.Graphics.FromImage(result);

        //    g.Clear(background);
        //    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
        //    g.DrawImage(bitmap, 0, 0);

        //    return result;
        //}


        /// <summary>
        /// Converts a PNG image to a icon (ico) with all the sizes windows likes
        /// </summary>
        /// <param name="inputBitmap">The input bitmap</param>
        /// <param name="output">The output stream</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public static bool ConvertToIcon(Bitmap inputBitmap, Stream output)
        {
            if (inputBitmap == null)
                return false;

            int[] sizes = new int[] { 256, 48, 32, 16 };

            // Generate bitmaps for all the sizes and toss them in streams
            List<MemoryStream> imageStreams = new List<MemoryStream>();
            foreach (int size in sizes)
            {
                Bitmap newBitmap = ResizeImage(inputBitmap, size, size);
                if (newBitmap == null)
                    return false;
                MemoryStream memoryStream = new MemoryStream();
                newBitmap.Save(memoryStream, ImageFormat.Png);
                imageStreams.Add(memoryStream);
            }

            BinaryWriter iconWriter = new BinaryWriter(output);
            if (output == null || iconWriter == null)
                return false;

            int offset = 0;

            // 0-1 reserved, 0
            iconWriter.Write((byte)0);
            iconWriter.Write((byte)0);

            // 2-3 image type, 1 = icon, 2 = cursor
            iconWriter.Write((short)1);

            // 4-5 number of images
            iconWriter.Write((short)sizes.Length);

            offset += 6 + (16 * sizes.Length);

            for (int i = 0; i < sizes.Length; i++)
            {
                // image entry 1
                // 0 image width
                iconWriter.Write((byte)sizes[i]);
                // 1 image height
                iconWriter.Write((byte)sizes[i]);

                // 2 number of colors
                iconWriter.Write((byte)0);

                // 3 reserved
                iconWriter.Write((byte)0);

                // 4-5 color planes
                iconWriter.Write((short)0);

                // 6-7 bits per pixel
                iconWriter.Write((short)32);

                // 8-11 size of image data
                iconWriter.Write((int)imageStreams[i].Length);

                // 12-15 offset of image data
                iconWriter.Write((int)offset);

                offset += (int)imageStreams[i].Length;
            }

            for (int i = 0; i < sizes.Length; i++)
            {
                // write image data
                // png data must contain the whole png data file
                iconWriter.Write(imageStreams[i].ToArray());
                imageStreams[i].Close();
            }

            iconWriter.Flush();

            return true;
        }

        /// <summary>
        /// Converts a PNG image to a icon (ico)
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="output">The output stream</param
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public static bool ConvertToIcon(Stream input, Stream output)
        {
            Bitmap inputBitmap = (Bitmap)Bitmap.FromStream(input);
            return ConvertToIcon(inputBitmap, output);
        }

        /// <summary>
        /// Converts a PNG image to a icon (ico)
        /// </summary>
        /// <param name="inputPath">The input path</param>
        /// <param name="outputPath">The output path</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public static bool ConvertToIcon(string inputPath, string outputPath)
        {
            using (FileStream inputStream = new FileStream(inputPath, FileMode.Open))
            using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            {
                return ConvertToIcon(inputStream, outputStream);
            }
        }



        /// <summary>
        /// Converts an image to a icon (ico)
        /// </summary>
        /// <param name="inputImage">The input image</param>
        /// <param name="outputPath">The output path</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public static bool ConvertToIcon(Image inputImage, string outputPath)
        {
            using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            {
                return ConvertToIcon(new Bitmap(inputImage), outputStream);
            }
        }


        /// <summary>
        /// Resize the image to the specified width and height.
        /// Found on stackoverflow: https://stackoverflow.com/questions/1922040/resize-an-image-c-sharp
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        //
        // Resize an image so that it has the width provided.  It will maintain the aspect ratio and calculate the required height.
        //
        public static Bitmap ResizeImage(Image image, int width)
        {
            //
            // Calculate the height using the provied width and the image's current width/height
            //
            double aspectRatio = ((double)image.Width / image.Height);

            // calculate the new height
            int height = (int)(width / aspectRatio);

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
