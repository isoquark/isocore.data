CREATE FUNCTION [Metadata].[fTabularFileMatrix](@SemanticFileTypeCode int, @FileFormatTypeCode int) returns table as return
select 
	[f].[SemanticFileTypeCode], 
	[f].[SemanticFileTypeName], 
	[f].[FileFormatTypeCode], 
	[f].[FileFormatTypeName], 
	[f].[DataMatrixIdentifier], 
	[f].[DataMatrixName], 
	[f].[ColumnPosition], 
	[f].[ColumnIdentifier], 
	[f].[SourceColumnName], 
	[f].[TargetColumnName], 
	[f].[DataTypeSchema], 
	[f].[DataTypeName], 
	[f].[IsNullable], 
	[f].[MaxLength], 
	[f].[Precision], 
	[f].[Scale], 
	[f].[ColumnDescription]
from 
	Metadata.vTabularFileMatrix f 
where 
	f.SemanticFileTypeCode = @SemanticFileTypeCode and f.FileFormatTypeCode = @FileFormatTypeCode
