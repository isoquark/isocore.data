CREATE VIEW Metadata.[vDescription] as
	select 
		x.class_desc as ClassName,
		x.major_id as MajorId,
		x.minor_id as MinorId,
		convert(nvarchar(250), x.value) as Value
	from 
		sys.extended_properties x where x.name = 'MS_Description'
