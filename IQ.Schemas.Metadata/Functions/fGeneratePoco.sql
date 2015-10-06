CREATE FUNCTION [Metadata].[fGeneratePoco](@ObjectSchema sysname, @ObjectName sysname) returns 
@Code table(Code nvarchar(MAX)) as
begin
	declare @Tab char(5) = '     '
	declare @CR char = char(13)
	declare @LF char = char(10)
	declare @CRLF char(2) = @CR + @LF

	declare @PropDocCommentTemplate nvarchar(MAX) =
	@Tab + '/// <summary>' + @CRLF +
	@Tab + '/// @Comment'  + @CRLF +
	@Tab + '/// </summary>';

	declare @CodeSegment table(Position int, Code nvarchar(max));

	insert @CodeSegment
		values(0, 'public class ' + @ObjectName + @CRLF + '{' + @CRLF);
			
	with Docs as (select
		c.ColumnName,
		case when	
		c.Description is not null and len( convert(nvarchar(250), c.Description)) <> 0 then
			replace(@PropDocCommentTemplate, '@Comment', convert(nvarchar(250), c.Description))
		else 
			replace(@PropDocCommentTemplate, 'TODO', convert(nvarchar(250), c.Description)) 
		end as Doc
	from 
		Metadata.vUserColumn c 
	where 
		c.ParentName = @ObjectName and
		c.ParentSchemaName = @ObjectSchema
	) 
	insert @CodeSegment
	select 1,
		Doc + @CRLF +
		@Tab + 'public ' + map.CSharpTypeName + ' ' + c.ColumnName + ' {get; set;} ' + @CRLF as Code
	from 
		Metadata.vUserColumn c inner join
		Metadata.AdoTypeMap map on map.SqlTypeName = c.DataTypeName left join
		Docs on Docs.ColumnName = c.ColumnName
	where 
		c.ParentName = @ObjectName and
		c.ParentSchemaName = @ObjectSchema
	order by
		c.Position

	insert @CodeSegment
		values(2, '}');		

	insert @Code
		select Code from @CodeSegment order by Position
	return

end
