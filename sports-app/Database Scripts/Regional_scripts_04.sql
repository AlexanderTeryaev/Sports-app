GO
/****** Object:  StoredProcedure [dbo].[DeleteRegional]    Script Date: 02/14/2019 01:01:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[DeleteRegional]
	@regionalid int 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  Update Regionals set IsArchive = 1 where RegionalId = @regionalid;
  Update UsersJobs set RegionalId = NULL where RegionalId = @regionalid;
  Update Clubs set RegionalId = NULL, DateOfClubApprovalByRegional= NULL,
  IsClubApproveByRegional = 0 where RegionalId = @regionalid;
  
  SELECT * FROM Regionals where RegionalId = @regionalid;

END

-----------

