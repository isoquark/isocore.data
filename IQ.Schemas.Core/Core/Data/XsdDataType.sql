--This is not complete, just enough to get started; see the formal spec
--or, for instance, http://www.w3schools.com/schema/schema_dtypes_string.asp
merge into [Core].XsdDataType as Dst using(values
	(1, 'string'),
	(2, 'dateTime'),
	(3, 'date'),
	(4, 'time'),
	(5, 'duration'),
	(6, 'decimal'),
	(7, 'integer'),
	(8, 'int'),
	(9, 'long')


) as Src(Id, Name) on Dst.Id = Src.Id
when matched then
	update set Name = Src.Name
when not matched by target then
	insert (Id, Name) values (Src.Id, Src.Name);