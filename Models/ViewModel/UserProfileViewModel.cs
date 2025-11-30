using System.Collections.Generic;

namespace Website_BDS.Models.ViewModel
{
    public class UserProfileViewModel
    {
        public User User { get; set; }
        public List<Product> Products { get; set; }
        public List<TransactionHistory> Transactions { get; set; }
    }
}