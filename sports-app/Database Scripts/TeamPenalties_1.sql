CREATE TABLE [dbo].[TeamPenalties] (
    [Id]      INT             IDENTITY (1, 1) NOT NULL,
    [StageId] INT             NOT NULL,
    [TeamId]  INT             NOT NULL,
    [Points]  DECIMAL (18, 2) NOT NULL,
    [Date]    DATETIME        NOT NULL,
    CONSTRAINT [PK_TeamPenalties] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[TeamPenalties] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamPenalties_Stages] FOREIGN KEY ([StageId]) REFERENCES [dbo].[Stages] ([StageId]);

ALTER TABLE [dbo].[TeamPenalties] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamPenalties_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[TeamPenalties] WITH CHECK CHECK CONSTRAINT [FK_TeamPenalties_Stages];

ALTER TABLE [dbo].[TeamPenalties] WITH CHECK CHECK CONSTRAINT [FK_TeamPenalties_Teams];