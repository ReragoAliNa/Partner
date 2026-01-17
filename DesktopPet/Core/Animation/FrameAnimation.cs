using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace DesktopPet.Core.Animation
{
    public class FrameAnimation
    {
        public string Name { get; set; }
        public List<BitmapSource> Frames { get; private set; } = new List<BitmapSource>();
        public double FrameDuration { get; set; } = 0.0833; // Default 12fps ~= 0.0833s
        public bool IsLooping { get; set; } = true;

        public void AddFrame(BitmapSource frame)
        {
            Frames.Add(frame);
        }
    }
}
