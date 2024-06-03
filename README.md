# MailToThings3FromConsole
Console application with ability to send TODOs to your things3 app using "Mail to things" feature.

## Setup steps
1. Clone repository.
2. Create SendGrid account (fully free) - [sendgrid.com](https://sendgrid.com/).
3. Get API key.
4. Verify email to send from.
5. Create `appsettings.json` file in folder with `csproj` file.
6. Add configuration to your appsettings file.
   Config file should looks like this:
   ```
   {
    "SendGrid": {
      "ApiKey": "Key",
      "FromEmail": "your email",
      "ToEmail": "things email"
      }
    }
   ```
7. Enjoy app :)
