--DROP TABLE dbo.Events
IF	(NOT EXISTS (
  SELECT  1
    FROM  INFORMATION_SCHEMA.TABLES
   WHERE  TABLE_SCHEMA = 'dbo'
     AND  TABLE_NAME   = 'Events'))
BEGIN
    PRINT 'Create "Event" table.'
	CREATE TABLE [dbo].[Events] (
        [EventId] int IDENTITY(1,1) NOT NULL,
		[LeagueId] int NOT NULL,
		[Title] nvarchar(250) NOT NULL,
		[EventTime] datetime NOT NULL,
		[Place] nvarchar(250) NOT NULL,
		[CreateDate] [datetime] NOT NULL,
		[IsPublished] [bit] NOT NULL,
		 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
		(
			[EventId] ASC
		)
	);

	ALTER TABLE [dbo].[Events] ADD  CONSTRAINT [DF_Events_CreateDate]  DEFAULT (getdate()) FOR [CreateDate];
    ALTER TABLE [dbo].[Events]  WITH CHECK ADD  CONSTRAINT [FK_Events_Leagues] FOREIGN KEY([LeagueId])
        REFERENCES [dbo].[Leagues] ([LeagueId]);
	ALTER TABLE [dbo].[Events] ADD  DEFAULT ((0)) FOR [IsPublished];
END
ELSE
    PRINT '"Event" table already exists. Skip table creation.'
GO				