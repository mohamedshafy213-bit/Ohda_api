$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=OhdaDb_TEST;Integrated Security=True;")
$con.Open()
$cmd = $con.CreateCommand()
$cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "TABLE: $($reader['TABLE_NAME'])"
}
$con.Close()
