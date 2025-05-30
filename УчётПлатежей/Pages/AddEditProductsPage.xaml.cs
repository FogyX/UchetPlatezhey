using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для AddEditProductsPage.xaml
    /// </summary>
    public partial class AddEditProductsPage : Page
    {
        private readonly bool _isEditMode = false;
        private readonly продукты _product;
        private readonly продукты _productCopy;

        public AddEditProductsPage(продукты product = null)
        {
            InitializeComponent();
            _product = product ?? new продукты();
            _isEditMode = product != null;
            textTitle.Text = _isEditMode ? "Редактирование продукта" : "Добавление продукта";
            DataContext = _product;
            _productCopy = new продукты()
            {
                наименование = _product.наименование,
                id_категории = _product.id_категории,
                цена = _product.цена,
                описание = _product.описание
            };
            cbCategory.ItemsSource = DatabaseContext.Context.категории.ToList();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                _product.наименование = _productCopy.наименование;
                _product.id_категории = _productCopy.id_категории;
                _product.цена = _productCopy.цена;
                _product.описание = _productCopy.описание;
            }
            PageManager.MainFrame.Navigate(new ProductsTable());
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode)
            {
                DatabaseContext.Context.продукты.Add(_product);
            }

            DatabaseContext.Context.SaveChanges();
            PageManager.MainFrame.Navigate(new ProductsTable());
        }
    }
}
