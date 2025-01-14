using System.Runtime.CompilerServices;
using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    ///     Helper methods for Notch Solution's components.
    /// </summary>
    public static class SafePaddingUtils
    {
        public static void UpdateRect(
            RectTransform rt,
            PaddingModes modes,
            Vector4 influence /* LDUR */,
            bool flipPadding)
        {
            // full stretch
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            var rect = GetCanvasSize(rt);
            var w = rect.x;
            var h = rect.y;

            var safeArea = GetScreenSafeAreaRelative();
            var insetRel = new Vector4( // relative inset
                safeArea.xMin, // l
                safeArea.yMin, // d
                1 - (safeArea.yMin + safeArea.height), // u
                1 - (safeArea.xMin + safeArea.width) // r
            );

            // fixed: sometimes relativeLDUR will be NAN when start at some android devices.
            // if relativeLDUR is NAN then sizeDelta will be NAN, the safe area will be wrong.
            if (float.IsNaN(insetRel[0])) insetRel[0] = 0;
            if (float.IsNaN(insetRel[1])) insetRel[1] = 0;
            if (float.IsNaN(insetRel[2])) insetRel[2] = 0;
            if (float.IsNaN(insetRel[3])) insetRel[3] = 0;


            var inset = new Vector4(
                Calculate(modes.left, w, influence.x, insetRel.x, insetRel.w), // l
                Calculate(modes.bottom, h, influence.y, insetRel.y, insetRel.z), // d
                Calculate(modes.top, h, influence.z, insetRel.z, insetRel.y), // u
                Calculate(modes.right, w, influence.w, insetRel.w, insetRel.x) // r
            );

            if (flipPadding)
            {
                (inset.x, inset.w) = (inset.w, inset.x); // x=L, w=R
                (inset.y, inset.z) = (inset.z, inset.y); // y=B, z=T
            }

            //Combined padding becomes size delta.
            var sizeDelta = new Vector2(-(inset.x + inset.w), -(inset.y + inset.z));
            rt.sizeDelta = sizeDelta;

            //Anchor position's answer is depending on pivot too. Where the pivot point is defines where 0 anchor point is.
            var delta = rt.pivot * sizeDelta;

            //Calculate like zero position is at bottom left first, then diff with the real zero position.
            rt.anchoredPosition = new Vector2(inset.x + delta.x, inset.y + delta.y);

#if DEBUG
            Debug.Log($"[SafePadding] safeArea={safeArea}, insetRel={insetRel}, inset={inset}, sizeDelta={sizeDelta}, anchoredPosition={rt.anchoredPosition}");
#endif
            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float Calculate(PaddingMode mode, float size, float influence, float a, float b)
            {
                if (mode is PaddingMode.Off) return 0;
                var value = mode is PaddingMode.On ? a : (b > a ? b : a);
                return size * influence * value;
            }
        }

        /// <summary>
        ///     Calculated from <see cref="Screen"/> API without caring about simulated value.
        ///     Note that 2019.3 Unity Device Simulator can mock the <see cref="Screen"/> so this is not
        ///     necessary real in editor.
        /// </summary>
        // TODO : Cache potential, but many pitfalls awaits so I have not done it.
        // - Some first frames (1~3) Unity didn't return a rect that take account of safe area for some reason. If we cache that then we failed.
        // - Orientation change requries clearing the cache again. Manually or automatically? How?
        private static Rect GetScreenSafeAreaRelative()
        {
            var safeArea = Screen.safeArea;
            return ToScreenRelativeRect(safeArea);

            static Rect ToScreenRelativeRect(Rect r)
            {
#if UNITY_EDITOR
                var gameViewSize = UnityEditor.Handles.GetMainGameViewSize();
                var w = gameViewSize.x;
                var h = gameViewSize.y;
#elif UNITY_STANDALONE
                var w = r.width;
                var h = r.height;
#else
                var w = Screen.currentResolution.width;
                var h = Screen.currentResolution.height;
#endif
                return new Rect(r.x / w, r.y / h, r.width / w, r.height / h);
            }
        }

        private static Vector2 GetCanvasSize(RectTransform rt)
        {
            var topLevelCanvas = rt.GetComponentInParent<Canvas>().rootCanvas;
            return topLevelCanvas.GetComponent<RectTransform>().sizeDelta;
        }
    }
}