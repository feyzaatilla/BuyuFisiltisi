using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ScreenFlash : MonoBehaviour
    {
        private const float FlashRate = 4f;

        private static ScreenFlash _instance;

        private float _start;
        private float _until;

        public static void Play(float seconds = 1.6f)
        {
            if (_instance == null)
            {
                var host = new GameObject("EkranParlamasi") { hideFlags = HideFlags.HideAndDontSave };
                _instance = host.AddComponent<ScreenFlash>();
            }

            Debug.Log("[Tuzak] Ekran parlaması başladı.");
            _instance._start = Time.unscaledTime;
            _instance._until = Time.unscaledTime + seconds;
        }

        private void OnGUI()
        {
            var now = Time.unscaledTime;
            if (Time.timeScale == 0f || now > _until)
                return;

            var progress = Mathf.InverseLerp(_start, _until, now);
            var strength = 1f - progress * progress;
            var red = Mathf.Repeat((now - _start) * FlashRate, 1f) < 0.5f;
            var tint = red ? new Color(1f, 0.1f, 0.15f) : new Color(0.15f, 0.35f, 1f);
            var blind = Mathf.Clamp01(1f - (now - _start) / 0.35f);

            GUI.depth = 10;
            var previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, blind * 0.9f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(tint.r, tint.g, tint.b, 0.45f * strength);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
