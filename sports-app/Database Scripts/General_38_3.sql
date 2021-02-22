ALTER TABLE [dbo].[Stages]
    ADD [CreateCrossesStage] BIT CONSTRAINT [DF_Stages_CreateCrossesStage] DEFAULT ((0)) NOT NULL,
        [IsCrossesStage]     BIT CONSTRAINT [DF_Stages_IsCrossesStage] DEFAULT ((0)) NOT NULL,
        [ParentStageId]      INT NULL;

ALTER TABLE [dbo].[Stages] WITH NOCHECK
    ADD CONSTRAINT [FK_Stages_Stages] FOREIGN KEY ([ParentStageId]) REFERENCES [dbo].[Stages] ([StageId]);

ALTER TABLE [dbo].[Stages] WITH CHECK CHECK CONSTRAINT [FK_Stages_Stages];