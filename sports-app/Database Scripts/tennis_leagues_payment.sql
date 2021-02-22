ALTER TABLE [dbo].[ClubBalances] Add IsPdfReport bit Null
ALTER TABLE [dbo].[ClubBalances] Add IsPaid bit Null

USE [LogLig]
GO

/****** Object:  Table [dbo].[TeamPlayersPayments]    Script Date: 2/3/2019 4:22:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TeamPlayersPayments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClubBalanceId] [int] NOT NULL,
	[TeamPlayerId] [int] NOT NULL,
	[Fee] [decimal](18, 2) NULL,
	[Validity] [datetime] NULL,
 CONSTRAINT [PK_TeamPlayersPayments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TeamPlayersPayments]  WITH CHECK ADD  CONSTRAINT [FK_TeamPlayersPayments_ClubBalances] FOREIGN KEY([ClubBalanceId])
REFERENCES [dbo].[ClubBalances] ([Id]) ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TeamPlayersPayments] CHECK CONSTRAINT [FK_TeamPlayersPayments_ClubBalances]
GO

ALTER TABLE [dbo].[TeamPlayersPayments]  WITH CHECK ADD  CONSTRAINT [FK_TeamPlayersPayments_TeamsPlayers] FOREIGN KEY([TeamPlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[TeamPlayersPayments] CHECK CONSTRAINT [FK_TeamPlayersPayments_TeamsPlayers]
GO

USE [LogLig]
GO

/****** Object:  Table [dbo].[TeamRegistrationPayments]    Script Date: 2/3/2019 4:38:58 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TeamRegistrationPayments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClubBalanceId] [int] NOT NULL,
	[TeamRegistrationId] [int] NOT NULL,
	[Fee] [decimal](18, 2) NULL,
 CONSTRAINT [PK_TeamRegistrationPayments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TeamRegistrationPayments]  WITH CHECK ADD  CONSTRAINT [FK_TeamRegistrationPayments_ClubBalances] FOREIGN KEY([ClubBalanceId])
REFERENCES [dbo].[ClubBalances] ([Id]) ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TeamRegistrationPayments]  WITH CHECK ADD  CONSTRAINT [FK_TeamRegistrationPayments_TeamRegistrations] FOREIGN KEY([TeamRegistrationId])
REFERENCES [dbo].[TeamRegistrations] ([Id])
GO

ALTER TABLE [dbo].[TeamRegistrationPayments] CHECK CONSTRAINT [FK_TeamRegistrationPayments_TeamRegistrations]
GO

