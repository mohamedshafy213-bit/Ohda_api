$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=master;Integrated Security=True;")
$con.Open()
$cmd = $con.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.databases"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "DATABASE: $($reader['name'])"
}
$con.Close()
