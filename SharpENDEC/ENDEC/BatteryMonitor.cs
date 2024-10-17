using System;
using System.Threading;
using System.Windows.Forms;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public class BatteryMonitor
        {
            public void Monitor()
            {
                // TODO: Add setting to enable and disable battery monitoring
                while (true)
                {
                    try
                    {
                        PowerStatus powerStatus = SystemInformation.PowerStatus;
                        BatteryChargeStatus chargeStatus = powerStatus.BatteryChargeStatus;

                        if (!(powerStatus.BatteryLifePercent > 0.20))
                        {
                            Console.WriteLine($"[Battery] The battery percentage is currently at {powerStatus.BatteryLifePercent}.");
                        }
                        Console.WriteLine($"Battery Status: {chargeStatus}");

                        if ((chargeStatus & BatteryChargeStatus.Charging) == BatteryChargeStatus.Charging)
                        {
                            Console.WriteLine("The system is currently charging.");
                        }
                        else if (chargeStatus == BatteryChargeStatus.NoSystemBattery)
                        {
                            Console.WriteLine("No battery is installed.");
                        }
                        else
                        {
                            Console.WriteLine("The system is not charging.");
                        }
                    }
                    catch (Exception)
                    {

                    }
                    Thread.Sleep(30000);
                }
            }
        }
    }
}
