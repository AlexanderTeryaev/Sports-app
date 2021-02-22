ALTER TABLE [dbo].[Activities]
    ADD [ParticipationPrice] BIT CONSTRAINT [DF_Activities_ParticipationPrice] DEFAULT ((0)) NOT NULL;