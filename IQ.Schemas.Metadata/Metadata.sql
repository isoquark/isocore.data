CREATE SCHEMA [Metadata]

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Defines views and functions to facilitate convenient metadata extraction from catalog views',
    @level0type = N'SCHEMA',
    @level0name = N'Metadata',
    @level1type = null,
    @level1name = null,
    @level2type = NULL,
    @level2name = NULL
GO
