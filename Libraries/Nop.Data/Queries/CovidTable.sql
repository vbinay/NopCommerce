USE [nopcommerce_prod_38]
GO

/****** Object:  Table [dbo].[CovidRefunds]    Script Date: 4/9/2020 11:14:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CovidRefunds](
	[CovidRefundId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[OrderId] [int] NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[GlCodeName1] [nvarchar](50) NULL,
	[GLAmount1] [decimal](16, 2) NULL,
	[GLCodeName2] [nvarchar](50) NULL,
	[GLAmount2] [decimal](16, 2) NULL,
	[GLCodeName3] [nvarchar](50) NULL,
	[GLAmount3] [decimal](16, 2) NULL,
	[TaxAmount1] [decimal](16, 2) NULL,
	[TaxName1] [nvarchar](50) NULL,
	[TaxAmount2] [decimal](16, 2) NULL,
	[TaxName2] [nvarchar](50) NULL,
	[TaxAmount3] [decimal](16, 2) NULL,
	[TaxName3] [nvarchar](50) NULL,
	[TotalRefund] [decimal](16, 2) NULL,
	[RefundedTaxAmount1] [decimal](16, 2) NULL,
	[RefundedTaxAmount2] [decimal](16, 2) NULL,
	[RefundedTaxAmount3] [decimal](16, 2) NULL,
	[DeliveryTaxName] [nvarchar](50) NULL,
	[DeliveryGLCodeName] [nvarchar](50) NULL,
	[DeliveryTax] [decimal](16, 2) NULL,
	[DeliveryPickupAmount] [decimal](16, 2) NULL,
	[ShippingAmount] [decimal](16, 2) NULL,
	[ShippingTax] [decimal](16, 2) NULL,
	[ShippingTaxName] [nvarchar](200) NULL,
	[Error] [nvarchar](1028) NULL,
	[Success] [nvarchar](10) NULL,
	[CreatedDateUtc] [datetime] NOT NULL,
	[OverriddenGlcode1][varchar](100) NULL,
	[OverriddenGlcode2][varchar](100) NULL,
	[OverriddenGlcode3][varchar](100) NULL
 CONSTRAINT [PK_CovidRefunds] PRIMARY KEY CLUSTERED 
(
	[CovidRefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


