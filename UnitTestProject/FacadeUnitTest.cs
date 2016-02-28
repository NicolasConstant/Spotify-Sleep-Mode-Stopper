using System;
using System.IO;
using System.Security.Policy;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Impl.Invocation.Specifications;
using SpotifyTools;
using SpotifyTools.Contracts;
using SpotifyTools.Tools;
using SpotifyTools.Tools.Model;

namespace UnitTestProject
{
    /// <summary>
    /// Use RhinoMocks
    /// 
    /// Quick Guide:
    /// http://www.wrightfully.com/using-rhino-mocks-quick-guide-to-generating-mocks-and-stubs/
    /// </summary>
    [TestClass]
    public class FacadeUnitTest
    {
        [TestMethod]
        public void CheckIfScreenSleepEnabledTestMethod()
        {
            var settingsManagerStub = MockRepository.GenerateMock<ISettingsManager<AppSettings>>();
            settingsManagerStub.Expect(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });

            var facade = new SpotifySaveModeStopperFacade(null, null, null, null, null, null, settingsManagerStub);

            Assert.IsTrue(facade.IsScreenSleepEnabled());
            settingsManagerStub.VerifyAllExpectations();
        }

        [TestMethod]
        public void CheckIfAutoStartEnabledTestMethod()
        {
            var settingsManagerStub = MockRepository.GenerateStub<ISettingsManager<AppSettings>>();
            settingsManagerStub.Stub(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });

            var autoStartManagerStub = MockRepository.GenerateMock<IAutoStartManager>();
            autoStartManagerStub.Expect(x => x.IsAutoStartSet()).Return(true);

            var facade = new SpotifySaveModeStopperFacade(null, null, null, null, null, autoStartManagerStub, settingsManagerStub);

            Assert.IsTrue(facade.IsAutoStartEnabled());
            autoStartManagerStub.VerifyAllExpectations();
        }

        [TestMethod]
        public void ChangeScreenSleepSettingsTestMethod()
        {
            var saveConfigFired = new ManualResetEvent(false);

            var settingsManagerStub = MockRepository.GenerateMock<ISettingsManager<AppSettings>>();
            settingsManagerStub.Stub(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });
            settingsManagerStub.Expect(x => x.SaveConfig(new AppSettings()))
                .Constraints(Property.Value("IsScreenSleepEnabled", true))
                .Do((Action<AppSettings>)(ant => { saveConfigFired.Set(); }));

            var facade = new SpotifySaveModeStopperFacade(null, null, null, null, null, null, settingsManagerStub);
            facade.ChangeScreenSleep(true);

            Assert.IsTrue(saveConfigFired.WaitOne(10));
            settingsManagerStub.VerifyAllExpectations();
        }

        [TestMethod]
        public void ChangeAutoStartSettingsTestMethod()
        {
            var autoStartChangeFired = new ManualResetEvent(false);

            var settingsManagerStub = MockRepository.GenerateStub<ISettingsManager<AppSettings>>();
            settingsManagerStub.Stub(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });

            var autoStartManagerStub = MockRepository.GenerateMock<IAutoStartManager>();
            autoStartManagerStub.Expect(x => x.SetAutoStart(Arg<bool>.Is.Equal(true)))
                .Do((Action<bool>)(ant => { autoStartChangeFired.Set(); }));

            var facade = new SpotifySaveModeStopperFacade(null, null, null, null, null, autoStartManagerStub, settingsManagerStub);
            facade.ChangeAutoStart(true);

            Assert.IsTrue(autoStartChangeFired.WaitOne(10));
            autoStartManagerStub.VerifyAllExpectations();
        }

        [TestMethod]
        public void ListeningTestMethod()
        {
            const string spotifyProcessName = "Spotify";

            var messageDisplayerStub = MockRepository.GenerateStub<IMessageDisplayer>();
            messageDisplayerStub.Stub(x => x.OutputMessage(Arg<string>.Is.Anything)).Repeat.Any();

            var settingsManagerStub = MockRepository.GenerateStub<ISettingsManager<AppSettings>>();
            settingsManagerStub.Stub(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });

            var processAnalyserStub = MockRepository.GenerateStub<IProcessAnalyser>();
            processAnalyserStub.Stub(x => x.IsAppRunning(spotifyProcessName))
                .Repeat.Any()
                .Return(true);

            var appStateStub = MockRepository.GenerateStub<IAppStatusReporting>();
            appStateStub.Stub(x => x.NotifyAntiSleepingModeIsActivated());
            appStateStub.Stub(x => x.NotifyAntiSleepingModeIsDisabled());

            var constantDisplayEnabled = new ManualResetEvent(false);
            var constantDisplayDisabled = new ManualResetEvent(false);

            var preventSleepScreenMock = MockRepository.GenerateMock<IPreventSleepScreen>();
            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(true, false))
                .Do((Action<bool, bool>)((contantPower, constantScreen) =>
                {
                    constantDisplayDisabled.Reset();
                    constantDisplayEnabled.Set();
                }));

            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(false, false))
                .Do((Action<bool, bool>)((contantPower, constantScreen) =>
                {
                    constantDisplayDisabled.Set();
                }));

            //Sequence: 
            //  false - false - true - true - false always
            var calledTimes = 0;
            var soundAnalyserMock = MockRepository.GenerateMock<ISoundAnalyser>();

            soundAnalyserMock.Expect(x => x.IsProcessNameOutputingSound(Arg<string>.Is.Equal(spotifyProcessName)))
                .Return(false)
                .WhenCalled(invk =>
                {
                    Interlocked.Increment(ref calledTimes);
                    if (calledTimes < 3)
                        invk.ReturnValue = false;
                    else if(calledTimes < 5)
                        invk.ReturnValue = true;
                    else
                        invk.ReturnValue = false;
                });

            var facade = new SpotifySaveModeStopperFacade(messageDisplayerStub, preventSleepScreenMock, soundAnalyserMock, processAnalyserStub, appStateStub, null, settingsManagerStub, 1);
            facade.StartListening();
            
            Assert.IsTrue(constantDisplayEnabled.WaitOne(2*2*1500));
            Assert.IsTrue(constantDisplayDisabled.WaitOne(2*3*1500));

            preventSleepScreenMock.VerifyAllExpectations();
            soundAnalyserMock.VerifyAllExpectations();
        }

        [TestMethod]
        public void StopListeningTestMethod()
        {
            const string spotifyProcessName = "Spotify";

            var messageDisplayerStub = MockRepository.GenerateStub<IMessageDisplayer>();
            messageDisplayerStub.Stub(x => x.OutputMessage(Arg<string>.Is.Anything)).Repeat.Any();

            var settingsManagerStub = MockRepository.GenerateStub<ISettingsManager<AppSettings>>();
            settingsManagerStub.Stub(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });

            var processAnalyserStub = MockRepository.GenerateStub<IProcessAnalyser>();
            processAnalyserStub.Stub(x => x.IsAppRunning(spotifyProcessName))
                .Repeat.Any()
                .Return(true);

            var appStateStub = MockRepository.GenerateStub<IAppStatusReporting>();
            appStateStub.Stub(x => x.NotifyAntiSleepingModeIsActivated());
            appStateStub.Stub(x => x.NotifyAntiSleepingModeIsDisabled());

            var constantDisplayEnabled = new ManualResetEvent(false);
            var constantDisplayDisabled = new ManualResetEvent(false);

            var preventSleepScreenMock = MockRepository.GenerateMock<IPreventSleepScreen>();
            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(true, false))
                .Do((Action<bool, bool>)((contantPower, constantScreen) =>
                {
                    constantDisplayDisabled.Reset();
                    constantDisplayEnabled.Set();
                }));

            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(false, false))
                .Do((Action<bool, bool>)((contantPower, constantScreen) =>
                {
                    constantDisplayDisabled.Set();
                }));

            //Sequence: 
            //  false - false - true always
            var calledTimes = 0;
            var soundAnalyserMock = MockRepository.GenerateMock<ISoundAnalyser>();

            soundAnalyserMock.Expect(x => x.IsProcessNameOutputingSound(Arg<string>.Is.Equal(spotifyProcessName)))
                .Return(false)
                .WhenCalled(invk =>
                {
                    Interlocked.Increment(ref calledTimes);
                    if (calledTimes < 3)
                        invk.ReturnValue = false;
                    else
                        invk.ReturnValue = true;
                });

            var facade = new SpotifySaveModeStopperFacade(messageDisplayerStub, preventSleepScreenMock, soundAnalyserMock, processAnalyserStub, appStateStub, null, settingsManagerStub, 1);
            facade.StartListening();

            Assert.IsTrue(constantDisplayEnabled.WaitOne(2 * 2 * 1500));
            Assert.IsFalse(constantDisplayDisabled.WaitOne(1000));

            facade.StopListening();

            Assert.IsTrue(constantDisplayDisabled.WaitOne(2 * 3 * 1500));

            preventSleepScreenMock.VerifyAllExpectations();
            soundAnalyserMock.VerifyAllExpectations();
        }

        [TestMethod]
        public void ChangeListeningTestMethod()
        {
            const string spotifyProcessName = "Spotify";

            var messageDisplayerStub = MockRepository.GenerateStub<IMessageDisplayer>();
            messageDisplayerStub.Stub(x => x.OutputMessage(Arg<string>.Is.Anything)).Repeat.Any();

            var settingsManagerStub = MockRepository.GenerateStub<ISettingsManager<AppSettings>>();
            settingsManagerStub.Stub(x => x.GetConfig()).Return(new AppSettings { IsScreenSleepEnabled = true });

            var processAnalyserStub = MockRepository.GenerateStub<IProcessAnalyser>();
            processAnalyserStub.Stub(x => x.IsAppRunning(spotifyProcessName))
                .Repeat.Any()
                .Return(true);

            var appStateStub = MockRepository.GenerateStub<IAppStatusReporting>();
            appStateStub.Stub(x => x.NotifyAntiSleepingModeIsActivated());
            appStateStub.Stub(x => x.NotifyAntiSleepingModeIsDisabled());

            var constantDisplaySleepScreenEnabled = new ManualResetEvent(false);
            var constantDisplayNoSleepScreenEnabled = new ManualResetEvent(false);
            var constantDisplayDisabled = new ManualResetEvent(false);

            var preventSleepScreenMock = MockRepository.GenerateMock<IPreventSleepScreen>();
            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(true, false))
                .Do((Action<bool, bool>)((contantPower, constantScreen) =>
                {
                    constantDisplayDisabled.Reset();
                    constantDisplayNoSleepScreenEnabled.Reset();
                    constantDisplaySleepScreenEnabled.Set();
                }));

            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(true, true))
               .Do((Action<bool, bool>)((contantPower, constantScreen) =>
               {
                   constantDisplaySleepScreenEnabled.Reset();
                   constantDisplayDisabled.Reset();
                   constantDisplayNoSleepScreenEnabled.Set();
               }));

            preventSleepScreenMock.Expect(x => x.EnableConstantDisplayAndPower(false, false))
                .Do((Action<bool, bool>)((contantPower, constantScreen) =>
                {
                    constantDisplayNoSleepScreenEnabled.Reset();
                    constantDisplaySleepScreenEnabled.Reset();
                    constantDisplayDisabled.Set();
                }));
            
            //Sequence: 
            //  false - false - true always
            var calledTimes = 0;
            var soundAnalyserMock = MockRepository.GenerateMock<ISoundAnalyser>();

            soundAnalyserMock.Expect(x => x.IsProcessNameOutputingSound(Arg<string>.Is.Equal(spotifyProcessName)))
                .Return(false)
                .WhenCalled(invk =>
                {
                    Interlocked.Increment(ref calledTimes);
                    if (calledTimes < 3)
                        invk.ReturnValue = false;
                    else
                        invk.ReturnValue = true;
                });

            var facade = new SpotifySaveModeStopperFacade(messageDisplayerStub, preventSleepScreenMock, soundAnalyserMock, processAnalyserStub, appStateStub, null, settingsManagerStub, 1);
            facade.StartListening();

            var debugCoef = 1;

#if DEBUG 
            debugCoef = 60;
#endif

            Assert.IsTrue(constantDisplaySleepScreenEnabled.WaitOne(debugCoef * 2 * 2 * 1500));
            Assert.IsFalse(constantDisplayDisabled.WaitOne(1000));

            facade.ChangeScreenSleep(false);

            Assert.IsTrue(constantDisplayDisabled.WaitOne(debugCoef * 2 * 1 * 1500));
            Assert.IsTrue(constantDisplayNoSleepScreenEnabled.WaitOne(debugCoef * 2 * 1 * 1500));

            Assert.IsFalse(constantDisplayDisabled.WaitOne(500));
            Assert.IsFalse(constantDisplaySleepScreenEnabled.WaitOne(500));

            preventSleepScreenMock.VerifyAllExpectations();
            soundAnalyserMock.VerifyAllExpectations();
        }
    }
}
