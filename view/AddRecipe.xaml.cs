using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RecipeAppFinal.view
{
    /// <summary>
    /// Interaction logic for AddRecipe.xaml
    /// </summary>
    public partial class AddRecipe : Window
    {
        public List<Ingredient> ingredients { get; }
        public List<double> Quantities { get; }
        public List<string> Units { get; }
        public List<string> Steps { get; }
        public string recipeName { get; set; }

        int ingredientCount = 0;
        int stepsCount = 0;
        public AddRecipe()
        {
            InitializeComponent();

            ingredients = new List<Ingredient>();
            Quantities = new List<double>();
            Units = new List<string>();
            Steps = new List<string>();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbFoodGroup.Items.Add("Fruit");
            cmbFoodGroup.Items.Add("Vegetable");
            cmbFoodGroup.Items.Add("Grains");
            cmbFoodGroup.Items.Add("Proteins");
            cmbFoodGroup.Items.Add("Dairy");
            cmbFoodGroup.Items.Add("Fats and Oils");
            cmbFoodGroup.Items.Add("Sugar and Sweets");
            cmbFoodGroup.Items.Add("Others");
        }
        private void Button_Ingredient(object sender, RoutedEventArgs e)
        {
            recipeName = string.Empty;
            string name = string.Empty;
            double quantity = 0.0;
            string unitOfM = string.Empty;
            double calories = 0.0;
            string foodGroup = string.Empty;

            recipeName = txtRecipeName.Text;
            Recipe recipe = new Recipe();

            if (recipeName.Length > 0)
            {
                ingredientCount++;

                try
                {
                    name = txtIngredientName.Text;

                    if (!double.TryParse(txtQuantity.Text, out quantity))
                    {
                        ingredientCount--;
                        throw new FormatException("Invalid quantity value. Please enter a numberic value.");
                    }
                    Quantities.Add(quantity);

                    unitOfM = txtUnitOfMeasurement.Text;
                    Units.Add(unitOfM);

                    if (unitOfM == "tablespoon" || unitOfM == "tablespoons")
                    {
                        if (quantity >= 16)
                        {
                            quantity /= 16;
                            quantity = Math.Round(quantity, 1);
                            unitOfM = "cup";
                        }
                    }
                    if (!double.TryParse(txtCalories.Text, out calories))
                    {
                        ingredientCount--;
                        throw new FormatException("Invalid calories value. Please enter a numeric value");
                    }
                    foodGroup = cmbFoodGroup.SelectedItem?.ToString();

                    if (string.IsNullOrEmpty(foodGroup))
                    {
                        ingredientCount--;
                        throw new Exception("Please select a food group");
                    }
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(ex.Message, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                ingredients.Add(new Ingredient { Name = name, Quantity = quantity, UnitOfMeasurement = unitOfM, Calories = calories, FoodGroup = foodGroup });

                recipe = new Recipe(recipeName, ingredients, Quantities, Units, Steps);

                DisplayInput.Items.Add($"Ingredient Number: {ingredientCount} for {recipeName}\n\tIngredient: {name} \n\tQuantity: {quantity}\n\tUnit Of Measurement {unitOfM}\n\tCalories: {calories}\n\tFood Group: {foodGroup}");

                recipe.ExceededCalories += (message, totalCalories) =>
                {
                    if (totalCalories > 0)
                    {
                        DisplayInput.Items.Add(message);
                        DisplayInput.Items.Add($"Total calories: {totalCalories}");
                    }
                    else
                    {
                        DisplayInput.Items.Add(message);
                        DisplayInput.Items.Add($"Total Calories: {totalCalories}");
                    }
                };
                txtIngredientName.Clear();
                txtQuantity.Clear();
                txtUnitOfMeasurement.Clear();
                txtCalories.Clear();
            }
            else
            {
                MessageBox.Show("Please enter recipe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ingredientCount = 0;
            }
        }
        private void Button_Step(object sender, RoutedEventArgs e)
        {
            string strSteps = "";

            if (ingredientCount == 0)
            {
                MessageBox.Show("Please add Ingredient First Before you add step.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                stepsCount = 0;
            }
            else
            {
                strSteps = txtList.Text;

                if (strSteps.Length == 0)
                {
                    MessageBox.Show("Please add steps.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    stepsCount = 0;
                }
                else
                {
                    stepsCount++;
                    Steps.Add(strSteps);
                    DisplayInput.Items.Add($"Step {stepsCount}:\n{strSteps}");
                    txtList.Clear();
                    txtList.Focus();
                }
            }
        }
        private void Button_AddRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (ingredients.Count == 0)
            {
                MessageBox.Show("Please add Recipe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // Calculate total calories of the recipe
                double totalCalories = 0;
                foreach (var ingredient in ingredients)
                {
                    totalCalories += ingredient.Calories;
                }

                // Check if total calories are greater than 300 and show MessageBox if true
                if (totalCalories > 300)
                {
                    MessageBox.Show($"Recipe added Successfully with high calories! Total Calories: {totalCalories}. This is a high-calorie recipe. You might want to consider a lighter option.", "High Calories Alert.", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {

                    MessageBox.Show("Recipe added Successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                this.Close();
            }
        }
    }
}
