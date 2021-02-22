-- Relate auditorium with discipline type for climbing section
Alter table [dbo].[Auditoriums]
Add 
	DisciplineId int null


ALTER TABLE [dbo].[Auditoriums] ADD CONSTRAINT [FKEY_AUDI_DISCID] FOREIGN KEY ([DisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

-- Senior competition checkbox for Tennis leagues 
Alter table [dbo].[Leagues]
Add 
	IsSeniorCompetition bit null Default(0)


-- Team managers notifications on league level
Alter table [dbo].[NotesMessages]
Add 
	SentForTeamManagers bit null Default(0)


-- Authorized signature on club level - additional data
Alter table [dbo].[Clubs]
Add 
	AuthorizedSignPersonName varchar(MAX) null,
	SignEachSeparately bit null Default(0),
	SignTogether bit null Default(0)


-- Event on union level
Alter table [dbo].[Events]
Add
	UnionId int null, 
	EventImage varchar(MAX) null,
	EventDescription varchar(MAX) null