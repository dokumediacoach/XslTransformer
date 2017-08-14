using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace XslTransformer
{
    /// <summary>
    /// Animation helpers for <see cref="Storyboard"/>
    /// </summary>
    public static class StoryboardHelpers
    {
        /// <summary>
        /// Adds a fade in animation to the storyboard
        /// </summary>
        /// <param name="storyboard">The storyboard to add the animation to</param>
        /// <param name="durationMilliseconds">The time the animation will take</param>
        public static void AddFadeIn(this Storyboard storyboard, int durationMilliseconds)
        {
            // Create the fade in animation
            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                From = 0,
                To = 1
            };

            // Set the target property name
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            // Add this to the storyboard
            storyboard.Children.Add(animation);
        }

        /// <summary>
        /// Adds a fade out animation to the storyboard
        /// </summary>
        /// <param name="storyboard">The storyboard to add the animation to</param>
        /// <param name="durationMilliseconds">The time the animation will take</param>
        public static void AddFadeOut(this Storyboard storyboard, int durationMilliseconds)
        {
            // Create the fade out animation
            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                From = 1,
                To = 0
            };

            // Set the target property name
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            // Add this to the storyboard
            storyboard.Children.Add(animation);
        }
    }
}
