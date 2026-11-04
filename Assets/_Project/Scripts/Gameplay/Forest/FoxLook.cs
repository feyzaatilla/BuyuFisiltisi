using System.Collections.Generic;
using BuyuFisiltisi.Core.Creatures;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    [RequireComponent(typeof(ShadowFox))]
    public sealed class FoxLook : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Animator _animator;
        [SerializeField] private float _fadeTime = 0.2f;
        [SerializeField] private float _blendSpeed = 1.2f;

        private readonly List<FurSlot> _slots = new List<FurSlot>();
        private MaterialPropertyBlock _block;
        private ShadowFox _fox;
        private string _playing;
        private float _shown = -1f;

        private void Awake()
        {
            _fox = GetComponent<ShadowFox>();
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            _block = new MaterialPropertyBlock();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                        _slots.Add(new FurSlot(renderer, i, ReadColor(materials[i])));
                }
            }
        }

        private void Update()
        {
            var state = _fox.State;
            var clip = FoxAnimation.StateFor(state);
            if (_animator != null && _animator.runtimeAnimatorController != null && clip != _playing)
            {
                _playing = clip;
                _animator.CrossFadeInFixedTime(clip, _fadeTime);
            }

            var target = state == FoxState.Calm ? 1f : 0f;
            if (Mathf.Approximately(_shown, target))
                return;

            _shown = _shown < 0f ? target : Mathf.MoveTowards(_shown, target, _blendSpeed * Time.deltaTime);
            Paint(_shown);
        }

        private void Paint(float calm)
        {
            foreach (var slot in _slots)
            {
                if (slot.Renderer == null)
                    continue;

                var color = FoxAnimation.Tint(slot.Original, calm);
                slot.Renderer.GetPropertyBlock(_block, slot.Index);
                _block.SetColor(BaseColorId, color);
                _block.SetColor(ColorId, color);
                slot.Renderer.SetPropertyBlock(_block, slot.Index);
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

        private readonly struct FurSlot
        {
            public FurSlot(Renderer renderer, int index, Color original)
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
