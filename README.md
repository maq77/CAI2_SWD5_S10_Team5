# setup-dev.ps1
Write-Host "🚀 TechXpress Development Environment Setup" -ForegroundColor Cyan

# Prompt for DB connection details
$server = Read-Host "Enter SQL Server name (e.g., localhost or .\SQLEXPRESS)"
$database = Read-Host "Enter database name (e.g., TechXpressDb)"
$user = Read-Host "Enter database user (leave blank for Integrated Security)"
$password = $null

if ($user -ne "") {
    $password = Read-Host "Enter password" -AsSecureString
    $passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    )
    $connectionString = "Server=$server;Database=$database;User Id=$user;Password=$passwordPlain;TrustServerCertificate=True;"
} else {
    $connectionString = "Server=$server;Database=$database;Integrated Security=True;TrustServerCertificate=True;"
}

# Set user-secret
Write-Host "`n🔐 Storing connection string in user secrets..."
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$connectionString"

Write-Host "`n✅ Done! You can now run the app. It will use the stored connection string from your user secrets." -ForegroundColor Green
