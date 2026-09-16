using UnityEngine;

namespace ChefMachine.Ingredients
{
    /// <summary>
    /// Inspector entry that says how one ingredient type looks when it is spawned.
    /// Stations keep a small list of these instead of hard-coding materials.
    /// </summary>
    [System.Serializable]
    public class IngredientVisual
    {
        public IngredientType Type = IngredientType.Cheese;
        public Material Material;
        public Vector3 Scale = new Vector3(0.35f, 0.35f, 0.35f);

        public static IngredientVisual Find(System.Collections.Generic.List<IngredientVisual> visuals, IngredientType type)
        {
            if (visuals == null) return null;

            for (int i = 0; i < visuals.Count; i++)
            {
                if (visuals[i] != null && visuals[i].Type == type) return visuals[i];
            }
            return null;
        }
    }
}
