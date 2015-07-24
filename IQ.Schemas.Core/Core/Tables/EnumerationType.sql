CREATE TABLE [Core].[EnumerationType] (
    [Id]             INT            NOT NULL,
    [WsdlDocumentId] INT            NOT NULL,
    [Name]           NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_EnumerationType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EnumerationType_WsdlDocument] FOREIGN KEY ([WsdlDocumentId]) REFERENCES [Core].[WsdlDocument] ([Id])
);

