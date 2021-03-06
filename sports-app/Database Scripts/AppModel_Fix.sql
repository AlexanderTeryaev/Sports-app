ALTER TABLE [dbo].[AdditionalGymnastics] DROP CONSTRAINT [DF__Additiona__IsReg__7E42ABEE];

ALTER TABLE [dbo].[AdditionalTeamGymnastics] DROP CONSTRAINT [DF__Additiona__IsReg__485B9C89];

ALTER TABLE [dbo].[CompetitionClubsCorrections] DROP CONSTRAINT [FK_CompetitionClubsCorrections_Clubs];

ALTER TABLE [dbo].[CompetitionClubsCorrections] DROP CONSTRAINT [FK_CompetitionClubsCorrections_Leagues];

ALTER TABLE [dbo].[CompetitionClubsCorrections] DROP CONSTRAINT [FK_CompetitionClubsCorrections_Seasons];

DROP PROCEDURE [dbo].[AddUpdateRegionals];

DROP PROCEDURE [dbo].[DeleteRegional];

DROP PROCEDURE [dbo].[GetRegionalById];

DROP PROCEDURE [dbo].[GetRegionals];

DROP FUNCTION [dbo].[FN_GetRegionalManagerById];

DROP FUNCTION [dbo].[FN_GetRegionalManagerEmailById];

ALTER TABLE [dbo].[AdditionalGymnastics] ALTER COLUMN [CompositionNumber] INT NOT NULL;

ALTER TABLE [dbo].[AdditionalTeamGymnastics] ALTER COLUMN [CompositionNumber] INT NOT NULL;

ALTER TABLE [dbo].[CompetitionClubsCorrections] ALTER COLUMN [ClubId] INT NOT NULL;

ALTER TABLE [dbo].[CompetitionClubsCorrections] ALTER COLUMN [Correction] DECIMAL (18, 2) NOT NULL;

ALTER TABLE [dbo].[CompetitionClubsCorrections] ALTER COLUMN [GenderId] INT NOT NULL;

ALTER TABLE [dbo].[CompetitionClubsCorrections] ALTER COLUMN [LeagueId] INT NOT NULL;

ALTER TABLE [dbo].[CompetitionClubsCorrections] ALTER COLUMN [SeasonId] INT NOT NULL;

ALTER TABLE [dbo].[AdditionalGymnastics]
    ADD CONSTRAINT [DF__Additiona__IsReg__7E42ABEE] DEFAULT ((0)) FOR [CompositionNumber];

ALTER TABLE [dbo].[AdditionalTeamGymnastics]
    ADD CONSTRAINT [DF__Additiona__IsReg__485B9C89] DEFAULT ((0)) FOR [CompositionNumber];

ALTER TABLE [dbo].[CompetitionClubsCorrections]
    ADD CONSTRAINT [DF_CompetitionClubsCorrections_Correction] DEFAULT ((0)) FOR [Correction];

ALTER TABLE [dbo].[CompetitionClubsCorrections] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionClubsCorrections_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[CompetitionClubsCorrections] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionClubsCorrections_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[CompetitionClubsCorrections] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionClubsCorrections_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[Regionals] WITH NOCHECK
    ADD CONSTRAINT [FK_Regionals_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[Regionals] WITH NOCHECK
    ADD CONSTRAINT [FK_Regionals_Unions] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]);

ALTER TABLE [dbo].[CompetitionClubsCorrections] WITH CHECK CHECK CONSTRAINT [FK_CompetitionClubsCorrections_Clubs];

ALTER TABLE [dbo].[CompetitionClubsCorrections] WITH CHECK CHECK CONSTRAINT [FK_CompetitionClubsCorrections_Leagues];

ALTER TABLE [dbo].[CompetitionClubsCorrections] WITH CHECK CHECK CONSTRAINT [FK_CompetitionClubsCorrections_Seasons];

ALTER TABLE [dbo].[Regionals] WITH CHECK CHECK CONSTRAINT [FK_Regionals_Seasons];

ALTER TABLE [dbo].[Regionals] WITH CHECK CHECK CONSTRAINT [FK_Regionals_Unions];