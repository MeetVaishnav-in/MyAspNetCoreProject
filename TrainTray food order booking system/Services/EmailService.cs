using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace TrainTray_food_order_booking_system.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SmtpPass"];

                Console.WriteLine($"SMTP Server: {smtpServer}");
                Console.WriteLine($"SMTP Port: {smtpPort}");
                Console.WriteLine($"Sender Email: {senderEmail}");
                Console.WriteLine($"Receiver Email: {toEmail}");

                var client = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                string emailBody = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #ffe8e8;
                            padding: 20px;
                            text-align: center;
                        }}
                        .container {{
                            background-color: #ffe8e8;
                            padding: 20px;
                            border-radius: 10px;
                            box-shadow: 0px 4px 8px rgba(0,0,0,0.2);
                            max-width: 400px;
                            margin: auto;
                        }}
                        h2 {{
                            color: #333;
                        }}
                        p {{
                            font-size: 16px;
                            color: #555;
                        }}
                        .otp {{
                            font-size: 20px;
                            font-weight: bold;
                            color: #1a73e8;
                            padding: 10px;
                            background-color: #e3f2fd;
                            display: inline-block;
                            border-radius: 5px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 10px 20px;
                            background-color: #1a73e8;
                            color: white;
                            text-decoration: none;
                            font-size: 16px;
                            border-radius: 5px;
                            margin-top: 10px;
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 12px;
                            color: #888;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Your OTP Code</h2>
                        <p>Use the code below to complete your verification:</p>
                        <div class='otp'>{otp}</div>
                        <p>This code will expire in 5 minutes.</p>

                        <p class='footer'>If you didn't request this, please ignore this email.</p>
                    </div>
                </body>
                </html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Your OTP Code",
                    Body = emailBody,
                    IsBodyHtml = true // **Important for HTML formatting**
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex}");
                return false;
            }
        }
    }
}
