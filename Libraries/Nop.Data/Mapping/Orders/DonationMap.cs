using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
	public partial class DonationMap : NopEntityTypeConfiguration<Donation>
	{
		/// <summary>
		/// Database Mapping for Donation Table
		/// </summary>
		public DonationMap()    /// NU-17
		{
			this.ToTable("Donation");
			this.HasKey(mp => mp.Id);

			this.Property(mp => mp.DonorFirstName).HasMaxLength(100);
			this.Property(mp => mp.DonorLastName).HasMaxLength(100);
			this.Property(mp => mp.DonorCompany).HasMaxLength(100);
			this.Property(mp => mp.DonorAddress).HasMaxLength(400);
			this.Property(mp => mp.DonorAddress2).HasMaxLength(400);
			this.Property(mp => mp.DonorCity).HasMaxLength(100);
			this.Property(mp => mp.DonorState).HasMaxLength(100);            
			this.Property(mp => mp.DonorZip).HasMaxLength(100);
			this.Property(mp => mp.DonorPhone).HasMaxLength(100);
			this.Property(mp => mp.DonorCountry).HasMaxLength(100);
			this.Property(mp => mp.Comments).HasMaxLength(400);
			this.Property(mp => mp.NotificationEmail).HasMaxLength(100);
			this.Property(mp => mp.OnBehalfOfFullName).HasMaxLength(200);
			this.Property(mp => mp.IncludeGiftAmount);
			this.Property(mp => mp.Amount);

			this.HasRequired(mp => mp.PurchasedWithOrderItem)
				.WithMany()
				.HasForeignKey(mp => mp.PurchasedWithOrderItemId)
				.WillCascadeOnDelete(false);
		}
	}
}