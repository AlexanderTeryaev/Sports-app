ALTER TABLE [dbo].[ActivityFormsDetails] DROP CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices];

ALTER TABLE [dbo].[ActivityFormsDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices] FOREIGN KEY ([CustomPriceId]) REFERENCES [dbo].[ActivityCustomPrices] ([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityFormsDetails] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices];