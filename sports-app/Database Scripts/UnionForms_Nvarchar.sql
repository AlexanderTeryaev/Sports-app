ALTER TABLE [dbo].[UnionForms] DROP CONSTRAINT [DF__UnionForm__IsDel__00FF1D08];

ALTER TABLE [dbo].[UnionForms] DROP CONSTRAINT [FK_UnionsForForms];

ALTER TABLE [dbo].[UnionForms] DROP CONSTRAINT [FK_SeasonsForForms];

BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_UnionForms] (
    [FormId]    INT            IDENTITY (1, 1) NOT NULL,
    [UnionId]   INT            NOT NULL,
    [SeasonId]  INT            NOT NULL,
    [Title]     NVARCHAR (150) NULL,
    [FilePath]  NVARCHAR (MAX) NULL,
    [IsDeleted] BIT            CONSTRAINT [DF__UnionForm__IsDel__00FF1D08] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK__UnionFor__FB05B7DD186D7C551] PRIMARY KEY CLUSTERED ([FormId] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[UnionForms])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_UnionForms] ON;
        INSERT INTO [dbo].[tmp_ms_xx_UnionForms] ([FormId], [UnionId], [SeasonId], [Title], [FilePath], [IsDeleted])
        SELECT   [FormId],
                 [UnionId],
                 [SeasonId],
                 [Title],
                 [FilePath],
                 [IsDeleted]
        FROM     [dbo].[UnionForms]
        ORDER BY [FormId] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_UnionForms] OFF;
    END

DROP TABLE [dbo].[UnionForms];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_UnionForms]', N'UnionForms';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK__UnionFor__FB05B7DD186D7C551]', N'PK__UnionFor__FB05B7DD186D7C55', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

ALTER TABLE [dbo].[UnionForms] WITH NOCHECK
    ADD CONSTRAINT [FK_UnionsForForms] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]);

ALTER TABLE [dbo].[UnionForms] WITH NOCHECK
    ADD CONSTRAINT [FK_SeasonsForForms] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[UnionForms] WITH CHECK CHECK CONSTRAINT [FK_UnionsForForms];

ALTER TABLE [dbo].[UnionForms] WITH CHECK CHECK CONSTRAINT [FK_SeasonsForForms];