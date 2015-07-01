merge into Core.DataKind as Dst using(values
	(0, 'Unspecified', 'Storage type is unknown'),
	(10, 'Bit', 'A single bit with may either have the value 0 or 1'),
	(20, 'UInt8', 'An unsigned 8-bit integer'),
	(21, 'UInt16', 'An unsigned 16-bit integer'),
	(22, 'UInt32', 'An unsigned 32-bit integer'),
	(23, 'UInt64', 'An unsigned 64-bit integer'),
	
	(30, 'Int8', 'A signed 8-bit integer'),
	(31, 'Int16', 'A signed 16-bit integer'),
	(32, 'Int32', 'A signed 32-bit integer'),
	(33, 'Int64', 'A signed 64-bit integer'),

	(40, 'BinaryFixed', 'A fixed-length sequence of bytes'),
	(41, 'BinaryVariable', 'A variable-length sequence of bytes with some specified upper-bound'),
	(42, 'BinaryMax', 'An arbitrarily long, variable-length sequence of bytes'),

	(50, 'AnsiTextFixed', 'A fixed-length block of ANSI text'),
	(51, 'AnsiTextVariable', 'A variable-length block of ANSI text with some specified upper-bound'),
	(52, 'AnsiTextMax', 'An arbitrarily long, variable-length block of ANSI text with'),
	
	(53, 'UnicodeTextFixed', 'A fixed-length block of Unicode text'),
	(54, 'UnicodeVariable', 'A variable-length block of Unicode text with some specified upper-bound'),
	(55, 'UnicodeTextMax', 'A arbitrarily long, variable-length block of Unicode text'),

	(60, 'DateTime32', 'A 32-bit date'),
	(61, 'DateTime64', 'A 64-bit date'),
	(62, 'DateTime', 'Corresponds to the datetime2 SQL data type'),
	(63, 'DateTimeOffset', 'A datetime offset'),
	(64, 'TimeOfDay', 'The time of day based on a 24-hour clock'),
	(65, 'Date', 'A date with no time-of-day component'),

	(70, 'Float32', 'A 32-bit floating-point number'),
	(71, 'Float64', 'A 64-bit floating-point number'),
	
	(80, 'Decimal', 'A decimal number'),
	(81, 'Money', 'A currency value'),
	
	(90, 'Guid', 'A 128-bit value which is presumably unique throughout time and space'),
	(100, 'Xml', 'An XML document'),
	(110, 'Variant', 'Corresponds to sql_variant'),
	
	(150, 'CustomTable', 'A custom tabular data type'),
	(151, 'CustomObject', 'A custom CLR type'),
	(152, 'CustomPrimitive', 'A custom primitive based on an intrinsic type'),

	(160, 'Geography', 'A spatial data type that represents geographic information'),
	(161, 'Geometry', 'A spatial data type that represents geometry information'),
	(162, 'Hierarchy', 'A path in a hierarchy')

) as Src(Id, Name, Description) on Dst.Id = Src.Id
when matched then
	update set Name = Src.Name, Description = Src.Description
when not matched by target then
	insert (Id, Name, Description) values (Src.Id, Src.Name, Src.Description);
	