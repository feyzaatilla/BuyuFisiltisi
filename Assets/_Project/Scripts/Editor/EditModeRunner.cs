using System;
using UnityEditor;

namespace BuyuFisiltisi.EditorTools
{
    public static class EditModeRunner
    {
        private static Action _pending;

        public static bool RunInEditMode(Action action)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return false;

            _pending = action;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.ExitPlaymode();
            return true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode)
                return;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            var action = _pending;
            _pending = null;
            action?.Invoke();
        }
    }
}
