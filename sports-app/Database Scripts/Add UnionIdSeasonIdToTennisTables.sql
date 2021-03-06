ALTER TABLE [dbo].[CompetitionAges]
  ADD UnionId [int]
ALTER TABLE [dbo].[CompetitionAges]
  ADD SeasonId [int]

ALTER TABLE [dbo].[CompetitionAges]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionAges_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[CompetitionAges]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionAges_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[CompetitionLevel]
  ADD UnionId [int]
ALTER TABLE [dbo].[CompetitionLevel]
  ADD SeasonId [int]

ALTER TABLE [dbo].[CompetitionLevel]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionLevel_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[CompetitionLevel]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionLevel_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[CompetitionRegion]
  ADD UnionId [int]
ALTER TABLE [dbo].[CompetitionRegion]
  ADD SeasonId [int]

ALTER TABLE [dbo].[CompetitionRegion]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionRegion_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[CompetitionRegion]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionRegion_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO