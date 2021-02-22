ALTER TABLE [dbo].[Activities]
    ADD [EnableBrotherDiscount]    BIT             CONSTRAINT [DF_Activities_EnableBrotherDiscount] DEFAULT ((0)) NOT NULL,
        [BrotherDiscountAmount]    DECIMAL (18, 2) CONSTRAINT [DF_Activities_BrotherDiscountAmount] DEFAULT ((0)) NOT NULL,
        [BrotherDiscountInPercent] BIT             CONSTRAINT [DF_Activities_BrotherDiscountInPercent] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [BrotherUserId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_BrothersUsers] FOREIGN KEY ([BrotherUserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_BrothersUsers];