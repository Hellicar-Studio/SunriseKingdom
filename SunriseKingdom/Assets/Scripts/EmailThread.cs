using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class EmailThread : MonoBehaviour
{
    [HideInInspector]
    public bool useThreading;
    [HideInInspector]
    public string imagesFolder;
    [HideInInspector]
    public string emailAccount;
    [HideInInspector]
    public string emailRecipient;
    [HideInInspector]
    public string messageBody;
    [HideInInspector]
    public string emailPassword;
    [HideInInspector]
    public string serverSMTP;
    [HideInInspector]
    public int portSMTP;
    [HideInInspector]
    public int item = 0;
    [HideInInspector]
    public bool debugActive;
    [HideInInspector]
    public bool emailSent = false;
    [HideInInspector]
    public int videosLength;

    private Thread _t1;
    private bool _t1Paused = false;
    private Mutex _mutex = new Mutex();

    void Start()
    {
        if (useThreading)
        {
            _t1 = new Thread(_func1);
            _t1.Start();
        }
    }

    private void _func1()
    {
        while (true)
        {
            _mutex.WaitOne();
            SendEmail();
            _mutex.ReleaseMutex();

            //do
            //{
            //    Thread.Sleep(200);
            //}
            //while (_t1Paused);
        }
    }

    public void SendEmail()
    {
        if (useThreading)
        {
            if (emailSent)
            {
                ProcessEmail();
                emailSent = false;
            }
        }
        else
        {
            ProcessEmail();
        }
        
        // pauses the thread and stops
        //_t1Paused = true;
        //emailSent = false;
        //_t1.Abort();
    }

    private void ProcessEmail()
    {
        // creates the email
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(emailAccount);
        mail.To.Add(emailRecipient);
        mail.Subject = "Sunrise Kingdom Images"; //subject + " " + item + ".png";
        mail.Body = messageBody;

        //// creates the attachment
        //string attachmentPath = imagesFolder + item + ".png";
        //Attachment attachment = new Attachment(attachmentPath);
        //mail.Attachments.Add(attachment);

        // creates an attachment array
        for (int i = 0; i < videosLength; i++)
        {
            string attachmentPath = imagesFolder + i + ".png";
            Attachment attachment = new Attachment(attachmentPath);
            mail.Attachments.Add(attachment);
        }

        // establishes a connection to the outgoing server (SMTP) and sends the email
        SmtpClient server = new SmtpClient(serverSMTP);
        server.Port = portSMTP;
        server.Credentials = new NetworkCredential(emailAccount, emailPassword) as ICredentialsByHost;
        server.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        server.SendAsync(mail, "Sending Email");
    }

    // kills thread on exit
    void OnApplicationQuit()
    {
        if (useThreading)
            _t1.Abort();
    }
}