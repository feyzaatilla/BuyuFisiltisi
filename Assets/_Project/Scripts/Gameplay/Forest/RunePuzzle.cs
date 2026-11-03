using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class RunePuzzle : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private Transform[] _stones = new Transform[0];
        [SerializeField] private string[] _names = new string[0];
        [SerializeField] private int[] _order = new int[0];
        [SerializeField] private Light[] _lights = new Light[0];
        [SerializeField] private GameObject _exit;
        [SerializeField] private Transform _hintPoint;
        [SerializeField] private string _hint = "Duvardaki yazı: \"Önce gece uyanır, sonra yıldızlar göz kırpar, en son güneş doğar.\" Taşların üstüne bu sırayla bas.";
        [SerializeField] private float _touchRadius = 1.3f;

        private int _progress;
        private int _touching = -1;
        private bool _hintShown;

        public bool IsSolved { get; private set; }

        private void Awake()
        {
            if (_exit != null)
                _exit.SetActive(false);
            SetLights(0);
        }

        private void Update()
        {
            if (IsSolved || _player == null)
                return;

            if (_hintPoint != null)
            {
                var hintDistance = Vector3.Distance(_player.position, _hintPoint.position);
                if (!_hintShown && hintDistance < 3f)
                {
                    _hintShown = true;
                    GameMessage.Show(_hint, new Color(1f, 0.9f, 0.6f), 8f);
                }
                else if (_hintShown && hintDistance > 5f)
                {
                    _hintShown = false;
                }
            }

            var touched = -1;
            for (var i = 0; i < _stones.Length; i++)
            {
                var offset = _player.position - _stones[i].position;
                offset.y = 0f;
                if (offset.magnitude <= _touchRadius)
                    touched = i;
            }

            if (touched == _touching)
                return;

            _touching = touched;
            if (touched < 0)
                return;

            if (_order.Length > _progress && _order[_progress] == touched)
            {
                _progress++;
                SetLights(_progress);
                if (_progress >= _order.Length)
                {
                    IsSolved = true;
                    if (_exit != null)
                        _exit.SetActive(true);
                    GameMessage.Show("Taşların hepsi parladı! Çıkış kapısı açıldı.", new Color(0.65f, 1f, 0.75f), 4f);
                }
                else
                {
                    GameMessage.Show(NameOf(touched) + " parladı.", new Color(0.8f, 0.9f, 1f), 2f);
                }
                return;
            }

            _progress = 0;
            SetLights(0);
            GameMessage.Show(NameOf(touched) + " söndü, taşlar sıfırlandı. " + _hint, new Color(1f, 0.6f, 0.55f), 8f);
        }

        private string NameOf(int index)
        {
            return index < _names.Length ? _names[index] : "Taş";
        }

        private void SetLights(int litCount)
        {
            for (var i = 0; i < _lights.Length; i++)
            {
                if (_lights[i] == null)
                    continue;

                var lit = false;
                for (var step = 0; step < litCount && step < _order.Length; step++)
                    if (_order[step] == i)
                        lit = true;
                _lights[i].intensity = lit ? 6f : 0.3f;
            }
        }
    }
}
