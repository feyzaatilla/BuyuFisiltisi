using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ScreenFlash : MonoBehaviour
    {
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
            var strength = Mathf.Sin(progress * Mathf.PI);
            var tint = Color.Lerp(new Color(0.9f, 0.2f, 0.3f), new Color(0.25f, 0.35f, 0.9f), Mathf.SmoothStep(0f, 1f, progress));
            var blind = Mathf.Sin(Mathf.Clamp01((now - _start) / 0.6f) * Mathf.PI);

            GUI.depth = 10;
            var previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, blind * 0.45f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(tint.r, tint.g, tint.b, 0.18f * strength);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
