namespace ChefMachine.Ingredients
{
    /// <summary>
    /// The preparation rules of the kitchen in one place: how an ingredient
    /// leaves the refrigerator, and what state a customer expects it in.
    /// </summary>
    public static class IngredientRules
    {
        /// <summary>State an ingredient has when taken from the refrigerator.</summary>
        public static IngredientState SpawnState(IngredientType type)
        {
            // Cheese needs no preparation, so it comes out ready to serve.
            return type == IngredientType.Cheese ? IngredientState.Ready : IngredientState.Raw;
        }

        /// <summary>State a customer order asks for.</summary>
        public static IngredientState ServeState(IngredientType type)
        {
            switch (type)
            {
                case IngredientType.Vegetable: return IngredientState.Chopped;
                case IngredientType.Cheese: return IngredientState.Ready;
                case IngredientType.Meat: return IngredientState.Cooked;
                default: return IngredientState.Raw;
            }
        }
    }
}
