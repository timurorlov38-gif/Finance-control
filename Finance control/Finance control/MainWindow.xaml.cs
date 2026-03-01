using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Финансовый_трекер.Models;

namespace Финансовый_трекер
{
    public partial class MainWindow : Window
    {
        private List<Transaction> transactions = new List<Transaction>();
        private decimal balance = 0;
        private int nextId = 1;
        private decimal expenseLimit = 30000;
        private bool isLocked = false;
        private const string CORRECT_PIN = "1234";

        public MainWindow()
        {
            InitializeComponent();
            UpdateBalanceDisplay();
            UpdateLimitDisplay();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

        }

        // === ОБРАБОТЧИКИ ДЛЯ НОВЫХ ЭЛЕМЕНТОВ ===

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Проверяем только если приложение заблокировано
            if (isLocked)
            {
                if (PasswordBox.Password == "1234")
                {
                    UnlockApp();
                }
                else if (PasswordBox.Password.Length == 4)
                {
                    // Неверный PIN из 4 символов
                    SecurityStatus.Text = "❌ Неверный PIN";
                    SecurityStatus.Foreground = Brushes.Red;
                }
            }
        }
        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            // Подсчёт статистики
            int totalTransactions = transactions.Count;
            decimal totalIncome = 0;
            decimal totalExpense = 0;

            foreach (var t in transactions)
            {
                if (t.IsIncome)
                    totalIncome += t.Amount;
                else
                    totalExpense += t.Amount;
            }

            // Переход на страницу статистики через Frame
            StatsFrame.Navigate(new StatisticsPage(totalTransactions, totalIncome, totalExpense));
        }
        private void LockImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isLocked = !isLocked;

            if (isLocked)
            {
                LockApp();
            }
            else
            {
                UnlockApp();
            }
        }
        private void LockApp()
        {
            SecurityStatus.Text = "🔒 ЗАБЛОКИРОВАНО";
            SecurityStatus.Foreground = Brushes.OrangeRed;

            AmountBox.IsEnabled = false;
            DescriptionBox.IsEnabled = false;
            IncomeCheckBox.IsEnabled = false;
            PasswordBox.IsEnabled = true;
            PasswordBox.Focus();
            PasswordBox.Clear();
        }

        private void UnlockApp()
        {
            SecurityStatus.Text = "✅ Разблокировано";
            SecurityStatus.Foreground = Brushes.Green;

            AmountBox.IsEnabled = true;
            DescriptionBox.IsEnabled = true;
            IncomeCheckBox.IsEnabled = true;
            PasswordBox.IsEnabled = true;
            PasswordBox.Clear();

            AmountBox.Focus();
        }

        private void ResetPin_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox.Clear();
            SecurityStatus.Text = "Разблокировано";
            SecurityStatus.Foreground = Brushes.Green;
            PasswordBox.Focus();
        }

        // === ОБРАБОТЧИКИ ДЛЯ СУЩЕСТВУЮЩИХ ЭЛЕМЕНТОВ ===

        private void AmountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AmountBox.Text.Length > 0)
            {
                AmountBox.ToolTip = $"Введено: {AmountBox.Text} ₽";
            }
        }

        private void DescriptionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DescriptionBox.Text.Length > 0)
            {
                DescriptionBox.ToolTip = $"Описание: {DescriptionBox.Text}";
            }
        }

        private void IncomeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IncomeCheckBox.ToolTip = "Тип: ДОХОД 💰";
        }

        private void IncomeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IncomeCheckBox.ToolTip = "Тип: РАСХОД 💸";
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (isLocked)
            {
                MessageBox.Show("Приложение заблокировано! Введите PIN-код.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal amount;
            if (!decimal.TryParse(AmountBox.Text, out amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму больше 0!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                MessageBox.Show("Введите описание транзакции!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                DescriptionBox.Focus();
                return;
            }

            bool isIncome = IncomeCheckBox.IsChecked == true;

            var transaction = new Transaction
            {
                Id = nextId++,
                Amount = amount,
                Description = DescriptionBox.Text.Trim(),
                Date = DateTime.Now,
                IsIncome = isIncome
            };

            transactions.Add(transaction);

            if (isIncome)
            {
                balance += amount;
            }
            else
            {
                balance -= amount;
            }

            UpdateBalanceDisplay();
            AddTransactionToUI(transaction);
            CheckLimitWarning();

            AmountBox.Clear();
            DescriptionBox.Clear();
            IncomeCheckBox.IsChecked = false;
            AmountBox.Focus();

            MessageBox.Show($"✅ Транзакция добавлена!\n\n" +
                          $"Тип: {(isIncome ? "Доход 💰" : "Расход 💸")}\n" +
                          $"Сумма: {amount} ₽\n" +
                          $"Описание: {transaction.Description}",
                          "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddTransactionToUI(Transaction transaction)
        {
            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var indicator = new Border
            {
                Width = 10,
                Height = 60,
                Background = transaction.IsIncome ? Brushes.LightGreen : Brushes.LightCoral,
                CornerRadius = new CornerRadius(3, 0, 0, 3),
                Margin = new Thickness(0, 0, 10, 0)
            };
            stackPanel.Children.Add(indicator);

            var detailsPanel = new StackPanel();

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"{(transaction.IsIncome ? "+" : "-")} {transaction.Amount:N2} ₽",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = transaction.IsIncome ? Brushes.DarkGreen : Brushes.DarkRed
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = transaction.Description,
                FontSize = 14,
                Margin = new Thickness(0, 3, 0, 0),
                FontWeight = FontWeights.Medium
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"🕐 {transaction.Date:dd.MM.yyyy HH:mm}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 3, 0, 0)
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"ID: #{transaction.Id}",
                FontSize = 10,
                Foreground = Brushes.LightGray,
                Margin = new Thickness(0, 2, 0, 0)
            });

            stackPanel.Children.Add(detailsPanel);

            var deleteButton = new Button
            {
                Content = "🗑️",
                Width = 40,
                Height = 40,
                Margin = new Thickness(10, 0, 0, 0),
                Background = Brushes.IndianRed,
                Foreground = Brushes.White,
                FontSize = 18,
                Cursor = Cursors.Hand,
                ToolTip = new ToolTip { Content = "Удалить транзакцию" }
            };
            deleteButton.Click += (s, e) => DeleteTransaction(transaction, border);
            stackPanel.Children.Add(deleteButton);

            border.Child = stackPanel;
            TransactionsPanel.Children.Insert(0, border);

            TransactionsScroll.ScrollToTop();
        }

        private void DeleteTransaction(Transaction transaction, Border borderElement)
        {
            var result = MessageBox.Show(
                $"Удалить транзакцию?\n\n{transaction.Description}\n{transaction.Amount} ₽",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (transaction.IsIncome)
                {
                    balance -= transaction.Amount;
                }
                else
                {
                    balance += transaction.Amount;
                }

                transactions.Remove(transaction);
                UpdateBalanceDisplay();
                CheckLimitWarning();

                TransactionsPanel.Children.Remove(borderElement);

                MessageBox.Show("Транзакция удалена", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateBalanceDisplay()
        {
            BalanceText.Text = $"{balance:N0} ₽";

            if (balance < 0)
            {
                BalanceText.Foreground = Brushes.OrangeRed;
                BalanceText.ToolTip = "Отрицательный баланс!";
            }
            else if (balance > 0)
            {
                BalanceText.Foreground = Brushes.LightGreen;
                BalanceText.ToolTip = "Положительный баланс";
            }
            else
            {
                BalanceText.Foreground = Brushes.White;
                BalanceText.ToolTip = "Баланс равен нулю";
            }
        }

        private void LimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLimitDisplay();
            CheckLimitWarning();
        }

        private void UpdateLimitDisplay()
        {
            expenseLimit = (decimal)LimitSlider.Value;
            if (LimitValueText != null)
            {
                LimitValueText.Text = $"Лимит: {expenseLimit:N0} ₽";
            }
        }

        private void CheckLimitWarning()
        {
            if (LimitWarningText == null || LimitValueText == null)
                return;

            decimal totalExpenses = 0;
            foreach (var t in transactions)
            {
                if (!t.IsIncome)
                {
                    totalExpenses += t.Amount;
                }
            }

            if (totalExpenses > expenseLimit)
            {
                LimitWarningText.Text = $"⚠️ Превышен лимит на {totalExpenses - expenseLimit:N0} ₽!";
                LimitWarningText.Visibility = Visibility.Visible;
                LimitValueText.Foreground = Brushes.Red;
            }
            else
            {
                decimal remaining = expenseLimit - totalExpenses;
                LimitWarningText.Text = $"✓ Осталось: {remaining:N0} ₽";
                LimitWarningText.Visibility = Visibility.Visible;
                LimitWarningText.Foreground = Brushes.Green;
                LimitValueText.Foreground = Brushes.OrangeRed;
            }
        }
    }
}