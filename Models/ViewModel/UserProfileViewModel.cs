using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_BDS.Models.ViewModel
{
    public class UserProfileViewModel
    {
        public User User { get; set; }
        public List<Product> Products { get; set; }
    }
}