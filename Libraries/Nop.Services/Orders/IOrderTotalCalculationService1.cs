using System;
namespace Nop.Services.Orders
{
    interface IOrderTotalCalculationService1
    {
        decimal AdjustShippingRate(decimal shippingRate, System.Collections.Generic.IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart, out System.Collections.Generic.List<Nop.Core.Domain.Discounts.Discount> appliedDiscounts);
    }
}
