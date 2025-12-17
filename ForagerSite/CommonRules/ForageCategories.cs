namespace ForagerSite.CommonRules
{
    namespace ForagerSite.CommonRules
    {
        public static class ForageCategories
        {
            public static readonly List<string> ForageCategory = new()
            {
                "Plants",
                "Fungi",
                "Fruits",
                "Nuts and Seeds",
                "Aquatic",
                "Animal Derived",
                "Trees",
                "Utility and Craft"
            };


            public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Subcategories =
                new Dictionary<string, IReadOnlyList<string>>
                {
                    ["Plants"] = new[]
                    {
                        "Leaves & Greens",
                        "Flowers",
                        "Shoots & Stems",
                        "Roots Tubers & Rhizomes",
                        "Culinary Herbs",
                        "Medicinal Plants",
                    },

                    ["Fungi"] = new[]
                    {
                        "Edible Mushrooms",
                        "Medicinal / Functional"
                    },

                    ["Fruits"] = new[]
                    {
                        "Tree Fruits",
                        "Berries",
                        "Vines & Ground Fruits"
                    },

                    ["Nuts and Seeds"] = new[]
                    {
                        "Tree Nuts",
                        "Seeds & Grains"
                    },

                    ["Aquatic"] = new[]
                    {
                        "Aquatic Plants",
                        "Shellfish & Crustaceans"
                    },

                    ["Animal Derived"] = new[]
                    {
                        "Honey & Resins",
                        "Insects"
                    },

                    ["Trees"] = new[]
                    {
                        "Sap & Syrups",
                        "Needles, Tips & Bark",
                        "Inner Bark & Cambium"
                    },

                    ["Utility and Craft"] = new[]
                    {
                        "Utility & Craft Materials"
                    }
                };

        }
    }

}
