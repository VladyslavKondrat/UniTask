using System;
using System.Collections.Generic;
using System.Text;

namespace UniTask.ApplicationFulfillment.Domain.Exceptions
{
    public class InvalidFeeAmountException : DomainException
    {
        public InvalidFeeAmountException(decimal amount)
            : base("application.invalid_fee_amount",
                $"Platform fee amount '{amount}' is invalid.")
        {
        }
    }
}
