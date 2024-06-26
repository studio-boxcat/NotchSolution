using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public abstract class NotchSolutionUIBehaviourBase : UIBehaviour, ILayoutSelfController, INotchSimulatorTarget
    {
        private readonly WaitForEndOfFrame eofWait = new WaitForEndOfFrame();

        [NonSerialized]
        private RectTransform m_Rect;

        protected DrivenRectTransformTracker m_Tracker;
        private Rect[] storedSimulatedCutoutsRelative = NotchSolutionUtility.defaultCutouts;

        private Rect storedSimulatedSafeAreaRelative = NotchSolutionUtility.defaultSafeArea;

        /// <summary>
        ///     Already taken account whether should trust Notch Simulator
        ///     or Unity's [Device Simulator package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/).
        /// </summary>
        protected Rect SafeAreaRelative
            => NotchSolutionUtility.ShouldUseNotchSimulatorValue
                ? storedSimulatedSafeAreaRelative
                : NotchSolutionUtility.ScreenSafeAreaRelative;

        protected RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }

                return m_Rect;
            }
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected virtual void OnEnable()
        {
            DelayedUpdate();
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected virtual void OnDisable()
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

        void INotchSimulatorTarget.SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
        {
            storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
            storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
            UpdateRectBase();
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

        private void DelayedUpdate()
        {
            StartCoroutine(DelayedUpdateRoutine());

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In Edit Mode, coroutines don't always work but we also can't call UpdateRectBase directly because it spams warning messages
                // when called directly from OnValidate. So we can wait for 1 editor frame in EditorApplication.update instead
                UnityEditor.EditorApplication.update += DelayedEditorUpdate;

                void DelayedEditorUpdate()
                {
                    UnityEditor.EditorApplication.update -= DelayedEditorUpdate;
                    if (this == null) return;
                    UpdateRectBase();
                };
            }
#endif
        }

        private IEnumerator DelayedUpdateRoutine()
        {
            yield return eofWait;
            UpdateRectBase();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                DelayedUpdate();
            }
        }
#endif
    }
}
