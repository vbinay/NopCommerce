USE [nopcommerce_prod_38]
GO

/****** Object:  Table [dbo].[BulkRefund]    Script Date: 5/9/2020 3:36:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BulkRefund](
	[BulkRefundId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[IsVerrtex] [bit] NOT NULL,
	[ProcessVertex] [bit] NOT NULL,
	[IsProcessed] [bit] NOT NULL,
	[TotalRefund] [decimal](16, 2) NOT NULL,
	[GlCode1] [varchar](50) NOT NULL,
	[GlCode2] [varchar](50) NULL,
	[GlCode3] [varchar](50) NULL,
	[Glcodeid1] [int] NOT NULL,
	[Glcodeid2] [int] NULL,
	[Glcodeid3] [int] NULL,
	[GlAmount1] [decimal](16, 2) NOT NULL,
	[GlAmount2] [decimal](16, 2) NULL,
	[GlAmount3] [decimal](16, 2) NULL,
	[RefundedTaxAmount1] [decimal](18, 0) NULL,
	[RefundedTaxAmount2] [decimal](18, 0) NULL,
	[RefundedTaxAmount3] [decimal](18, 0) NULL,
	[DeliveryGLName] [varchar](50) NULL,
	[DeliveryPickupAmount] [decimal](16, 2) NULL,
	[DeliveryTax] [decimal](16, 2) NULL,
	[CreatedDateUtc] [datetime] NOT NULL,
	[ProcessedDateUtc] [datetime] NULL,
	[Errors] [nvarchar](1024) NULL,
 CONSTRAINT [PK_BulkRefund] PRIMARY KEY CLUSTERED 
(
	[BulkRefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


