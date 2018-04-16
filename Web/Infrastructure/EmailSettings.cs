using System;
using System.Linq;

namespace Web.Infrastructure
{
    public class EmailSettings
    {
        public string MailToAddress = "example@email.com";
        public string MailFromAddress = "example@email.com";
        public bool UseSsl = true;
        public string Username = "Your_Username";
        public string Password = "Your_Password";
        public string ServerName = "Your_Server";
        public int ServerPort = 587;
        public bool WriteAsFile = false;
        public string FileLocation = AppDomain.CurrentDomain.BaseDirectory + "App_Data";

        public void ReadFromFile(string path)   //Settings are loaded from a plain text file with 9 lines, one for each field.
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            if (lines == null || lines.Count() < 9) return;

            MailToAddress = lines[0];
            MailFromAddress = lines[1];
            UseSsl = bool.Parse(lines[2]);
            Username = lines[3];
            Password = lines[4];
            ServerName = lines[5];
            ServerPort = int.Parse(lines[6]);
            WriteAsFile = bool.Parse(lines[7]);
            FileLocation = AppDomain.CurrentDomain.BaseDirectory + lines[8];
        }
    }
}