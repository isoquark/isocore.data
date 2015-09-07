create view Metadata.vObject as
with Objects as (select 
		DB_ID() as CatalogId,
		DB_NAME() as CatalogName,
		x.schema_id as SchemaId,
		x.object_id as ObjectId,
		x.name as ObjectName,
		x.parent_object_id as ParentObjectId,
		x.type_desc as ObjectType,
		isnull(convert(bit,case x.is_ms_shipped 
			when 0 then 1
			when 1 then 0 end), 0) as IsUserDefined
	from 
		sys.all_objects x 
)
select 
	[o].CatalogId,
	[o].CatalogName,
	[o].[SchemaId], 
	[s].[name] as SchemaName, 
	[o].[ObjectId], 
	[o].[ObjectName], 
	[o].[ParentObjectId], 
	[o].[ObjectType], 
		isnull(
			case 
				when o.IsUserDefined = 1 and s.name ='Metadata' then 0
				else o.IsUserDefined
			end , 0)as IsUserDefined
from Objects o 
	left join sys.schemas s on s.schema_id = o.SchemaId