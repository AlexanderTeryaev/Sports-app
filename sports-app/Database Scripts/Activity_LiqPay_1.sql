ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [LiqPayOrderId] UNIQUEIDENTIFIER NULL;
	
ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [LiqPayPaymentCompleted] BIT      CONSTRAINT [DF_ActivityFormsSubmittedData_LiqPayPaymentCompleted] DEFAULT ((0)) NOT NULL,
        [LiqPayPaymentDate]      DATETIME NULL;

CREATE TABLE [dbo].[LiqPayPaymentsNotifications] (
    [Id]          INT              IDENTITY (1, 1) NOT NULL,
    [OrderId]     UNIQUEIDENTIFIER NOT NULL,
    [Status]      NVARCHAR (50)    NOT NULL,
    [JsonData]    NVARCHAR (MAX)   NOT NULL,
    [DateCreated] DATETIME         NOT NULL,
    CONSTRAINT [PK_LiqPayPaymentsNotifications] PRIMARY KEY CLUSTERED ([Id] ASC)
);