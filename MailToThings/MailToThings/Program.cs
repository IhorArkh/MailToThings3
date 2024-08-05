using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MailToThings;

class Program
{
    static async Task Main(string[] args)
    {
        ConfigureConsole();

        ApplicationStart:
        EnteringTodoSubject:
        Console.WriteLine("\x1b[33m\x1b[1mTODO subject:\x1b[0m"); // yellow bold
        string subject = Console.ReadLine() ?? throw new ArgumentNullException(nameof(subject));

        if (subject.Length < 1)
        {
            Console.WriteLine("TODO subject can`t be empty.");
            goto EnteringTodoSubject;
        }

        EnteringTodoBody:
        Console.WriteLine("\x1b[33m\x1b[1mTODO body:\x1b[0m"); // yellow bold
        Console.WriteLine("\x1b[3mType \"mt\" if you want to enter multiple lines\x1b[0m"); // italic

        string body = Console.ReadLine() ?? throw new ArgumentNullException(nameof(body));

        if (body == "mt") // multi-line input
        {
            Console.WriteLine("\x1b[33mMulti-line body:\x1b[0m"); // yellow
            Console.WriteLine("\x1b[3mType 'END' in new line to finish\x1b[0m"); // italic
            
            var bodyBuilder = new StringBuilder();
            string? line;
            while ((line = Console.ReadLine()) != null && line != "END")
            {
                bodyBuilder.AppendLine(line);
            }

            body = bodyBuilder.ToString();
        }

        if (body.Length > 4000)
        {
            Console.WriteLine("\x1B[31m\x1b[1mTODO can't be longer than 4000 characters.\x1b[0m"); // red bold
            goto EnteringTodoBody;
        }

        ConfirmSending:
        Console.WriteLine("Confirm sending? Type y/n:");
        var confirmSendingAnswer = Console.ReadLine();

        if (confirmSendingAnswer != "y" && confirmSendingAnswer != "n")
        {
            Console.WriteLine("\x1B[31m\x1b[1mWrong answer!\x1b[0m"); // red bold
            goto ConfirmSending;
        }

        if (confirmSendingAnswer == "y")
        {
            var configData = GetDataFromConfig();

            await SendEmailAsync(
                configData.apiKey,
                configData.fromEmail,
                configData.toEmail,
                subject,
                body,
                " ");
        }
        else
        {
            Console.WriteLine("\x1B[31mSending rejected.\x1B[0m"); // red
        }

        Console.WriteLine("\x1b[33mPress enter to close or type '+' to add new TODO\x1b[0m"); // yellow
        var addNewTodoAnswer = Console.ReadLine();
        if (addNewTodoAnswer == "+")
        {
            Console.Clear();
            goto ApplicationStart;
        }
    }

    #region Private methods

    private static async Task SendEmailAsync(
        string apiKey,
        string fromEmail,
        string toEmail,
        string? subject,
        string plainTextContent,
        string? htmlContent)
    {
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "Sender");
        var to = new EmailAddress(toEmail, "Things 3");
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);

        if (response.StatusCode is HttpStatusCode.Accepted)
        {
            Console.WriteLine("\x1B[32mEmail sent successfully!\x1b[0m"); // green
        }
        else
        {
            Console.WriteLine("\x1B[31m\x1b[1mFailed to send email.\x1b[0m"); // red bold
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Body.ReadAsStringAsync().Result);
        }
    }

    private static (string apiKey, string fromEmail, string toEmail) GetDataFromConfig()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true)
            .Build();

        if (configuration is null)
        {
            Console.WriteLine("File with configuration not found!");
            throw new ArgumentNullException(nameof(configuration));
        }

        string apiKey = configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException(nameof(apiKey));
        string fromEmail = configuration["SendGrid:FromEmail"] ??
                           throw new ArgumentNullException(nameof(fromEmail));
        string toEmail = configuration["SendGrid:ToEmail"] ?? throw new ArgumentNullException(nameof(toEmail));

        return (apiKey, fromEmail, toEmail);
    }

    private static void ConfigureConsole()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.Title = "MailToThings";
    }

    #endregion
}