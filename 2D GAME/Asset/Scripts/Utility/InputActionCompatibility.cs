using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputSystem
{
    [Serializable]
    public struct InputBinding
    {
    }

    [Serializable]
    public class InputAction
    {
        public List<InputBinding> bindings = new List<InputBinding>();

        public bool triggered
        {
            get { return Input.GetKeyDown(KeyCode.Escape); }
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public T ReadValue<T>() where T : struct
        {
            object value = default(T);

            if (typeof(T) == typeof(Vector2))
            {
                value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            else if (typeof(T) == typeof(float))
            {
                value = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) ? 1f : 0f;
            }

            return (T)value;
        }
    }

    public sealed class Keyboard
    {
        public static readonly Keyboard current = new Keyboard();

        public readonly KeyControl leftShiftKey = new KeyControl(KeyCode.LeftShift);
        public readonly KeyControl rightShiftKey = new KeyControl(KeyCode.RightShift);
        public readonly KeyControl backquoteKey = new KeyControl(KeyCode.BackQuote);
        public readonly KeyControl enterKey = new KeyControl(KeyCode.Return);

        public KeyControl FindKeyOnCurrentKeyboardLayout(string keyName)
        {
            if (Enum.TryParse(keyName, true, out KeyCode parsedKey))
            {
                return new KeyControl(parsedKey);
            }

            if (keyName == "`" || keyName == "backquote")
            {
                return backquoteKey;
            }

            if (keyName == "enter" || keyName == "return")
            {
                return enterKey;
            }

            return new KeyControl(KeyCode.None);
        }
    }

    public sealed class KeyControl
    {
        private readonly KeyCode keyCode;

        public KeyControl(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }

        public bool isPressed
        {
            get { return keyCode != KeyCode.None && Input.GetKey(keyCode); }
        }

        public bool wasPressedThisFrame
        {
            get { return keyCode != KeyCode.None && Input.GetKeyDown(keyCode); }
        }
    }
}
