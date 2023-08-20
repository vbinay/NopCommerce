using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Orders
{
    public enum BlackboardCmdType
    {
  
        Credit = 0,

        Debit = 1,

        BalanceReturn = 2,

        CloseAccount = 3,

        CreateAccountAndSetBalance = 4

    }
}
