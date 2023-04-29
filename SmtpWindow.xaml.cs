using Http.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Http
{
    public partial class SmtpWindow : Window
    {
        private dynamic? emailConfig;
        private readonly DataContext _dataContext;
        Random rand = new();

        public SmtpWindow()
        {
            InitializeComponent();
            _dataContext = App.DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.AuthUser is null)
            {
                MessageBox.Show("Неавторизованный доступ");
                Close();
                return;
            }
            UserNameTextbox.Text = App.AuthUser.Name;
            UserEmailTextbox.Text = App.AuthUser.Email;


            String configFilename = "emailconfig.json";
            try
            {
                emailConfig = JsonSerializer.Deserialize<dynamic>(
                    System.IO.File.ReadAllText(configFilename)
                );
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show($"Не найден файл конфигурации '{configFilename}'");
                this.Close();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Ошибка преобразования конфигурации '{ex.Message}'");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки конфигурации '{ex.Message}'");
                this.Close();
            }
            if (emailConfig is null)
            {
                MessageBox.Show("Ошибка получения конфигурации");
                this.Close();
            }

         
            if (App.AuthUser.ConfirmCode is null) {
                ConfirmDockPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ConfirmDockPanel.Visibility = Visibility.Visible;
            }
            RegisterButton.IsEnabled = false;
        }

        private SmtpClient GetSmtpClient()
        {
            if (emailConfig is null) { return null!; }
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");

            String host = gmail.GetProperty("host").GetString()!;
            int port = gmail.GetProperty("port").GetInt32();
            String mailbox = gmail.GetProperty("email").GetString()!;
            String password = gmail.GetProperty("password").GetString()!;
            bool ssl = gmail.GetProperty("ssl").GetBoolean();

            return new(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
        }



        private void SendTest2ButtonButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = "<u>Test</u> <i>message</i> from <b style='color:green'>SmtpWindow</b>",
                IsBodyHtml = true,
                Subject = "Test Message"
            };
            mailMessage.To.Add(new MailAddress("7crimas@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);
                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }

        private void SendTestButtonButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            try
            {
                smtpClient.Send(
                    from: mailbox,
                    recipients: "7crimas@gmail.com",
                    subject: "Test Message",
                    body: "Test message from SmtpWindow");

                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }
        static Random random = new Random();

        private void SendPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            const string chars = "abcdefghijkmnopqrstuvwxy23456789";
            string password = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            JsonElement smtpConfig = emailConfig.GetProperty("smtp");
            JsonElement gmailConfig = smtpConfig.GetProperty("gmail");

            SmtpClient smtpClient = new SmtpClient()
            {
                Host = gmailConfig.GetProperty("host").GetString(),
                Port = gmailConfig.GetProperty("port").GetInt32(),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    gmailConfig.GetProperty("email").GetString(),
                    gmailConfig.GetProperty("password").GetString())
            };
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(gmailConfig.GetProperty("email").GetString()),
                Body = $"Ваш новый пароль: {password}",
                IsBodyHtml = false,
                Subject = "Новый пароль для входа"
            };
            mailMessage.To.Add(new MailAddress("7crimas@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);
                MessageBox.Show("Пароль отправлен на указанный адрес электронной почты");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            String emailPattern = System.IO.File.ReadAllText("email.html");
            String confirmCode = RandomString(6);
            String emailBody = emailPattern
                .Replace("*name", UserNameTextbox.Text)
                .Replace("*code*",confirmCode);
            if (emailConfig is null) return;
            /* 
             * emailConfig.GetProperty("smtp").GetProperty("gmail").GetProperty("host").GetString();
             */



            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String host = gmail.GetProperty("host").GetString()!;
            int port = gmail.GetProperty("port").GetInt32()!;
            String mailbox = gmail.GetProperty("email").GetString()!;
            String password = gmail.GetProperty("password").GetString()!;
            bool ssl = gmail.GetProperty("ssl").GetBoolean()!;




            using SmtpClient smtmClient = new(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password),
            };
           // String confirmCode = $"{random.Next(0, 10)}{random.Next(0, 10)}{random.Next(0, 10)}{random.Next(0, 10)}{random.Next(0, 10)}{random.Next(0, 10)}";

            //var emailMessage = emailConfig
            //.Replace("%name%", UserNameTextbox.Text)
            //.Replace("%code%", confirmCode);
            // MessageBox.Show(emailMessage);

            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = emailBody,
                IsBodyHtml = true,
                Subject = "Code For Confirm"
            };
            mailMessage.To.Add(new MailAddress(UserEmailTextbox.Text));
            smtmClient.Send(mailMessage);
            _dataContext.NpUsers.Add(new()
            {
                Id = Guid.NewGuid(),
                Name = UserNameTextbox.Text,
                Email = UserEmailTextbox.Text,
                ConfirmCode = confirmCode
            });
            _dataContext.SaveChanges();

            ConfirmDockPanel.Visibility = Visibility.Visible;
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var user = _dataContext.NpUsers
                .FirstOrDefault(u => u.Name == UserNameTextbox.Text && u.Email == UserEmailTextbox.Text);

            if (user != null && user.ConfirmCode == ConfirmTextbox.Text)
            {
                user.ConfirmCode = null;
                _dataContext.SaveChanges();

                MessageBox.Show("Confirmation successful!");
            }
            else
            {
                MessageBox.Show("Incorrect confirmation code.");
            }
        }
        private String RandomString(int l)
        {
            if (l <= 0) return String.Empty;
            string result = "";
            for (int i = 0; i < l; i++)
            {
                char nextC = (char)rand.Next('a', 'z');   
                int nextI = rand.Next(0, 10);
                if (rand.Next(0, 2) == 0) result += nextI;  
                else
                {
                    if (nextC == 'o' || nextC == 'l' || nextC == 'z')
                    {
                        nextC = (char)rand.Next('a', 'z');
                    }
                    result += nextC;
                }
            }
            return result;
            
        }

        private void SendImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filename = openFileDialog.FileName;

                using SmtpClient smtpClient = GetSmtpClient();
                JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
                String mailbox = gmail.GetProperty("email").GetString()!;
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(mailbox),
                    Body = "<u>Test</u> <i>message</i> from <b style='color:green'>SmtpWindow</b>",
                    IsBodyHtml = true,
                    Subject = "Test Message"
                };
                mailMessage.To.Add(new MailAddress("7crimas@gmail.com"));

                String mimeType = System.IO.Path.GetExtension(filename) switch
                {
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".pdf" => "application/pdf",
                    ".ppt" => "application/vnd.ms-powerpoint",
                    ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    ".zip" => "application/zip",
                    ".rar" => "application/vnd.rar",
                    ".7z" => "application/x-7z-compressed",
                    ".mp3" => "audio/mpeg",
                    ".wav" => "audio/wav",
                    ".mp4" => "video/mp4",
                    ".avi" => "video/x-msvideo",
                    ".txt" => "text/plain",
                    _ => "application/octet-stream",
                };

                mailMessage.Attachments.Add(new Attachment(filename, mimeType));

                try
                {
                    smtpClient.Send(mailMessage);
                    MessageBox.Show("Sent OK");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Sent error '{ex.Message}'");
                }
            }
        }
    }
}