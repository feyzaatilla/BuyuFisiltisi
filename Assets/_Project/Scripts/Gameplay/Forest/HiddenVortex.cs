using BuyuFisiltisi.Gameplay.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class HiddenVortex : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _destination;
        [SerializeField] private Transform _swirl;
        [SerializeField] private Transform _hint;
        [SerializeField] private Vector3 _areaSize = new Vector3(24f, 6f, 5f);
        [SerializeField] private float _pullSeconds = 1.2f;

        private bool _used;
        private float _pullUntil = -1f;

        private void Awake()
        {
            if (_swirl != null)
                _swirl.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_hint != null)
                _hint.Rotate(0f, 90f * Time.deltaTime, 0f);

            if (_player == null)
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
            {
                Debug.Log("[Girdap] F9 ile doğrudan parkura geçildi.");
                _used = true;
                _pullUntil = 0f;
                Drop();
                return;
            }

            if (_used)
                return;

            if (_pullUntil < 0f)
            {
                var local = _player.position - transform.position;
                if (Mathf.Abs(local.x) > _areaSize.x * 0.5f || Mathf.Abs(local.z) > _areaSize.z * 0.5f || Mathf.Abs(local.y) > _areaSize.y)
                    return;

                _pullUntil = Time.time + _pullSeconds;
                Debug.Log("[Girdap] Cadı girdaba girdi: " + _player.position);
                GameSound.Play(SoundKind.Vortex, 0.8f);
                GameSound.Play(SoundKind.Ouch, 0.9f);
                if (_hint != null)
                    _hint.gameObject.SetActive(false);
                if (_swirl != null)
                {
                    _swirl.gameObject.SetActive(true);
                    _swirl.position = new Vector3(_player.position.x, transform.position.y + 0.05f, _player.position.z);
                    _swirl.localScale = Vector3.one * 0.2f;
                }
                GameMessage.Show("Ayağının altında toprak dönmeye başladı... Gizli bir girdap seni içine çekiyor!", new Color(0.8f, 0.6f, 1f), 3f);
            }

            if (_swirl != null)
            {
                _swirl.Rotate(0f, 540f * Time.deltaTime, 0f);
                _swirl.localScale = Vector3.MoveTowards(_swirl.localScale, Vector3.one * 3f, Time.deltaTime * 4f);
            }

            if (Time.time < _pullUntil)
                return;

            _used = true;
            Drop();
        }

        private void Drop()
        {
            if (_swirl != null)
                _swirl.gameObject.SetActive(false);
            if (_hint != null)
                _hint.gameObject.SetActive(false);

            var respawnable = _player.GetComponentInParent<IRespawnable>();
            if (respawnable != null && _destination != null)
                respawnable.Respawn(_destination.position, _destination.rotation);
            ScreenFlash.Play(2f);
            Debug.Log("[Girdap] Cadı parkura taşındı: " + (_destination != null ? _destination.position.ToString() : "hedef yok"));
            GameMessage.Show("Köklerin altındaki karanlık bir mağaraya düştün. Tuzaklara dikkat et ve çıkışı bul!", new Color(0.65f, 1f, 0.75f), 4f);
        }
    }
}
