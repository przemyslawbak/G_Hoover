using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Logging;
using Prism.Events;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace G_Hoover.Services.Input
{

    public class InputService : IInputService
    {

        private readonly IInputSimulator _simulator;
        private readonly IEventAggregator _eventAggregator;
        private readonly IParamsLogger _log;


        private readonly Random _rndX; //random int for X
        private readonly Random _rndY; //random int for Y
        private int _corrX; //correction for X
        private int _corrY; //correction for Y

        public InputService(IEventAggregator eventAggregator, IInputSimulator simulator, IParamsLogger log)
        {
            _eventAggregator = eventAggregator;
            _simulator = simulator;
            _log = log;

            _rndX = new Random();
            _rndY = new Random();
            _corrX = 0;
            _corrY = 55;

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);
        }

        public bool Paused { get; set; } //is paused by the user?
        public bool PleaseWaitVisible { get; set; } //is work in progress?

        public void OnUpdateControls(UiPropertiesModel obj)
        {
            Paused = obj.Paused;
            PleaseWaitVisible = obj.PleaseWaitVisible;
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int posX, int posY);

        public async Task ClickAudioChallangeInputAsync()
        {
            await MouseLeftButtonClick(236, 892);
        }

        public async Task ClickCheckboxInputAsync()
        {
            await MouseLeftButtonClick(83, 212);
        }

        public async Task ClickNewAudioChallengeInputAsync(bool inputCorrection)
        {
            if (inputCorrection)
            {
                _corrY = _corrY + 75;
            }

            await MouseLeftButtonClick(165, 440);
        }

        public async Task ClickPlayInputAsync(bool inputCorrection)
        {
            if (inputCorrection)
            {
                _corrY = _corrY + 75;
            }

            await MouseLeftButtonClick(300, 200);
        }

        public async Task ClickTextBoxInputAsync()
        {
            await MouseLeftButtonClick(329, 280);
        }


        public async Task ClickSearchBarInputAsync()
        {
            await MouseLeftButtonClick(820, 565);
        }

        public async Task ClickSearchButtonInputAsync()
        {
            await MouseLeftButtonClick(850, 670);
        }

        public async Task EnterResulInputAsync(string audioResult)
        {
            await KeyboardTextInput(audioResult);
        }

        public async Task EnterPhraseInputAsync(string phrase)
        {
            await KeyboardTextInput(phrase);
        }

        public async Task MouseLeftButtonClick(int posX, int posY)
        {
            _log.Called(posX, posY);

            try
            {
                //add randomity
                int randX = _rndX.Next(-10, 10);
                int randY = _rndY.Next(-10, 10);

                Pause(); //if paused

                await Task.Delay(1000);

                SetCursorPos(posX + randX + _corrX, posY + randY + _corrY); //move to x-y

                _simulator.Mouse.LeftButtonClick(); //click l.btn
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        public async Task KeyboardTextInput(string text)
        {
            _log.Called(text);

            Pause(); //if paused

            await Task.Delay(1);

            _simulator.Keyboard.TextEntry(text);

            await Task.Delay(1000);

            _simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        public void Pause()
        {
            while (Paused || PleaseWaitVisible)
                Thread.Sleep(50);
        }

    }
}
