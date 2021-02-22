CREATE TABLE [dbo].[RankedStandingCorrections] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [TeamId]   INT NOT NULL,
    [LeagueId] INT NOT NULL,
    [Value]    INT NOT NULL,
    CONSTRAINT [PK_RankedStandingCorrections] PRIMARY KEY CLUSTERED ([Id] ASC)
);
ALTER TABLE [dbo].[RankedStandingCorrections]
    ADD CONSTRAINT [DF_RankedStandingCorrections_Value] DEFAULT ((0)) FOR [Value];
ALTER TABLE [dbo].[RankedStandingCorrections] WITH NOCHECK
    ADD CONSTRAINT [FK_RankedStandingCorrections_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);
ALTER TABLE [dbo].[RankedStandingCorrections] WITH NOCHECK
    ADD CONSTRAINT [FK_RankedStandingCorrections_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);
ALTER TABLE [dbo].[RankedStandingCorrections] WITH CHECK CHECK CONSTRAINT [FK_RankedStandingCorrections_Teams];
ALTER TABLE [dbo].[RankedStandingCorrections] WITH CHECK CHECK CONSTRAINT [FK_RankedStandingCorrections_Leagues];