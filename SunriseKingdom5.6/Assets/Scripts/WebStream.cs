using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;

// Created by James Bentley 
// Created is a strong word, more like stolen from here: 
// http://answers.unity3d.com/questions/1151512/show-video-from-ip-camera-source.html

// Uses an HTTP Request to stream video over the web from the axis cam
// Works only with mjpeg version because it uses an HTTP request

public class WebStream : MonoBehaviour
{

    public MeshRenderer frame;    //Mesh for displaying video

    public string sourceURL = "http://192.168.1.180/axis-cgi/mjpg/video.cgi";
    private Texture2D texture;
    private Stream stream;

    public void Start()
    {
        GetVideo();
    }

    public void GetVideo()
    {
        texture = new Texture2D(2, 2);
        // create HTTP request
        WebRequest req = (WebRequest)WebRequest.Create(sourceURL);
        //Optional (if authorization is Digest)
        req.Credentials = new NetworkCredential("root", "pass");
        // get response
        WebResponse resp = req.GetResponse();

        // get response stream
        stream = resp.GetResponseStream();
        StartCoroutine(GetFrame());
    }

    IEnumerator GetFrame()
    {
        Byte[] JpegData = new Byte[100000];

        while (true)
        {
            int bytesToRead = FindLength(stream);
            print(bytesToRead);
            if (bytesToRead == -1)
            {
                print("End of stream");
                yield break;
            }

            int leftToRead = bytesToRead;

            while (leftToRead > 0)
            {
                leftToRead -= stream.Read(JpegData, bytesToRead - leftToRead, leftToRead);
                yield return null;
            }

            MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

            texture.LoadImage(ms.GetBuffer());
            frame.material.mainTexture = texture;
            stream.ReadByte(); // CR after bytes
            stream.ReadByte(); // LF after bytes
        }
    }

    int FindLength(Stream stream)
    {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;

        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue; // ignore LF char
            if (b == 13)
            { // CR
                if (atEOL)
                {  // two blank lines means end of header
                    stream.ReadByte(); // eat last LF
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
            }
        }
        return -1;
    }
}