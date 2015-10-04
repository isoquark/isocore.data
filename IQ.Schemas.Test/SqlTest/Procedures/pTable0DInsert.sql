CREATE PROCEDURE [SqlTest].[pTable0DInsert](@Records SqlTest.TableType01 readonly) as
begin

insert SqlTest.Table0D(Col01, Col02, Col03)
	select TTCol01, TTCol02, TTCol03 from @Records

end
