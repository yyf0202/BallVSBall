using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace BVB
{
    [AddComponentMenu("BVB UI/ExpressButton", 30)]
    [RequireComponent(typeof(RectTransform))]
    public class ExpressButton : Button
    {
        public const float CLICK_INTERVAL = 0.5f;
        public static float ClickIntervalTimer = 0f;

        public static void UpdateExpressButton()
        {
            ClickIntervalTimer += Time.deltaTime;
        }

        public static bool IsClickAllowed()
        {
            return ClickIntervalTimer >= CLICK_INTERVAL;
        }

        public static void ResetClickIntervalTimer()
        {
            ClickIntervalTimer = 0;
        }

        public bool bMute = false;
        public bool bIgnoreClickInterval = false;
        public Action OnHoverBeginAction = null;
        public Action OnHoverEndAction = null;

        public float GazeActionTime
        {
            get;
            set;
        }

        public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!bIgnoreClickInterval && !IsClickAllowed())
            {
                return;
            }

            if (!bIgnoreClickInterval)
            {
                ResetClickIntervalTimer();
            }

            base.OnPointerClick(eventData);

            //AudioModule.Instance.PlayOnClickSound();
        }

        public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (OnHoverBeginAction != null)
            {
                OnHoverBeginAction.Invoke();
            }

            if (FadeOnHover != null)
            {
                if (m_fadeOnHoverTweener != null)
                    m_fadeOnHoverTweener.Kill();

                FadeOnHover.gameObject.SetActive(true);
                m_fadeOnHoverTweener = FadeOnHover.DOFade(1, 0.2f);
            }

            if (HoverEffect != null)
                HoverEffect.SetActive(true);
            //AudioModule.Instance.PlayOnHoverBeginSound();
        }

        public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            if (OnHoverEndAction != null)
            {
                OnHoverEndAction.Invoke();
            }

            if (FadeOnHover != null)
            {
                if (m_fadeOnHoverTweener != null)
                    m_fadeOnHoverTweener.Kill();

                m_fadeOnHoverTweener = FadeOnHover.DOFade(0, 0.2f)
                                                      .OnComplete(() => { FadeOnHover.gameObject.SetActive(false); });
            }

            if (HoverEffect != null)
                HoverEffect.SetActive(false);
        }

        #region multi-image transition

        protected override void OnEnable()
        {
            base.OnEnable();

            if (HoverEffect != null)
                HoverEffect.SetActive(false);
        }

        [SerializeField]
        private bool m_enableMutiGraphics = false;

        public ColorBlock GraphicsColorBlock = new ColorBlock();
        public List<Graphic> ColorTweenGraphics = new List<Graphic>();

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            Color tintColor;
            Color mulitColor;
            Sprite transitionSprite;
            string triggerName;
            Vector3 targetScale = this.NormalScale;
            Vector3 targetLocalPosition = this.NormalLocalPosition;
            bool handleGradient = false;
            //        Debug.Log(this.gameObject.name + " state is " + state);
            switch (state)
            {
                case SelectionState.Normal:

                    tintColor = this.colors.normalColor;
                    mulitColor = GraphicsColorBlock.normalColor;
                    transitionSprite = null;
                    triggerName = this.animationTriggers.normalTrigger;
                    targetScale = this.NormalScale;
                    targetLocalPosition = this.NormalLocalPosition;
                    break;

                case SelectionState.Highlighted:
                    tintColor = this.colors.highlightedColor;
                    mulitColor = GraphicsColorBlock.highlightedColor;
                    transitionSprite = this.spriteState.highlightedSprite;
                    triggerName = this.animationTriggers.highlightedTrigger;
                    targetScale = this.HoverScale;
                    targetLocalPosition = this.HoverLocalPosition;
                    break;

                case SelectionState.Pressed:
                    tintColor = this.colors.pressedColor;
                    mulitColor = GraphicsColorBlock.pressedColor;
                    transitionSprite = this.spriteState.pressedSprite;
                    triggerName = this.animationTriggers.pressedTrigger;
                    targetScale = this.PressScale;
                    targetLocalPosition = this.PressLocalPosition;
                    break;

                case SelectionState.Disabled:
                    tintColor = this.colors.disabledColor;
                    mulitColor = GraphicsColorBlock.disabledColor;
                    transitionSprite = this.spriteState.disabledSprite;
                    triggerName = this.animationTriggers.disabledTrigger;
                    break;

                default:
                    tintColor = Color.black;
                    mulitColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            if (gameObject.activeInHierarchy)
            {
                //Debug.Log(this.gameObject.name + " state is " + state + ",m_enableMutiGraphics is " + m_enableMutiGraphics + ", color is " + mulitColor * GraphicsColorBlock.colorMultiplier);
                if (m_enableMutiGraphics)
                    ColorTween(mulitColor * GraphicsColorBlock.colorMultiplier, instant);

                if (m_bEnableScaleTransition)
                    ScaleTween(targetScale, TweenDuration);

                if (m_bEnablePositionTransition)
                    PositionTween(targetLocalPosition, TweenDuration);

                switch (this.transition)
                {
                    case Transition.ColorTint:
                        if (targetGraphic == null)
                            return;

                        //if (m_targetGradient != null && state == SelectionState.Disabled)
                        //{
                        //    //m_targetGradient.enabled = false;
                        //    targetGraphic.color = this.colors.disabledColor;
                        //}
                        //else
                        //{
                        //    //if (m_targetGradient != null && !m_targetGradient.enabled)
                        //        //m_targetGradient.enabled = true;

                        //    if (m_targetGradient != null && targetGraphic.color != Color.white)
                        //        targetGraphic.color = Color.white;

                        //    targetGraphic.CrossFadeColor(tintColor * this.colors.colorMultiplier, instant ? 0f : this.colors.fadeDuration, true, true);
                        //}
                        targetGraphic.CrossFadeColor(tintColor * this.colors.colorMultiplier, instant ? 0f : this.colors.fadeDuration, true, true);
                        break;
                    case Transition.SpriteSwap:
                        if (image == null)
                            return;

                        image.overrideSprite = transitionSprite;
                        break;
                    case Transition.Animation:
                        if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggerName))
                            return;

                        animator.ResetTrigger(this.animationTriggers.normalTrigger);
                        animator.ResetTrigger(this.animationTriggers.pressedTrigger);
                        animator.ResetTrigger(this.animationTriggers.highlightedTrigger);
                        animator.ResetTrigger(this.animationTriggers.disabledTrigger);
                        animator.SetTrigger(triggerName);
                        break;
                }
            }

            base.DoStateTransition(state, instant);
        }

        private void ColorTween(Color targetColor, bool instant)
        {
            foreach (Graphic g in this.ColorTweenGraphics)
                if (g != null)
                    g.CrossFadeColor(targetColor, (!instant) ? this.GraphicsColorBlock.fadeDuration : 0f, true, true);
        }
        #endregion

        #region Scale & Position Offset Position Transition
        [SerializeField]
        private bool m_bEnableScaleTransition = false;

        [SerializeField]
        private bool m_bEnablePositionTransition = false;

        public bool EnablePositionTransition
        {
            get
            {
                return m_bEnablePositionTransition;
            }
            set
            {
                m_bEnablePositionTransition = value;
            }
        }

        public bool EnableScaleTransition
        {
            get
            {
                return m_bEnableScaleTransition;
            }
            set
            {
                m_bEnableScaleTransition = value;
            }
        }

        public Vector3 NormalScale = Vector3.one;
        public Vector3 HoverScale = Vector3.one;
        public Vector3 PressScale = Vector3.one;
        public Transform ScaleTarget;

        public Vector3 NormalLocalPosition = Vector3.zero;
        public Vector3 HoverLocalPosition = Vector3.zero;
        public Vector3 PressLocalPosition = Vector3.zero;
        public Transform OffsetTarget;
        public float TweenDuration;

        public Ease TweenEase = Ease.InOutCubic;

        private Tweener m_scaleTweener;
        private Tweener m_positionTweener;


        private void ScaleTween(Vector3 targetScale, float duration)
        {
            if (m_scaleTweener != null)
                m_scaleTweener.Kill();

            if (ScaleTarget == null)
                ScaleTarget = this.transform;

            m_scaleTweener = ScaleTarget.DOScale(targetScale, duration).SetEase(TweenEase);
        }

        public bool EnablePivotPositionMove = false;

        private void PositionTween(Vector3 targetPosition, float duration)
        {
            if (m_positionTweener != null)
                m_positionTweener.Kill();

            if (OffsetTarget == null)
                OffsetTarget = this.transform;

            if (EnablePivotPositionMove)
                m_positionTweener = OffsetTarget.GetComponent<RectTransform>().DOAnchorPos3D(targetPosition, duration).SetEase(TweenEase);
            else
                m_positionTweener = OffsetTarget.DOLocalMove(targetPosition, duration).SetEase(TweenEase);
        }
        #endregion

        public void OnGazeHover(PointerEventData eventData = null)
        {

        }

        public void OnGazeClick(PointerEventData eventData = null)
        {
            if (onClick != null)
                onClick.Invoke();
        }

        [Header("CanvasGroup which will fade in when button on hover!")]
        public CanvasGroup FadeOnHover;
        private Tweener m_fadeOnHoverTweener;

        [Header("Set active when button on hover!")]
        public GameObject HoverEffect;
    }
}