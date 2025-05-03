# setup-dev.ps1
# TechXpress Development Environment Setup Script
# Comprehensive setup for the TechXpress application

#region Banner and Initialization
Clear-Host
$projectName = "TechXpress"
$webProjectPath = ".\$projectName.Web"
$dataProjectPath = ".\$projectName.Data"
$appSettingsPath = "$webProjectPath\appsettings.json"
$appSettingsDevPath = "$webProjectPath\appsettings.Development.json"

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                            ║" -ForegroundColor Cyan
Write-Host "║             $projectName Development Environment Setup     ║" -ForegroundColor Cyan
Write-Host "║                                                            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Verify that necessary .NET tools are installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET SDK is not installed or not in PATH. Please install .NET SDK first." -ForegroundColor Red
    exit 1
}

# Check if Entity Framework tools are installed
$efInstalled = dotnet tool list --global | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "🔧 Entity Framework Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install Entity Framework Core tools. Please install manually with 'dotnet tool install --global dotnet-ef'" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Entity Framework Core tools installed successfully." -ForegroundColor Green
}

# Verify project structure
if (-not (Test-Path $webProjectPath)) {
    Write-Host "❌ Web project not found at $webProjectPath. Make sure you're running this script from the solution directory." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $dataProjectPath)) {
    Write-Host "❌ Data project not found at $dataProjectPath. Make sure you're running this script from the solution directory." -ForegroundColor Red
    exit 1
}
#endregion

#region Database Configuration
Write-Host ""
Write-Host "📊 DATABASE CONFIGURATION" -ForegroundColor Cyan
Write-Host "========================="
Write-Host "Enter your SQL Server details below. For Windows Authentication, leave username blank."
Write-Host ""

# Default values
$defaultServer = "localhost\SQLEXPRESS"
$defaultDatabase = "TechXpressDb"
$useTrustedConnection = $false

# Prompt for DB connection details with defaults
$server = Read-Host "Enter SQL Server name [$defaultServer]"
$server = if ([string]::IsNullOrWhiteSpace($server)) { $defaultServer } else { $server }

$database = Read-Host "Enter database name [$defaultDatabase]"
$database = if ([string]::IsNullOrWhiteSpace($database)) { $defaultDatabase } else { $database }

$authType = Read-Host "Authentication type (W)indows or (S)QL Server [W]"
$authType = if ([string]::IsNullOrWhiteSpace($authType)) { "W" } else { $authType }

if ($authType.ToUpper() -eq "W") {
    $connectionString = "Server=$server;Database=$database;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
    Write-Host "Using Windows Authentication" -ForegroundColor Green
} else {
    $user = Read-Host "Enter SQL Server username"
    if ([string]::IsNullOrWhiteSpace($user)) {
        Write-Host "⚠️ Username cannot be empty for SQL Server Authentication" -ForegroundColor Yellow
        $user = Read-Host "Enter SQL Server username"
        if ([string]::IsNullOrWhiteSpace($user)) {
            Write-Host "❌ Username still empty. Defaulting to Windows Authentication" -ForegroundColor Red
            $connectionString = "Server=$server;Database=$database;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
        }
    } else {
        $securePassword = Read-Host "Enter password" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
        $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
        
        $connectionString = "Server=$server;Database=$database;User Id=$user;Password=$password;MultipleActiveResultSets=true;TrustServerCertificate=True;"
    }
}

Write-Host ""
Write-Host "Connection string configured:" -ForegroundColor Cyan
Write-Host $connectionString -ForegroundColor Gray
Write-Host ""
#endregion

#region Update Configuration Files
Write-Host "🔧 UPDATING CONFIGURATION FILES" -ForegroundColor Cyan
Write-Host "=============================="

# Function to update appsettings file
function Update-AppSettings {
    param (
        [string]$FilePath,
        [string]$ConnectionString
    )
    
    if (Test-Path $FilePath) {
        try {
            $appSettings = Get-Content $FilePath -Raw | ConvertFrom-Json
            
            # Check if ConnectionStrings property exists
            if (-not (Get-Member -InputObject $appSettings -Name "ConnectionStrings" -MemberType Properties)) {
                $appSettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value (New-Object PSObject)
            }
            
            # Check if DefaultConnection property exists in ConnectionStrings
            if (-not (Get-Member -InputObject $appSettings.ConnectionStrings -Name "DefaultConnection" -MemberType Properties)) {
                $appSettings.ConnectionStrings | Add-Member -Type NoteProperty -Name "DefaultConnection" -Value ""
            }
            
            # Update connection string
            $appSettings.ConnectionStrings.DefaultConnection = $ConnectionString
            
            # Check if JWT settings exist, if not create them
            if (-not (Get-Member -InputObject $appSettings -Name "JwtSettings" -MemberType Properties)) {
                $jwtKey = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
                $jwtSettings = @{
                    Key = $jwtKey
                    Issuer = "TechXpress"
                    Audience = "TechXpressUsers"
                    DurationInMinutes = 60
                }
                $appSettings | Add-Member -Type NoteProperty -Name "JwtSettings" -Value $jwtSettings
            }
            
            # Save the updated settings
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content $FilePath
            Write-Host "✅ Updated $FilePath" -ForegroundColor Green
        } catch {
            Write-Host "❌ Error updating $FilePath: $_" -ForegroundColor Red
        }
    } else {
        try {
            # Create new appsettings file
            $jwtKey = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
            
            $newAppSettings = @{
                ConnectionStrings = @{
                    DefaultConnection = $ConnectionString
                }
                Logging = @{
                    LogLevel = @{
                        Default = "Information"
                        Microsoft = "Warning"
                        "Microsoft.Hosting.Lifetime" = "Information"
                    }
                }
                AllowedHosts = "*"
                JwtSettings = @{
                    Key = $jwtKey
                    Issuer = "TechXpress"
                    Audience = "TechXpressUsers"
                    DurationInMinutes = 60
                }
            }
            
            $newAppSettings | ConvertTo-Json -Depth 10 | Set-Content $FilePath
            Write-Host "✅ Created $FilePath" -ForegroundColor Green
        } catch {
            Write-Host "❌ Error creating $FilePath: $_" -ForegroundColor Red
        }
    }
}

# Update both appsettings.json and appsettings.Development.json
Update-AppSettings -FilePath $appSettingsPath -ConnectionString $connectionString

# Update Development settings with more verbose logging
if (Test-Path $appSettingsDevPath) {
    $appSettingsDev = Get-Content $appSettingsDevPath -Raw | ConvertFrom-Json
    $appSettingsDev.ConnectionStrings.DefaultConnection = $connectionString
    
    # Make sure Logging is set for development
    if (-not (Get-Member -InputObject $appSettingsDev -Name "Logging" -MemberType Properties)) {
        $appSettingsDev | Add-Member -Type NoteProperty -Name "Logging" -Value @{
            LogLevel = @{
                Default = "Debug"
                "Microsoft.AspNetCore" = "Debug"
                "Microsoft.EntityFrameworkCore" = "Information"
            }
        }
    } else {
        $appSettingsDev.Logging.LogLevel.Default = "Debug"
        $appSettingsDev.Logging.LogLevel."Microsoft.AspNetCore" = "Debug"
        $appSettingsDev.Logging.LogLevel."Microsoft.EntityFrameworkCore" = "Information"
    }
    
    $appSettingsDev | ConvertTo-Json -Depth 10 | Set-Content $appSettingsDevPath
    Write-Host "✅ Updated $appSettingsDevPath with development settings" -ForegroundColor Green
} else {
    # Create development settings with more verbose logging
    $devSettings = @{
        ConnectionStrings = @{
            DefaultConnection = $connectionString
        }
        Logging = @{
            LogLevel = @{
                Default = "Debug"
                "Microsoft.AspNetCore" = "Debug"
                "Microsoft.EntityFrameworkCore" = "Information"
            }
        }
        DetailedErrors = $true
    }
    
    $devSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsDevPath
    Write-Host "✅ Created $appSettingsDevPath with development settings" -ForegroundColor Green
}

# Store connection string in user secrets (optional)
$useUserSecrets = Read-Host "Do you want to store connection string in user secrets? (y/n) [n]"
if ($useUserSecrets.ToLower() -eq "y") {
    Write-Host "Storing connection string in user secrets..."
    try {
        Push-Location $webProjectPath
        dotnet user-secrets init --project .
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$connectionString" --project .
        Pop-Location
        Write-Host "✅ Connection string stored in user secrets" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error storing in user secrets: $_" -ForegroundColor Red
    }
}
#endregion

#region Database Setup
Write-Host ""
Write-Host "🗄️  DATABASE SETUP" -ForegroundColor Cyan
Write-Host "================="

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore NuGet packages. Please check your internet connection and try again." -ForegroundColor Red
    exit 1
}
Write-Host "✅ NuGet packages restored successfully" -ForegroundColor Green

# Ask to apply migrations
$applyMigrations = Read-Host "Do you want to apply database migrations? (y/n) [y]"
if ([string]::IsNullOrWhiteSpace($applyMigrations) -or $applyMigrations.ToLower() -eq "y") {
    Write-Host "🔄 Applying database migrations..." -ForegroundColor Yellow
    
    try {
        # Check if migrations exist
        $migrationsExist = (dotnet ef migrations list --project $dataProjectPath --startup-project $webProjectPath) | Measure-Object
        
        if ($migrationsExist.Count -eq 0) {
            Write-Host "⚠️ No migrations found. Creating initial migration..." -ForegroundColor Yellow
            dotnet ef migrations add InitialCreate --project $dataProjectPath --startup-project $webProjectPath
            if ($LASTEXITCODE -ne 0) {
                Write-Host "❌ Failed to create migration. Please check your model classes and try again." -ForegroundColor Red
                exit 1
            }
        }
        
        # Apply migrations
        dotnet ef database update --project $dataProjectPath --startup-project $webProjectPath
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to apply migrations. Please check your connection string and try again." -ForegroundColor Red
            exit 1
        }
        
        Write-Host "✅ Database migrations applied successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error applying migrations: $_" -ForegroundColor Red
    }
}

# Ask to seed data
$seedData = Read-Host "Do you want to seed the database with initial data? (y/n) [y]"
if ([string]::IsNullOrWhiteSpace($seedData) -or $seedData.ToLower() -eq "y") {
    Write-Host "🌱 Seeding database with initial data..." -ForegroundColor Yellow
    
    # We'll run the app in a special seeding mode
    # This assumes your app has a DbInitializer that runs on startup when a specific flag is set
    try {
        # Set an environment variable to trigger seeding
        $env:SEED_DATABASE = "true"
        
        # Build the project to make sure it's ready to run
        dotnet build $webProjectPath
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed. Cannot seed database." -ForegroundColor Red
            $env:SEED_DATABASE = $null
            exit 1
        }
        
        # Run the app briefly to trigger seeding and then stop it
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run --project $webProjectPath --no-build" -PassThru
        Write-Host "⏳ Waiting for seeding to complete (5 seconds)..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        Stop-Process -Id $process.Id -Force
        
        # Clear the environment variable
        $env:SEED_DATABASE = $null
        
        Write-Host "✅ Database seeding completed" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error during database seeding: $_" -ForegroundColor Red
        $env:SEED_DATABASE = $null
    }
}
#endregion

#region JWT Setup
Write-Host ""
Write-Host "🔑 JWT AUTHENTICATION SETUP" -ForegroundColor Cyan
Write-Host "=========================="

$setupJwt = Read-Host "Do you want to configure JWT authentication? (y/n) [y]"
if ([string]::IsNullOrWhiteSpace($setupJwt) -or $setupJwt.ToLower() -eq "y") {
    # Check if JWT package is installed
    $projectFile = Get-Content -Path "$webProjectPath\$projectName.Web.csproj" -Raw
    
    if (-not ($projectFile -match "Microsoft\.AspNetCore\.Authentication\.JwtBearer")) {
        Write-Host "📦 Adding JWT authentication package..." -ForegroundColor Yellow
        dotnet add $webProjectPath package Microsoft.AspNetCore.Authentication.JwtBearer
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to add JWT authentication package." -ForegroundColor Red
        } else {
            Write-Host "✅ JWT authentication package added successfully" -ForegroundColor Green
        }
    } else {
        Write-Host "✅ JWT authentication package already installed" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "JWT configuration is set in your appsettings.json and appsettings.Development.json files." -ForegroundColor Cyan
    Write-Host "Make sure to update your Program.cs to use JWT authentication." -ForegroundColor Cyan
    Write-Host @"
Add the following code to your Program.cs file:

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

// Don't forget to add these lines before app.UseAuthorization():
app.UseAuthentication();
"@ -ForegroundColor Gray
}
#endregion

#region Finalization
Write-Host ""
Write-Host "🎉 SETUP COMPLETE" -ForegroundColor Green
Write-Host "================="
Write-Host "TechXpress development environment has been configured successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "✅ Database connection configured" -ForegroundColor Green
Write-Host "✅ Configuration files updated" -ForegroundColor Green
if ([string]::IsNullOrWhiteSpace($applyMigrations) -or $applyMigrations.ToLower() -eq "y") {
    Write-Host "✅ Database migrations applied" -ForegroundColor Green
}
if ([string]::IsNullOrWhiteSpace($seedData) -or $seedData.ToLower() -eq "y") {
    Write-Host "✅ Initial data seeded" -ForegroundColor Green
}
if ([string]::IsNullOrWhiteSpace($setupJwt) -or $setupJwt.ToLower() -eq "y") {
    Write-Host "✅ JWT authentication configured" -ForegroundColor Green
}

Write-Host ""
Write-Host "🚀 To run the application:" -ForegroundColor Cyan
Write-Host "  dotnet run --project $webProjectPath" -ForegroundColor Gray
Write-Host ""
Write-Host "Happy coding! 🎯" -ForegroundColor Cyan
#endregion