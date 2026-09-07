$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=OhdaDb_TEST;Integrated Security=True;")
$con.Open()
$cmd = $con.CreateCommand()
$cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Users' ORDER BY ORDINAL_POSITION"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "COL: $($reader['COLUMN_NAME']) ($($reader['DATA_TYPE']))"
}
$con.Close()
