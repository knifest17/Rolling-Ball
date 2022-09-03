using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class InputManager : MonoBehaviour
    {
        float standartScreenSqw = 2000000;
        float screenSqw;
        float k;
        Vector3 startMousePosition;
        Vector2 startTouchPosition;
        Vector3 input;

        public event Action<Vector3> InputEnded;
        public event Action InputBegined;
        public Vector3 Input => input;

        void Awake()
        {
            screenSqw = Screen.height * Screen.width;
            k = screenSqw / standartScreenSqw;
        }

        void Update()
        {
#if UNITY_ANDROID
            if (UnityEngine.Input.touchCount <= 0) return;
            var touch = UnityEngine.Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                InputBegined?.Invoke();
            }
            input = (touch.position - startTouchPosition) / (100 * k);
            input.z = input.y;
            input.y = 0f;
            print(input);
            if (touch.phase == TouchPhase.Ended)
            {
                InputEnded?.Invoke(input);
            }
#else
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                startMousePosition = UnityEngine.Input.mousePosition;
                InputBegined?.Invoke();
            }
            input = (UnityEngine.Input.mousePosition - startMousePosition) / (100 * k);
            input.z = input.y;
            input.y = 0f;
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                InputEnded?.Invoke(input);
            }
#endif
        }
    }
}