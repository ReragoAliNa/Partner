using System;
using System.Windows.Media.Imaging;

namespace DesktopPet.Core.Animation
{
    public class AnimationController
    {
        private FrameAnimation _currentAnimation;
        private double _timer;
        private int _currentFrameIndex;

        public BitmapSource CurrentFrame
        {
            get
            {
                if (_currentAnimation == null || _currentAnimation.Frames.Count == 0)
                    return null;
                return _currentAnimation.Frames[_currentFrameIndex];
            }
        }

        public void Play(FrameAnimation animation)
        {
            if (_currentAnimation == animation) return;

            _currentAnimation = animation;
            _currentFrameIndex = 0;
            _timer = 0;
        }

        public void Update(double deltaTime)
        {
            if (_currentAnimation == null || _currentAnimation.Frames.Count == 0) return;

            _timer += deltaTime;

            // Cycle frames based on FrameDuration (e.g., 12fps)
            // independent of Update loop (e.g., 60fps)
            if (_timer >= _currentAnimation.FrameDuration)
            {
                // Handle multiple frame skips if delta is large
                while (_timer >= _currentAnimation.FrameDuration)
                {
                    _timer -= _currentAnimation.FrameDuration;
                    _currentFrameIndex++;
                }

                if (_currentFrameIndex >= _currentAnimation.Frames.Count)
                {
                    if (_currentAnimation.IsLooping)
                    {
                        // Reset to start
                        _currentFrameIndex = _currentFrameIndex % _currentAnimation.Frames.Count;
                    }
                    else
                    {
                        // Stick at end
                        _currentFrameIndex = _currentAnimation.Frames.Count - 1;
                    }
                }
            }
        }
    }
}
