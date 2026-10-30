using System.Collections.Generic;
using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Interaction;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Book
{
    public sealed class SpellBookView : MonoBehaviour, ICompanionCommandIssuer, IRequiresEventBus
    {
        private static readonly string[] Pages = { "Emirler", "Fısıltı" };

        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private MonoBehaviour _switchTarget;
        [SerializeField] private MonoBehaviour _fireflyTarget;
        [SerializeField] private CollectibleFlower[] _fetchFlowers = new CollectibleFlower[0];

        private readonly List<string> _deeds = new List<string>();
        private ICompanionCommandReceiver _receiver;
        private IEventBus _eventBus;
        private bool _open;
        private int _page;
        private string _note = string.Empty;
        private GUIStyle _titleStyle;
        private GUIStyle _textStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _tabStyle;
        private Texture2D _cover;
        private Texture2D _paper;

        public bool IsOpen => _open;

        public void SetCommandReceiver(ICompanionCommandReceiver receiver)
        {
            _receiver = receiver;
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            if (_eventBus != null)
                _eventBus.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);

            _eventBus = eventBus;
            if (_eventBus == null)
                return;

            _eventBus.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
                _eventBus.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDeedRecorded(DeedRecordedEvent deed)
        {
            if (!_deeds.Contains(deed.Description))
                _deeds.Add(deed.Description);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                _open = !_open;
                _note = string.Empty;
            }
            else if (_open && keyboard.escapeKey.wasPressedThisFrame)
            {
                _open = false;
            }
        }

        private void OnGUI()
        {
            if (!_open)
            {
                if (Time.timeScale > 0f)
                    DrawHint();
                return;
            }

            EnsureStyles();

            var width = Mathf.Min(820f, Screen.width - 40f);
            var height = Mathf.Min(520f, Screen.height - 80f);
            var x = (Screen.width - width) / 2f;
            var y = (Screen.height - height) / 2f;

            GUI.DrawTexture(new Rect(x - 14f, y - 14f, width + 28f, height + 28f), _cover);
            GUI.DrawTexture(new Rect(x, y, width, height), _paper);

            GUI.Label(new Rect(x, y + 10f, width, 40f), "Büyü Kitabı · " + Pages[_page], _titleStyle);

            var tabWidth = (width - 40f) / Pages.Length;
            for (var i = 0; i < Pages.Length; i++)
            {
                var rect = new Rect(x + 20f + i * tabWidth, y + 56f, tabWidth - 8f, 34f);
                if (GUI.Toggle(rect, _page == i, Pages[i], _tabStyle) && _page != i)
                {
                    _page = i;
                    _note = string.Empty;
                }
            }

            var area = new Rect(x + 30f, y + 104f, width - 60f, height - 150f);
            switch (_page)
            {
                case 0: DrawCommands(area); break;
                default: DrawWhisper(area); break;
            }

            GUI.Label(new Rect(x + 30f, y + height - 44f, width - 60f, 30f), _note, _textStyle);
        }

        private void DrawHint()
        {
            if (_textStyle == null)
                EnsureStyles();

            GUI.Label(new Rect(Screen.width - 260f, 40f, 240f, 26f), "Tab · Büyü Kitabı", _textStyle);
        }

        private void DrawCommands(Rect area)
        {
            var y = area.y;
            if (Button(area, ref y, "Yanıma gel"))
                Send(CompanionCommand.FollowPlayer(), "Fısıl: Geliyorum, geliyorum...");

            if (Button(area, ref y, "Bekle"))
                Send(CompanionCommand.Wait(), "Fısıl: Burada bekliyorum. Ama uzun sürmesin.");

            if (Button(area, ref y, "Şalteri çek") && _switchTarget is IInteractable lever)
                Send(CompanionCommand.InteractWith(lever), "Fısıl: Yarıktan ben geçerim, sen bekle.");

            if (Button(area, ref y, "Çiçeği getir"))
            {
                var flower = NextFlower();
                if (flower != null)
                    Send(CompanionCommand.FetchItem(flower), "Fısıl: " + flower.DisplayName + " mi? Hemen.");
                else
                    _note = "Getirilecek çiçek kalmadı.";
            }

            if (Button(area, ref y, "Ateş böceğine çay götür") && _fireflyTarget is IInteractable firefly)
                Send(CompanionCommand.InteractWith(firefly), "Fısıl: Çayı ben götürürüm, dökme sakın.");
        }

        private void DrawWhisper(Rect area)
        {
            var text = "Yaşlı Meşe'nin yanına git ve E'ye bas. Ona içinden geldiği gibi yaz.\n" +
                       "Meşe rüşvetten nefret eder; ona yaptığın iyilikleri dürüstçe anlat.";
            GUI.Label(new Rect(area.x, area.y, area.width, 70f), text, _textStyle);

            var y = area.y + 84f;
            GUI.Label(new Rect(area.x, y, area.width, 28f), "Ormanın kaydettiği iyiliklerin:", _textStyle);
            y += 32f;

            if (_deeds.Count == 0)
            {
                GUI.Label(new Rect(area.x, y, area.width, 28f), "Henüz yok.", _textStyle);
                return;
            }

            foreach (var deed in _deeds)
            {
                GUI.Label(new Rect(area.x, y, area.width, 28f), "• " + deed, _textStyle);
                y += 28f;
            }
        }

        private bool Button(Rect area, ref float y, string label)
        {
            var pressed = GUI.Button(new Rect(area.x, y, Mathf.Min(360f, area.width), 40f), label, _buttonStyle);
            y += 50f;
            return pressed;
        }

        private CollectibleFlower NextFlower()
        {
            foreach (var flower in _fetchFlowers)
            {
                if (flower != null && flower.gameObject.activeInHierarchy && flower.CanInteract(gameObject))
                    return flower;
            }

            return null;
        }

        private void Send(CompanionCommand command, string note)
        {
            if (_receiver == null)
            {
                _note = "Fısıl seni duymuyor.";
                return;
            }

            _receiver.IssueCommand(command);
            _note = note;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            var ink = new Color(0.32f, 0.1f, 0.55f);
            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleStyle.normal.textColor = ink;
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            _textStyle.normal.textColor = ink;
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            _tabStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            _cover = Solid(new Color(0.35f, 0.08f, 0.1f));
            _paper = Solid(new Color(0.95f, 0.9f, 0.76f));
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
