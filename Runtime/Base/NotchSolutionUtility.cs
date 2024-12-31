using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    ///     Helper methods for Notch Solution's components.
    /// </summary>
    public static class NotchSolutionUtility
    {
        /// <summary>
        ///     Calculated from <see cref="Screen"/> API without caring about simulated value.
        ///     Note that 2019.3 Unity Device Simulator can mock the <see cref="Screen"/> so this is not
        ///     necessary real in editor.
        /// </summary>
        // TODO : Cache potential, but many pitfalls awaits so I have not done it.
        // - Some first frames (1~3) Unity didn't return a rect that take account of safe area for some reason. If we cache that then we failed.
        // - Orientation change requries clearing the cache again. Manually or automatically? How?
        internal static Rect ScreenSafeAreaRelative
        {
            get
            {
                var absolutePaddings = Screen.safeArea;
                return ToScreenRelativeRect(absolutePaddings);
            }
        }

        private static Rect ToScreenRelativeRect(Rect absoluteRect)
        {
#if UNITY_EDITOR
            var size = UnityEditor.Handles.GetMainGameViewSize();
            var w = size.x;
            var h = size.y;
#elif UNITY_STANDALONE
            var w = absoluteRect.width;
            var h = absoluteRect.height;
#else
            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
#endif
            //Debug.Log($"{w} {h} {Screen.currentResolution} {absoluteRect}");
            return new Rect(
                absoluteRect.x / w,
                absoluteRect.y / h,
                absoluteRect.width / w,
                absoluteRect.height / h
            );
        }
    }
}