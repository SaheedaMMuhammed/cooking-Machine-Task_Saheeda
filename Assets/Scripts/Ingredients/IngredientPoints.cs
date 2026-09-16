namespace ChefMachine.Ingredients
{
    /// <summary>
    /// Single source of truth for ingredient score values, so no gameplay
    /// script needs to hard-code point numbers.
    /// </summary>
    public static class IngredientPoints
    {
        public const int Vegetable = 20;
        public const int Cheese = 10;
        public const int Meat = 30;

        public static int For(IngredientType type)
        {
            switch (type)
            {
                case IngredientType.Vegetable: return Vegetable;
                case IngredientType.Cheese: return Cheese;
                case IngredientType.Meat: return Meat;
                default: return 0;
            }
        }
    }
}
