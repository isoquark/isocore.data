merge into Metadata.FileFormatType as Dst using(values
	(1, 'DelimitedText', ''),
	(2, 'FixedWithText', ''),
	(3, 'ExcelWorkbook', ''),
	(4, 'XML', ''),
	(5, 'JSON', '')

) as Src(Id, Name, Description) on Dst.TypeCode = Src.Id
when matched then
	update 
		set Name = Src.Name,
			Description = src.Description
when not matched by target then
	insert (TypeCode, Name, Description) values (Src.Id, Src.Name, Src.Description);