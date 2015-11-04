CREATE VIEW [Metadata].vTabularFileMatrix as
	select 
		m.SemanticFileTypeCode,
		s.Name as SemanticFileTypeName,
		m.FileFormatTypeCode,
		f.Name as FileFormatTypeName,
		[m].[DataMatrixIdentifier], 
		[m].[DataMatrixName],
		[c].[ColumnPosition], 
		[c].[ColumnIdentifier], 
		[c].[SourceColumnName], 
		[c].[TargetColumnName], 
		[c].[DataTypeSchema], 
		[c].[DataTypeName], 
		[c].[IsNullable], 
		[c].[MaxLength], 
		[c].[Precision], 
		[c].[Scale], 
		[c].[ColumnDescription]
	from 
		[Metadata].TabularFileMatrix m 
		inner join 
			[Metadata].TabularFileColumn c 
		on 
			c.SemanticFileTypeCode = m.SemanticFileTypeCode and
			c.FileFormatTypeCode = m.FileFormatTypeCode and
			c.DataMatrixIdentifier = m.DataMatrixIdentifier
		inner join
			[Metadata].SemanticFileType s on s.TypeCode = c.SemanticFileTypeCode
		inner join
			[Metadata].FileFormatType f on f.TypeCode = c.FileFormatTypeCode

