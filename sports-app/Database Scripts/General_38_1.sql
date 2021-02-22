ALTER TABLE [dbo].[Stages]
    ADD [RankedStandingsEnabled] BIT CONSTRAINT [DF_Stages_RankedStandingsEnabled] DEFAULT ((0)) NOT NULL;