﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.VerifyModel
{
    public class VerifyViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string ProfileImage { get; set; }
        public string RoleName { get; set; }
        public string VerifyStatus { get; set; }
        public string VerifyImage { get; set; }
    }
}
