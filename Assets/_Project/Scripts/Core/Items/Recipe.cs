using System;

namespace BuyuFisiltisi.Core.Items
{
    public sealed class Recipe
    {
        public const string LightTeaId = "isik_cayi";

        public static readonly Recipe LightTea = new Recipe(LightTeaId, "isik_cicegi", 3, LightTeaId);

        public Recipe(string id, string ingredientId, int ingredientAmount, string productId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Tarif kimliği boş olamaz.", nameof(id));

            if (string.IsNullOrEmpty(ingredientId) || string.IsNullOrEmpty(productId))
                throw new ArgumentException("Malzeme ve ürün kimliği boş olamaz.");

            if (ingredientAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(ingredientAmount), "Malzeme miktarı en az 1 olmalı.");

            Id = id;
            IngredientId = ingredientId;
            IngredientAmount = ingredientAmount;
            ProductId = productId;
        }

        public string Id { get; }
        public string IngredientId { get; }
        public int IngredientAmount { get; }
        public string ProductId { get; }
    }
}