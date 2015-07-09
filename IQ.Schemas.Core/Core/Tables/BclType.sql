CREATE TABLE Core.[BclType]
(
	[Id] INT NOT NULL,
	FullName nvarchar(250),
	constraint PK_BclType primary key(Id),
	constraint UQ_BclType unique(FullName)


)
