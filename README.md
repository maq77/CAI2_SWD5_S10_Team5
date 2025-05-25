## 🔧 Developer Setup

1. Run the setup script:
   - Windows: `.\setup.ps1`
   - Linux/macOS: `./setup.sh` ( Recommended ) 

2. Follow the prompts to configure your database.

   The script will:
   - Configure `appsettings.json`
   - Apply EF migrations
   - Seed the database

3. Seed Data Manually
- Uncomment line 38 in Program.cs
- Created Credintials are
- Email : admin@gmail.com
- Password : Admin@123
4. Use Chatbot when Production
- uncomment line in send message in try block
- it uses real AI Model (Mistral)

## Visit Demo
=> http://maqmohamed8-001-site1.jtempurl.com/
