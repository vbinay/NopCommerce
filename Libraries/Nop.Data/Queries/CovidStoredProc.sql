SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<William Smith (william.smith2@sodexo.com)>
-- Create date: <04/09/2020 11:12AM PST>
-- Description:	<Inserts records for covid related customized refunds.>
-- =============================================
CREATE PROCEDURE InsertCovidRefundRecord
(
@ProductId as int = 0, 
@OrderId as int = 0, 
@OrderItemId as int = 0, 
@GlCodeName1 as nvarchar(50) = '', 
@GLAmount1 as decimal(16,2) = 0.0, 
@GLCodeName2 as nvarchar(50) = '', 
@GLAmount2 as decimal(16,2) = 0.0, 
@GLCodeName3 as nvarchar(50) = '',
@GLAmount3 as decimal(16,2) = 0.0,
@TaxAmount1 as decimal(16,2) = 0.0,
@TaxName1 as nvarchar(50) = '',
@TaxAmount2 as decimal(16,2) = 0.0,
@TaxName2 as nvarchar(50) = '',
@TaxAmount3 as decimal(16,2) = 0.0,
@TaxName3 as nvarchar(50) = '',
@TotalRefund as decimal(16,2) = 0.0,
@RefundedTaxAmount1 as decimal(16,2) = 0.0,
@RefundedTaxAmount2 as decimal(16,2) = 0.0,
@RefundedTaxAmount3 as decimal(16,2) = 0.0,
@DeliveryTaxName as nvarchar(50) = '',
@DeliveryGLCodeName as nvarchar(50) = '',
@DeliveryTax as decimal(16,2) = 0.0,
@DeliveryPickupAmount as decimal(16,2) = 0.0,
@ShippingAmount as decimal(16,2) = 0.0,
@ShippingTax as decimal(16,2) = 0.0,
@ShippingTaxName as nvarchar(50) = '',
@Error as nvarchar(1028) = '',
@Success as nvarchar(10) = '',
@CreatedDateUtc as datetime,
@OverridenGlcode1 as varchar(100),
@OverridenGlcode2 as varchar(100),
@OverridenGlcode3 as varchar(100)
) as
BEGIN
INSERT INTO CovidRefunds ([ProductId],[OrderId],[OrderItemId],[GlCodeName1],[GLAmount1],[GLCodeName2],[GLAmount2],[GLCodeName3],[GLAmount3],[TaxAmount1],[TaxName1],[TaxAmount2],[TaxName2],[TaxAmount3],[TaxName3],[TotalRefund],[RefundedTaxAmount1],[RefundedTaxAmount2],[RefundedTaxAmount3],[DeliveryTaxName],[DeliveryGLCodeName],[DeliveryTax],[DeliveryPickupAmount],[ShippingAmount],[ShippingTax],[ShippingTaxName],[Error],[Success],[CreatedDateUtc],OverriddenGlcode1,OverriddenGlcode2,OverriddenGlcode3)
VALUES (@ProductId, @OrderId, @OrderItemId, @GlCodeName1, @GLAmount1, @GLCodeName2, @GLAmount2, @GLCodeName3,
@GLAmount3,@TaxAmount1,@TaxName1,@TaxAmount2,@TaxName2,@TaxAmount3,@TaxName3,@TotalRefund,@RefundedTaxAmount1,
@RefundedTaxAmount2,@RefundedTaxAmount3,@DeliveryTaxName,@DeliveryGLCodeName,@DeliveryTax,@DeliveryPickupAmount, +
@ShippingAmount,@ShippingTax,@ShippingTaxName,@Error,@Success,@CreatedDateUtc,@OverridenGlcode1,@OverridenGlcode2,@OverridenGlcode3)	
END
GO
