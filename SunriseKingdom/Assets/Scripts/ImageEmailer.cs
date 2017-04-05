using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class ImageEmailer : MonoBehaviour
{
    public string fromAddress = "from@email.here";
    public string toAddress = "to@email.here";
    public string subject = "Image from Sunrise Kingdom";
    public string messageBody = "";
    public string password = "";
    public string SMTPServer = "smtp.gmail.com";
    public int SMTPPort = 587;

    public bool emailSent = false;
    public bool debugActive = false;

    void Update()
    {
        if (emailSent)
        {
            SendEmail();
        }
    }

    public void SendEmail()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress(fromAddress);
        mail.To.Add(toAddress);
        mail.Subject = subject;
        mail.Body = messageBody;

        string attachmentPath = @"D:\SunriseData\Images\0.png";
        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(attachmentPath);
        mail.Attachments.Add(attachment);

        SmtpClient smtpServer = new SmtpClient(SMTPServer);
        smtpServer.Port = SMTPPort;
        smtpServer.Credentials = new System.Net.NetworkCredential(fromAddress, password) as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);

        if (debugActive)
            Debug.Log("Email has been successfully sent!");

        emailSent = false;
    }
}