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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private категории _selectedCategory;
        private категории _nullCategory = new категории() { название = "Все категории" };

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set 
            { 
                if (_dateFrom != value)
                {
                    _dateFrom = value;
                    OnPropertyChanged(nameof(DateFrom));
                }
            }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set
            {
                if (_dateTo != value)
                {
                    _dateTo = value;
                    OnPropertyChanged(nameof(DateTo));
                }
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            dgPayments.ItemsSource = GetPayments()
                .Where(payment => DateFrom == null || payment.дата_платежа >= DateFrom)
                .Where(payment => DateTo == null || payment.дата_платежа <= DateTo)
                .Where(payment => SelectedCategory == _nullCategory || payment.продукты.категории == SelectedCategory);
        }

        public MainPage()
        {
            InitializeComponent();

            пользователи user = AppState.CurrentUser;

            if (user == null)
            {
                PageManager.MainFrame.Navigate(new Authentication());
                return;
            }

            dgPayments.ItemsSource = GetPayments();
            var categories = DatabaseContext.Context.категории.ToList();
            categories.Insert(0, _nullCategory);
            cbFilterCategory.ItemsSource = categories;
            SelectedCategory = _nullCategory;
            DataContext = this;
        }

        private IEnumerable<платежи> GetPayments()
        {
            return DatabaseContext.Context.платежи
                .ToList()
                .Where(payment => payment.пользователи == AppState.CurrentUser);
        }

        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            PageManager.MainFrame.Navigate(new ProductsTable());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            PageManager.MainFrame.Navigate(new AddEditPaymentsPage());
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            платежи selectedPayment = dgPayments.SelectedItems.Cast<платежи>().FirstOrDefault();

            if (selectedPayment == null)
            {
                MessageBox.Show("Выберите платеж для редактирования");
                return;
            }

            PageManager.MainFrame.Navigate(new AddEditPaymentsPage(selectedPayment));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayments = dgPayments.SelectedItems.Cast<платежи>().ToList();

            if (selectedPayments.Count == 0)
            {
                MessageBox.Show("Выберите платежи для удаления");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Вы точно хотите удалить {selectedPayments.Count} элементов?", "Внимание!", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            DatabaseContext.Context.платежи.RemoveRange(selectedPayments);
            DatabaseContext.Context.SaveChanges();
            dgPayments.ItemsSource = null;
            dgPayments.ItemsSource = DatabaseContext.Context.платежи
                .ToList()
                .Where(payment => payment.пользователи == AppState.CurrentUser);
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            DateFrom = null;
            DateTo = null;
            SelectedCategory = _nullCategory;
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            ReportPrinter.PrintReport(dgPayments.ItemsSource.Cast<платежи>(), DateFrom, DateTo, SelectedCategory == _nullCategory ? null : SelectedCategory);
        }
    }
}
