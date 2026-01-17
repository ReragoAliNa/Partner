using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopPet.Core.Animation
{
    public static class AssetLoader
    {
        public static FrameAnimation LoadSingleImage(string filePath, double frameDuration = 1.0)
        {
             // Use LoadFromSpriteSheet but we need to know size first. 
             // Simpler: Just load it and add as 1 frame.
             var anim = new FrameAnimation 
            { 
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                FrameDuration = frameDuration 
            };

            try 
            {
                var uri = new Uri(filePath, UriKind.Absolute);
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = uri;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                
                // Color Keying: Ensure standard BGRA32 format for pixel manipulation
                FormatConvertedBitmap converted = new FormatConvertedBitmap();
                converted.BeginInit();
                converted.Source = image;
                converted.DestinationFormat = PixelFormats.Bgra32;
                converted.EndInit();

                var wb = new WriteableBitmap(converted);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);

                // Flood Fill Algorithm
                // 1. Identify Background Color from (0,0)
                byte bBg = pixels[0];
                byte gBg = pixels[1];
                byte rBg = pixels[2];
                
                // Only proceed if top-left is "White-ish"
                if (rBg > 200 && gBg > 200 && bBg > 200)
                {
                    bool[] visited = new bool[width * height];
                    Queue<int> queue = new Queue<int>();
                    
                    // Start at 0,0
                    queue.Enqueue(0);
                    visited[0] = true;

                    while (queue.Count > 0)
                    {
                        int index = queue.Dequeue();
                        int cx = index % width;
                        int cy = index / width;
                        int pixelOffset = (cy * stride) + (cx * 4);

                        // Set Transparent
                        pixels[pixelOffset + 3] = 0;

                        // Check Neighbors (Up, Down, Left, Right)
                        int[] dx = { 0, 0, -1, 1 };
                        int[] dy = { -1, 1, 0, 0 };

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = cx + dx[i];
                            int ny = cy + dy[i];

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                int nIndex = ny * width + nx;
                                if (!visited[nIndex])
                                {
                                    int nOffset = (ny * stride) + (nx * 4);
                                    byte nb = pixels[nOffset];
                                    byte ng = pixels[nOffset + 1];
                                    byte nr = pixels[nOffset + 2];

                                    // Check similarity to background (Tolerance)
                                    if (Math.Abs(nr - rBg) < 20 && Math.Abs(ng - gBg) < 20 && Math.Abs(nb - bBg) < 20)
                                    {
                                        visited[nIndex] = true;
                                        queue.Enqueue(nIndex);
                                    }
                                }
                            }
                        }
                    }
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                wb.Freeze();

                anim.AddFrame(wb);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
            }
            return anim;
        }

        public static FrameAnimation LoadFromDirectory(string directoryPath, double frameDuration)
        {
            var anim = new FrameAnimation 
            { 
                Name = new DirectoryInfo(directoryPath).Name,
                FrameDuration = frameDuration 
            };

            if (!Directory.Exists(directoryPath)) return anim;

            // Load PNGs, sorted by name (e.g., walk1, walk2)
            var files = Directory.GetFiles(directoryPath, "*.png");
            Array.Sort(files);

            foreach (var file in files)
            {
                // Reuse the clean single-loader (handles Freezing & Transparency)
                var tempAnim = LoadSingleImage(file);
                if (tempAnim.Frames.Count > 0)
                {
                    anim.AddFrame(tempAnim.Frames[0]);
                }
            }
            return anim;
        }

        public static FrameAnimation LoadFromSpriteSheet(string filePath, int frameWidth, int frameHeight, double frameDuration)
        {
            var anim = new FrameAnimation 
            { 
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                FrameDuration = frameDuration 
            };

            try 
            {
                var uri = new Uri(filePath, UriKind.Absolute);
                var fullSheet = new BitmapImage();
                fullSheet.BeginInit();
                fullSheet.UriSource = uri;
                fullSheet.CacheOption = BitmapCacheOption.OnLoad; // Load into memory
                fullSheet.EndInit();
                fullSheet.Freeze(); // Make cross-thread accessible

                int cols = fullSheet.PixelWidth / frameWidth;
                int rows = fullSheet.PixelHeight / frameHeight;

                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        var rect = new Int32Rect(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                        var frame = new CroppedBitmap(fullSheet, rect);
                        frame.Freeze();
                        anim.AddFrame(frame);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading spritesheet: {ex.Message}");
            }

            return anim;
        }
    }
}
