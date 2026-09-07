$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=OhdaDb_TEST;Integrated Security=True;")
$con.Open()
$cmd = $con.CreateCommand()
$cmd.CommandText = "SELECT Id, Username, Email, Role, UserGroupId FROM Users"
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output "User: $($r['Username']) | Email: $($r['Email']) | Role: $($r['Role']) | UserGroupId: $($r['UserGroupId'])"
}
$con.Close()
