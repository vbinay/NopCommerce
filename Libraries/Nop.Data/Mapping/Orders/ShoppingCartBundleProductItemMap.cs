using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
    public partial class ShoppingCartBundleProductItemMap : NopEntityTypeConfiguration<ShoppingCartBundleProductItem>
    {
        public ShoppingCartBundleProductItemMap()
        {
            this.ToTable("ShoppingCartBundleProductItem");
            this.HasKey(shoppingCartBundleProductItem => shoppingCartBundleProductItem.Id);

            this.Property(shoppingCartBundleProductItem => shoppingCartBundleProductItem.ShoppingCartItemId);
            this.Property(shoppingCartBundleProductItem => shoppingCartBundleProductItem.ParentProductId);
            this.Property(shoppingCartBundleProductItem => shoppingCartBundleProductItem.AssociatedProductId);
            this.Property(shoppingCartBundleProductItem => shoppingCartBundleProductItem.Quantity);
            this.Property(shoppingCartBundleProductItem => shoppingCartBundleProductItem.Price).HasPrecision(18, 4);
        }
    }
}
