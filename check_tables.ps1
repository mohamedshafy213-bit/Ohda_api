$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=OhdaDb_TEST;Integrated Security=True;")
try {
    $con.Open()
    $cmd = $con.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
    $count = $cmd.ExecuteScalar()
    Write-Output "TableCount: $count"
    $con.Close()
} catch {
    Write-Output "Error: $($_.Exception.Message)"
}
