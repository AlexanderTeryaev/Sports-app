CREATE TABLE [dbo].[ActivityBranchesState] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [ActivityBranchId] INT NOT NULL,
    [UserId]           INT NOT NULL,
    [Collapsed]        BIT NOT NULL,
    CONSTRAINT [PK_ActivityBranchesState] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityBranchesState]
    ADD CONSTRAINT [DF_ActivityBranchesState_Collapsed] DEFAULT ((0)) FOR [Collapsed];
	
ALTER TABLE [dbo].[ActivityBranchesState] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityBranchesState_ActivityBranches] FOREIGN KEY ([ActivityBranchId]) REFERENCES [dbo].[ActivityBranches] ([AtivityBranchId]);

ALTER TABLE [dbo].[ActivityBranchesState] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityBranchesState_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityBranchesState] WITH CHECK CHECK CONSTRAINT [FK_ActivityBranchesState_ActivityBranches];

ALTER TABLE [dbo].[ActivityBranchesState] WITH CHECK CHECK CONSTRAINT [FK_ActivityBranchesState_Users];