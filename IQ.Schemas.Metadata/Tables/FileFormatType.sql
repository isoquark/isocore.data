CREATE TABLE [Metadata].[FileFormatType]
(
	TypeCode int NOT NULL,
	Name nvarchar(50) NOT NULL,
	Description nvarchar(250)
	
	constraint PK_FileFormatType primary key(TypeCode) NOT NULL,
	constraint UQ_FileFormatType unique(Name)
)
