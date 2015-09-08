CREATE VIEW [Metadata].vProperty as
	select 
		x.class_desc as ClassName,
		x.major_id as MajorId,
		x.minor_id as MinorId,
		x.name as PropertyName,
		x.value as PropertyValue
	from 
		sys.extended_properties x where x.name in ('MS_Description','MD_KEY')
