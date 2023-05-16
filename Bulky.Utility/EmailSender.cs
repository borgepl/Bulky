using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // logic to send email - still to be implemented
            // return Task.CompletedTask;

            // prepare email
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("eacea-lsa@ec.europa.eu"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage};

            // send the email smtp
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("mail-internal.eacea.cec.eu.int", 25, MailKit.Security.SecureSocketOptions.None);
                //emailClient.Authenticate("user@test.com","Password");
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;

        }
    }
}