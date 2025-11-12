using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTroca.Application.Interfaces
{
    public interface ITextModerationServices
    {
        Task<bool> IsTextSafeAsync(string text);

    }
}
