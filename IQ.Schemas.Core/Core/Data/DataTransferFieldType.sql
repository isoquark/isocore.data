merge into [Core].DataTransferFieldType as Dst using(values
	(1, 'binary'),
	(2, 'boolean'),
	(3, 'uint8'),
	(4, 'int8'),
	(5, 'int16'),
	(6, 'uint16'),
	(7, 'int32'),
	(8, 'uint32'),
	(9, 'int64'),
	(10, 'uint64'),
	(11, 'float32'),
	(12, 'float64'),
	(13, 'decimal'),
	(14, 'guid'),
	(15, 'datetime'),
	(16, 'timespan'),
	(17, 'datetimeOffset'),
	(18, 'text')
) as Src(Id, Name) on Dst.Id = Src.Id
when matched then
	update set Name = Src.Name
when not matched by target then
	insert (Id, Name) values (Src.Id, Src.Name);