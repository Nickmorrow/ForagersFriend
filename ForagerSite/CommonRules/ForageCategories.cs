namespace ForagerSite.CommonRules
{
    public static class ForageCategories
    {
        public enum ForageCategory
        {
            Plants,
            Fungi,
            Fruits,
            NutsAndSeeds,
            HerbsAndMedicinals,
            Aquatic,
            AnimalDerived,
            Trees,
            UtilityAndCraft
        }

        public static readonly IReadOnlyDictionary<ForageCategory, IReadOnlyList<string>> Subcategories =
            new Dictionary<ForageCategory, IReadOnlyList<string>>
            {
                [ForageCategory.Plants] = new List<string>
                {
                "Leaves & Greens",
                "Flowers",
                "Shoots & Stems",
                "Roots, Tubers & Rhizomes",
                "Dye Plants"
                },

                [ForageCategory.Fungi] = new List<string>
                {
                "Edible Mushrooms",
                "Medicinal / Functional"
                },

                [ForageCategory.Fruits] = new List<string>
                {
                "Tree Fruits",
                "Berries",
                "Vines & Ground Fruits"
                },

                [ForageCategory.NutsAndSeeds] = new List<string>
                {
                "Tree Nuts",
                "Seeds & Grains"
                },

                [ForageCategory.HerbsAndMedicinals] = new List<string>
                {
                "Culinary Herbs",
                "Medicinal Plants"
                },

                [ForageCategory.Aquatic] = new List<string>
                {
                "Aquatic Plants",
                "Shellfish & Crustaceans"
                },

                [ForageCategory.AnimalDerived] = new List<string>
                {
                "Honey & Resins",
                "Insects"
                },

                [ForageCategory.Trees] = new List<string>
                {
                "Sap & Syrups",
                "Needles, Tips & Bark",
                "Inner Bark & Cambium"
                },

                [ForageCategory.UtilityAndCraft] = new List<string>
                {
                "Utility & Craft Materials"
                }

            };

        public static IReadOnlyList<string> GetSubcategories(ForageCategory category)
            => Subcategories.TryGetValue(category, out var subs) ? subs : Array.Empty<string>();
    }
}
