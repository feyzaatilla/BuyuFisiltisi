using System.Collections;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class DarkCloud : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.22f, 0.18f, 0.3f);
        [SerializeField] private float _fadeSeconds = 1.4f;
        [SerializeField] private float _bobHeight = 0.25f;

        private bool _dispersed;
        private Vector3 _basePosition;
        private float _seed;
        private Light _lightning;
        private float _nextFlash;

        public bool IsDispersed => _dispersed;

        private void Awake()
        {
            _basePosition = transform.position;
            _seed = Random.value * 10f;
            _lightning = GetComponentInChildren<Light>();
            _nextFlash = Time.time + Random.Range(1f, 4f);

            var material = new Material(GetComponentInChildren<Renderer>().sharedMaterial) { color = _color };
            foreach (var cloudRenderer in GetComponentsInChildren<Renderer>())
                cloudRenderer.sharedMaterial = material;
        }

        private void Update()
        {
            if (_dispersed)
                return;

            var time = Time.time + _seed;
            transform.position = _basePosition + new Vector3(Mathf.Sin(time * 0.3f) * 0.4f, Mathf.Sin(time * 0.8f) * _bobHeight, 0f);

            if (_lightning == null)
                return;

            if (Time.time >= _nextFlash)
            {
                _lightning.intensity = 6f;
                _nextFlash = Time.time + Random.Range(2f, 5f);
            }

            _lightning.intensity = Mathf.MoveTowards(_lightning.intensity, 0.4f, Time.deltaTime * 20f);
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
            var puffs = new Transform[transform.childCount];
            var starts = new Vector3[puffs.Length];
            var scales = new Vector3[puffs.Length];
            for (var i = 0; i < puffs.Length; i++)
            {
                puffs[i] = transform.GetChild(i);
                starts[i] = puffs[i].localPosition;
                scales[i] = puffs[i].localScale;
            }

            if (_lightning != null)
            {
                _lightning.color = new Color(1f, 0.95f, 0.7f);
                _lightning.intensity = 10f;
            }

            var elapsed = 0f;
            while (elapsed < _fadeSeconds)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _fadeSeconds);
                for (var i = 0; i < puffs.Length; i++)
                {
                    var direction = starts[i].sqrMagnitude > 0.001f ? starts[i].normalized : Vector3.up;
                    puffs[i].localPosition = starts[i] + (direction + Vector3.up * 0.5f) * (t * 3f);
                    puffs[i].localScale = Vector3.Lerp(scales[i], Vector3.zero, t);
                }

                if (_lightning != null)
                    _lightning.intensity = Mathf.Lerp(10f, 0f, t);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
