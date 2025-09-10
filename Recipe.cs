using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipeAppFinal
{
    public class Recipe
    {
        public delegate void ExceededCaloriesEventHandler(string message, double totalCalories);
        public event ExceededCaloriesEventHandler ExceededCalories;

        public string RecipeName { get; set; }
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        private List<double> OriginalQuantities { get; set; } = new List<double>();
        private List<string> OriginalUnits { get; set; } = new List<string>();

        public List<string> Steps { get; set; } = new List<string>();

        public Recipe() { }

        public Recipe(string recipeName, List<Ingredient> ingredients, List<double> quantities, List<string> units, List<string> steps)
        {
            RecipeName = recipeName;
            Ingredients = ingredients;
            OriginalQuantities = quantities;
            OriginalUnits = units;
            Steps = steps;

            for (int i = 0; i < Ingredients.Count; i++)
            {
                Ingredients[i].Calories = ingredients[i].Calories;
            }
        }

        public string DisplayRecipe()
        {
            if (Ingredients.Count == 0)
            {
                return "No ingredients have been added.";
            }
            else
            {
                StringBuilder info = new StringBuilder();
                info.Append("Ingredients:");

                foreach (Ingredient ingredient in Ingredients.OrderBy(i => i.Name))
                {
                    info.AppendLine($"\t{ingredient.Quantity} {ingredient.UnitOfMeasurement} - {ingredient.Name} (Food Group: {ingredient.FoodGroup})");
                }

                info.AppendLine($"\nSteps By Step to make {RecipeName}: ");
                for (int i = 0; i < Steps.Count; i++)
                {
                    info.AppendLine($"\t{i + 1}. {Steps[i]}");
                }

                info.AppendLine("\nAdditional Information:");
                return info.ToString();
            }
        }

        public void ScaleRecipe(double scalingNumber)
        {
            for (int i = 0; i < Ingredients.Count; i++)
            {
                double quantity = Ingredients[i].Quantity * scalingNumber;
                string unitOfMeasurement = Ingredients[i].UnitOfMeasurement;

                if (unitOfMeasurement == "tablespoon" || unitOfMeasurement == "tablespoon")
                {
                    if (quantity >= 16)
                    {
                        quantity /= 16;
                        quantity = Math.Round(quantity, 1);
                        unitOfMeasurement = "cup";
                    }
                }

                Ingredients[i].Quantity = quantity;
                Ingredients[i].UnitOfMeasurement = unitOfMeasurement;
                Ingredients[i].Calories *= scalingNumber;
            }
        }

        public bool ResetRecipe()
        {
            if (Ingredients.Count == 0)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < Ingredients.Count; i++)
                {
                    Ingredients[i].Quantity = OriginalQuantities[i];
                    Ingredients[i].UnitOfMeasurement = OriginalUnits[i];
                }
                return true;
            }
        }

        public double TotalCalories()
        {
            return Ingredients.Sum(ingredient => ingredient.Calories);
        }

        public void notifier()
        {
            double totalCalories = TotalCalories();
            if (totalCalories > 300)
            {
                ExceededCalories?.Invoke($"\t* Alert!! Total calories for {RecipeName} exceeded 300! The recipe contains high Calories", totalCalories);
            }
        }

        public string GenerateDescription()
        {
            StringBuilder description = new StringBuilder($"This recipe for {RecipeName} uses");

            for (int i = 0; i < Ingredients.Count; i++)
            {
                Ingredient ingredient = Ingredients[i];
                description.Append($" {ingredient.Quantity} {ingredient.UnitOfMeasurement} of {ingredient.Name}");

                if (i < Ingredients.Count - 2)
                {
                    description.Append(", ");
                }
                else if (i == Ingredients.Count - 2)
                {
                    description.Append(" and ");
                }
            }

            description.Append(" to create a delightful preparation.");
            return description.ToString();
        }
    }
}
