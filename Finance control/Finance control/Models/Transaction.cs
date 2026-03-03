using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_control.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsIncome { get; set; }
        public int WalletId { get; set; }  // ⬅️ ДОБАВЬТЕ ЭТУ СТРОКУ
    }
}