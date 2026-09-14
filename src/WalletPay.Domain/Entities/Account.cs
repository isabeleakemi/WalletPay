using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPay.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Document { get; private set; } = string.Empty;
        public decimal Balance { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Account()
        {
        }

        public Account(string name, string document)
        {
            Id = Guid.NewGuid();
            Name = name;
            Document = document;
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
