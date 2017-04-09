using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class EmailThread : MonoBehaviour
{
    #region Public data
    [HideInInspector]
    public string imagesFolder;
    [HideInInspector]
    public string emailAccount;
    [HideInInspector]
    public string emailRecipient;
    [HideInInspector]
    public string subject;
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
    #endregion

    #region Private data
    private Thread _t1;
    private bool _t1Paused = false;
    private Mutex _mutex = new Mutex();
    #endregion

    #region Start
    void Start()
    {
        _t1 = new Thread(_func1);
    }
    #endregion

    #region Threads
    private void _func1()
    {
        while (emailSent)
        {
            _mutex.WaitOne();
            SendEmail();
            _mutex.ReleaseMutex();

            do
            {
                Thread.Sleep(200);
            }
            while (_t1Paused);
        }
    }

    void Update()
    {
        if (emailSent)
        {
            if (!_t1.IsAlive)
            {
                _t1.Start();
            }
            else
                _t1Paused = !_t1Paused;
        }
    }
    #endregion

    private void SendEmail()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress(emailAccount);
        mail.To.Add(emailRecipient);
        mail.Subject = subject + " " + item + ".png";
        mail.Body = messageBody;

        string attachmentPath = imagesFolder + item + ".png";
        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(attachmentPath);
        mail.Attachments.Add(attachment);

        SmtpClient server = new SmtpClient(serverSMTP);
        server.Port = portSMTP;
        server.Credentials = new System.Net.NetworkCredential(emailAccount, emailPassword) as ICredentialsByHost;
        server.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        server.Send(mail);

        if (debugActive)
            Debug.Log("Email "+ item + " has been successfully sent!");
        
        // pauses the thread and stops
        _t1Paused = true;
        emailSent = false;
    }
}