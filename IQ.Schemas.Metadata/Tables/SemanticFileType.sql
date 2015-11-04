CREATE TABLE Metadata.SemanticFileType
(
	TypeCode int NOT NULL,
	Name nvarchar(50) NOT NULL,
	Description nvarchar(250)
	
	constraint PK_FileType primary key(TypeCode) NOT NULL,
	constraint UQ_FileType unique(Name)
)
