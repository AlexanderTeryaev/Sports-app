INSERT INTO [dbo].[Sections] VALUES ('1','Multi-sport','multi-sport',0);
INSERT INTO [dbo].[JobsRoles] VALUES(11,'Department manager','departmentmgr',5);
ALTER TABLE [dbo].[Clubs] ADD ParentClubId INT NULL FOREIGN KEY (ParentClubId) REFERENCES Clubs(ClubId)
ALTER TABLE [dbo].[Clubs] ADD SportSectionId INT NULL FOREIGN KEY (SportSectionId) REFERENCES Sections(SectionId)