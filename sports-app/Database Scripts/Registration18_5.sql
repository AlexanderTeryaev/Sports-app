CREATE TABLE [dbo].[MedicalCertApprovements] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [UserId]   INT NOT NULL,
    [SeasonId] INT NOT NULL,
    [Approved] BIT NOT NULL,
    CONSTRAINT [PK_MedicalCertApprovements] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[MedicalCertApprovements] WITH NOCHECK
    ADD CONSTRAINT [FK_MedicalCertApprovements_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[MedicalCertApprovements] WITH NOCHECK
    ADD CONSTRAINT [FK_MedicalCertApprovements_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[MedicalCertApprovements] WITH CHECK CHECK CONSTRAINT [FK_MedicalCertApprovements_Seasons];

ALTER TABLE [dbo].[MedicalCertApprovements] WITH CHECK CHECK CONSTRAINT [FK_MedicalCertApprovements_Users];