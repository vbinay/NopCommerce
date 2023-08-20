using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
      public partial class ProductFulfillmentCalendarMap : NopEntityTypeConfiguration<ProductFulfillmentCalendar>
    {
        public ProductFulfillmentCalendarMap()
        {
            this.ToTable("ProductFulfillmentCalendar");
            this.HasKey(productFulfillmentCalendar => productFulfillmentCalendar.Id);

            this.Property(productFulfillmentCalendar => productFulfillmentCalendar.ProductId);
            this.Property(productFulfillmentCalendar => productFulfillmentCalendar.Date);

            this.HasRequired(productFulfillmentCalendar => productFulfillmentCalendar.Product)
               .WithMany()
               .HasForeignKey(productFulfillmentCalendar => productFulfillmentCalendar.ProductId);
        }
    }
}
