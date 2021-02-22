UPDATE [dbo].[Clubs] SET [InitialDateOfClubApproval] = [DateOfClubApproval]
WHERE [UnionId] = 36
UPDATE [dbo].[Clubs] SET [DateOfClubApproval] = NULL
WHERE [UnionId] = 36