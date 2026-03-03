using System;
using System.Windows;
using System.Windows.Controls;
using Finance_control;
using System.Linq;


namespace Finance_control
{
    public partial class StatisticsPage : Page
    {
        public StatisticsPage(int totalTransactions, decimal totalIncome, decimal totalExpense)
        {
            InitializeComponent();

            TotalTransactionsText.Text = $"Всего транзакций: {totalTransactions}";
            TotalIncomeText.Text = $"Общий доход: {totalIncome:N0} ₽";
            TotalExpenseText.Text = $"Общий расход: {totalExpense:N0} ₽";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Простой возврат к главному окну
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Закрыть текущее окно статистики
            Window window = Application.Current.Windows
                .Cast<Window>()
                .FirstOrDefault(w => w.IsActive);
            window?.Close();
        }
            
        }
    }
