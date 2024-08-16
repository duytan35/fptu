﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.ProductModel
{
    public class UpdateProductModel
    {
        public IFormFile? ProductImage { get; set; }
        public int? ProductQuantity { get; set; }
        public long? ProductPrice { get; set; }
        public int? CategoryId { get; set; }
        public string? RequestedProduct { get; set; }
    }
}
