merge into Core.BclType as Dst using(values
	(1, 'System.Object'),
	(2, 'System.Boolean'),
	(3, 'System.Byte'),
	(4, 'System.SByte'),
	(5, 'System.Int16'),
	(6, 'System.UInt16'),
	(7, 'System.Int32'),
	(8, 'System.UInt32'),
	(9, 'System.Int64'),
	(10, 'System.UInt64'),
	(11, 'System.Single'),
	(12, 'System.Double'),
	(13, 'System.Decimal'),
	(14, 'System.Guid'),
	(15, 'System.DateTime'),
	(16, 'System.TimeSpan'),
	(17, 'System.DateTimeOffset'),
	(18, 'System.String'),
	(19, 'System.Char')
) as Src(Id, FullName) on Dst.Id = Src.Id
when matched then
	update set FullName = Src.FullName
when not matched by target then
	insert (Id, FullName) values (Src.Id, Src.FullName);