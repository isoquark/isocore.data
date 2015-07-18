MERGE INTO [Metadata].[IntrinsicTypeMap] AS Target
USING (VALUES
  ('bigint','Int64','BigInt','GetSqlInt64','Int64','GetInt64')
 ,('binary','Byte[]','VarBinary','GetSqlBinary','Binary','GetBytes')
 ,('bit','Boolean','Bit','GetSqlBoolean','Boolean','GetBoolean')
 ,('char','String','Char','GetSqlString','AnsiStringFixedLength','GetString')
 ,('date','DateTime','Date','GetSqlDateTime','Date','GetDateTime')
 ,('datetime','DateTime','DateTime','GetSqlDateTime','DateTime','GetDateTime')
 ,('datetime2','DateTime','DateTime2','None','DateTime2','GetDateTime')
 ,('datetimeoffset','DateTimeOffset','DateTimeOffset','none','DateTimeOffset','GetDateTimeOffset')
 ,('decimal','Decimal','Decimal','GetSqlDecimal','Decimal','GetDecimal')
 ,('float','Double','Float','GetSqlDouble','Double','GetDouble')
 ,('image','Byte[]','Binary','GetSqlBinary','Binary','GetBytes')
 ,('int','Int32','Int','GetSqlInt32','Int32','GetInt32')
 ,('money','Decimal','Money','GetSqlMoney','Decimal','GetDecimal')
 ,('nchar','String','NChar','GetSqlString','StringFixedLength','GetString')
 ,('ntext','String','NText','GetSqlString','String','GetString')
 ,('numeric','Decimal','Decimal','GetSqlDecimal','Decimal','GetDecimal')
 ,('nvarchar','String','NVarChar','GetSqlString','String','GetString')
 ,('real','Single','Real','GetSqlSingle','Single','GetFloat')
 ,('rowversion','Byte[]','Timestamp','GetSqlBinary','Binary','GetBytes')
 ,('smalldatetime','DateTime','DateTime','GetSqlDateTime','DateTime','GetDateTime')
 ,('smallint','Int16','SmallInt','GetSqlInt16','Int16','GetInt16')
 ,('smallmoney','Decimal','SmallMoney','GetSqlMoney','Decimal','GetDecimal')
 ,('sql_variant','Object ','Variant','GetSqlValue','Object','GetValue')
 ,('text','String','Text','GetSqlString','String','GetString')
 ,('time','TimeSpan','Time','none','Time','GetDateTime')
 ,('timestamp','Byte[]','Timestamp','GetSqlBinary','Binary','GetBytes')
 ,('tinyint','Byte','TinyInt','GetSqlByte','Byte','GetByte')
 ,('uniqueidentifie','Guid','UniqueIdentifier','GetSqlGuid','Guid','GetGuid')
 ,('varbinary','Byte[]','VarBinary','GetSqlBinary','Binary','GetBytes')
 ,('varchar','String','VarChar','GetSqlString','AnsiString','GetString')
 ,('xml','Xml','Xml','GetSqlXml','Xml','none')
) AS Source ([EngineTypeName],[BclTypeName],[SqlDbTypeEnum],[SqlDbTypeDataReader],[DbTypeEnum],[DbTypeDataReader])
ON (Target.[EngineTypeName] = Source.[EngineTypeName])
WHEN MATCHED AND (
	NULLIF(Source.[BclTypeName], Target.[BclTypeName]) IS NOT NULL OR NULLIF(Target.[BclTypeName], Source.[BclTypeName]) IS NOT NULL OR 
	NULLIF(Source.[SqlDbTypeEnum], Target.[SqlDbTypeEnum]) IS NOT NULL OR NULLIF(Target.[SqlDbTypeEnum], Source.[SqlDbTypeEnum]) IS NOT NULL OR 
	NULLIF(Source.[SqlDbTypeDataReader], Target.[SqlDbTypeDataReader]) IS NOT NULL OR NULLIF(Target.[SqlDbTypeDataReader], Source.[SqlDbTypeDataReader]) IS NOT NULL OR 
	NULLIF(Source.[DbTypeEnum], Target.[DbTypeEnum]) IS NOT NULL OR NULLIF(Target.[DbTypeEnum], Source.[DbTypeEnum]) IS NOT NULL OR 
	NULLIF(Source.[DbTypeDataReader], Target.[DbTypeDataReader]) IS NOT NULL OR NULLIF(Target.[DbTypeDataReader], Source.[DbTypeDataReader]) IS NOT NULL) THEN
 UPDATE SET
  [BclTypeName] = Source.[BclTypeName], 
  [SqlDbTypeEnum] = Source.[SqlDbTypeEnum], 
  [SqlDbTypeDataReader] = Source.[SqlDbTypeDataReader], 
  [DbTypeEnum] = Source.[DbTypeEnum], 
  [DbTypeDataReader] = Source.[DbTypeDataReader]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([EngineTypeName],[BclTypeName],[SqlDbTypeEnum],[SqlDbTypeDataReader],[DbTypeEnum],[DbTypeDataReader])
 VALUES(Source.[EngineTypeName],Source.[BclTypeName],Source.[SqlDbTypeEnum],Source.[SqlDbTypeDataReader],Source.[DbTypeEnum],Source.[DbTypeDataReader]);