using RecipeAppFinal.view;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RecipeAppFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ingredient> allIngredients;
        private List<double> allOriginalQuantities;
        private List<string> allOriginalUnits;
        private List<string> allSteps;
        private string recipeName;
        private static Dictionary<string, Recipe> Recipes = new Dictionary<string, Recipe>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            anotherRecipeLable.Visibility = Visibility.Collapsed;
        }

        private void btnAddRecipe_Click(object sender, RoutedEventArgs e)
        {
            AddRecipe addRecipe = new AddRecipe();
            addRecipe.ShowDialog();

            // Retrieve the ingredient details from the AddIngredient window

            allIngredients = addRecipe.ingredients;
            allOriginalQuantities = addRecipe.Quantities;
            allOriginalUnits = addRecipe.Units;
            allSteps = addRecipe.Steps;
            recipeName = addRecipe.recipeName;
            if (recipeName != null)
            {
                Recipe recipe = new Recipe(recipeName, allIngredients, allOriginalQuantities, allOriginalUnits, allSteps);

                // Check if the recipe name already exists in the dictionary

                if (!Recipes.ContainsKey(recipe.RecipeName))
                {
                    // Add the recipe to the dictionary
                    Recipes.Add(recipe.RecipeName, recipe);
                    DisplayDetails.Items.Clear();
                    LoadRecipeNames(); // Call the method that loads recipe names
                }
                else
                {
                    MessageBox.Show("A recipe with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            // Make the label visible after the function is done executing
            anotherRecipeLable.Visibility = Visibility.Visible;
        }

        private void btnDisplayRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (Recipes.Count == 0)
            {
                displayNoRecipe();
            }
            else
            {
                DisplayDetails.Items.Clear();
                List<string> sortedKeys = new List<string>(Recipes.Keys);
                sortedKeys.Sort();

                foreach (string key in sortedKeys)
                {
                    Recipe recipe = Recipes[key];

                    // Unsubscribe from the ExceededCalories event to prevent multiple subscriptions
                    recipe.ExceededCalories -= ExceededCaloriesHandler;

                    // Subscribe to the ExceededCalories event
                    recipe.ExceededCalories += ExceededCaloriesHandler;
                    DisplayDetails.Items.Add(recipe.DisplayRecipe());
                    recipe.notifier(); // Call Notifyer method which internally calculates total calories
                    DisplayDetails.Items.Add("-------------------------------------------------------" +
                        "-------------------------------------------------------------------------------");
                }
            }
        }

        private void btnDisplaySpecificRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (Recipes.Count == 0)
            {
                displayNoRecipe();
            }
            else
            {
                DisplayDetails.Items.Clear();
                string recipeName = txtSpecificRecipe.Text;
                if (Recipes.ContainsKey(recipeName))
                {
                    Recipe recipe = Recipes[recipeName];

                    // Display recipe details
                    DisplayDetails.Items.Add($"Recipe Name: {recipe.RecipeName}");
                    DisplayDetails.Items.Add($"Description: {recipe.GenerateDescription}");
                    DisplayDetails.Items.Add($"Ingredients:");
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        DisplayDetails.Items.Add($"- {ingredient.Name}: {ingredient.Quantity} {ingredient.UnitOfMeasurement}");
                    }
                    DisplayDetails.Items.Add($"Instructions:");
                    foreach (var step in recipe.Steps)
                    {
                        DisplayDetails.Items.Add($"- {step}");
                    }

                    // Calculate and display total calories
                    double totalCalories = recipe.TotalCalories();
                    DisplayDetails.Items.Add($"Total Calories: {totalCalories}");
                }
                else
                {
                    MessageBox.Show("Invalid recipe name. Please check the spelling of the recipe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExceededCaloriesHandler(string message, double totalCalories)
        {
            // Concatenate total calories to the message
            string messageWithCalories = $"\t* Total calories (Calories quantify food energy intake): {totalCalories}\n" + message + "\n";

            // Add the message with total calories to the lstDisplayDetails ListBox
            DisplayDetails.Items.Add(messageWithCalories);
        }

        private void btnScaleRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (Recipes.Count == 0)
            {
                displayNoRecipe();
            }
            else
            {
                RadioButton selectedRadioButton = sender as RadioButton;

                if (selectedRadioButton.IsChecked == true)
                {
                    double scalingNumber = 0;
                    if (selectedRadioButton == scaleByHalf)
                        scalingNumber = 0.5;
                    else if (selectedRadioButton == scaleByTwo)
                        scalingNumber = 2;
                    else if (selectedRadioButton == scaleByThree)
                        scalingNumber = 3;

                    scaleRecipeSpecificName(scalingNumber);
                }
            }
        }

        private void btnClearRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (Recipes.Count == 0)
            {
                displayNoRecipe();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all recipes? (y/n):", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                DisplayDetails.Items.Clear();

                if (result == MessageBoxResult.Yes)
                {
                    Recipes.Clear();
                    txtSpecificRecipe.Items.Clear();
                    txtSpecificRecipeToScale.Items.Clear();
                    txtResetRecipe.Items.Clear();
                    DisplayDetails.ItemsSource = null;
                    MessageBox.Show("All recipes have been cleared.", "Success", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("You canceled to clear the recipe.", "Canceled", MessageBoxButton.OK);
                }
            }
        }

        private void btnResetRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (Recipes.Count == 0)
            {
                displayNoRecipe();
            }
            else
            {
                string resetRecipeName = txtResetRecipe.Text;
                if (Recipes.ContainsKey(resetRecipeName))
                {
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to reset the {resetRecipeName} recipe?", "Confirmation", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        DisplayDetails.Items.Clear();
                        bool success = Recipes[resetRecipeName].ResetRecipe();
                        if (success)
                        {
                            DisplayDetails.Items.Add("The recipe has been reset. Here is the original recipe:");
                            DisplayDetails.Items.Add(Recipes[resetRecipeName].DisplayRecipe());
                            double totalCalories = Recipes[resetRecipeName].TotalCalories();
                            Recipes[resetRecipeName].notifier();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("There is no recipe with that name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnaddMenu_Click(object sender, RoutedEventArgs e)
        {
            if (Recipes.Count == 0)
            {
                displayNoRecipe();
            }
            else
            {
                RecipePieChart recipePieChart = new RecipePieChart(Recipes);
                recipePieChart.ShowDialog();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecipeNames();
        }

        private void LoadRecipeNames()
        {
            txtSpecificRecipe.Items.Clear();
            txtSpecificRecipeToScale.Items.Clear();
            txtResetRecipe.Items.Clear();
            foreach (var recipeName in Recipes.Keys)
            {
                txtSpecificRecipe.Items.Add(recipeName);
                txtSpecificRecipeToScale.Items.Add(recipeName);
                txtResetRecipe.Items.Add(recipeName);
            }
        }

        private void displayNoRecipe()
        {
            MessageBox.Show("There is no recipe in the list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void scaleRecipeSpecificName(double scalingNumber)
        {
            DisplayDetails.Items.Clear();
            string recipeName1 = txtSpecificRecipeToScale.Text;
            if (Recipes.ContainsKey(recipeName1))
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to scale the {recipeName1} recipe?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Recipes[recipeName1].ScaleRecipe(scalingNumber);
                    DisplayDetails.Items.Add("The recipe has been scaled. Here is the updated recipe:\n");
                    DisplayDetails.Items.Add(Recipes[recipeName1].DisplayRecipe());
                    double totalCalories = Recipes[recipeName1].TotalCalories();
                    Recipes[recipeName1].notifier();
                }
            }
            else
            {
                MessageBox.Show("There is no recipe with that name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New method for checking all other buttons
        private void btnCheckAllButtons_Click(object sender, RoutedEventArgs e)
        {
            // You can simply call each button's click event handler here
            // Example:
            btnAddRecipe_Click(sender, e);
            btnDisplayRecipe_Click(sender, e);
            btnDisplaySpecificRecipe_Click(sender, e);
            // Call other button click event handlers as needed
            // Remember to handle exceptions and errors appropriately
        }
    }
}
