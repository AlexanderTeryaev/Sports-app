ALTER TABLE [dbo].[Clubs] ADD ClubNumber INT NULL;
ALTER TABLE [dbo].[Clubs] ADD DateOfClubApproval DATETIME NULL;
ALTER TABLE [dbo].[Clubs] ADD CertificateOfIncorporation VARCHAR(MAX) NULL;
ALTER TABLE [dbo].[Clubs] ADD ApprovalOfInsuranceCover VARCHAR(MAX) NULL;
ALTER TABLE [dbo].[Clubs] ADD AuthorizedSignatories VARCHAR(MAX) NULL;
ALTER TABLE [dbo].[Clubs] ADD DisciplineIds VARCHAR(255) NULL;
ALTER TABLE [dbo].[Users] ADD IDFile VARCHAR(MAX) NULL;
ALTER TABLE [dbo].[Users] ADD DisciplineIds VARCHAR(255) NULL;
ALTER TABLE [dbo].[Clubs] ADD IsCertificateApproved BIT NULL;
ALTER TABLE [dbo].[Clubs] ADD IsInsuranceCoverApproved BIT NULL;
ALTER TABLE [dbo].[Clubs] ADD IsAuthorizedSignatoriesApproved BIT NULL;
GO

ALTER TABLE [dbo].[GamesCycles] DROP CONSTRAINT [FK__GamesCycl__Athle__0D99FE17]
GO
ALTER TABLE [dbo].[GamesCycles] DROP COLUMN [AthleteBracketId]
GO

CREATE TABLE [dbo].[ColumnVisibility](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ColumnIndex] [int] NOT NULL,
	[Visible] [bit] NOT NULL,
 CONSTRAINT [PK_ColumnVisibility] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TeamsPlayers] ADD IsApprovedByManager BIT NULL;
ALTER TABLE [dbo].[TeamsPlayers] ADD [StartPlaying] DATETIME NULL;
ALTER TABLE [dbo].[TeamsPlayers] DROP CONSTRAINT [DF__TeamsPlay__IsPla__1881A0DE]
ALTER TABLE [dbo].[TeamsPlayers] DROP COLUMN [IsPlayereInTeamLessThan3year]
ALTER TABLE [dbo].[TeamsPlayers] ADD ClubComment nvarchar(255) NULL;
ALTER TABLE [dbo].[TeamsPlayers] ADD UnionComment nvarchar(255) NULL;