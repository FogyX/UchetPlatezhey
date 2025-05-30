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
using System.Windows.Navigation;
using System.Windows.Shapes;
using УчётПлатежей.Helpers;

namespace УчётПлатежей.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditPaymentsPage.xaml
    /// </summary>
    public partial class AddEditPaymentsPage : Page
    {
        private readonly bool _isEditMode = false;
        private readonly платежи _payment;
        private readonly платежи _paymentCopy;

        public AddEditPaymentsPage(платежи payment = null)
        {
            InitializeComponent();
            _payment = payment ?? new платежи() { пользователи = AppState.CurrentUser};
            _isEditMode = payment != null;
            textTitle.Text = _isEditMode ? "Редактирование платежа" : "Добавление платежа";
            DataContext = _payment;
            _paymentCopy = new платежи()
            {
                id_пользователя = _payment.id_пользователя,
                дата_платежа = _payment.дата_платежа,
                id_продукта = _payment.id_продукта,
                количество = _payment.количество
            };
            cbProduct.ItemsSource = DatabaseContext.Context.продукты.ToList();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                _payment.дата_платежа = _paymentCopy.дата_платежа;
                _payment.id_продукта = _paymentCopy.id_продукта;
                _payment.количество = _paymentCopy.количество;
            }
            PageManager.MainFrame.Navigate(new MainPage());
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode)
            {
                DatabaseContext.Context.платежи.Add(_payment);
            }

            DatabaseContext.Context.SaveChanges();
            DatabaseContext.Context.Entry(_payment).Reload();
            PageManager.MainFrame.Navigate(new MainPage());
        }
    }
}
