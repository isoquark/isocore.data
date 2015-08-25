MERGE INTO [Metadata].AdoTypeMap AS Target
USING (VALUES
  ('bigint','System.Int64','BigInt','GetSqlInt64','Int64','GetInt64')
 ,('binary','System.Byte[]','VarBinary','GetSqlBinary','Binary','GetBytes')
 ,('bit','System.Boolean','Bit','GetSqlBoolean','Boolean','GetBoolean')
 ,('char','System.String','Char','GetSqlString','AnsiStringFixedLength','GetString')
 ,('date','System.DateTime','Date','GetSqlDateTime','Date','GetDateTime')
 ,('datetime','System.DateTime','DateTime','GetSqlDateTime','DateTime','GetDateTime')
 ,('datetime2','System.DateTime','DateTime2','None','DateTime2','GetDateTime')
 ,('datetimeoffset','System.DateTimeOffset','DateTimeOffset','none','DateTimeOffset','GetDateTimeOffset')
 ,('decimal','System.Decimal','Decimal','GetSqlDecimal','Decimal','GetDecimal')
 ,('float','System.Double','Float','GetSqlDouble','Double','GetDouble')
 ,('image','System.Byte[]','Binary','GetSqlBinary','Binary','GetBytes')
 ,('int','System.Int32','Int','GetSqlInt32','Int32','GetInt32')
 ,('money','System.Decimal','Money','GetSqlMoney','Decimal','GetDecimal')
 ,('nchar','System.String','NChar','GetSqlString','StringFixedLength','GetString')
 ,('ntext','System.String','NText','GetSqlString','String','GetString')
 ,('numeric','System.Decimal','Decimal','GetSqlDecimal','Decimal','GetDecimal')
 ,('nvarchar','System.String','NVarChar','GetSqlString','String','GetString')
 ,('real','System.Single','Real','GetSqlSingle','Single','GetFloat')
 ,('rowversion','System.Byte[]','Timestamp','GetSqlBinary','Binary','GetBytes')
 ,('smalldatetime','System.DateTime','DateTime','GetSqlDateTime','DateTime','GetDateTime')
 ,('smallint','System.Int16','SmallInt','GetSqlInt16','Int16','GetInt16')
 ,('smallmoney','System.Decimal','SmallMoney','GetSqlMoney','Decimal','GetDecimal')
 ,('sql_variant','System.Object ','Variant','GetSqlValue','Object','GetValue')
 ,('text','System.String','Text','GetSqlString','String','GetString')
 ,('time','System.TimeSpan','Time','none','Time','GetDateTime')
 ,('timestamp','System.Byte[]','Timestamp','GetSqlBinary','Binary','GetBytes')
 ,('tinyint','System.Byte','TinyInt','GetSqlByte','Byte','GetByte')
 ,('uniqueidentifie','System.Guid','UniqueIdentifier','GetSqlGuid','Guid','GetGuid')
 ,('varbinary','System.Byte[]','VarBinary','GetSqlBinary','Binary','GetBytes')
 ,('varchar','System.String','VarChar','GetSqlString','AnsiString','GetString')
 ,('xml','Xml','Xml','GetSqlXml','Xml','none')
) AS Source ([EngineTypeName],[BclTypeName],[SqlDbTypeEnum],[SqlDbTypeDataReader],[DbTypeEnum],[DbTypeDataReader])
ON (Target.SqlTypeName = Source.[EngineTypeName])
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
 INSERT(SqlTypeName,[BclTypeName],[SqlDbTypeEnum],[SqlDbTypeDataReader],[DbTypeEnum],[DbTypeDataReader])
 VALUES(Source.[EngineTypeName],Source.[BclTypeName],Source.[SqlDbTypeEnum],Source.[SqlDbTypeDataReader],Source.[DbTypeEnum],Source.[DbTypeDataReader]);