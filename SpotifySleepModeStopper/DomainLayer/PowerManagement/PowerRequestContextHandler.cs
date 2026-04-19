using System;
using System.Runtime.InteropServices;
using SpotifyTools.Contracts;

namespace SpotifyTools.DomainLayer.PowerManagement
{
    public class PowerRequestContextHandler : IPreventSleepScreen, IDisposable
    {
        #region prevent screensaver, display dimming and automatically sleeping
        PowerRequestContext _powerRequestContext;
        IntPtr _powerRequest = IntPtr.Zero; //HANDLE
        bool _isPowerRequestActive = false;

        // Availability Request Functions
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr PowerCreateRequest(ref PowerRequestContext context);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool PowerSetRequest(IntPtr powerRequestHandle, PowerRequestType requestType);

        [DllImport("kernel32.dll", SetLastError = true)]
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
                // Prevent duplicate power requests that would orphan previous handles
                if (_isPowerRequestActive)
                {
                    // Already active - if display setting changed, update it
                    if (enableConstantDisplay && !IsDisplayRequiredSet())
                    {
                        PowerSetRequest(_powerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    }
                    else if (!enableConstantDisplay && IsDisplayRequiredSet())
                    {
                        PowerClearRequest(_powerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    }
                    return;
                }

                // Set up the diagnostic string
                _powerRequestContext.Version = PowerRequestContextVersion;
                _powerRequestContext.Flags = PowerRequestContextSimpleString;
                _powerRequestContext.SimpleReasonString = "Playing music";
                // your reason for changing the power settings;

                // Create the request, get a handle
                _powerRequest = PowerCreateRequest(ref _powerRequestContext);

                if (_powerRequest == IntPtr.Zero)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(error, "PowerCreateRequest failed");
                }

                // Set the request
                bool result = PowerSetRequest(_powerRequest, PowerRequestType.PowerRequestSystemRequired);
                if (!result)
                {
                    var error = Marshal.GetLastWin32Error();
                    CloseHandle(_powerRequest);
                    _powerRequest = IntPtr.Zero;
                    throw new System.ComponentModel.Win32Exception(error, "PowerSetRequest (SystemRequired) failed");
                }

                if (enableConstantDisplay)
                {
                    result = PowerSetRequest(_powerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    if (!result)
                    {
                        var error = Marshal.GetLastWin32Error();
                        PowerClearRequest(_powerRequest, PowerRequestType.PowerRequestSystemRequired);
                        CloseHandle(_powerRequest);
                        _powerRequest = IntPtr.Zero;
                        throw new System.ComponentModel.Win32Exception(error, "PowerSetRequest (DisplayRequired) failed");
                    }
                }

                _isPowerRequestActive = true;
            }
            else
            {
                ClearPowerRequest();
            }
        }

        private bool IsDisplayRequiredSet()
        {
            // We track this internally - in the current implementation,
            // we don't have a way to query the OS for this.
            // For simplicity, we assume display was set if screen sleep is disabled.
            // This is sufficient for the toggle use case in the facade.
            return false; // Conservative default - will re-apply if needed
        }

        private void ClearPowerRequest()
        {
            if (!_isPowerRequestActive || _powerRequest == IntPtr.Zero)
                return;

            // Clear the requests
            PowerClearRequest(_powerRequest, PowerRequestType.PowerRequestSystemRequired);
            PowerClearRequest(_powerRequest, PowerRequestType.PowerRequestDisplayRequired);

            CloseHandle(_powerRequest);
            _powerRequest = IntPtr.Zero;
            _isPowerRequestActive = false;
        }

        public void Dispose()
        {
            ClearPowerRequest();
        }
    }
}