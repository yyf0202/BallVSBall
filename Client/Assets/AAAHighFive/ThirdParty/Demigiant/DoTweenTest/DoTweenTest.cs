using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace DG
{
    public class DoTweenTest : MonoBehaviour
    {
        public GameObject Cube;
        public Image Image;

        public float FloatValue = 0;
        private Tweener _Tweener = null;

        void Test1()
        {
            Tweener tweener = Image.rectTransform.DOMove(Vector3.zero, 1f);
            tweener.SetUpdate(true) //设置这个Tween不受Time.scale影响
                   .SetEase(Ease.InQuad)
                   .OnComplete(() => { Debug.Log("移动完毕事件"); })
                   .Play();
        }

        void Test2()
        {
            if (_Tweener != null)
            {
                _Tweener.Kill();
            }
            FloatValue = 0;
            _Tweener = DOTween.To(() => { return FloatValue; }, (f) => { FloatValue = f; }, 100, 5)
                              .OnStart(() => { Debug.Log("Tween 100 Start..."); }).OnComplete(() => { Debug.Log("Tween 100 Finished..."); })
                              .ChangeValues(0, 1);
        }

        void Test3()
        {
            if (_Tweener != null)
            {
                _Tweener.Kill();
            }
            _Tweener = DOTween.To(() => { return FloatValue; }, (f) => { FloatValue = f; }, 0, 5)
                              .OnStart(() => { Debug.Log("Tween 0 Start..."); })
                              .OnComplete(() => { Debug.Log("Tween 0 Finished..."); });
        }

        void Test4()
        {
            if (_Tweener != null)
            {
                _Tweener.Kill();
            }

            Vector3 scale = Vector3.one;
            _Tweener = DOTween.To(() => { return scale; }, (f) => { scale = f; Image.transform.localScale = scale; }, new Vector3(1.2f, 1.2f, 1.2f), 0.2f);
            _Tweener.SetEase(Ease.InSine)
                   .OnStart(() => { Debug.Log("Tween expend Start..."); })
                   .OnComplete(() =>
                   {
                       Debug.Log("Tween expend Finished...");
                       _Tweener = DOTween.To(() => { return scale; }, (f) => { scale = f; Image.transform.localScale = scale; }, Vector3.one, 0.2f);
                       _Tweener.SetEase(Ease.OutSine)
                                     .OnStart(() =>
                                     {
                                         Debug.Log("Tween shrink Start...");
                                     })
                                     .OnComplete(() =>
                                     {
                                         Debug.Log("Tween shrink Finished...");
                                     });
                   });
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Test1"))
            {
                Test1();
            }
            if (GUILayout.Button("Test2"))
            {
                Test2();
            }
            if (GUILayout.Button("Test3"))
            {
                Test3();
            }
            if (GUILayout.Button("Test4"))
            {
                Test4();
            }
        }
    }
}