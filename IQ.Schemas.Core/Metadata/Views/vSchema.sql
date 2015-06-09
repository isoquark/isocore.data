CREATE VIEW Metadata.vSchema as
	select 
		x.schema_id as SchemaId,
		x.name as SchemaName,
		d.Value as Desription
	from 
		sys.schemas x
		left join Metadata.vDescription d on d.MajorId = x.schema_id 
	where
		x.name not in('db_owner',
			'db_accessadmin',
			'db_securityadmin',
			'db_ddladmin',
			'db_backupoperator',
			'db_datareader',
			'db_datawriter',
			'db_denydatareader',
			'db_denydatawriter',
			'guest')
