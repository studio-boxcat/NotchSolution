using System;
using UnityEngine;
using UnityEngine.UI;

namespace E7.NotchSolution
{
    /// <summary>
    ///     A base class to derive from if you want to make a notch-aware <see cref="UIBehaviour"/> component.
    ///     <see cref="UpdateRect"/> will be called at the "correct moment".
    ///     You change the <see cref="rectTransform"/> as you like in there.
    /// </summary>
    /// <remarks>
    ///     It helps you store the simulated values from Notch Simulator and expose them as `protected` fields.
    ///     Plus you can use <see cref="GetCanvasRect"/> to travel to the closest <see cref="Canvas"/> that is
    ///     this component's parent. Usually you will want to do something related to the "entire screen".
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class NotchSolutionUIBehaviourBase : UIBehaviour, ILayoutSelfController
    {
        [NonSerialized] private RectTransform m_Rect;
        protected DrivenRectTransformTracker m_Tracker;

        protected RectTransform rectTransform => m_Rect ??= (RectTransform) transform;

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>
        /// </summary>
        private void OnEnable()
        {
            UpdateRectBase();
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>
        /// </summary>
        private void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>.
        ///     This doesn't work when flipping the orientation to opposite side (180 deg).
        ///     It only works for 90 deg. rotation because that makes the rect transform changes dimension.
        /// </summary>
        protected virtual void OnRectTransformDimensionsChange()
        {
            UpdateRectBase();
        }

        void ILayoutController.SetLayoutHorizontal()
        {
            UpdateRectBase();
        }

        void ILayoutController.SetLayoutVertical()
        {
        }

        protected abstract void UpdateRect();

        protected Rect GetCanvasRect()
        {
            var topLevelCanvas = GetTopLevelCanvas();
            var topRectSize = topLevelCanvas.GetComponent<RectTransform>().sizeDelta;
            return new Rect(Vector2.zero, topRectSize);

            Canvas GetTopLevelCanvas()
            {
                var canvas = GetComponentInParent<Canvas>();
                var rootCanvas = canvas.rootCanvas;
                return rootCanvas;
            }
        }

        private void UpdateRectBase()
        {
            if (!(enabled && gameObject.activeInHierarchy))
            {
                return;
            }

            UpdateRect();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>.
        /// </summary>
        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                UpdateRectBase();
            }
        }
#endif
    }
}
