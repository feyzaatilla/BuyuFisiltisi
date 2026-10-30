using System.Collections;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class DarkCloud : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.18f, 0.18f, 0.24f);
        [SerializeField] private float _fadeSeconds = 1.2f;

        private bool _dispersed;

        public bool IsDispersed => _dispersed;

        private void Awake()
        {
            var cloudRenderer = GetComponent<Renderer>();
            if (cloudRenderer != null)
                cloudRenderer.material.color = _color;
        }

        public void Disperse()
        {
            if (_dispersed)
                return;

            _dispersed = true;
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            var start = transform.localScale;
            var elapsed = 0f;
            while (elapsed < _fadeSeconds)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _fadeSeconds);
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
                transform.position += Vector3.up * (Time.deltaTime * 2f);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
