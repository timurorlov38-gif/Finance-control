using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Finance_control.Models;
using Finance_control;


namespace Finance_control
{
    public partial class MainWindow : Window
    {
        private List<Transaction> transactions = new List<Transaction>();
        private List<Wallet> wallets = new List<Wallet>();
        private int currentWalletId = 1;
        private bool isLocked = false;
        private int monthlyLimit = 30000;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWallets();
            UpdateWalletsList();
            UpdateTotalBalanceDisplay();
            UpdateLimitDisplay();
        }

        private void InitializeWallets()
        {
            wallets.Add(new Wallet(1, "Основной", "💳", "#3498DB"));
            wallets.Add(new Wallet(2, "Наличные", "💰", "#27AE60"));
            wallets.Add(new Wallet(3, "Сберегательный", "🏦", "#9B59B6"));

            WalletComboBox.SelectedIndex = 0;
        }

        private void UpdateWalletsList()
        {
            WalletsList.ItemsSource = null;
            WalletsList.ItemsSource = wallets;

            decimal totalBalance = wallets.Sum(w => w.Balance);
            TotalWalletsBalanceText.Text = $"{totalBalance:N0} ₽";
        }

        private void UpdateWalletBalanceDisplay()
        {
            var currentWallet = wallets.FirstOrDefault(w => w.Id == currentWalletId);
            if (currentWallet != null)
            {
                WalletBalanceText.Text = $"Баланс кошелька: {currentWallet.Balance:N0} ₽";
            }
        }

        private void UpdateTotalBalanceDisplay()
        {
            decimal totalBalance = wallets.Sum(w => w.Balance);
            BalanceText.Text = $"{totalBalance:N0} ₽";

            if (totalBalance >= 0)
                BalanceText.Foreground = Brushes.White;
            else
                BalanceText.Foreground = Brushes.LightCoral;
        }

        private void WalletComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WalletComboBox.SelectedIndex >= 0 && WalletComboBox.SelectedIndex < wallets.Count)
            {
                currentWalletId = wallets[WalletComboBox.SelectedIndex].Id;
                UpdateWalletBalanceDisplay();
            }
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (isLocked)
            {
                MessageBox.Show("Приложение заблокировано. Введите PIN для разблокировки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal amount = 0;

            if (!decimal.TryParse(AmountBox.Text, out amount))
            {
                MessageBox.Show("Введите корректную сумму!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (amount <= 0)
            {
                MessageBox.Show("Сумма должна быть больше нуля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string description = DescriptionBox.Text.Trim();
            if (string.IsNullOrEmpty(description))
                description = "Без описания";

            bool isIncome = IsIncomeCheckBox.IsChecked == true;

            var transaction = new Transaction
            {
                Id = transactions.Count + 1,
                Amount = amount,
                Description = description,
                Date = DateTime.Now,
                IsIncome = isIncome,
                WalletId = currentWalletId
            };

            transactions.Add(transaction);

            var wallet = wallets.FirstOrDefault(w => w.Id == currentWalletId);
            if (wallet != null)
            {
                if (isIncome)
                    wallet.Balance += amount;
                else
                    wallet.Balance -= amount;
            }

            UpdateWalletBalanceDisplay();
            UpdateWalletsList();
            UpdateTotalBalanceDisplay();
            AddTransactionToUI(transaction);
            CheckLimit();

            AmountBox.Clear();
            DescriptionBox.Clear();
            IsIncomeCheckBox.IsChecked = false;
        }

        private void AddTransactionToUI(Transaction transaction)
        {
            Border border = new Border
            {
                Background = transaction.IsIncome ? (Brush)new BrushConverter().ConvertFromString("#E8F8F5") : (Brush)new BrushConverter().ConvertFromString("#FADBD8"),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = transaction.IsIncome ? Brushes.LightGreen : Brushes.LightCoral,
                BorderThickness = new Thickness(2, 0, 0, 0)
            };

            StackPanel stackPanel = new StackPanel();

            TextBlock amountText = new TextBlock
            {
                Text = (transaction.IsIncome ? "+ " : "- ") + $"{transaction.Amount:N0} ₽",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = transaction.IsIncome ? Brushes.Green : Brushes.Red
            };

            TextBlock descText = new TextBlock
            {
                Text = transaction.Description,
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0)
            };

            TextBlock dateText = new TextBlock
            {
                Text = $"🕐 {transaction.Date:dd.MM.yyyy HH:mm}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(amountText);
            stackPanel.Children.Add(descText);
            stackPanel.Children.Add(dateText);

            border.Child = stackPanel;

            Border finalBorder = new Border
            {
                Child = border,
                Tag = transaction.Id
            };

            DockPanel panel = new DockPanel();
            panel.Children.Add(finalBorder);

            Button deleteButton = new Button
            {
                Content = "🗑️",
                Width = 40,
                Height = 40,
                Background = Brushes.IndianRed,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Tag = transaction.Id,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Stretch
            };
            deleteButton.Click += DeleteTransaction_Click;
            panel.Children.Add(deleteButton);

            TransactionsList.Children.Add(panel);
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null)
            {
                int id = (int)button.Tag;

                var transaction = transactions.FirstOrDefault(t => t.Id == id);
                if (transaction != null)
                {
                    var wallet = wallets.FirstOrDefault(w => w.Id == transaction.WalletId);
                    if (wallet != null)
                    {
                        if (transaction.IsIncome)
                            wallet.Balance -= transaction.Amount;
                        else
                            wallet.Balance += transaction.Amount;
                    }

                    transactions.Remove(transaction);

                    DockPanel dockPanel = button.Parent as DockPanel;
                    if (dockPanel != null)
                    {
                        StackPanel parentPanel = dockPanel.Parent as StackPanel;
                        if (parentPanel != null)
                        {
                            parentPanel.Children.Remove(dockPanel);
                        }
                    }

                    UpdateWalletBalanceDisplay();
                    UpdateWalletsList();
                    UpdateTotalBalanceDisplay();
                }
            }
        }
        private void CheckLimit()
        {
            decimal totalExpenses = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

            if (totalExpenses > monthlyLimit)
            {
                MessageBox.Show($"⚠️ ВНИМАНИЕ!\n\nВы превысили месячный лимит расходов!\nПотрачено: {totalExpenses:N0} ₽\nЛимит: {monthlyLimit:N0} ₽\nПревышение: {totalExpenses - monthlyLimit:N0} ₽",
                    "Превышение лимита!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateLimitDisplay()
        {
            LimitValueText.Text = $"Лимит: {monthlyLimit:N0} ₽";
        }

        private void LockImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isLocked = !isLocked;

            if (isLocked)
            {
                SecurityStatus.Text = "Заблокировано";
                SecurityStatus.Foreground = Brushes.Red;
                AmountBox.IsEnabled = false;
                DescriptionBox.IsEnabled = false;
                IsIncomeCheckBox.IsEnabled = false;
                WalletComboBox.IsEnabled = false;
            }
            else
            {
                SecurityStatus.Text = "Разблокировано";
                SecurityStatus.Foreground = Brushes.Green;
                AmountBox.IsEnabled = true;
                DescriptionBox.IsEnabled = true;
                IsIncomeCheckBox.IsEnabled = true;
                WalletComboBox.IsEnabled = true;
            }
        }

        private void ResetPin_Click(object sender, RoutedEventArgs e)
        {
            PinBox.Clear();
            MessageBox.Show("PIN сброшен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            int totalTransactions = transactions.Count;
            decimal totalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal totalExpense = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

            StatisticsPage statsPage = new StatisticsPage(totalTransactions, totalIncome, totalExpense);
            this.Content = statsPage;
        }
    }
}