ALTER TABLE [dbo].[Clubs]
    ADD [SportType] INT NULL;
	
ALTER TABLE [dbo].[Unions]
    ADD [SportType] INT NULL;

CREATE TABLE [dbo].[Sports] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (50) NOT NULL,
    [NameHeb]   NVARCHAR (50) NOT NULL,
    [SectionId] INT           NOT NULL,
    CONSTRAINT [PK_Sports] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[Sports] WITH NOCHECK
    ADD CONSTRAINT [FK_Sports_Sections] FOREIGN KEY ([SectionId]) REFERENCES [dbo].[Sections] ([SectionId]);

ALTER TABLE [dbo].[Clubs] WITH NOCHECK
    ADD CONSTRAINT [FK_Clubs_Sports] FOREIGN KEY ([SportType]) REFERENCES [dbo].[Sports] ([Id]);

ALTER TABLE [dbo].[Unions] WITH NOCHECK
    ADD CONSTRAINT [FK_Unions_Sports] FOREIGN KEY ([SportType]) REFERENCES [dbo].[Sports] ([Id]);

ALTER TABLE [dbo].[Sports] WITH CHECK CHECK CONSTRAINT [FK_Sports_Sections];

ALTER TABLE [dbo].[Clubs] WITH CHECK CHECK CONSTRAINT [FK_Clubs_Sports];

ALTER TABLE [dbo].[Unions] WITH CHECK CHECK CONSTRAINT [FK_Unions_Sports];