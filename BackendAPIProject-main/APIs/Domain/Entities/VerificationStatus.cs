﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class VerificationStatus
    {
        public int VerificationStatusId { get; set; }
        public string VerificationStatusName { get; set; }
        public ICollection<VerifyUser> VerifyUsers { get; set; }    
    }
}
