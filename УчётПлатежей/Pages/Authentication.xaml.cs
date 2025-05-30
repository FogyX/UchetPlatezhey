using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using УчётПлатежей.Helpers;

namespace УчётПлатежей.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authentication.xaml
    /// </summary>
    public partial class Authentication : Page, INotifyPropertyChanged
    {
        private readonly AuthenticationLockoutState _lockoutState;
        private const int ATTEMPTS_BEFORE_CAPTCHA = 1;
        private const int ATTEMPTS_BEFORE_LOCKOUT = 3;
        private const int LOCKOUT_SECONDS = 30;
        private Window _parentWindow;

        private bool _isCaptchaVisible = false;
        public bool IsCaptchaVisible
        {
            get => _isCaptchaVisible;
            set
            {
                if (IsCaptchaVisible != value)
                {
                    _isCaptchaVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCaptchaVisible)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Authentication()
        {
            InitializeComponent();
            DataContext = this;
            _lockoutState = AuthenticationLockoutState.Load();
            CheckLockoutState();
        }

        private void CheckLockoutState()
        {
            if (_lockoutState.IsLockedOut == false)
                return;

            if (_lockoutState.LockedOutUntil < DateTime.Now)
                return;
            
            IsCaptchaVisible = true;
            UpdateCaptcha();
            SetLockoutTimer();
        }
        private void SetLockoutTimer()
        {
            textTimer.Visibility = Visibility.Visible;
            btnLogin.IsEnabled = false;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            void TimerHandler(object sender, EventArgs e)
            {
                TimeSpan remaining = _lockoutState.LockedOutUntil - DateTime.Now;
                if (remaining.TotalSeconds <= 0)
                {
                    timer.Tick -= TimerHandler;
                    timer.Stop();
                    textTimer.Visibility = Visibility.Collapsed;
                    btnLogin.IsEnabled = true;
                }
                textTimer.Text = $"Слишком много попыток входа. Повторите через {(int)remaining.TotalSeconds} секунд";

            }

            TimerHandler(null, null);
            timer.Tick += TimerHandler;
            timer.Start();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text;
            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин");
                return;
            }

            string password = tbPassword.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            if (IsCaptchaVisible)
            {
                if (tbCaptcha.Text != textCaptcha.Text)
                {
                    MessageBox.Show("Неверная капча");
                    ProcessFail();
                    return;
                }
            }

            пользователи user = DatabaseContext.Context.пользователи.FirstOrDefault(u => u.логин == login);
            if (user == null || user.пароль != PasswordHasher.GetPasswordHash(password))
            { 
                MessageBox.Show("Неверный логин или пароль");
                ProcessFail();
                return;
            }

            _lockoutState.ResetLockout();
            AppState.CurrentUser = user;
            PageManager.MainFrame.Navigate(new MainPage());
        }

        private void ProcessFail()
        {
            _lockoutState.IncrementFailedLoginAttempts();
            ShowWarningText();

            if (_lockoutState.FailedLoginAttempts >= ATTEMPTS_BEFORE_CAPTCHA)
            {
                IsCaptchaVisible = true;
                UpdateCaptcha();
            }

            if (_lockoutState.FailedLoginAttempts >= ATTEMPTS_BEFORE_LOCKOUT)
            {
                _lockoutState.LockOut(LOCKOUT_SECONDS);
                SetLockoutTimer();
            }
        }

        private void ShowWarningText()
        {
            int attemptsLeft = ATTEMPTS_BEFORE_LOCKOUT - _lockoutState.FailedLoginAttempts;
            attemptsLeft = attemptsLeft > 0 ? attemptsLeft : 1;
            textWarning.Text = $"Внимание! Осталось {attemptsLeft} попыток входа до блокировки";
            textWarning.Visibility = Visibility.Visible;
        }

        private void UpdateCaptcha()
        {
            textCaptcha.Text = CaptchaService.GenerateCaptcha(6);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            _parentWindow.Closing += SaveLockoutState;

            var navigationService = PageManager.MainFrame.NavigationService;
            navigationService.Navigated += SaveLockoutState;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _parentWindow.Closing -= SaveLockoutState;

            var navigationService = PageManager.MainFrame.NavigationService;
            navigationService.Navigated -= SaveLockoutState;
        }
        
        private void SaveLockoutState(object sender, EventArgs e)
        {
            AuthenticationLockoutState.Save(_lockoutState);
        }
    }
}
