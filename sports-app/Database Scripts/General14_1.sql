ALTER TABLE [dbo].[CompetitionRoutes] DROP CONSTRAINT [FK_CompetitionRoutes_Instruments]
ALTER TABLE [dbo].[CompetitionRoutes] DROP COLUMN [InstrumentId]
ALTER TABLE [dbo].[CompetitionRoutes] ADD [InstrumentIds] VARCHAR(MAX) NULL