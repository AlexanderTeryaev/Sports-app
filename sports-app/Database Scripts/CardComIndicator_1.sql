CREATE TABLE [dbo].[CardComIndicators] (
    [Id]                   INT              IDENTITY (1, 1) NOT NULL,
    [LowProfileCode]       UNIQUEIDENTIFIER NOT NULL,
    [TerminalNumber]       INT              NOT NULL,
    [Operation]            INT              NOT NULL,
    [ActivityId]           INT              NULL,
    [ApiUserName]          NVARCHAR (255)   NULL,
    [CardComIndicatorInfo] NVARCHAR (MAX)   NULL,
    [DateCreated]          DATETIME         NOT NULL,
    CONSTRAINT [PK_CardComIndicators] PRIMARY KEY CLUSTERED ([Id] ASC)
);