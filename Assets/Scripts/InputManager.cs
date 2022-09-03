using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class InputManager : MonoBehaviour
    {
        int standartScreenSqw = 2000000;
        int screenSqw;
        float k;
        Vector3 startMousePosition;
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
        }
    }
}