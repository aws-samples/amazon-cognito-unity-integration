using UnityEngine;
using TMPro;

namespace Amazon.XR.Keyboard
{
    [RequireComponent(typeof(TMP_InputField))]
    public class KeyboardListener : MonoBehaviour
    {
        private TMP_InputField _inputField;
        private static KeyboardListener _activeListener;

        private void Awake()
        {
            _inputField = this.GetComponent<TMP_InputField>();
        }

        public void EnableListener()
        {
            if (_activeListener != null)
            {
                _activeListener.DisableListener();
            }

            Keyboard.OnTextUpdate += HandleUpdateText;
            _activeListener = this;
            Debug.Log($"[KeyboardListener] Active: {_activeListener.gameObject.name}");
        }

        public void DisableListener()
        {
            Keyboard.OnTextUpdate -= HandleUpdateText;
        }

        private void HandleUpdateText(string text)
        {
            _inputField.text = text;
            _inputField.ForceLabelUpdate();
        }
    }
}

