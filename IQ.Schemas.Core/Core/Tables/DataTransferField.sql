CREATE TABLE [Core].DataTransferField
(
	Id int, 
	DataTransferObjectId int not null,
	DataTypeId int not null,
	Position smallint not null,
	DisplayName nvarchar(100) not null,
	LogicalName nvarchar(100) not null,	
    Description NVARCHAR(300) null,	
	IsNullable bit not null, 
	Length int null,
	Precision tinyint null,
	Scale tinyint null,

    constraint PK_DataTransferField primary key(Id), 
    CONSTRAINT FK_DataTransferField_DataTransferObject FOREIGN KEY (DataTransferObjectId) REFERENCES [Core].DataTransferObject(Id),
	CONSTRAINT FK_DataTransferField_DataType FOREIGN KEY (DataTypeId) REFERENCES [Core].DataTransferFieldType(Id)

)
