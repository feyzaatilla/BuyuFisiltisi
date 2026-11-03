using System.Collections.Generic;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class OakMoodTint : MonoBehaviour, IRequiresEventBus
    {
        public const string TargetName = "YasliMese_Gorunum";
        public const string LeafMaterialName = "leafs";

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private float _blendSpeed = 1.5f;

        private readonly List<LeafSlot> _leaves = new List<LeafSlot>();
        private readonly List<Light> _lights = new List<Light>();
        private readonly List<Color> _lightColors = new List<Color>();
        private MaterialPropertyBlock _block;
        private IEventBus _bus;
        private int _trust;
        private float _shown;
        private float _target;

        public int Trust => _trust;

        public void InjectEventBus(IEventBus eventBus)
        {
            _bus?.Unsubscribe<TrustChangedEvent>(OnTrustChanged);
            _bus = eventBus;
            _bus?.Subscribe<TrustChangedEvent>(OnTrustChanged);
        }

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null || !material.name.ToLowerInvariant().Contains(LeafMaterialName))
                        continue;

                    _leaves.Add(new LeafSlot(renderer, i, ReadColor(material)));
                }
            }

            foreach (var light in GetComponentsInChildren<Light>(true))
            {
                _lights.Add(light);
                _lightColors.Add(light.color);
            }
        }

        private void OnDestroy()
        {
            _bus?.Unsubscribe<TrustChangedEvent>(OnTrustChanged);
        }

        private void OnTrustChanged(TrustChangedEvent changed)
        {
            _trust = changed.CurrentTrust;
            _target = OakMood.Mood(_trust);
        }

        private void Update()
        {
            if (Mathf.Approximately(_shown, _target))
                return;

            _shown = Mathf.MoveTowards(_shown, _target, _blendSpeed * Time.unscaledDeltaTime);
            Paint(_shown);
        }

        private void Paint(float mood)
        {
            foreach (var leaf in _leaves)
            {
                if (leaf.Renderer == null)
                    continue;

                var color = OakMood.Blend(leaf.Original, OakMood.WitheredLeaves, OakMood.BloomingLeaves, mood);
                leaf.Renderer.GetPropertyBlock(_block, leaf.Index);
                _block.SetColor(BaseColorId, color);
                _block.SetColor(ColorId, color);
                leaf.Renderer.SetPropertyBlock(_block, leaf.Index);
            }

            for (var i = 0; i < _lights.Count; i++)
            {
                if (_lights[i] != null)
                    _lights[i].color = OakMood.Blend(_lightColors[i], OakMood.ColdLight, OakMood.WarmLight, mood);
            }
        }

        private static Color ReadColor(Material material)
        {
            if (material.HasProperty(BaseColorId))
                return material.GetColor(BaseColorId);
            if (material.HasProperty(ColorId))
                return material.GetColor(ColorId);
            return Color.white;
        }

        private readonly struct LeafSlot
        {
            public LeafSlot(Renderer renderer, int index, Color original)
            {
                Renderer = renderer;
                Index = index;
                Original = original;
            }

            public Renderer Renderer { get; }
            public int Index { get; }
            public Color Original { get; }
        }
    }
}
