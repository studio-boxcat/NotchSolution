using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace E7.NotchSolution
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    internal class SafePadding : UIBehaviour
    {
        [SerializeField]
        private PerEdgeEvaluationModes portraitOrDefaultPaddings;
        [Tooltip("Scale down the resulting value read from an edge to be less than an actual value.")]
        [SerializeField] [Range(0f, 1f)]
        private float influence = 1;
        [Tooltip("The value read from all edges are applied to the opposite side of a RectTransform instead. Useful when you have rotated or negatively scaled RectTransform.")]
        [SerializeField]
        private bool flipPadding;

        [NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform => m_Rect ??= (RectTransform) transform;


        private void Awake() => UpdateRect();

        private Vector2 GetCanvasSize()
        {
            var topLevelCanvas = GetComponentInParent<Canvas>().rootCanvas;
            return topLevelCanvas.GetComponent<RectTransform>().sizeDelta;
        }

        [Button]
        private void UpdateRect()
        {
            var rt = rectTransform;

            // full stretch
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            var rect = GetCanvasSize();
            var w = rect.x;
            var h = rect.y;

            var safeArea = NotchSolutionUtility.GetScreenSafeAreaRelative();
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


            var e = portraitOrDefaultPaddings;
            var inset = new Vector4(
                Calculate(e.left, w, insetRel.x, insetRel.w), // l
                Calculate(e.bottom, h, insetRel.y, insetRel.z), // d
                Calculate(e.top, h, insetRel.z, insetRel.y), // u
                Calculate(e.right, w, insetRel.w, insetRel.x) // r
            );

            //Apply influence to the calculated padding
            inset *= influence;

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

            Debug.Log($"[SafePadding] safeArea={safeArea}, insetRel={insetRel}, inset={inset}, sizeDelta={sizeDelta}, anchoredPosition={rt.anchoredPosition}");
            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float Calculate(EdgeEvaluationMode mode, float size, float a, float b)
            {
                if (mode is EdgeEvaluationMode.Off) return 0;
                return size * (mode is EdgeEvaluationMode.On ? a : (b > a ? b : a));
            }
        }

#if UNITY_EDITOR
        private DrivenRectTransformTracker m_Tracker;

        private void OnValidate()
        {
            //Lock the anchor mode to full stretch first.
            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform,
                DrivenTransformProperties.AnchorMinX |
                DrivenTransformProperties.AnchorMaxX |
                DrivenTransformProperties.AnchorMinY |
                DrivenTransformProperties.AnchorMaxY |
                DrivenTransformProperties.SizeDeltaX |
                DrivenTransformProperties.SizeDeltaY |
                DrivenTransformProperties.AnchoredPositionX |
                DrivenTransformProperties.AnchoredPositionY);
        }
#endif
    }
}