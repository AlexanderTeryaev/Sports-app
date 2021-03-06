USE [LogLig]
GO

-- 1. Creating MedicalInstitutes table
CREATE TABLE [dbo].[MedicalInstitutes](
	[MedicalInstitutesId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Address] [nvarchar](250) NULL,
  [SeasonId] [int] NOT NULL,
  [UnionId] [int] NOT NULL,
  
 CONSTRAINT [PK_MedicalInstitutes] PRIMARY KEY CLUSTERED 
(
	[MedicalInstitutesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[MedicalInstitutes]  WITH CHECK ADD  CONSTRAINT [FK_MedicalInstitutes_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[MedicalInstitutes]  WITH CHECK ADD  CONSTRAINT [FK_MedicalInstitutes_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO

-- 4.1 Add flag isNationalTeam
ALTER TABLE dbo.Clubs ADD
	IsNationalTeam bit NOT NULL DEFAULT(0)
GO

-- 6. Add flag IsMastersCompetition
ALTER TABLE dbo.Leagues ADD
	IsMastersCompetition bit NOT NULL DEFAULT(0)
GO

-- 3. Adding flags IsHeated and IsIndoor to Auditoriums
ALTER TABLE dbo.Auditoriums ADD
	IsHeated bit NOT NULL DEFAULT(0)
GO
ALTER TABLE dbo.Auditoriums ADD
	IsIndoor bit NOT NULL DEFAULT(0)
GO

-- 5.1. Adding PassportValidity
ALTER TABLE [dbo].[Users] ADD PassportValidity DATETIME NULL
GO

-- 5.2. Adding Masters flag
ALTER TABLE [dbo].[TeamsPlayers] ADD Masters bit NOT NULL DEFAULT(0)
GO

-- 5.4. Adding CountryOfBirth
ALTER TABLE [dbo].[Users] ADD CountryOfBirth nvarchar(MAX) null
GO

-- 5.7. Adding MedicalInstitutes to TeamsPlayers
ALTER TABLE TeamsPlayers ADD MedicalInstituteId INT NULL
GO
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_MedicalInstituteId] FOREIGN KEY (MedicalInstituteId) REFERENCES [dbo].[MedicalInstitutes] ([MedicalInstitutesId]);
GO