CREATE TABLE [Metadata].TabularFileColumn
(
	SemanticFileTypeCode int not null,
	FileFormatTypeCode int not null,
	DataMatrixIdentifier nvarchar(128) not null,
	ColumnPosition int not null,
	ColumnIdentifier nvarchar(128) not null,
	SourceColumnName nvarchar(128) not null,
	TargetColumnName nvarchar(128) not null,
	DataTypeSchema sysname not null,
	DataTypeName sysname not null,
	IsNullable bit not null,
	MaxLength int not null,
	Precision tinyint not null,
	Scale tinyint not null,
	ColumnDescription nvarchar(250) not null,


	constraint PK_DataMatrixColumn primary key(SemanticFileTypeCode, FileFormatTypeCode, DataMatrixIdentifier, ColumnPosition),
	constraint FK_DataMatrixColumn_DataMatrix foreign key(SemanticFileTypeCode, FileFormatTypeCode,DataMatrixIdentifier) references [Metadata].[TabularFileMatrix](SemanticFileTypeCode, FileFormatTypeCode,DataMatrixIdentifier) on update cascade on delete cascade
)
