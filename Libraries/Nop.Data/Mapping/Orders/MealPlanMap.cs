using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
    public partial class MealPlanMap : NopEntityTypeConfiguration<MealPlan> /// NU-16
    {
        public MealPlanMap()
        {
            this.ToTable("MealPlan");
            this.HasKey(mp => mp.Id);

            this.Property(mp => mp.RecipientName).HasMaxLength(100);
            this.Property(mp => mp.RecipientAddress).HasMaxLength(400);
            this.Property(mp => mp.RecipientPhone).HasMaxLength(50);
            this.Property(mp => mp.RecipientEmail).HasMaxLength(200);
            this.Property(mp => mp.RecipientAcctNum).HasMaxLength(100);

            this.HasRequired(mp => mp.PurchasedWithOrderItem)
                .WithMany()
                .HasForeignKey(mp => mp.PurchasedWithOrderItemId)
                .WillCascadeOnDelete(false);
        }
    }
}