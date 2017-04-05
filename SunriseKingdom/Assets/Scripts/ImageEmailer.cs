using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class ImageEmailer : MonoBehaviour
{
    public bool emailSent = false;

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

        mail.From = new MailAddress("jason@glitchbeam.com");
        mail.To.Add("jason@glitchbeam.com");
        mail.CC.Add("james@hellicarstudio.com");
        mail.Subject = "Email from Sunrise Kingdom";
        mail.Body = ":-)";

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential("jason@glitchbeam.com", "Ustepski76jaW") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);
        Debug.Log("success");

        emailSent = false;
    }
}