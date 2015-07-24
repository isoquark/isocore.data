CREATE TABLE [Core].[EnumerationValue] (
    [Id]                INT            NOT NULL,
    [EnumerationTypeId] INT            NOT NULL,
    [Name]              NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_EnumerationValue] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EnumerationValue_EnumerationType] FOREIGN KEY ([EnumerationTypeId]) REFERENCES [Core].[EnumerationType] ([Id])
);

