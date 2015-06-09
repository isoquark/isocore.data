CREATE TABLE SqlTest.[Table01]
(
	Col01 INT NOT NULL PRIMARY KEY,
	Col02 bigint,
	Col03 nvarchar(50) NOT NULL,
	Col04 nvarchar(Max)
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'SQL Test Table01',
    @level0type = N'SCHEMA',
    @level0name = N'SqlTest',
    @level1type = N'TABLE',
    @level1name = N'Table01',
    @level2type = NULL,
    @level2name = NULL
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Col01 Descripiton Text',
    @level0type = N'SCHEMA',
    @level0name = N'SqlTest',
    @level1type = N'TABLE',
    @level1name = N'Table01',
    @level2type = N'COLUMN',
    @level2name = N'Col01'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Col03 Descripiton Text',
    @level0type = N'SCHEMA',
    @level0name = N'SqlTest',
    @level1type = N'TABLE',
    @level1name = N'Table01',
    @level2type = N'COLUMN',
    @level2name = N'Col03'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Col04 Descripiton Text',
    @level0type = N'SCHEMA',
    @level0name = N'SqlTest',
    @level1type = N'TABLE',
    @level1name = N'Table01',
    @level2type = N'COLUMN',
    @level2name = N'Col04'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Col02 Descripiton Text',
    @level0type = N'SCHEMA',
    @level0name = N'SqlTest',
    @level1type = N'TABLE',
    @level1name = N'Table01',
    @level2type = N'COLUMN',
    @level2name = N'Col02'