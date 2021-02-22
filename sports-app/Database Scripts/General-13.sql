ALTER TABLE [dbo].[Users] ADD [CompetitiveLicenseNumber] VARCHAR(255) NULL
ALTER TABLE [dbo].[Users] ADD [LicenseValidity] DATETIME NULL
ALTER TABLE [dbo].[Users] ADD [LicenseLevel] VARCHAR(255) NULL
ALTER TABLE [dbo].[Schools] ADD [IsCamp] BIT NOT NULL DEFAULT 0

CREATE TABLE [dbo].[VehicleProduct](
	[Id] [int] NOT NULL,
	[Type] varchar(255) NOT NULL,
	[Name] varchar(255) NOT NULL

 CONSTRAINT [PK_RehicleProduct] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[VehicleModel](
	[Id] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Name] varchar(255) NOT NULL

 CONSTRAINT [PK_VehicleModel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[VehicleModel]  WITH CHECK ADD  CONSTRAINT [FK_VehicleModel_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[VehicleProduct] ([Id])
GO


CREATE TABLE [dbo].[DriverLicenseTypes](
	[Id] [int] NOT NULL,
	[Name] varchar(255) NOT NULL,
	[Type] VARCHAR(255) NOT NULL

 CONSTRAINT [PK_DriverLicenseTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[DriverDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SportsmanId] [int] NOT NULL,
	[OwnerShipDate] [datetime] NOT NULL,
	[IssueDate] [datetime] NOT NULL,
	[NumberOfPreviousOwners] [int] NOT NULL


 CONSTRAINT [PK_DriverDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DriverDetails]  WITH CHECK ADD  CONSTRAINT [FK_DriverDetails_Sportsman] FOREIGN KEY([SportsmanId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO


CREATE TABLE [dbo].[VehicleLicenses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LicenseNumber] varchar(255) NOT NULL,
	[Valid] [datetime] NOT NULL



 CONSTRAINT [PK_VehicleLicenses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[VehicleDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] varchar(255) NOT NULL,
	[ProductId] int NOT NULL,
	[ModelId] int NOT NULL,
	[YearOfProduction] datetime NOT NULL,
	[Weight] float NOT NULL,
	[ChassisNo] varchar(255) NOT NULL,
	[DriverLivenceTypeId] int NOT NULL



 CONSTRAINT [PK_VehicleDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[VehicleDetails]  WITH CHECK ADD  CONSTRAINT [FK_VehicleDetails_VehicleProduct] FOREIGN KEY([ProductId])
REFERENCES [dbo].[VehicleProduct] ([Id])
GO

ALTER TABLE [dbo].[VehicleDetails]  WITH CHECK ADD  CONSTRAINT [FK_VehicleDetails_VehicleModel] FOREIGN KEY([DriverLivenceTypeId])
REFERENCES [dbo].[VehicleModel] ([Id])
GO

ALTER TABLE [dbo].[VehicleDetails]  WITH CHECK ADD  CONSTRAINT [FK_VehicleDetails_DriverLicenseTypes] FOREIGN KEY([DriverLivenceTypeId])
REFERENCES [dbo].[DriverLicenseTypes] ([Id])
GO


CREATE TABLE [dbo].[EngineDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EngineNo] int NOT NULL,
	[EngineVolume] varchar(255) NOT NULL,
	[EngineProduct] varchar(255) NOT NULL,
	[MaxPowerHp] float NOT NULL,
	[TermsAndConditions] varchar(max) NOT NULL,
	[NumberOfImportEnrty] int NOT NULL



 CONSTRAINT [PK_EngineDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[Vehicles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DriverDetailsId] [int] NULL,
	[VehicleLicenseId] [int] NOT NULL,
	[VehicleDetailsId] [int] NOT NULL,
	[EngineDetailsId] [int] NOT NULL


 CONSTRAINT [PK_Vehicles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD CONSTRAINT [FK_Vehicles_DriverDetails] FOREIGN KEY([DriverDetailsId])
REFERENCES [dbo].[DriverDetails] ([Id])
GO

ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD CONSTRAINT [FK_Vehicles_VehicleLicense] FOREIGN KEY([VehicleLicenseId])
REFERENCES [dbo].[VehicleLicenses] ([Id])
GO

ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD CONSTRAINT [FK_Vehicles_VehicleDetails] FOREIGN KEY([VehicleDetailsId])
REFERENCES [dbo].[VehicleDetails] ([Id])
GO

ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD CONSTRAINT [FK_Vehicles_EngineDetails] FOREIGN KEY([EngineDetailsId])
REFERENCES [dbo].[EngineDetails] ([Id])
GO

ALTER TABLE [dbo].[Vehicles] ADD [SeasonId] INT NOT NULL
ALTER TABLE [dbo].[Vehicles] ADD [UnionId] INT NOT NULL

ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD CONSTRAINT [FK_Vehicles_Union] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
GO

ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD CONSTRAINT [FK_Vehicles_Season] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[Vehicles] ADD [IsDeleted] BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[EngineDetails] ALTER COLUMN EngineNo VARCHAR(255) NOT NULL