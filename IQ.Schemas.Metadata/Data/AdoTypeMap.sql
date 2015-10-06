MERGE INTO [Metadata].AdoTypeMap AS Target
USING (VALUES
  ('bigint','System.Int64', 'long', 'BigInt','GetSqlInt64','Int64','GetInt64')
 ,('binary','System.Byte[]', 'byte', 'VarBinary','GetSqlBinary','Binary','GetBytes')
 ,('bit','System.Boolean', 'bool', 'Bit','GetSqlBoolean','Boolean','GetBoolean')
 ,('char','System.String', 'char', 'Char','GetSqlString','AnsiStringFixedLength','GetString')
 ,('date','System.DateTime', 'System.DateTime', 'Date','GetSqlDateTime','Date','GetDateTime')
 ,('datetime','System.DateTime','System.DateTime', 'DateTime','GetSqlDateTime','DateTime','GetDateTime')
 ,('datetime2','System.DateTime','System.DateTime', 'DateTime2','None','DateTime2','GetDateTime')
 ,('datetimeoffset','System.DateTimeOffset', 'System.DateTimeOffset', 'DateTimeOffset','none','DateTimeOffset','GetDateTimeOffset')
 ,('decimal','System.Decimal', 'decimal', 'Decimal','GetSqlDecimal','Decimal','GetDecimal')
 ,('float','System.Double', 'double', 'Float','GetSqlDouble','Double','GetDouble')
 ,('image','System.Byte[]', 'byte[]', 'Binary','GetSqlBinary','Binary','GetBytes')
 ,('int','System.Int32', 'int', 'Int','GetSqlInt32','Int32','GetInt32')
 ,('money','System.Decimal', 'decimal', 'Money','GetSqlMoney','Decimal','GetDecimal')
 ,('nchar','System.String', 'string', 'NChar','GetSqlString','StringFixedLength','GetString')
 ,('ntext','System.String', 'string', 'NText','GetSqlString','String','GetString')
 ,('numeric','System.Decimal', 'decimal', 'Decimal','GetSqlDecimal','Decimal','GetDecimal')
 ,('nvarchar','System.String', 'string', 'NVarChar','GetSqlString','String','GetString')
 ,('real','System.Single','Real', 'float', 'GetSqlSingle','Single','GetFloat')
 ,('rowversion','System.Byte[]', 'byte[]', 'Timestamp','GetSqlBinary','Binary','GetBytes')
 ,('smalldatetime','System.DateTime', 'System.DateTime', 'DateTime','GetSqlDateTime','DateTime','GetDateTime')
 ,('smallint','System.Int16', 'short', 'SmallInt','GetSqlInt16','Int16','GetInt16')
 ,('smallmoney','System.Decimal', 'decimal', 'SmallMoney','GetSqlMoney','Decimal','GetDecimal')
 ,('sql_variant','System.Object', 'object', 'Variant','GetSqlValue','Object','GetValue')
 ,('text','System.String', 'string', 'Text','GetSqlString','String','GetString')
 ,('time','System.TimeSpan', 'System.TimeSpan', 'Time','none','Time','GetDateTime')
 ,('timestamp','System.Byte[]', 'byte[]', 'Timestamp','GetSqlBinary','Binary','GetBytes')
 ,('tinyint','System.Byte', 'byte', 'TinyInt','GetSqlByte','Byte','GetByte')
 ,('uniqueidentifier','System.Guid', 'System.Guid', 'UniqueIdentifier','GetSqlGuid','Guid','GetGuid')
 ,('varbinary','System.Byte[]', 'byte[]', 'VarBinary','GetSqlBinary','Binary','GetBytes')
 ,('varchar','System.String', 'string', 'VarChar','GetSqlString','AnsiString','GetString')
 ,('xml','Xml', 'string', 'Xml','GetSqlXml','Xml','none')
 ,('geography', 'Microsoft.SqlServer.Types.SqlGeography','Microsoft.SqlServer.Types.SqlGeography','none', 'none', 'none', 'none')
 ,('hierarchyid', 'Microsoft.SqlServer.Types.SqlHierarchyId', 'Microsoft.SqlServer.Types.SqlHierarchyId', 'none', 'none', 'none', 'none')
 ,('geometry', 'Microsoft.SqlServer.Types.SqlGeometry', 'Microsoft.SqlServer.Types.SqlGeometry','none', 'none', 'none', 'none')
 ,('sysname','System.String', 'string', 'NText','GetSqlString','String','GetString')

) AS Source ([EngineTypeName],[BclTypeName],[CSharpTypeName],[SqlDbTypeEnum],[SqlDbTypeDataReader],[DbTypeEnum],[DbTypeDataReader])
ON (Target.SqlTypeName = Source.[EngineTypeName])
WHEN MATCHED AND (
	NULLIF(Source.[BclTypeName], Target.[BclTypeName]) IS NOT NULL OR NULLIF(Target.[BclTypeName], Source.[BclTypeName]) IS NOT NULL OR 
	NULLIF(Source.[CSharpTypeName], Target.[CSharpTypeName]) IS NOT NULL OR NULLIF(Target.[CSharpTypeName], Source.[CSharpTypeName]) IS NOT NULL OR 
	NULLIF(Source.[SqlDbTypeEnum], Target.[SqlDbTypeEnum]) IS NOT NULL OR NULLIF(Target.[SqlDbTypeEnum], Source.[SqlDbTypeEnum]) IS NOT NULL OR 
	NULLIF(Source.[SqlDbTypeDataReader], Target.[SqlDbTypeDataReader]) IS NOT NULL OR NULLIF(Target.[SqlDbTypeDataReader], Source.[SqlDbTypeDataReader]) IS NOT NULL OR 
	NULLIF(Source.[DbTypeEnum], Target.[DbTypeEnum]) IS NOT NULL OR NULLIF(Target.[DbTypeEnum], Source.[DbTypeEnum]) IS NOT NULL OR 
	NULLIF(Source.[DbTypeDataReader], Target.[DbTypeDataReader]) IS NOT NULL OR NULLIF(Target.[DbTypeDataReader], Source.[DbTypeDataReader]) IS NOT NULL) THEN
 UPDATE SET
  [BclTypeName] = Source.[BclTypeName], 
  CSharpTypeName = Source.CSharpTypeName,
  [SqlDbTypeEnum] = Source.[SqlDbTypeEnum], 
  [SqlDbTypeDataReader] = Source.[SqlDbTypeDataReader], 
  [DbTypeEnum] = Source.[DbTypeEnum], 
  [DbTypeDataReader] = Source.[DbTypeDataReader]
WHEN NOT MATCHED BY TARGET THEN
 INSERT(SqlTypeName,[BclTypeName],CSharpTypeName,[SqlDbTypeEnum],[SqlDbTypeDataReader],[DbTypeEnum],[DbTypeDataReader])
 VALUES(Source.[EngineTypeName],Source.[BclTypeName], Source.CSharpTypeName, Source.[SqlDbTypeEnum],Source.[SqlDbTypeDataReader],Source.[DbTypeEnum],Source.[DbTypeDataReader]);