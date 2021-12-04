using System.Collections.Generic;
using System.Linq;

namespace Bottleneck.Util
{
    public static class ItemUtil
    {
        private static Dictionary<int, List<int>> _itemPrecursorCache = new();
        private static Dictionary<int, List<int>> _itemSuccessorCache = new();

        public static List<int> DirectPrecursorItems(int itemId)
        {
            if (_itemPrecursorCache.ContainsKey(itemId))
            {
                return _itemPrecursorCache[itemId];
            }

            RecipeProto[] allRecipes = LDB.recipes.dataArray;

            var result = new List<int>();
            for (int index = 0; index < allRecipes.Length; ++index)
            {
                var recipeProto = allRecipes[index];
                if (recipeProto.Results.ToList().Contains(itemId))
                {
                    foreach (var precursor in recipeProto.Items)
                    {
                        if (precursor != itemId)
                            result.Add(precursor);
                    }
                }
            }

            _itemPrecursorCache.Add(itemId, result);
            return result;
        }

        public static List<int> DirectSuccessorItems(int itemId)
        {
            if (_itemSuccessorCache.ContainsKey(itemId))
            {
                return _itemSuccessorCache[itemId];
            }

            RecipeProto[] allRecipes = LDB.recipes.dataArray;

            var result = new List<int>();
            for (int index = 0; index < allRecipes.Length; ++index)
            {
                var recipeProto = allRecipes[index];
                if (recipeProto.Items.Contains(itemId))
                {
                    foreach (var successor in recipeProto.Results)
                    {
                        if (successor != itemId)
                            result.Add(successor);
                    }
                }
            }

            _itemSuccessorCache[itemId] = result;
            return result;
        }

        public static bool HasPrecursors(int productId)
        {
            return DirectPrecursorItems(productId).Count > 0;
        }

        public static bool HasConsumers(int productId)
        {
            return DirectSuccessorItems(productId).Count > 0;
        }
    }
}