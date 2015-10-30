CREATE TABLE [Metadata].[DataMatrix]
(
	DataMatrixName nvarchar(128) not null,
	DataMatrixIdentifier nvarchar(128) not null,


	constraint PK_DataMatrix primary key(DataMatrixName)
)
