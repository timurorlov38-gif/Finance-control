using System;
using System.Windows;
using System.Windows.Controls;

namespace Финансовый_трекер
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
            try
            {
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    // Альтернативный способ - найти MainWindow и очистить Frame
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.StatsFrame.Content = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка возврата: {ex.Message}");
            }
        }
    }
}