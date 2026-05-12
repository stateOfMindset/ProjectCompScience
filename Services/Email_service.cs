using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Services
{
    internal class Email_service
    {      
        public static async Task SendReceiptAsync(string userEmail, string subject, string body)
        {

            string envPath = LocalDataService.EnvFilePath;
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }

            string BotEmail = Environment.GetEnvironmentVariable("BOT_EMAIL");
            string AppPassword = Environment.GetEnvironmentVariable("BOT_PASSWORD");


            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(BotEmail, AppPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(BotEmail, "My Trading App"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false 
                };

                mailMessage.To.Add(userEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
