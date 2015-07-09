merge into Core.DataKind as Dst using(values
	(0, 'Unspecified', 'Storage type is unknown', 'none'),
	(10, 'Bit', 'A single bit with may either have the value 0 or 1', 'bit'),
	(20, 'UInt8', 'An unsigned 8-bit integer', 'uint8'),
	(21, 'UInt16', 'An unsigned 16-bit integer', 'uint16'),
	(22, 'UInt32', 'An unsigned 32-bit integer', 'uint32'),
	(23, 'UInt64', 'An unsigned 64-bit integer', 'uint32'),
	
	(30, 'Int8', 'A signed 8-bit integer', 'int8'),
	(31, 'Int16', 'A signed 16-bit integer', 'int16'),
	(32, 'Int32', 'A signed 32-bit integer', 'int32'),
	(33, 'Int64', 'A signed 64-bit integer', 'int64'),

	(40, 'BinaryFixed', 'A fixed-length sequence of bytes', 'binf'),
	(41, 'BinaryVariable', 'A variable-length sequence of bytes with some specified upper-bound', 'binv'),
	(42, 'BinaryMax', 'An arbitrarily long, variable-length sequence of bytes', 'binm'),

	(50, 'AnsiTextFixed', 'A fixed-length block of ANSI text', 'atextf'),
	(51, 'AnsiTextVariable', 'A variable-length block of ANSI text with some specified upper-bound', 'atextv'),
	(52, 'AnsiTextMax', 'An arbitrarily long, variable-length block of ANSI text with', 'atextm'),
	
	(53, 'UnicodeTextFixed', 'A fixed-length block of Unicode text', 'utextf'),
	(54, 'UnicodeVariable', 'A variable-length block of Unicode text with some specified upper-bound', 'utextv'),
	(55, 'UnicodeTextMax', 'A arbitrarily long, variable-length block of Unicode text', 'utextm'),

	(62, 'DateTime', 'Corresponds to the datetime2 SQL data type', 'datetime'),
	(63, 'DateTimeOffset', 'A datetime offset', 'tdoffset'),
	(64, 'TimeOfDay', 'The time of day based on a 24-hour clock', 'tod'),
	(65, 'Date', 'A date with no time-of-day component', 'date'),
	(66, 'Duration', 'A calendar-independent length of time', 'duration'),

	(70, 'Float32', 'A 32-bit floating-point number', 'float32'),
	(71, 'Float64', 'A 64-bit floating-point number', 'float64'),
	
	(80, 'Decimal', 'A decimal number', 'decimal'),
	(81, 'Money', 'A currency value', 'money'),
	
	(90, 'Guid', 'A 128-bit value which is presumably unique throughout time and space', 'guid'),
	(100, 'Xml', 'An XML document', 'xml'),
	(101, 'Json', 'An Json document', 'json'),
	(110, 'Flexible', 'Corresponds to sql_variant', 'flexible'),
	
	(150, 'Geography', 'A spatial data type that represents geographic information', 'geography'),
	(151, 'Geometry', 'A spatial data type that represents geometry information', 'geometry'),
	(152, 'Hierarchy', 'A path in a hierarchy', 'hierarchy'),

	(160, 'TypedDocument', 'A structured document of some sort', 'tdoc'),

	(170, 'CustomTable', 'A custom tabular data type', 'ctable'),
	(171, 'CustomObject', 'A custom CLR type', 'cobject'),
	(172, 'CustomPrimitive', 'A custom primitive based on an intrinsic type', 'cprimitive')

) as Src(Id, Name, Description, ShortName) on Dst.Id = Src.Id
when matched then
	update set Name = Src.Name, Description = Src.Description
when not matched by target then
	insert (Id, Name, Description, ShortName) values (Src.Id, Src.Name, Src.Description, shortName);
	