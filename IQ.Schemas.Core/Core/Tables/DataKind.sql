CREATE TABLE Core.DataKind
(
	Id TINYINT not null,
	Name nvarchar(50) not null,
	Description nvarchar(250) not null,
	constraint PK_DataKind primary key(Id),
	constraint UQ_DataKind unique(Name)
)
