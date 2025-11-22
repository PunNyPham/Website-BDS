using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website_BDS.Models.ViewModel
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public User Seller { get; set; }
    }
}