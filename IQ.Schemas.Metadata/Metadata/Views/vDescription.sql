CREATE VIEW Metadata.[vDescription] as
	select 
		x.major_id as MajorId,
		x.minor_id as MinorId,
		x.value as Value
	from 
		sys.extended_properties x where x.name = 'MS_Description'
