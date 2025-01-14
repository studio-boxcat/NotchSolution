using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    ///     Helper methods for Notch Solution's components.
    /// </summary>
    internal static class NotchSolutionUtility
    {
        /// <summary>
        ///     Calculated from <see cref="Screen"/> API without caring about simulated value.
        ///     Note that 2019.3 Unity Device Simulator can mock the <see cref="Screen"/> so this is not
        ///     necessary real in editor.
        /// </summary>
        // TODO : Cache potential, but many pitfalls awaits so I have not done it.
        // - Some first frames (1~3) Unity didn't return a rect that take account of safe area for some reason. If we cache that then we failed.
        // - Orientation change requries clearing the cache again. Manually or automatically? How?
        internal static Rect GetScreenSafeAreaRelative()
        {
            var safeArea = Screen.safeArea;
            return ToScreenRelativeRect(safeArea);
        }

        private static Rect ToScreenRelativeRect(Rect r)
        {
#if UNITY_STANDALONE
            var w = r.width;
            var h = r.height;
#else
            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
#endif
            return new Rect(r.x / w, r.y / h, r.width / w, r.height / h);
        }
    }
}