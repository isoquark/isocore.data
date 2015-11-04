CREATE TABLE [Metadata].[TabularFile]
(
	SemanticFileTypeCode int not null,
	FileFormatTypeCode int not null,
	constraint FK_TabularFile_FileFormatType foreign key(FileFormatTypeCode) references Metadata.FileFormatType(TypeCode),
	constraint FK_TabularFile_SemanticFileType foreign key(SemanticFileTypeCode) references Metadata.SemanticFileType(TypeCode),
	constraint PK_TabularFile primary key(SemanticFileTypeCode, FileFormatTypeCode)
	
)
