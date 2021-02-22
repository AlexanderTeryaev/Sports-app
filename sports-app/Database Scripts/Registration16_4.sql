ALTER TABLE [dbo].[Activities]
    ADD [AllowOnlyApprovedMembers] BIT CONSTRAINT [DF_Activities_AllowOnlyApprovedMembers] DEFAULT ((0)) NOT NULL;