CREATE TABLE [Core].[DataTransferObject]
(
	[Id] INT NOT NULL,

	DisplayName nvarchar(100) not null,
	LogicalName nvarchar(100) not null,
	LogicalNamespace nvarchar(300) not null,
    Description NVARCHAR(300) NULL,
	
	constraint PK_DataTransferObject primary key(Id), 

)
