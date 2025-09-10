using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows;

namespace RecipeAppFinal.view
{
 
    public partial class RecipePieChart : Window, INotifyPropertyChanged
    {
        public SeriesCollection MyFoodGroup { get; set; }
        private Dictionary<string, Recipe> recipes;
        private List<string> chosenRecipes = new List<string>();
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RecipePieChart(Dictionary<string, Recipe> recipes)
        {
            InitializeComponent();
            DataContext = this;
            this.recipes = recipes; 

            MyFoodGroup = new SeriesCollection();
        }

        private void btnAddToMenu(object sender, RoutedEventArgs e)
        {
            listMenuRecipe.Items.Clear();
            string selectedRecipe = cmbRecipeName.SelectedItem?.ToString();

            if (selectedRecipe != null && recipes.ContainsKey(selectedRecipe))
            {
                if (!chosenRecipes.Contains(selectedRecipe))
                {
                    chosenRecipes.Add(selectedRecipe);
                    Recipe recipe = recipes[selectedRecipe];

                    // Hook up the event handler only if it's not already subscribed
                    recipe.ExceededCalories -= Recipe_ExceededCalories; // Unsubscribe first to ensure no duplicate subscription
                    recipe.ExceededCalories += Recipe_ExceededCalories; // Subscribe to the event
                }

                // Separate loop for displaying the recipes
                foreach (string item in chosenRecipes)
                {
                    Recipe recipe = recipes[item];
                    listMenuRecipe.Items.Add(recipe.DisplayRecipe());
                    recipe.notifier();
                }
            }
            else
            {
                MessageBox.Show("Invalid recipe name. Please check the spelling of the recipe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler for the ExceededCalories event
        private void Recipe_ExceededCalories(string message, double totalCalories)
        {
            string fullMessage = $"\t* Total calories (Calories quantify food energy intake.): {totalCalories}\n {message}";
            listMenuRecipe.Items.Add(fullMessage);
        }

        // Event handler when the window is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string recipeName in recipes.Keys)
            {
                cmbRecipeName.Items.Add(recipeName);
            }
        }

        // Event handler for the "Create Pie Chart" button
        private void btnCreatePieChart(object sender, RoutedEventArgs e)
        {
            GeneratePieChart();
        }

        // Method to generate the PieChart based on chosen recipes
        private void GeneratePieChart()
        {
            // Initialize food group totals in a dictionary
            Dictionary<string, int> foodGroupCounts = new Dictionary<string, int>
            {
                {"Fruit", 0}, {"Vegetable", 0}, {"Grains", 0}, {"Proteins", 0},
                {"Dairy", 0}, {"Fats and Oils", 0}, {"Sugar and Sweets", 0}, {"Others", 0}
            };

            // Count each food group
            foreach (string recipeName in chosenRecipes)
            {
                Recipe recipe = recipes[recipeName];

                foreach (Ingredient ingredient in recipe.Ingredients)
                {
                    string foodGroup = ingredient.FoodGroup;

                    if (foodGroupCounts.ContainsKey(foodGroup))
                    {
                        foodGroupCounts[foodGroup]++;
                    }
                }
            }

            // Sum of all food groups
            int totalIngredients = foodGroupCounts.Values.Sum();

            // Create the PieSeries for each food group
            MyFoodGroup.Clear();

            foreach (string foodGroup in foodGroupCounts.Keys)
            {
                MyFoodGroup.Add(new PieSeries
                {
                    Title = foodGroup,
                    Values = new ChartValues<int> { foodGroupCounts[foodGroup] },
                    DataLabels = true,
                    LabelPoint = chartPoint =>
                    {
                        double percentage = chartPoint.Y / totalIngredients;
                        return string.Format("{0:P}", percentage);
                    }
                });
            }
        }
    }
}
