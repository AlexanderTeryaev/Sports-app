ALTER TABLE [dbo].[Activities]
    ADD [AllowCompetitiveMembers] BIT CONSTRAINT [DF_Activities_AllowCompetitiveMembers] DEFAULT ((0)) NOT NULL;
ALTER TABLE [dbo].[Users]
    ADD [IsCompetitiveMember] BIT CONSTRAINT [DF_Users_IsCompetitiveMember] DEFAULT ((0)) NOT NULL;