USE [LogLig]
GO

CREATE TABLE [dbo].[AthleteNumbers] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [UserId]   INT NOT NULL,
    [SeasonId]   INT NOT NULL,
    [AthleteNumber]   INT NULL,
    [IsAthleteNumberProduced]  BIT NOT NULL
    CONSTRAINT [PK_AthleteNumbers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AthleteNumbers]  WITH NOCHECK ADD  CONSTRAINT [FK_Users_AthleteNumbers] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[AthleteNumbers] CHECK CONSTRAINT [FK_Users_AthleteNumbers]
GO

ALTER TABLE [dbo].[AthleteNumbers]  WITH CHECK ADD  CONSTRAINT [FK_AthleteNumbers_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[AthleteNumbers] CHECK CONSTRAINT [FK_AthleteNumbers_Seasons]
GO

INSERT INTO [dbo].[AthleteNumbers] ([UserId], [SeasonId], [AthleteNumber], [IsAthleteNumberProduced])
SELECT u.UserId, 1106, u.AthleteNumber, u.IsAthleteNumberProduced FROM [Users] u


ALTER TABLE [LogLig].[dbo].[Users] DROP COLUMN AthleteNumber;
ALTER TABLE [LogLig].[dbo].[Users] DROP COLUMN IsAthleteNumberProduced;
GO
