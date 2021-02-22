PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__IsClubMan__25D17A5B];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__IsClubFee__24DD5622];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__IsTrainin__3D7E1B63];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__IsClubApp__32F66B4F];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__IsReports__14D10B8B];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__Statement__0682EC34];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [DF__Clubs__IsFlowerO__6AC5C326];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [FK__Clubs__ParentClu__347EC10E];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [FK__Clubs__SportSect__3572E547];


GO
PRINT N'Altering [dbo].[Clubs]...';


GO
ALTER TABLE [dbo].[Clubs] ALTER COLUMN [Name] NVARCHAR (250) NULL;


GO
PRINT N'Creating [dbo].[DF__Clubs__IsClubApp__32F66B4F]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__IsClubApp__32F66B4F] DEFAULT ((0)) FOR [IsClubApproveByRegional];


GO
PRINT N'Creating [dbo].[DF__Clubs__IsClubFee__24DD5622]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__IsClubFee__24DD5622] DEFAULT ((0)) FOR [IsClubFeesPaid];


GO
PRINT N'Creating [dbo].[DF__Clubs__IsClubMan__25D17A5B]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__IsClubMan__25D17A5B] DEFAULT ((0)) FOR [IsClubManagerCanSeePayReport];


GO
PRINT N'Creating [dbo].[DF__Clubs__IsFlowerO__6AC5C326]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__IsFlowerO__6AC5C326] DEFAULT ((0)) FOR [IsFlowerOfSport];


GO
PRINT N'Creating [dbo].[DF__Clubs__IsReports__14D10B8B]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__IsReports__14D10B8B] DEFAULT ((0)) FOR [IsReportsEnabled];


GO
PRINT N'Creating [dbo].[DF__Clubs__IsTrainin__3D7E1B63]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__IsTrainin__3D7E1B63] DEFAULT ((0)) FOR [IsTrainingEnabled];


GO
PRINT N'Creating [dbo].[DF__Clubs__Statement__0682EC34]...';


GO
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [DF__Clubs__Statement__0682EC34] DEFAULT ((0)) FOR [StatementApproved];


GO
PRINT N'Creating [dbo].[FK__Clubs__ParentClu__347EC10E]...';


GO
ALTER TABLE [dbo].[Clubs] WITH NOCHECK
    ADD CONSTRAINT [FK__Clubs__ParentClu__347EC10E] FOREIGN KEY ([ParentClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);


GO
PRINT N'Creating [dbo].[FK__Clubs__SportSect__3572E547]...';


GO
ALTER TABLE [dbo].[Clubs] WITH NOCHECK
    ADD CONSTRAINT [FK__Clubs__SportSect__3572E547] FOREIGN KEY ([SportSectionId]) REFERENCES [dbo].[Sections] ([SectionId]);


GO
PRINT N'Checking existing data against newly created constraints';

GO
ALTER TABLE [dbo].[Clubs] WITH CHECK CHECK CONSTRAINT [FK__Clubs__ParentClu__347EC10E];

ALTER TABLE [dbo].[Clubs] WITH CHECK CHECK CONSTRAINT [FK__Clubs__SportSect__3572E547];


GO
PRINT N'Update complete.';