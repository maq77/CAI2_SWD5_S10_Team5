#!/bin/bash

# TechXpress Development Environment Setup Script
# Comprehensive setup for the TechXpress application

#region Banner and Initialization
clear
PROJECT_NAME="TechXpress"
WEB_PROJECT_PATH="./$PROJECT_NAME.Web"
DATA_PROJECT_PATH="./$PROJECT_NAME.Data"
APP_SETTINGS_PATH="$WEB_PROJECT_PATH/appsettings.json"
APP_SETTINGS_DEV_PATH="$WEB_PROJECT_PATH/appsettings.Development.json"

# Color definitions
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

echo -e "${CYAN}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                                                            ║${NC}"
echo -e "${CYAN}║             $PROJECT_NAME Development Environment Setup     ║${NC}"
echo -e "${CYAN}║                                                            ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK is not installed or not in PATH. Please install .NET SDK first.${NC}"
    exit 1
fi

# Check if Entity Framework tools are installed
if ! dotnet tool list --global | grep -q "dotnet-ef"; then
    echo -e "${YELLOW}🔧 Entity Framework Core tools not found. Installing...${NC}"
    dotnet tool install --global dotnet-ef
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to install Entity Framework Core tools. Please install manually with 'dotnet tool install --global dotnet-ef'${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ Entity Framework Core tools installed successfully.${NC}"
fi

# Check if jq is installed (for JSON manipulation)
if ! command -v jq &> /dev/null; then
    echo -e "${RED}❌ jq is required but not installed.${NC}"
    echo "Please install jq first:"
    echo "  - On Ubuntu/Debian: sudo apt-get install jq"
    echo "  - On macOS: brew install jq"
    echo "  - On CentOS/RHEL: sudo yum install jq"
    exit 1
fi

# Verify project structure
if [ ! -d "$WEB_PROJECT_PATH" ]; then
    echo -e "${RED}❌ Web project not found at $WEB_PROJECT_PATH. Make sure you're running this script from the solution directory.${NC}"
    exit 1
fi

if [ ! -d "$DATA_PROJECT_PATH" ]; then
    echo -e "${RED}❌ Data project not found at $DATA_PROJECT_PATH. Make sure you're running this script from the solution directory.${NC}"
    exit 1
fi
#endregion

#region Database Configuration
echo ""
echo -e "${CYAN}📊 DATABASE CONFIGURATION${NC}"
echo -e "${CYAN}=========================${NC}"
echo "Enter your SQL Server details below. For Windows Authentication, leave username blank."
echo ""

# Default values
DEFAULT_SERVER="localhost\\SQLEXPRESS"
DEFAULT_DATABASE="TechXpressDb"

# Prompt for DB connection details with defaults
read -p "Enter SQL Server name [$DEFAULT_SERVER]: " server
server=${server:-$DEFAULT_SERVER}

read -p "Enter database name [$DEFAULT_DATABASE]: " database
database=${database:-$DEFAULT_DATABASE}

read -p "Authentication type (W)indows or (S)QL Server [W]: " auth_type
auth_type=${auth_type:-W}

if [[ ${auth_type^^} == "W" ]]; then
    connection_string="Server=$server;Database=$database;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
    echo -e "${GREEN}Using Windows Authentication${NC}"
else
    read -p "Enter SQL Server username: " user
    if [[ -z "$user" ]]; then
        echo -e "${YELLOW}⚠️ Username cannot be empty for SQL Server Authentication${NC}"
        read -p "Enter SQL Server username: " user
        if [[ -z "$user" ]]; then
            echo -e "${RED}❌ Username still empty. Defaulting to Windows Authentication${NC}"
            connection_string="Server=$server;Database=$database;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
        fi
    else
        read -s -p "Enter password: " password
        echo ""
        connection_string="Server=$server;Database=$database;User Id=$user;Password=$password;MultipleActiveResultSets=true;TrustServerCertificate=True;"
    fi
fi

echo ""
echo -e "${CYAN}Connection string configured:${NC}"
echo -e "${GRAY}$connection_string${NC}"
echo ""
#endregion

#region Update Configuration Files
echo -e "${CYAN}🔧 UPDATING CONFIGURATION FILES${NC}"
echo -e "${CYAN}==============================${NC}"

# Function to update appsettings file
update_app_settings() {
    local file_path=$1
    local conn_str=$2
    local is_dev=${3:-false}
    
    # Generate JWT key if needed
    jwt_key=$(cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 32 | head -n 1)
    
    if [ -f "$file_path" ]; then
        echo -e "${YELLOW}Updating existing $file_path...${NC}"
        
        # Create a temporary file
        tmp_file=$(mktemp)
        
        if [[ "$is_dev" == "true" ]]; then
            # Development environment with more verbose logging
            jq --arg conn "$conn_str" \
               --arg key "$jwt_key" \
               '.ConnectionStrings.DefaultConnection = $conn | 
                if .JwtSettings == null then 
                  .JwtSettings = {
                    "Key": $key, 
                    "Issuer": "TechXpress", 
                    "Audience": "TechXpressUsers", 
                    "DurationInMinutes": 60
                  } 
                else . end |
                .Logging.LogLevel.Default = "Debug" |
                .Logging.LogLevel."Microsoft.AspNetCore" = "Debug" |
                .Logging.LogLevel."Microsoft.EntityFrameworkCore" = "Information" |
                .DetailedErrors = true' "$file_path" > "$tmp_file"
        else
            # Production/default environment
            jq --arg conn "$conn_str" \
               --arg key "$jwt_key" \
               '.ConnectionStrings.DefaultConnection = $conn | 
                if .JwtSettings == null then 
                  .JwtSettings = {
                    "Key": $key, 
                    "Issuer": "TechXpress", 
                    "Audience": "TechXpressUsers", 
                    "DurationInMinutes": 60
                  } 
                else . end' "$file_path" > "$tmp_file"
        fi
        
        # Replace the original file with the temporary file
        mv "$tmp_file" "$file_path"
        echo -e "${GREEN}✅ Updated $file_path${NC}"
    else
        echo -e "${YELLOW}Creating new $file_path...${NC}"
        
        if [[ "$is_dev" == "true" ]]; then
            # Development environment with more verbose logging
            cat > "$file_path" << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$conn_str"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "DetailedErrors": true,
  "JwtSettings": {
    "Key": "$jwt_key",
    "Issuer": "TechXpress",
    "Audience": "TechXpressUsers",
    "DurationInMinutes": 60
  }
}
EOF
        else
            # Production/default environment
            cat > "$file_path" << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$conn_str"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Key": "$jwt_key",
    "Issuer": "TechXpress",
    "Audience": "TechXpressUsers",
    "DurationInMinutes": 60
  }
}
EOF
        fi
        
        echo -e "${GREEN}✅ Created $file_path${NC}"
    fi
}

# Make sure directory exists
mkdir -p "$(dirname "$APP_SETTINGS_PATH")"

# Update both appsettings.json and appsettings.Development.json
update_app_settings "$APP_SETTINGS_PATH" "$connection_string" false
update_app_settings "$APP_SETTINGS_DEV_PATH" "$connection_string" true

# Store user secrets (not directly applicable in bash, but for completeness)
read -p "Do you want to store connection string in user secrets? (y/n) [n]: " use_secrets
use_secrets=${use_secrets:-n}

if [[ ${use_secrets,,} == "y" ]]; then
    echo -e "${YELLOW}Storing connection string in user secrets...${NC}"
    pushd "$WEB_PROJECT_PATH" > /dev/null
    dotnet user-secrets init --project .
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$connection_string" --project .
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Connection string stored in user secrets${NC}"
    else
        echo -e "${RED}❌ Error storing in user secrets${NC}"
    fi
    popd > /dev/null
fi
#endregion

#region Database Setup
echo ""
echo -e "${CYAN}🗄️  DATABASE SETUP${NC}"
echo -e "${CYAN}=================${NC}"

# Restore packages
echo -e "${YELLOW}📦 Restoring NuGet packages...${NC}"
dotnet restore
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Failed to restore NuGet packages. Please check your internet connection and try again.${NC}"
    exit 1
fi
echo -e "${GREEN}✅ NuGet packages restored successfully${NC}"

# Ask to apply migrations
read -p "Do you want to apply database migrations? (y/n) [y]: " apply_migrations
apply_migrations=${apply_migrations:-y}

if [[ ${apply_migrations,,} == "y" ]]; then
    echo -e "${YELLOW}🔄 Applying database migrations...${NC}"
    
    # Check if migrations exist
    migrations_count=$(dotnet ef migrations list --project "$DATA_PROJECT_PATH" --startup-project "$WEB_PROJECT_PATH" | wc -l)
    
    if [ "$migrations_count" -eq 0 ]; then
        echo -e "${