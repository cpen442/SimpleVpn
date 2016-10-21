using System;
using System.Net;
using SimpleVpn.Comms;
using SimpleVpn.Constants;
using System.Windows.Forms;

namespace SimpleVpn
{
    class Program
    {

        static void Main(string[] args)
        {

            try
            {
                Conversation conversation;
                Int32 mode = 1;

                try
                {
                    Console.Write("Please enter {0} for Server mode, or {1} for client mode: ", (int)Mode.Server, (int)Mode.Client);
                    var selectedMode = Console.ReadLine();
                    mode = Convert.ToInt32(selectedMode);

                }
                catch (FormatException)
                {
                    throw new Exception("Error: please enter 1 or 0");
                }

                Console.Write("do you want to enable VERBOSE mode?: ");
                SendKeys.SendWait("yes"); //hello text will be editable :)
                var vb = Console.ReadLine();
                if (vb.ToLower().StartsWith("y"))
                {
                    CConsole.verbose = true;

                    Console.Write("do you want to enable SINGLE STEP mode?: ");
                    SendKeys.SendWait("yes"); //hello text will be editable :)
                    var st = Console.ReadLine();
                    if (st.ToLower().StartsWith("y")) CConsole.step = true;
                }


                switch (mode)
                {
                    case (int)Mode.Server:

                        Console.Write("Please enter the server port: ");
                        var inputPort = Console.ReadLine();
                        var port = Convert.ToInt32(inputPort);

                        var server = new Server(port);



                        Console.Write("Please enter the secret shared key:");
                        string sharedKey = Console.ReadLine();
                        conversation = server.Converse(sharedKey);

                        break;

                    case (int)Mode.Client:

                        Console.Write("Server IP address: ");
                        var inputSvrIpAddr = Console.ReadLine();
                        var svrIpAddr = IPAddress.Parse(inputSvrIpAddr);

                        Console.Write("Server port: ");
                        var inputSvrPort = Console.ReadLine();
                        var svrPort = Convert.ToInt32(inputSvrPort);


                        var client = new Client(svrIpAddr, svrPort);
                        Console.Write("Please enter the shared secret key:");
                        string sharedKey_ = Console.ReadLine();
                        conversation = client.Converse(sharedKey_);

                        break;

                    default:
                        throw new ArgumentException("Please enter a valid mode of operation.");
                }

                while (true)
                {
                    Console.Write(Variables.SendMsg);
                    var msg = Console.ReadLine();

                    conversation.Speak(msg);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                CConsole.WriteLine("Authentication Failed : " + e.Message, ConsoleColor.Red, forceVerbose: true);
            }
            catch (Exception e)
            {
                CConsole.WriteLine(e.Message, ConsoleColor.Red);
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");

                while (!Console.KeyAvailable)
                {
                }
                Environment.Exit(-1);
            }

        }
    }
}