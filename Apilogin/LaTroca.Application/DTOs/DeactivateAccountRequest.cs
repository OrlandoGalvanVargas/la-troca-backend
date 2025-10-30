using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTroca.Application.DTOs
{
    public class DeactivateAccountRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
