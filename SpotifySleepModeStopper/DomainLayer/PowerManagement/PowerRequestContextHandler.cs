using System;
using System.Runtime.InteropServices;
using SpotifyTools.Contracts;

namespace SpotifyTools.DomainLayer.PowerManagement
{
    public class PowerRequestContextHandler : IPreventSleepScreen
    {
        #region prevent screensaver, display dimming and automatically sleeping
        PowerRequestContext _powerRequestContext;
        IntPtr _powerRequest; //HANDLE

        // Availability Request Functions
        [DllImport("kernel32.dll")]
        static extern IntPtr PowerCreateRequest(ref PowerRequestContext context);

        [DllImport("kernel32.dll")]
        static extern bool PowerSetRequest(IntPtr powerRequestHandle, PowerRequestType requestType);

        [DllImport("kernel32.dll")]
        static extern bool PowerClearRequest(IntPtr powerRequestHandle, PowerRequestType requestType);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern int CloseHandle(IntPtr hObject);

        // Availablity Request Enumerations and Constants
        enum PowerRequestType
        {
            PowerRequestDisplayRequired = 0,
            PowerRequestSystemRequired,
            PowerRequestAwayModeRequired,
            PowerRequestMaximum
        }

        const int PowerRequestContextVersion = 0;
        const int PowerRequestContextSimpleString = 0x1;
        const int PowerRequestContextDetailedString = 0x2;

        // Availablity Request Structures
        // Note:  Windows defines the POWER_REQUEST_CONTEXT structure with an
        // internal union of SimpleReasonString and Detailed information.
        // To avoid runtime interop issues, this version of 
        // POWER_REQUEST_CONTEXT only supports SimpleReasonString.  
        // To use the detailed information,
        // define the PowerCreateRequest function with the first 
        // parameter of type POWER_REQUEST_CONTEXT_DETAILED.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PowerRequestContext
        {
            public UInt32 Version;
            public UInt32 Flags;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string
                SimpleReasonString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PowerRequestContextDetailedInformation
        {
            public IntPtr LocalizedReasonModule;
            public UInt32 LocalizedReasonId;
            public UInt32 ReasonStringCount;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string[] ReasonStrings;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PowerRequestContextDetailed
        {
            public UInt32 Version;
            public UInt32 Flags;
            public PowerRequestContextDetailedInformation DetailedInformation;
        }
        #endregion
        
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        /// <summary>
        /// Prevent screensaver, display dimming and power saving. This function wraps PInvokes on Win32 API. 
        /// </summary>
        /// <param name="enableConstantDisplay"></param>
        /// <param name="enableConstantPower"></param>
        public void EnableConstantDisplayAndPower(bool enableConstantPower, bool enableConstantDisplay)
        {
            if (enableConstantPower)
            {
                // Set up the diagnostic string
                _powerRequestContext.Version = PowerRequestContextVersion;
                _powerRequestContext.Flags = PowerRequestContextSimpleString;
                _powerRequestContext.SimpleReasonString = "Playing music";
                // your reason for changing the power settings;

                // Create the request, get a handle
                _powerRequest = PowerCreateRequest(ref _powerRequestContext);

                // Set the request
                PowerSetRequest(_powerRequest, PowerRequestType.PowerRequestSystemRequired);

                if (enableConstantDisplay)
                    PowerSetRequest(_powerRequest, PowerRequestType.PowerRequestDisplayRequired);
            }
            else
            {
                // Clear the request
                PowerClearRequest(_powerRequest, PowerRequestType.PowerRequestSystemRequired);
                PowerClearRequest(_powerRequest, PowerRequestType.PowerRequestDisplayRequired);

                CloseHandle(_powerRequest);
            }
        }
    }
}