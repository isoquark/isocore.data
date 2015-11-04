CREATE TABLE [Metadata].[TabularFileMatrix]
(
	SemanticFileTypeCode int not null,
	FileFormatTypeCode int not null,
	DataMatrixIdentifier nvarchar(128) not null,
	DataMatrixName nvarchar(128) not null,

	constraint PK_TabularFileMatrix primary key(SemanticFileTypeCode, FileFormatTypeCode, DataMatrixIdentifier),
	constraint FK_TabularFileMatrix_TabularFile foreign key(SemanticFileTypeCode, FileFormatTypeCode) references [Metadata].[TabularFile](SemanticFileTypeCode, FileFormatTypeCode) on update cascade,
)
