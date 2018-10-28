using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace UdpListenerLib.Demo.Core.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Options options = new Options();
                var (parseResult, parseErrorMessage) = ParseArguments(args, options);
                if (parseResult)
                {
                    UdpListener listener;

                    if (options.SourceAddress != null)
                        listener = new UdpListener(options.SourceAddress, options.Port);
                    else
                        listener = new UdpListener(options.Port);

                    listener.DataReceive += Listener_DataReceive;
                    listener.StartListening();
                    Console.WriteLine($"UDP Listener started at port {listener.Port} and source address {listener.SourceAddress.ToString()}");
                    Console.WriteLine("Press Enter to close");
                    Console.ReadLine();
                    listener.StopListening();
                }
                else
                {
                    WriteLine(parseErrorMessage, ConsoleColor.Red);
                    ShowHelp();
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }

        private static (bool result, string errorMessage) ParseArguments(string[] args, Options options)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-p", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (i == args.Length - 1)
                        return (result: false, errorMessage: "No port number given");
                    if (!UInt16.TryParse(args[i + 1], out UInt16 port))
                        return (result: false, errorMessage: $"Invalid port number: {args[i + 1]}");
                    options.Port = port;
                    i++;
                }
                else if (args[i].Equals("-s", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (i == args.Length - 1)
                        return (result: false, errorMessage: "No source address given");
                    if (!IPAddress.TryParse(args[i + 1], out IPAddress address))
                        return (result: false, errorMessage: $"Invalid source address: {args[i + 1]}");
                    options.SourceAddress = address;
                    i++;
                }
            }

            if (options.Port == 0)
                return (result: false, errorMessage: "No port number given");

            return (result: true, errorMessage: "");
        }

        private static void ShowHelp()
        {
            Console.WriteLine("-p <port number>: UDP port number to listen to");
            Console.WriteLine("-s <source ip address> (optional) : source ip address to listen to. If no source is specified \"Any\" is used as the source");
        }

        private static void WriteLine(string str, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.WriteLine(str);
                Console.ForegroundColor = oldColor;
            }
            else
                Console.WriteLine(str);
        }

        private static void Listener_DataReceive(object sender, UdpDataReceiveEventArgs e)
        {
            string str = Encoding.UTF8.GetString(e.Data);
            str = Regex.Replace(str, @"\r\n?|\n", string.Empty);
            WriteLine($"Received \"{str}\" from {e.RemoteIP?.ToString()}", ConsoleColor.Cyan);
        }
    }
}
