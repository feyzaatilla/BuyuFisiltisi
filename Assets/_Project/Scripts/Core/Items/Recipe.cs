using System;
using System.Collections.Generic;

namespace BuyuFisiltisi.Core.Items
{
    public sealed class Recipe
    {
        public const string LightTeaId = "isik_cayi";
        public const string SilenceSalveId = "sessizlik_merhemi";
        public const string WhisperEssenceId = "fisilti_ozu";

        public static readonly Recipe LightTea = new Recipe(LightTeaId, LightTeaId,
            new Ingredient(ItemCatalog.SilverHyacinth), new Ingredient(ItemCatalog.NightViolet));

        public static readonly Recipe SilenceSalve = new Recipe(SilenceSalveId, SilenceSalveId,
            new Ingredient(ItemCatalog.RedWhistleWeed), new Ingredient(ItemCatalog.MistFlower));

        public static readonly Recipe WhisperEssence = new Recipe(WhisperEssenceId, WhisperEssenceId,
            new Ingredient(ItemCatalog.RootPomegranate), new Ingredient(ItemCatalog.SilverHyacinth));

        public static readonly IReadOnlyList<Recipe> All = new[] { LightTea, SilenceSalve, WhisperEssence };

        public Recipe(string id, string productId, params Ingredient[] ingredients)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Tarif kimliği boş olamaz.", nameof(id));

            if (string.IsNullOrEmpty(productId))
                throw new ArgumentException("Ürün kimliği boş olamaz.", nameof(productId));

            if (ingredients == null || ingredients.Length == 0)
                throw new ArgumentException("Tarifte en az bir malzeme olmalı.", nameof(ingredients));

            Id = id;
            ProductId = productId;
            Ingredients = (Ingredient[])ingredients.Clone();
        }

        public string Id { get; }
        public string ProductId { get; }
        public IReadOnlyList<Ingredient> Ingredients { get; }
    }
}
