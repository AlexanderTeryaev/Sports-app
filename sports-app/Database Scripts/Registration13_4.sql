ALTER TABLE [dbo].[ActivityCustomPrices] DROP CONSTRAINT [FK_ActivityCustomPrices_Activities];
ALTER TABLE [dbo].[ActivityForms] DROP CONSTRAINT [FK_ActivityForms_Activities];

ALTER TABLE [dbo].[ActivityFormsFiles] DROP CONSTRAINT [FK_ActivityFormsFiles_Activities];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP CONSTRAINT [FK_ActivityFormsSubmittedData_Activities];

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] DROP CONSTRAINT [FK_ActivityStatusColumnsVisibility_Activities];

ALTER TABLE [dbo].[Activities]
    ADD [AllowNewTeamRegistration] BIT CONSTRAINT [DF_Activities_AllowNewTeamRegistration] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityCustomPrices] ALTER COLUMN [ActivityId] INT NULL;

ALTER TABLE [dbo].[ActivityForms] ALTER COLUMN [ActivityId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsFiles] ALTER COLUMN [ActivityId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] ALTER COLUMN [ActivityId] INT NULL;

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] ALTER COLUMN [ActivityId] INT NULL;

ALTER TABLE [dbo].[ActivityCustomPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityCustomPrices_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityForms] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityForms_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityFormsFiles] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsFiles_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsVisibility_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityCustomPrices] WITH CHECK CHECK CONSTRAINT [FK_ActivityCustomPrices_Activities];

ALTER TABLE [dbo].[ActivityForms] WITH CHECK CHECK CONSTRAINT [FK_ActivityForms_Activities];

ALTER TABLE [dbo].[ActivityFormsFiles] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsFiles_Activities];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Activities];

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsVisibility_Activities];