using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_control.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public decimal Balance { get; set; }

        public Wallet(int id, string name, string icon, string color)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Color = color;
            Balance = 0;
        }

        public override string ToString()
        {
            return $"{Icon} {Name}";
        }
    }
}