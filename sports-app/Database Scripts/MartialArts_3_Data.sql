/*
This script was created by Visual Studio on 31.05.2017 at 9:07 PM.
This script performs its actions in the following order:
1. Disable foreign-key constraints.
2. Perform DELETE commands. 
3. Perform UPDATE commands.
4. Perform INSERT commands.
5. Re-enable foreign-key constraints.
Please back up your target database before running this script.
*/
SET NUMERIC_ROUNDABORT OFF
GO
SET XACT_ABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
/*Pointer used for text / image updates. This might not be needed, but is declared here just in case*/
DECLARE @pv binary(16)
BEGIN TRANSACTION
ALTER TABLE [dbo].[SportRanks] DROP CONSTRAINT [FK_SportRanks_Sports]
ALTER TABLE [dbo].[Sports] DROP CONSTRAINT [FK_Sports_Sections]
ALTER TABLE [dbo].[PlayerAchievements] DROP CONSTRAINT [FK_PlayerAchievements_Users]
ALTER TABLE [dbo].[PlayerAchievements] DROP CONSTRAINT [FK_PlayerAchievements_SportRanks]
ALTER TABLE [dbo].[Clubs] DROP CONSTRAINT [FK_Clubs_Sports]
ALTER TABLE [dbo].[Unions] DROP CONSTRAINT [FK_Unions_Sports]
SET IDENTITY_INSERT [dbo].[Sports] ON
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (1, N'Karate', N'קארטה', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (2, N'Judo', N'ג''ודו', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (3, N'Jujutsu', N'ג''וג''יטסו', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (4, N'Capoeira', N'קפוארה', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (5, N'Krav Maga', N'קרב מגע', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (6, N'Sambo', N'סמבו', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (7, N'Boxing', N'אגרוף', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (8, N'Wrestling', N'האבקות', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (9, N'Sumo', N'סומו', 12)
INSERT INTO [dbo].[Sports] ([Id], [Name], [NameHeb], [SectionId]) VALUES (11, N'Aikido', N'אייקידו', 12)
SET IDENTITY_INSERT [dbo].[Sports] OFF
SET IDENTITY_INSERT [dbo].[SportRanks] ON
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (9, 1, N'Q8 – yellow', N'Q8 – צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (10, 1, N'Q7 – orange', N'Q7 – כתום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (11, 1, N'Q6 – green', N'Q6 – ירוק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (12, 1, N'Q5 – purple', N'Q5 – סגול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (13, 1, N'Q4 – blue', N'Q4 – כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (14, 1, N'Q3 – brown', N'Q3 – חום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (15, 1, N'Q2 – brown', N'Q2 – חום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (16, 1, N'Q1 – brown', N'Q1 – חום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (17, 1, N'Dan 1 – black', N'DAN 1 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (18, 1, N'Dan 2 – black', N'DAN 2 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (19, 1, N'Dan 3 – black', N'DAN 3 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (20, 1, N'Dan 4 – black', N'DAN 4 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (21, 1, N'Dan 5 – black', N'DAN 5 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (23, 1, N'Dan 6 – black', N'DAN 6 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (24, 1, N'Dan 7 – black', N'DAN 7 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (25, 1, N'Dan 8 – black', N'DAN 8 – שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (26, 1, N'Dan 9 – black', N'DAN 9 - שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (27, 2, N'KYU9-white', N'KYU9-לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (28, 2, N'KYU8-white/yellow', N'KYU8-לבן/צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (29, 2, N'KYU7-yellow', N'KYU7-צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (30, 2, N'KYU6-yellow/orange', N'KYU6-צהוב/ כתום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (31, 2, N'KYU5-orange', N'KYU5-כתום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (32, 2, N'KYU4-orange/green', N'KYU4-כתום/ירוק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (33, 2, N'KYU3-green', N'KYU3-ירוק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (34, 2, N'KYU2-blue', N'KYU2-כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (35, 2, N'KYU1-brown', N'KYU1-חום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (36, 2, N'DAN1-black', N'DAN1-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (37, 2, N'DAN2-black', N'DAN2-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (38, 2, N'DAN3-black', N'DAN3-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (39, 2, N'DAN4-black', N'DAN4-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (40, 2, N'DAN5-black', N'DAN5-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (41, 2, N'DAN6-black', N'DAN6-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (42, 2, N'DAN7-black', N'DAN7-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (43, 2, N'DAN8-black', N'DAN8-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (44, 2, N'DAN9-black', N'DAN9-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (45, 2, N'DAN10-black', N'DAN10-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (46, 4, N'1-green', N'1-ירוק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (47, 4, N'2-green/yellow', N'2-ירוק/צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (48, 4, N'3—yellow', N'3—צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (49, 4, N'4-green/blue', N'4-ירוק/כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (50, 4, N'5-yellow/blue', N'5-צהוב/כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (51, 4, N'6-blue', N'6-כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (52, 4, N'7-strong green/yellow/blue', N'7-ירוק חזק/צהוב/כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (53, 4, N'8-green/strong yellow/blue', N'8-ירוק/צהוב חזק/כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (54, 4, N'9-green/yellow/strong blue', N'9-ירוק/צהוב/כחול חזק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (55, 4, N'10-green/white', N'10-ירוק/לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (56, 4, N'11-yellow/white', N'11-צהוב/לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (57, 4, N'12-blue/white', N'12-כחול/לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (58, 4, N'13-white (master)', N'13-לבן (מסטר)')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (59, 5, N'1-yellow', N'1-צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (60, 5, N'2-orange', N'2-כתום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (61, 5, N'3-green', N'3-ירוק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (62, 5, N'4-blue', N'4-כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (63, 5, N'5-brown', N'5-חום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (64, 5, N'DAN1-black', N'DAN1-שחור')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (65, 5, N'DAN2-black/2 red lines', N'DAN2-שחור/אדום 2 פסים')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (66, 5, N'DAN3-black/3 red lines', N'DAN3-שחור/אדום 3 פסים')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (67, 5, N'DAN4-black/red scope', N'DAN4-שחור/היקף אדום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (68, 5, N'DAN5-black/red', N'DAN5-שחור/אדום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (69, 5, N'DAN6-red/white', N'DAN6-אדום/לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (70, 5, N'DAN7-red/white', N'DAN7-אדום/לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (71, 5, N'DAN8-red', N'DAN8-אדום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (72, 5, N'DAN9-red', N'DAN9-אדום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (73, 5, N'DAN10-red', N'DAN10-אדום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (74, 3, N'U16-1-white', N'U16-1-לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (75, 3, N'U16-2-yellow', N'U16-2-צהוב')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (76, 3, N'U16-3-orange', N'U16-3-כתום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (77, 3, N'U16-4-green', N'U16-4-ירוק')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (78, 3, N'White', N'לבן')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (79, 3, N'Blue', N'כחול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (80, 3, N'Purple', N'סגול')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (81, 3, N'Brown', N'חום')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (82, 3, N'Black-1 strip', N'שחור פס 1')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (83, 3, N'Black-2 strips', N'שחור 2 פסים')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (84, 3, N'Black-3-6 stripes', N'שחור 3-6 פסים')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (85, 3, N'Black/red-7-8 stripes', N'שחור/אדום 7-8 פסים')
INSERT INTO [dbo].[SportRanks] ([Id], [SportId], [RankName], [RankNameHeb]) VALUES (86, 3, N'Black-9-10 stripes', N'שחור 9-10 פסים')
SET IDENTITY_INSERT [dbo].[SportRanks] OFF
ALTER TABLE [dbo].[SportRanks]
    ADD CONSTRAINT [FK_SportRanks_Sports] FOREIGN KEY ([SportId]) REFERENCES [dbo].[Sports] ([Id])
ALTER TABLE [dbo].[Sports]
    ADD CONSTRAINT [FK_Sports_Sections] FOREIGN KEY ([SectionId]) REFERENCES [dbo].[Sections] ([SectionId])
ALTER TABLE [dbo].[PlayerAchievements]
    ADD CONSTRAINT [FK_PlayerAchievements_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId])
ALTER TABLE [dbo].[PlayerAchievements]
    ADD CONSTRAINT [FK_PlayerAchievements_SportRanks] FOREIGN KEY ([RankId]) REFERENCES [dbo].[SportRanks] ([Id])
ALTER TABLE [dbo].[Clubs]
    ADD CONSTRAINT [FK_Clubs_Sports] FOREIGN KEY ([SportType]) REFERENCES [dbo].[Sports] ([Id])
ALTER TABLE [dbo].[Unions]
    ADD CONSTRAINT [FK_Unions_Sports] FOREIGN KEY ([SportType]) REFERENCES [dbo].[Sports] ([Id])
COMMIT TRANSACTION
