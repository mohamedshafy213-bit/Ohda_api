$script = Get-Content -Raw "C:\Users\m.abdalshafy\Desktop\ohdanew\Ohda_api\create_schema.sql"
$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=OhdaDb_TEST;Integrated Security=True;")
$con.Open()

# Split into batches by GO
$batches = $script -split "(?m)^\s*GO\s*$"

foreach ($batch in $batches) {
    $trimmed = $batch.Trim()
    if (![string]::IsNullOrWhiteSpace($trimmed)) {
        $cmd = $con.CreateCommand()
        $cmd.CommandText = $trimmed
        try {
            $cmd.ExecuteNonQuery() > $null
        } catch {
            Write-Output "Error executing batch: $($_.Exception.Message)"
        }
    }
}
Write-Output "ALL MIGRATION BATCHES EXECUTED!"
$con.Close()
