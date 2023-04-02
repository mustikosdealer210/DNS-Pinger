using System;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Security.Principal;

namespace DNSPinger
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkInterfaceType interfaceType;

            while (true)
            {
                Console.WriteLine("                                                                                                                             ");
                Console.WriteLine("                                                                                                                             ");
                Console.WriteLine("                                                                                                                             ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("                          [1] Ethernet                                           [2] Wi-Fi   ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("                      Used if you have a cable                               Select this for wirless ");
                Console.WriteLine("                        connected to your pc                                     connections                                 ");
                Console.WriteLine("                                                                                                                             ");
                Console.WriteLine("                                                                                                                             ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[");

                bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator); 

                if (isAdmin)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("ADMINISTRATOR");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("USER");
                }
                Console.ResetColor();
                Console.Write("]What internet method do you want to change DNS servers for?: ");

                //ConsoleKeyInfo key = Console.ReadKey(intercept: true);  // You can use it instead of string selection

                string selection = Console.ReadLine();


                if (selection == "1")
                {
                    interfaceType = NetworkInterfaceType.Ethernet;
                    break;
                }
                else if (selection == "2")
                {
                    interfaceType = NetworkInterfaceType.Wireless80211; //Wifi Type
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid option! Please retry.");

                }
            }

            Console.WriteLine($"Hold tight! Searching for the best {interfaceType}'s DNS!");

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            NetworkInterface selectedInterface = interfaces.FirstOrDefault(i => i.NetworkInterfaceType == interfaceType && i.OperationalStatus == OperationalStatus.Up); 

            if (selectedInterface == null)
            {
                Console.WriteLine($"No {interfaceType} is found!"); //For PCs with no Wi-Fi support pops out this message
                return;
            }

            string[] dnsServers = { "8.8.8.8", "1.1.1.1", "1.0.0.1", "8.4.4.8", "9.9.9.9", "149.112.112.112", "209.244.0.4", "209.244.0.3", "208.67.222.222", "208.67.220.220", }; //List of DNS' IPs. The more you put the longer it takes

            Dictionary<string, long> responseTimes = new Dictionary<string, long>();

            foreach (string dnsServer in dnsServers)
            {
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(dnsServer);

                if (reply.Status == IPStatus.Success)
                {
                    responseTimes[dnsServer] = reply.RoundtripTime;
                }
            }

            if (responseTimes.Count > 0)
            {
                var primaryDns = responseTimes.OrderBy(x => x.Value).First().Key;

                Console.WriteLine($"Best DNS have been found and applied:  {primaryDns}");

                ProcessStartInfo startInfo = new ProcessStartInfo(); //Starts applying
                startInfo.FileName = "netsh";
                startInfo.Arguments = $"interface ip set dns name=\"{selectedInterface.Name}\" static \"{primaryDns}\""; //Sets the DNS to the selected internet 
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                try
                {
                    Process.Start(startInfo);
                    Console.WriteLine($"Setting {selectedInterface.Name}'s DNS...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set DNS servers for: {selectedInterface.Name}. Error: {ex.Message}"); //Error with exception
                }
            }
            else
            {
                Console.WriteLine("Unable to find any DNSes"); //if you try to set invalid dns that powershell doesn't let u
            }
            bool adminapproval = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);//stack overflow ( no way I would've found it 
            if (!adminapproval)
            {

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                startInfo.Verb = "runas";
                try
                {
                    Process.Start(startInfo);
                }
                catch
                {
                    Console.WriteLine("Please accept the Administrative approval.");
                    Console.ReadLine();
                    return;
                }

                
                Environment.Exit(0); //doesn't close the windows just stop the code
            }

                Console.ReadLine();
            }
        }
    }