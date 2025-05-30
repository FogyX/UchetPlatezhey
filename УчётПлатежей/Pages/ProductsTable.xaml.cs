using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using УчётПлатежей.Helpers;

namespace УчётПлатежей.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsTable.xaml
    /// </summary>
    public partial class ProductsTable : Page, INotifyPropertyChanged
    {
        private категории _selectedCategory;
        private категории _nullCategory = new категории { название = "Все категории" };
        public категории SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            dgProducts.ItemsSource = DatabaseContext.Context.продукты
                .ToList()
                .Where(product => SelectedCategory == _nullCategory || product.категории == SelectedCategory )
                .Where(product => SearchText == null || product.наименование.ToLower().Contains(SearchText.ToLower()));
        }

        public ProductsTable()
        {
            InitializeComponent();
            dgProducts.ItemsSource = DatabaseContext.Context.продукты.ToList();
            var categories = DatabaseContext.Context.категории.ToList();
            categories.Insert(0, _nullCategory);
            cbFilterCategory.ItemsSource = categories;
            SelectedCategory = _nullCategory;
            DataContext = this;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageManager.MainFrame.Navigate(new MainPage());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            PageManager.MainFrame.Navigate(new AddEditProductsPage());
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            продукты selectedProduct = dgProducts.SelectedItems.Cast<продукты>().FirstOrDefault();

            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите продукт для редактирования");
                return;
            }    
            PageManager.MainFrame.Navigate(new AddEditProductsPage(selectedProduct));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedProducts = dgProducts.SelectedItems.Cast<продукты>().ToList();

            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Выберите продукты для удаления");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Вы точно хотите удалить {selectedProducts.Count} элементов?", "Внимание!", MessageBoxButton.YesNo);
            
            if (result == MessageBoxResult.No)
                return;

            DatabaseContext.Context.продукты.RemoveRange(selectedProducts);
            DatabaseContext.Context.SaveChanges();
            dgProducts.ItemsSource = null;
            dgProducts.ItemsSource = DatabaseContext.Context.продукты.ToList();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategory = _nullCategory;
            SearchText = null;
        }
    }
}
