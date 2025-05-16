#!/bin/bash

# TechXpress Development Environment Setup Script
# A helper script for developers to configure their local environment
# Usage: ./setup.sh

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
echo -e "${CYAN}║          $PROJECT_NAME Developer Configuration Helper       ║${NC}"
echo -e "${CYAN}║                                                            ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${YELLOW}This script will help you set up your local development environment.${NC}"
echo -e "${YELLOW}It will guide you through configuring database connections and payment gateways.${NC}"
echo -e "${YELLOW}You can accept the defaults by pressing Enter or provide your own values.${NC}"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED} .NET SDK is not installed or not in PATH. Please install .NET SDK first.${NC}"
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
    echo -e "${GREEN} Entity Framework Core tools installed successfully.${NC}"
fi

# Check if jq is installed (for JSON manipulation)
if ! command -v jq &> /dev/null; then
    echo -e "${RED} jq is required but not installed.${NC}"
    echo "Please install jq first:"
    echo "  - On Ubuntu/Debian: sudo apt-get install jq"
    echo "  - On macOS: brew install jq"
    echo "  - On CentOS/RHEL: sudo yum install jq"
    exit 1
fi

# Check for existing config
if [ -f "$APP_SETTINGS_PATH" ]; then
    echo -e "${YELLOW}Existing appsettings.json detected.${NC}"
    read -p "Do you want to use values from existing configuration as defaults? (y/n) [y]: " use_existing_config
    use_existing_config=${use_existing_config:-y}
    
    if [[ ${use_existing_config,,} == "y" ]]; then
        echo -e "${GREEN}Using existing configuration as defaults...${NC}"
        
        # Extract existing values if file exists and jq is available
        if command -v jq &> /dev/null; then
            # Database connection
            if [[ $(jq -r '.ConnectionStrings.MyCon // ""' "$APP_SETTINGS_PATH") != "" ]]; then
                EXISTING_CONN=$(jq -r '.ConnectionStrings.MyCon' "$APP_SETTINGS_PATH")
                # Try to extract server and database from connection string
                if [[ $EXISTING_CONN =~ Server=([^;]+) ]]; then
                    DEFAULT_SERVER="${BASH_REMATCH[1]}"
                fi
                if [[ $EXISTING_CONN =~ Database=([^;]+) ]]; then
                    DEFAULT_DATABASE="${BASH_REMATCH[1]}"
                fi
            fi
            
            # PayPal settings
            if [[ $(jq -r '.PayPalSettings.ClientId // ""' "$APP_SETTINGS_PATH") != "" ]]; then
                DEFAULT_PAYPAL_CLIENT=$(jq -r '.PayPalSettings.ClientId' "$APP_SETTINGS_PATH")
            fi
            if [[ $(jq -r '.PayPalSettings.Secret // ""' "$APP_SETTINGS_PATH") != "" ]]; then
                DEFAULT_PAYPAL_SECRET=$(jq -r '.PayPalSettings.Secret' "$APP_SETTINGS_PATH")
            fi
            
            # Stripe settings
            if [[ $(jq -r '.StripeSettings.SecretKey // ""' "$APP_SETTINGS_PATH") != "" ]]; then
                DEFAULT_STRIPE_SECRET=$(jq -r '.StripeSettings.SecretKey' "$APP_SETTINGS_PATH")
            fi
            if [[ $(jq -r '.StripeSettings.PublishableKey // ""' "$APP_SETTINGS_PATH") != "" ]]; then
                DEFAULT_STRIPE_PUBLISHABLE=$(jq -r '.StripeSettings.PublishableKey' "$APP_SETTINGS_PATH")
            fi
            if [[ $(jq -r '.StripeSettings.WebhookSecret // ""' "$APP_SETTINGS_PATH") != "" ]]; then
                DEFAULT_STRIPE_WEBHOOK=$(jq -r '.StripeSettings.WebhookSecret' "$APP_SETTINGS_PATH")
            fi
        fi
    fi
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
echo ""
echo -e "${CYAN} DATABASE CONFIGURATION${NC}"
echo -e "${CYAN}=========================${NC}"
echo "Enter your SQL Server details below. For Windows Authentication, leave username blank."
echo ""

# Default values - customize these as needed
DEFAULT_SERVER="AMIN\\SQLSERVER"
DEFAULT_DATABASE="TechXpressDB"

# These are just examples - don't use these in production!
DEFAULT_PAYPAL_CLIENT="AXKECOlak31IzBQhMqcxS3XuJ66Nmqy0zFkFuCLG8YlyoqiFPHNg0LmIGvTUB0HdBzGqHHYAq37f54W4"
DEFAULT_PAYPAL_SECRET="EMC3Uvhria-yK52jMx9HR2iseHy_GqyVTiKVEDQ71RLRzO23UtMvamRwZzDkrNOGHnGy3EGzDy-q_LDD"
DEFAULT_STRIPE_SECRET="sk_test_51RMBv8PMMP3T4W3DEaGEqNcrSYR19fxgQfhSMGxi1uzXm6tuWIDKCqrg9ddGCW80MtBUmbcv7tWTZjij14Y5CHKC003mzIifmI"
DEFAULT_STRIPE_PUBLISHABLE="pk_test_51RMBv8PMMP3T4W3D3NCQC0Au6TkWbnRlw2U5qMjTs7HolzCfTTcLd9Zpo30OZ21wYDALQyTZwIDhAzd3Cz3524es00IdHHFijI"
DEFAULT_STRIPE_WEBHOOK="whsec_w9DHgp4lNwrJFh5wpJf9Df4NXWqerUnk"

# Prompt for DB connection details with defaults
read -p "Enter SQL Server name [$DEFAULT_SERVER]: " server
server=${server:-$DEFAULT_SERVER}

read -p "Enter database name [$DEFAULT_DATABASE]: " database
database=${database:-$DEFAULT_DATABASE}

read -p "Authentication type (W)indows or (S)QL Server [W]: " auth_type
auth_type=${auth_type:-W}

if [[ ${auth_type^^} == "W" ]]; then
    connection_string="Server=$server;Database=$database;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"
    echo -e "${GREEN}Using Windows Authentication${NC}"
else
    read -p "Enter SQL Server username: " user
    if [[ -z "$user" ]]; then
        echo -e "${YELLOW}⚠️ Username cannot be empty for SQL Server Authentication${NC}"
        read -p "Enter SQL Server username: " user
        if [[ -z "$user" ]]; then
            echo -e "${RED}❌ Username still empty. Defaulting to Windows Authentication${NC}"
            connection_string="Server=$server;Database=$database;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"
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

#region Payment Gateway Configuration
echo -e "${CYAN} PAYMENT GATEWAY CONFIGURATION${NC}"
echo -e "${CYAN}===============================${NC}"
echo -e "${YELLOW}💡 For development purposes, you can use placeholder values${NC}"
echo -e "${YELLOW}💡 For team development, ask your team lead for the correct development credentials${NC}"
echo ""

# PayPal Configuration
echo -e "${YELLOW}📝 PayPal Configuration${NC}"
read -p "Configure PayPal integration? (y/n) [y]: " configure_paypal
configure_paypal=${configure_paypal:-y}

if [[ ${configure_paypal,,} == "y" ]]; then
    read -p "Enter PayPal Client ID [$DEFAULT_PAYPAL_CLIENT]: " paypal_client_id
    paypal_client_id=${paypal_client_id:-"$DEFAULT_PAYPAL_CLIENT"}

    read -p "Enter PayPal Secret [$DEFAULT_PAYPAL_SECRET]: " paypal_secret
    paypal_secret=${paypal_secret:-"$DEFAULT_PAYPAL_SECRET"}

    read -p "Use PayPal Sandbox? (y/n) [y]: " use_paypal_sandbox
    use_paypal_sandbox=${use_paypal_sandbox:-y}
    if [[ ${use_paypal_sandbox,,} == "y" ]]; then
        paypal_sandbox="true"
    else
        paypal_sandbox="false"
    fi
else
    paypal_client_id="$DEFAULT_PAYPAL_CLIENT"
    paypal_secret="$DEFAULT_PAYPAL_SECRET"
    paypal_sandbox="true"
fi

# Stripe Configuration
echo -e "${YELLOW}📝 Stripe Configuration${NC}"
read -p "Configure Stripe integration? (y/n) [y]: " configure_stripe
configure_stripe=${configure_stripe:-y}

if [[ ${configure_stripe,,} == "y" ]]; then
    read -p "Enter Stripe Secret Key [$DEFAULT_STRIPE_SECRET]: " stripe_secret_key
    stripe_secret_key=${stripe_secret_key:-"$DEFAULT_STRIPE_SECRET"}

    read -p "Enter Stripe Publishable Key [$DEFAULT_STRIPE_PUBLISHABLE]: " stripe_publishable_key
    stripe_publishable_key=${stripe_publishable_key:-"$DEFAULT_STRIPE_PUBLISHABLE"}

    read -p "Enter Stripe Webhook Secret [$DEFAULT_STRIPE_WEBHOOK]: " stripe_webhook_secret
    stripe_webhook_secret=${stripe_webhook_secret:-"$DEFAULT_STRIPE_WEBHOOK"}
else
    stripe_secret_key="$DEFAULT_STRIPE_SECRET"
    stripe_publishable_key="$DEFAULT_STRIPE_PUBLISHABLE"
    stripe_webhook_secret="$DEFAULT_STRIPE_WEBHOOK"
fi

echo -e "${GREEN}✅ Payment gateway configuration complete${NC}"
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
    
    # Generate JWT key or use existing one
    jwt_key=$(cat /dev/urandom | tr -dc 'a-zA-Z0-9+/' | fold -w 40 | head -n 1 | base64)
    
    if [ -f "$file_path" ]; then
        echo -e "${YELLOW}Updating existing $file_path...${NC}"
        
        # Check if file already has a JWT key to preserve it
        if command -v jq &> /dev/null && [[ $(jq -r '.JwtSettings.Secretkey // ""' "$file_path") != "" ]]; then
            jwt_key=$(jq -r '.JwtSettings.Secretkey' "$file_path")
            echo -e "${YELLOW}Using existing JWT key from configuration${NC}"
        fi
        
        # Create a temporary file
        tmp_file=$(mktemp)
        
        if [[ "$is_dev" == "true" ]]; then
            # Development environment with more verbose logging
            jq --arg conn "$conn_str" \
               --arg key "$jwt_key" \
               --arg paypal_client_id "$paypal_client_id" \
               --arg paypal_secret "$paypal_secret" \
               --arg paypal_sandbox "$paypal_sandbox" \
               --arg stripe_secret_key "$stripe_secret_key" \
               --arg stripe_publishable_key "$stripe_publishable_key" \
               --arg stripe_webhook_secret "$stripe_webhook_secret" \
               '.ConnectionStrings.MyCon = $conn | 
                .JwtSettings = {
                  "Secretkey": $key, 
                  "Issuer": "BackUrl", 
                  "Audience": "FrontUrl", 
                  "ExpiryMinutes": 3,
                  "RefreshTokenExpiryDays": 7
                } |
                .PayPalSettings = {
                  "ClientId": $paypal_client_id,
                  "Secret": $paypal_secret,
                  "UseSandbox": ($paypal_sandbox | test("true"))
                } |
                .StripeSettings = {
                  "SecretKey": $stripe_secret_key,
                  "PublishableKey": $stripe_publishable_key,
                  "WebhookSecret": $stripe_webhook_secret
                } |
                .Logging.LogLevel.Default = "Debug" |
                .Logging.LogLevel."Microsoft.AspNetCore" = "Debug" |
                .Logging.LogLevel."Microsoft.EntityFrameworkCore" = "Information" |
                .DetailedErrors = true' "$file_path" > "$tmp_file"
        else
            # Production/default environment
            jq --arg conn "$conn_str" \
               --arg key "$jwt_key" \
               --arg paypal_client_id "$paypal_client_id" \
               --arg paypal_secret "$paypal_secret" \
               --arg paypal_sandbox "$paypal_sandbox" \
               --arg stripe_secret_key "$stripe_secret_key" \
               --arg stripe_publishable_key "$stripe_publishable_key" \
               --arg stripe_webhook_secret "$stripe_webhook_secret" \
               '.ConnectionStrings.MyCon = $conn | 
                .JwtSettings = {
                  "Secretkey": $key, 
                  "Issuer": "BackUrl", 
                  "Audience": "FrontUrl", 
                  "ExpiryMinutes": 3,
                  "RefreshTokenExpiryDays": 7
                } |
                .PayPalSettings = {
                  "ClientId": $paypal_client_id,
                  "Secret": $paypal_secret,
                  "UseSandbox": ($paypal_sandbox | test("true"))
                } |
                .StripeSettings = {
                  "SecretKey": $stripe_secret_key,
                  "PublishableKey": $stripe_publishable_key,
                  "WebhookSecret": $stripe_webhook_secret
                } |
                .Logging.LogLevel = {
                  "Default": "Information",
                  "Microsoft": "Warning",
                  "Microsoft.AspNetCore": "Warning",
                  "Microsoft.AspNetCore.Routing": "Error",
                  "Microsoft.AspNetCore.Authentication": "Error",
                  "Microsoft.AspNetCore.Authorization": "Error",
                  "Microsoft.AspNetCore.Mvc": "Warning",
                  "Microsoft.Hosting.Lifetime": "Information"
                }' "$file_path" > "$tmp_file"
        fi
        
        # Replace the original file with the temporary file
        mv "$tmp_file" "$file_path"
        echo -e "${GREEN}Updated $file_path${NC}"
    else
        echo -e "${YELLOW}Creating new $file_path...${NC}"
        
        if [[ "$is_dev" == "true" ]]; then
            # Development environment with more verbose logging
            cat > "$file_path" << EOF
{
  "ConnectionStrings": {
    "MyCon": "$conn_str"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "DetailedErrors": true,
  "AllowedHosts": "*",
  "JwtSettings": {
    "Secretkey": "$jwt_key",
    "Issuer": "BackUrl",
    "Audience": "FrontUrl",
    "ExpiryMinutes": 3,
    "RefreshTokenExpiryDays": 7
  },
  "PayPalSettings": {
    "ClientId": "$paypal_client_id",
    "Secret": "$paypal_secret",
    "UseSandbox": $paypal_sandbox
  },
  "StripeSettings": {
    "SecretKey": "$stripe_secret_key",
    "PublishableKey": "$stripe_publishable_key",
    "WebhookSecret": "$stripe_webhook_secret"
  }
}
EOF
        else
            # Production/default environment
            cat > "$file_path" << EOF
{
  "ConnectionStrings": {
    "MyCon": "$conn_str"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Routing": "Error",
      "Microsoft.AspNetCore.Authentication": "Error",
      "Microsoft.AspNetCore.Authorization": "Error",
      "Microsoft.AspNetCore.Mvc": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Secretkey": "$jwt_key",
    "Issuer": "BackUrl",
    "Audience": "FrontUrl",
    "ExpiryMinutes": 3,
    "RefreshTokenExpiryDays": 7
  },
  "PayPalSettings": {
    "ClientId": "$paypal_client_id",
    "Secret": "$paypal_secret",
    "UseSandbox": $paypal_sandbox
  },
  "StripeSettings": {
    "SecretKey": "$stripe_secret_key",
    "PublishableKey": "$stripe_publishable_key",
    "WebhookSecret": "$stripe_webhook_secret"
  }
}
EOF
        fi
        
        echo -e "${GREEN} Created $file_path${NC}"
    fi
}

# Make sure directory exists
mkdir -p "$(dirname "$APP_SETTINGS_PATH")"

# Update both appsettings.json and appsettings.Development.json
update_app_settings "$APP_SETTINGS_PATH" "$connection_string" false
update_app_settings "$APP_SETTINGS_DEV_PATH" "$connection_string" true

# Store user secrets (not directly applicable in bash, but for completeness)
read -p "Do you want to store sensitive settings in user secrets? (y/n) [n]: " use_secrets
use_secrets=${use_secrets:-n}

if [[ ${use_secrets,,} == "y" ]]; then
    echo -e "${YELLOW}Storing sensitive settings in user secrets...${NC}"
    pushd "$WEB_PROJECT_PATH" > /dev/null
    dotnet user-secrets init --project TechXpress.Web
    dotnet user-secrets set "ConnectionStrings:MyCon" "$connection_string" --project TechXpress.Web
    dotnet user-secrets set "JwtSettings:Secretkey" "$jwt_key" --project TechXpress.Web
    dotnet user-secrets set "PayPalSettings:ClientId" "$paypal_client_id" --project TechXpress.Web
    dotnet user-secrets set "PayPalSettings:Secret" "$paypal_secret" --project TechXpress.Web
    dotnet user-secrets set "StripeSettings:SecretKey" "$stripe_secret_key" --project TechXpress.Web
    dotnet user-secrets set "StripeSettings:PublishableKey" "$stripe_publishable_key" --project TechXpress.Web
    dotnet user-secrets set "StripeSettings:WebhookSecret" "$stripe_webhook_secret" --project TechXpress.Web
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}Settings stored in user secrets${NC}"
    else
        echo -e "${RED}Error storing in user secrets${NC}"
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
        echo -e "${YELLOW}No existing migrations found. Creating initial migration...${NC}"
        dotnet ef migrations add InitialCreate --project "$DATA_PROJECT_PATH" --startup-project "$WEB_PROJECT_PATH"
        if [ $? -ne 0 ]; then
            echo -e "${RED}❌ Failed to create initial migration. Please check your database context and try again.${NC}"
            exit 1
        fi
    fi
    
    # Apply migrations
    dotnet ef database update --project "$DATA_PROJECT_PATH" --startup-project "$WEB_PROJECT_PATH"
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to apply migrations. Please check your database connection and try again.${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Database migrations applied successfully${NC}"
else
    echo -e "${YELLOW}⚠️ Skipping database migrations${NC}"
fi

# Seed database with initial data
read -p "Do you want to seed the database with initial data? (y/n) [y]: " seed_database
seed_database=${seed_database:-y}

if [[ ${seed_database,,} == "y" ]]; then
    echo -e "${YELLOW}🌱 Seeding database with initial data...${NC}"
    # Run the appropriate seeding command for your application
    # This is a placeholder - replace with your actual seeding command
    dotnet run --project "$WEB_PROJECT_PATH" -- --seed
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to seed database. Please check the seeder implementation and try again.${NC}"
        # Not exiting here as this is optional
    else
        echo -e "${GREEN}✅ Database seeded successfully${NC}"
    fi
else
    echo -e "${YELLOW}⚠️ Skipping database seeding${NC}"
fi
#endregion

# Final setup and next steps
echo ""
echo -e "${CYAN}🚀 SETUP COMPLETE${NC}"
echo -e "${CYAN}=================${NC}"
echo -e "${GREEN}✅ TechXpress development environment has been configured successfully!${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "1. ${CYAN}Start the application:${NC}"
echo -e "   ${GRAY}dotnet run --project $WEB_PROJECT_PATH${NC}"
echo -e ""
echo -e "2. ${CYAN}Access the application at:${NC}"
echo -e "   ${GRAY}https://localhost:7170${NC} (or the port specified in launchSettings.json)"
echo -e ""
echo -e "3. ${CYAN}For API documentation:${NC}"
echo -e "   ${GRAY}https://localhost:7170/swagger${NC}"
echo -e ""
echo -e "4. ${CYAN}Review the generated configuration files:${NC}"
echo -e "   ${GRAY}$APP_SETTINGS_PATH${NC}" 
echo -e "   ${GRAY}$APP_SETTINGS_DEV_PATH${NC}"
echo -e ""
echo -e "${YELLOW}💡 TIP: You can run this script again anytime to reconfigure your environment.${NC}"
echo -e "${YELLOW}💡 TIP: For team settings, consult your team lead or DevOps engineer.${NC}"
echo ""
echo -e "${GREEN}Happy coding!${NC}"