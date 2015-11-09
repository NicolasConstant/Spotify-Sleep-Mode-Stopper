using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SpotifyTools.Contracts;
using SpotifyTools.Tools;

namespace SpotifyTools.Domain.PowerManagement
{
    public class SetThreadExecutionStateHandler : IPreventSleepScreen
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        [Flags]
        private enum ExecutionState : uint
        {
            EsAwaymodeRequired = 0x00000040,
            EsContinuous = 0x80000000,
            EsDisplayRequired = 0x00000002,
            EsSystemRequired = 0x00000001
        }

        public static void PreventSleep()
        {
            SetThreadExecutionState(ExecutionState.EsDisplayRequired | ExecutionState.EsContinuous | ExecutionState.EsSystemRequired);
        }

        public static void AllowSleep()
        {
            SetThreadExecutionState(ExecutionState.EsContinuous);
        }

        private Task _keepScreenUp;
        private CancellationTokenSource _cToken;

        public void EnableConstantDisplayAndPower(bool enableConstantDisplayAndPower)
        {
            if (enableConstantDisplayAndPower)
            {
                _cToken?.Cancel();
                _cToken = new CancellationTokenSource();

                Repeat.Interval(TimeSpan.FromSeconds(10), PreventSleep, _cToken.Token);
            }
            else
            {
                _cToken?.Cancel();
                AllowSleep();
            }
        }
    }
}
