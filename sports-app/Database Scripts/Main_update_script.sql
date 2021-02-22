IF ( NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Seasons'))
BEGIN
	PRINT 'Table season not exist'
	PRINT N'Creating [dbo].[Seasons]...';

	CREATE TABLE [dbo].[Seasons] (
		[Id]          INT            IDENTITY (1, 1) NOT NULL,
		[Name]        NVARCHAR (80)  NOT NULL,
		[Description] NVARCHAR (200) NULL,
		[StartDate]   DATETIME       NOT NULL,
		[EndDate]     DATETIME       NOT NULL,
		[UnionId]	  INT			 NOT NULL,
    CONSTRAINT [PK_Seasons] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Seasons_Unions] FOREIGN KEY ([UnionId]) REFERENCES [LogLig].[dbo].[Unions] ([UnionId])
);

PRINT N'Altering [dbo].[Games]...';
ALTER TABLE [dbo].[Games]
    ADD [SeasonId] INT NULL;


PRINT N'Altering [dbo].[Groups]...';


ALTER TABLE [dbo].[Groups]
    ADD [SeasonId] INT NULL;


PRINT N'Altering [dbo].[GroupsTeams]...';


ALTER TABLE [dbo].[GroupsTeams]
    ADD [SeasonId] INT NULL;


PRINT N'Altering [dbo].[Leagues]...';


ALTER TABLE [dbo].[Leagues]
    ADD [SortOrder] SMALLINT CONSTRAINT [DF_Leagues_SortOrder] DEFAULT ((0)) NOT NULL,
        [SeasonId]  INT      NULL;


IF (NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[Unions]') AND name = 'Logo'))
BEGIN
ALTER TABLE [dbo].[Unions] ADD [Logo] [nvarchar](50) NULL;
ALTER TABLE [dbo].[Unions] ADD [PrimaryImage] [nvarchar](50) NULL;
ALTER TABLE [dbo].[Unions] ADD [Description] [nvarchar](250) NULL;
ALTER TABLE [dbo].[Unions] ADD [IndexImage] [nvarchar](50) NULL;
ALTER TABLE [dbo].[Unions] ADD [IndexAbout] [nvarchar](2000) NULL;
ALTER TABLE [dbo].[Unions] ADD [TermsCondition] [nvarchar](50) NULL;
ALTER TABLE [dbo].[Unions] ADD [ContactPhone] [nvarchar](20) NULL;
ALTER TABLE [dbo].[Unions] ADD [Address] [nvarchar](250) NULL;
ALTER TABLE [dbo].[Unions] ADD [Email] [nvarchar](50) NULL;
ALTER TABLE [dbo].[Unions] ADD [CreateDate] [datetime] NULL;
END

CREATE TABLE [dbo].[UnionsDocs](
	[DocId] [int] IDENTITY(1,1) NOT NULL,
	[UnionId] [int] NOT NULL,
	[FileName] [nvarchar](150) NULL,
	[DocFile] [varbinary](max) NULL,
	[IsArchive] [bit] NOT NULL,
 CONSTRAINT [PK_UnionsDocs] PRIMARY KEY CLUSTERED 
(
	[DocId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

SET ANSI_PADDING OFF;

ALTER TABLE [dbo].[UnionsDocs]  WITH CHECK ADD  CONSTRAINT [FK_UnonsDocs_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId]);

ALTER TABLE [dbo].[UnionsDocs] CHECK CONSTRAINT [FK_UnonsDocs_Unions];


ALTER TABLE [dbo].[Auditoriums] ADD [Address] [nvarchar](250) NULL;

--PRINT N'Altering [dbo].[LeaguesDocs]...';

--ALTER TABLE [dbo].[LeaguesDocs]
--    ADD [SeasonId] INT NULL;


--PRINT N'Altering [dbo].[PlayoffBrackets]...';

--ALTER TABLE [dbo].[PlayoffBrackets]
--    ADD [SeasonId] INT NULL;


--PRINT N'Altering [dbo].[Teams]...';

--ALTER TABLE [dbo].[Teams]
--    ADD [SeasonId] INT NULL;

--ALTER TABLE [dbo].[Teams]
--	ADD [LeagueId] INT NULL;

PRINT N'Altering [dbo].[LeagueTeams]...';
ALTER TABLE [dbo].[LeagueTeams]
	ADD [SeasonId] INT NULL;

PRINT N'Altering [dbo].[TeamsPlayers]...';


ALTER TABLE [dbo].[TeamsPlayers]
    ADD [SeasonId] INT NULL;

PRINT N'Altering [dbo].[Auditoriums]...';
ALTER TABLE [LogLig].[dbo].[Auditoriums]
    ADD [SeasonId] INT NULL;

	

--Adding constraint

 IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_GroupsTeams_Groups]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
 BEGIN
  PRINT N'Creating [dbo].[FK_Games_Seasons]...';
   ALTER TABLE [dbo].[GroupsTeams] WITH NOCHECK
  ADD CONSTRAINT [FK_GroupsTeams_Groups] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]) ON DELETE CASCADE;
 END


	PRINT N'Creating [dbo].[FK_Games_Seasons]...';
    ALTER TABLE [dbo].[Games] WITH NOCHECK
    ADD CONSTRAINT [FK_Games_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

	PRINT N'Creating [dbo].[FK_Groups_Seasons]...';
    ALTER TABLE [dbo].[Groups] WITH NOCHECK
    ADD CONSTRAINT [FK_Groups_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);



    PRINT N'Creating [dbo].[FK_GroupsTeams_Seasons]...';

	ALTER TABLE [dbo].[GroupsTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_GroupsTeams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


    PRINT N'Creating [dbo].[FK_Leagues_Seasons]...';

	ALTER TABLE [dbo].[Leagues] WITH NOCHECK
    ADD CONSTRAINT [FK_Leagues_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

	--PRINT N'Creating [dbo].[FK_LeaguesDocs_Seasons]...';

	--ALTER TABLE [dbo].[LeaguesDocs] WITH NOCHECK
    --ADD CONSTRAINT [FK_LeaguesDocs_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


    --PRINT N'Creating [dbo].[FK_PlayoffBrackets_Seasons]...';

	--ALTER TABLE [dbo].[PlayoffBrackets] WITH NOCHECK
    --ADD CONSTRAINT [FK_PlayoffBrackets_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);



	--PRINT N'Creating [dbo].[FK_Teams_Seasons]...';
    --ALTER TABLE [dbo].[Teams] WITH NOCHECK
    --ADD CONSTRAINT [FK_Teams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


	PRINT N'Creating [dbo].[FK_TeamsPlayers_Seasons]...';
    ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	
	PRINT N'Creating [dbo].[FK_Auditoriums_Seasons]...';
	ALTER TABLE [LogLig].[dbo].[Auditoriums] WITH NOCHECK
    ADD CONSTRAINT [FK_Auditoriums_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [LogLig].[dbo].[Seasons] ([Id]);

PRINT N'Checking existing data against newly created constraints';

ALTER TABLE [LogLig].[dbo].[GroupsTeams] WITH CHECK CHECK CONSTRAINT [FK_GroupsTeams_Groups];

ALTER TABLE [LogLig].[dbo].[Games] WITH CHECK CHECK CONSTRAINT [FK_Games_Seasons];

ALTER TABLE [LogLig].[dbo].[Groups] WITH CHECK CHECK CONSTRAINT [FK_Groups_Seasons];

ALTER TABLE [LogLig].[dbo].[GroupsTeams] WITH CHECK CHECK CONSTRAINT [FK_GroupsTeams_Seasons];

ALTER TABLE [LogLig].[dbo].[Leagues] WITH CHECK CHECK CONSTRAINT [FK_Leagues_Seasons];

--ALTER TABLE [LogLig].[dbo].[LeaguesDocs] WITH CHECK CHECK CONSTRAINT [FK_LeaguesDocs_Seasons];

--ALTER TABLE [LogLig].[dbo].[PlayoffBrackets] WITH CHECK CHECK CONSTRAINT [FK_PlayoffBrackets_Seasons];

--ALTER TABLE [LogLig].[dbo].[Teams] WITH CHECK CHECK CONSTRAINT [FK_Teams_Seasons];

ALTER TABLE [LogLig].[dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamsPlayers_Seasons];

ALTER TABLE [LogLig].[dbo].[Auditoriums] WITH CHECK CHECK CONSTRAINT [FK_Auditoriums_Seasons];

-- CLUBS

CREATE TABLE [LogLig].[dbo].[Clubs](
	[ClubId] [int] IDENTITY(1,1) NOT NULL,
	[SectionId] [int] NOT NULL CONSTRAINT [DF_Clubs_SectionId]  DEFAULT ((1)),
	[Name] [nvarchar](50) NULL,
	[IsArchive] [bit] NOT NULL,
	[Logo] [nvarchar](50) NULL,
	[PrimaryImage] [nvarchar](50) NULL,
	[Description] [nvarchar](250) NULL,
	[IndexImage] [nvarchar](50) NULL,
	[IndexAbout] [nvarchar](2000) NULL,
	[TermsCondition] [nvarchar](50) NULL,
	[ContactPhone] [nvarchar](20) NULL,
	[Address] [nvarchar](250) NULL,
	[Email] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_Club] PRIMARY KEY CLUSTERED 
(
	[ClubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY];


ALTER TABLE [LogLig].[dbo].[Clubs]  WITH CHECK ADD  CONSTRAINT [FK_Clubs_Sections] FOREIGN KEY([SectionId])
REFERENCES [LogLig].[dbo].[Sections] ([SectionId]);

ALTER TABLE [LogLig].[dbo].[Clubs] CHECK CONSTRAINT [FK_Clubs_Sections];

CREATE TABLE [LogLig].[dbo].[ClubTeams](
	[ClubId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[SeasonId] [int] NULL,
 CONSTRAINT [PK_ClubTeams] PRIMARY KEY CLUSTERED 
(
	[ClubId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY];


ALTER TABLE [LogLig].[dbo].[ClubTeams]  WITH CHECK ADD  CONSTRAINT [FK_ClubTeams_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
ON DELETE CASCADE;

ALTER TABLE [LogLig].[dbo].[ClubTeams] CHECK CONSTRAINT [FK_ClubTeams_Clubs];

ALTER TABLE [LogLig].[dbo].[ClubTeams]  WITH CHECK ADD  CONSTRAINT [FK_ClubTeams_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
ON DELETE CASCADE;

ALTER TABLE [LogLig].[dbo].[ClubTeams] CHECK CONSTRAINT [FK_ClubTeams_Teams];

ALTER TABLE [LogLig].[dbo].[ClubTeams]  WITH CHECK ADD  CONSTRAINT [FK_ClubTeams_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [LogLig].[dbo].[ClubTeams] CHECK CONSTRAINT [FK_ClubTeams_Seasons];

--Insert initial data
INSERT INTO [dbo].[Seasons] (Name,Description,StartDate,EndDate,UnionId) SELECT '2015-2016', 'Initial season', CAST(N'2015-09-01 00:00:00' AS DateTime), CAST(N'2016-06-30 00:00:00' AS DateTime), UnionId FROM [dbo].[Unions];
PRINT N'Updating leagues...';
EXEC('UPDATE [dbo].[Leagues] SET SeasonId=(SELECT Id FROM [dbo].[Seasons] WHERE [dbo].[Seasons].UnionId=[dbo].[Leagues].UnionId);');
PRINT N'Updating Auditoriums...';
EXEC('UPDATE [dbo].[Auditoriums] SET SeasonId=(SELECT Id FROM [dbo].[Seasons] WHERE [dbo].[Seasons].[UnionId]=[dbo].[Auditoriums].[UnionId]);');
PRINT N'Updating Games...';
EXEC('UPDATE [dbo].[Games] SET SeasonId=(SELECT SeasonId FROM [dbo].[Leagues] WHERE [dbo].[Leagues].[LeagueId]=[dbo].[Games].[LeagueId]);');
PRINT N'Updating LeagueTeams...';
EXEC('UPDATE [dbo].[LeagueTeams] SET SeasonId=(SELECT SeasonId FROM [dbo].[Leagues] WHERE [dbo].[Leagues].[LeagueId]=[dbo].[LeagueTeams].[LeagueId]);');
--PRINT N'Adding new CLUB jobs...';
--BEGIN
--   IF NOT EXISTS (SELECT * FROM [dbo].[Jobs] 
--                   WHERE [dbo].[Jobs].JobName = 'Club Manager')
--   BEGIN
--       INSERT INTO [dbo].[Jobs] (SectionId, RoleId, JobName, IsArchive)
--       VALUES (1, 1, 'Club Manager')
--   END
--END

END

ELSE
 Print 'Table Season exist check if necessary tables has Foreign key'
	BEGIN
	 IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_GroupsTeams_Groups]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	BEGIN
		PRINT N'Creating [dbo].[FK_Games_Seasons]...';
	  ALTER TABLE [dbo].[GroupsTeams] WITH NOCHECK
		ADD CONSTRAINT [FK_GroupsTeams_Groups] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]) ON DELETE CASCADE;
	END


	IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Games_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	BEGIN
		PRINT N'Creating [dbo].[FK_Games_Seasons]...';
		ALTER TABLE [dbo].[Games] WITH NOCHECK
		ADD CONSTRAINT [FK_Games_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	END


	IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Groups_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	BEGIN
		PRINT N'Creating [dbo].[FK_Groups_Seasons]...';
		ALTER TABLE [dbo].[Groups] WITH NOCHECK
		ADD CONSTRAINT [FK_Groups_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	END



--	IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_GroupsTeams_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
--	BEGIN
--		PRINT N'Creating [dbo].[FK_GroupsTeams_Seasons]...';

--		ALTER TABLE [dbo].[GroupsTeams] WITH NOCHECK
--		ADD CONSTRAINT [FK_GroupsTeams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
--	END


	IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Leagues_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	BEGIN
		PRINT N'Creating [dbo].[FK_Leagues_Seasons]...';

		ALTER TABLE [dbo].[Leagues] WITH NOCHECK
		ADD CONSTRAINT [FK_Leagues_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	END

	--IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_LeaguesDocs_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	--BEGIN
	--	PRINT N'Creating [dbo].[FK_LeaguesDocs_Seasons]...';

	--	ALTER TABLE [dbo].[LeaguesDocs] WITH NOCHECK
	--	ADD CONSTRAINT [FK_LeaguesDocs_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	--END


	--IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_PlayoffBrackets_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	--BEGIN
	--	PRINT N'Creating [dbo].[FK_PlayoffBrackets_Seasons]...';

	--	ALTER TABLE [dbo].[PlayoffBrackets] WITH NOCHECK
	--	ADD CONSTRAINT [FK_PlayoffBrackets_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	--END


--	IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Teams_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
--	BEGIN
--		PRINT N'Creating [dbo].[FK_Teams_Seasons]...';
--		ALTER TABLE [dbo].[Teams] WITH NOCHECK
--		ADD CONSTRAINT [FK_Teams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
--	END

	IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_TeamsPlayers_Seasons]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
	BEGIN
		PRINT N'Creating [dbo].[FK_TeamsPlayers_Seasons]...';
		ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
		ADD CONSTRAINT [FK_TeamsPlayers_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);
	END
	
END

GO
