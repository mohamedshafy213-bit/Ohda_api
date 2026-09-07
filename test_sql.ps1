$connStr = "Server=localhost;Database=master;Integrated Security=True;"
$con = New-Object System.Data.SqlClient.SqlConnection($connStr)
try {
    $con.Open()
    Write-Output "SUCCESS: Connected to localhost"
    $cmd = $con.CreateCommand()
    $cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OhdaDb_TEST') CREATE DATABASE OhdaDb_TEST"
    $cmd.ExecuteNonQuery()
    Write-Output "SUCCESS: OhdaDb_TEST created or already exists"
    $con.Close()
} catch {
    Write-Output "ERROR: $($_.Exception.ToString())"
}
