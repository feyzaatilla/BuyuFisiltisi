using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.UI.Intro
{
    public sealed class IntroView : MonoBehaviour
    {
        private enum Phase
        {
            Lost,
            Arrival,
            Pages,
            Shrink,
            Done
        }

        private static readonly string[] LostLines =
        {
            "Cadı, karanlık ormanın derinliklerinde yolunu kaybetti...",
            "Ağaçlar fısıldıyor, ama hiçbiri yol göstermiyor."
        };

        private static readonly string[] ArrivalLines =
        {
            "???: Kaybolmuşsun, değil mi? Hıh... Neyse ki ben buradayım.",
            "Fısıl: Ben Fısıl, eski bir büyü kitabıyım. Sana yardım edebilirim. Bak..."
        };

        private static readonly string[] PageLines =
        {
            "Bu orman bir zamanlar şarkı söylerdi. Bir cadının ihaneti onu susturdu ve Orman Kalbi'nin kapısı mühürlendi.",
            "Önce kara bulutları rüzgâr büyüsüyle dağıt: buluta yaklaş ve sol tıkla. Köprü kalkık; kök duvarındaki yarıktan yalnızca ben geçip şalteri çekebilirim.",
            "Çiçek Vadisi'nde altı çiçek topla, büyü kazanında üç tarifi demle. Hasta ateş böceğini ve Yaşlı Meşe'nin yaralı kökünü iyileştir.",
            "Kapıyı Yaşlı Meşe korur ve cadılara güvenmez. Onu sözlerinle ve iyiliklerinle ikna et. Bana emir vermek için Tab ile sayfalarımı aç."
        };

        [SerializeField] private Transform _witch;
        [SerializeField] private Transform _companionVisual;
        [SerializeField] private Transform _introBook;
        [SerializeField] private GameObject _closedBook;
        [SerializeField] private GameObject _openBook;
        [SerializeField] private Behaviour _cameraRig;
        [SerializeField] private float _lineSeconds = 4f;

        private Phase _phase = Phase.Lost;
        private int _line;
        private float _phaseStarted;
        private float _lineStarted;
        private Vector3 _companionScale = Vector3.one;
        private Vector3 _bookScale = Vector3.one;
        private Vector3 _bookTarget;
        private Vector3 _shrinkFrom;
        private Transform _camera;
        private GUIStyle _subtitleStyle;
        private GUIStyle _pageStyle;
        private GUIStyle _hintStyle;
        private Texture2D _black;
        private Texture2D _paper;

        public bool IsRunning => _phase != Phase.Done;

        private void Start()
        {
            if (_witch == null || _introBook == null)
            {
                _phase = Phase.Done;
                return;
            }

            Time.timeScale = 0f;
            _camera = Camera.main != null ? Camera.main.transform : null;
            if (_cameraRig != null)
                _cameraRig.enabled = false;

            if (_companionVisual != null)
            {
                _companionScale = _companionVisual.localScale;
                _companionVisual.localScale = Vector3.zero;
            }

            _bookScale = _introBook.localScale;
            _bookTarget = _witch.position + _witch.forward * 2.2f + Vector3.up * 0.7f;
            _introBook.position = _bookTarget + Vector3.up * 5f;
            _introBook.gameObject.SetActive(false);
            SetBookOpen(false);
            Begin(Phase.Lost);
        }

        private void OnDisable()
        {
            if (_phase != Phase.Done)
                Finish();
        }

        private void Begin(Phase phase)
        {
            _phase = phase;
            _line = 0;
            _phaseStarted = Time.unscaledTime;
            _lineStarted = Time.unscaledTime;

            if (phase == Phase.Arrival)
                _introBook.gameObject.SetActive(true);

            if (phase == Phase.Pages)
                SetBookOpen(true);

            if (phase == Phase.Shrink)
            {
                SetBookOpen(false);
                _shrinkFrom = _introBook.position;
            }
        }

        private void Update()
        {
            if (_phase == Phase.Done)
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Finish();
                return;
            }

            var next = keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
            var elapsed = Time.unscaledTime - _phaseStarted;
            var lineElapsed = Time.unscaledTime - _lineStarted;

            switch (_phase)
            {
                case Phase.Lost:
                    OrbitCamera(elapsed);
                    if (next || lineElapsed >= _lineSeconds)
                        AdvanceLine(LostLines.Length, Phase.Arrival);
                    break;

                case Phase.Arrival:
                    var fall = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 2.5f));
                    _introBook.position = Vector3.Lerp(_bookTarget + Vector3.up * 5f, _bookTarget, fall) + Vector3.up * Mathf.Sin(Time.unscaledTime * 2f) * 0.08f;
                    _introBook.Rotate(Vector3.up, 40f * Time.unscaledDeltaTime, Space.World);
                    LookAtBook();
                    if (elapsed > 2.5f && (next || lineElapsed >= _lineSeconds))
                        AdvanceLine(ArrivalLines.Length, Phase.Pages);
                    break;

                case Phase.Pages:
                    _introBook.position = _bookTarget + Vector3.up * Mathf.Sin(Time.unscaledTime * 2f) * 0.08f;
                    FaceWitch();
                    LookAtBook();
                    if (next || lineElapsed >= _lineSeconds + 2f)
                        AdvanceLine(PageLines.Length, Phase.Shrink);
                    break;

                case Phase.Shrink:
                    var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 1.6f));
                    var destination = _companionVisual != null ? _companionVisual.position : _witch.position + Vector3.up;
                    _introBook.position = Vector3.Lerp(_shrinkFrom, destination, t);
                    _introBook.localScale = Vector3.Lerp(_bookScale, _bookScale * 0.15f, t);
                    _introBook.Rotate(Vector3.up, 360f * Time.unscaledDeltaTime, Space.World);
                    LookAtBook();
                    if (t >= 1f)
                        Finish();
                    break;
            }
        }

        private void AdvanceLine(int count, Phase nextPhase)
        {
            _line++;
            _lineStarted = Time.unscaledTime;
            if (_line >= count)
                Begin(nextPhase);
        }

        private void OrbitCamera(float elapsed)
        {
            if (_camera == null)
                return;

            var angle = 200f + elapsed * 12f;
            var offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 4.5f);
            var head = _witch.position + Vector3.up * 0.6f;
            _camera.position = head + offset + Vector3.up * 1.4f;
            _camera.LookAt(head);
        }

        private void LookAtBook()
        {
            if (_camera == null)
                return;

            var right = Vector3.Cross(Vector3.up, _witch.forward).normalized;
            var wanted = _witch.position - _witch.forward * 2.6f + right * 1.1f + Vector3.up * 1.3f;
            _camera.position = Vector3.Lerp(_camera.position, wanted, 1f - Mathf.Exp(-3f * Time.unscaledDeltaTime));
            var focus = Vector3.Lerp(_witch.position + Vector3.up * 0.6f, _introBook.position, 0.6f);
            _camera.rotation = Quaternion.Slerp(_camera.rotation, Quaternion.LookRotation(focus - _camera.position), 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
        }

        private void FaceWitch()
        {
            var toWitch = _witch.position - _introBook.position;
            toWitch.y = 0f;
            if (toWitch.sqrMagnitude > 0.001f)
                _introBook.rotation = Quaternion.Slerp(_introBook.rotation, Quaternion.LookRotation(toWitch), 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
        }

        private void SetBookOpen(bool open)
        {
            if (_closedBook != null)
                _closedBook.SetActive(!open);
            if (_openBook != null)
                _openBook.SetActive(open);
        }

        private void Finish()
        {
            _phase = Phase.Done;
            Time.timeScale = 1f;
            if (_introBook != null)
                _introBook.gameObject.SetActive(false);
            if (_companionVisual != null)
                _companionVisual.localScale = _companionScale;
            if (_cameraRig != null)
                _cameraRig.enabled = true;
        }

        private void OnGUI()
        {
            if (_phase == Phase.Done)
                return;

            EnsureStyles();

            if (_phase == Phase.Lost)
            {
                var previous = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.55f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _black);
                GUI.color = previous;
                Subtitle(LostLines[Mathf.Min(_line, LostLines.Length - 1)]);
            }
            else if (_phase == Phase.Arrival)
            {
                Subtitle(ArrivalLines[Mathf.Min(_line, ArrivalLines.Length - 1)]);
            }
            else if (_phase == Phase.Pages)
            {
                var width = Mathf.Min(640f, Screen.width - 40f);
                var rect = new Rect((Screen.width - width) / 2f, Screen.height - 250f, width, 180f);
                GUI.DrawTexture(rect, _paper);
                GUI.Label(new Rect(rect.x + 24f, rect.y + 16f, rect.width - 48f, rect.height - 46f), PageLines[Mathf.Min(_line, PageLines.Length - 1)], _pageStyle);
                GUI.Label(new Rect(rect.x, rect.yMax - 30f, rect.width, 24f), "Sayfa " + (Mathf.Min(_line, PageLines.Length - 1) + 1) + " / " + PageLines.Length, _hintStyle);
            }

            GUI.Label(new Rect(0f, Screen.height - 40f, Screen.width, 30f), "Space: devam · Esc: geç", _hintStyle);
        }

        private void Subtitle(string text)
        {
            var width = Mathf.Min(900f, Screen.width - 40f);
            GUI.Label(new Rect((Screen.width - width) / 2f, Screen.height - 180f, width, 110f), text, _subtitleStyle);
        }

        private void EnsureStyles()
        {
            if (_subtitleStyle != null)
                return;

            _subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _subtitleStyle.normal.textColor = new Color(1f, 0.95f, 0.8f);
            _pageStyle = new GUIStyle(GUI.skin.label) { fontSize = 21, alignment = TextAnchor.MiddleCenter, wordWrap = true, fontStyle = FontStyle.Italic };
            _pageStyle.normal.textColor = new Color(0.32f, 0.1f, 0.55f);
            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            _hintStyle.normal.textColor = new Color(0.6f, 0.55f, 0.5f);
            _black = new Texture2D(1, 1);
            _black.SetPixel(0, 0, Color.black);
            _black.Apply();
            _paper = new Texture2D(1, 1);
            _paper.SetPixel(0, 0, new Color(0.95f, 0.9f, 0.76f, 0.96f));
            _paper.Apply();
        }
    }
}
