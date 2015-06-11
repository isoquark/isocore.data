CREATE TABLE Core.StorageKind
(
	Id TINYINT not null,
	Name nvarchar(50) not null,
	Description nvarchar(250) not null,
	constraint PK_StorageType primary key(Id),
	constraint UQ_StorageType unique(Name)
)
