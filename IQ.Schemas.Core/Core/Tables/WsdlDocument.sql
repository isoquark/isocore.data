CREATE TABLE [Core].[WsdlDocument] (
    [Id]          INT NOT NULL,
    [WsdlContent] XML NOT NULL,
    CONSTRAINT [PK_WsdlDocument] PRIMARY KEY CLUSTERED ([Id] ASC)
);

