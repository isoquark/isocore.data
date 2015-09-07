CREATE VIEW Metadata.vSchema as
	select 
		DB_ID() as CatalogId,
		DB_NAME() as CatalogName,
		x.schema_id as SchemaId,
		x.name as SchemaName,
		d.Value as Description,
		isnull(case 
			when x.name in ('dbo', 'sys', 'INFORMATION_SCHEMA') then convert(bit, 0)
			else convert(bit, 1)
		end,0) as IsUserDefined
	from 
		sys.schemas x
		left join Metadata.vDescription d on d.MajorId = x.schema_id and d.ClassName='SCHEMA'
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
