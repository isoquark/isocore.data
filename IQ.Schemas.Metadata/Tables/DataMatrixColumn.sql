CREATE TABLE [Metadata].[DataMatrixColumn]
(
	DataMatrixName nvarchar(128) not null,
	ColumnName nvarchar(128) not null,
	ColumnIdentifier nvarchar(128) not null,
	Position int not null,
	DataTypeSchema sysname not null,
	DataTypeName sysname not null,
	IsNullable bit not null,
	MaxLength int not null,
	Precision tinyint not null,
	Scale tinyint not null,
	ColumnDescription nvarchar(250) not null,


	constraint PK_DataMatrixColumn primary key(DataMatrixName, ColumnName),
	constraint FK_DataMatrixColumn_DataMatrix foreign key(DataMatrixName) references [Metadata].[DataMatrix](DataMatrixName),
	constraint UQ_DataMatrixColumn_Identifier unique(DataMatrixName, ColumnIdentifier),
	constraint UQ_DataMatrixColumn_Name unique(DataMatrixName, ColumnName),
	constraint UQ_DataMatrixColumn_Position unique(DataMatrixName, Position)
)
