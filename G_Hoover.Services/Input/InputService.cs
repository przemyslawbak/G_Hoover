using G_Hoover.Events;
using G_Hoover.Models;
using Params_Logger;
using Prism.Events;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace G_Hoover.Services.Input
{
    /// <summary>
    /// service class that uses IInputSimulator for generating simulated inputs and uses User32.dll for moving the mouse coursor
    /// </summary>
    public class InputService : IInputService
    {
        private readonly IInputSimulator _simulator;
        private readonly IEventAggregator _eventAggregator;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();


        private readonly Random _rndX; //random int for X
        private readonly Random _rndY; //random int for Y
        private int _corrX; //correction for X
        private int _corrY; //correction for Y

        public InputService(IEventAggregator eventAggregator, IInputSimulator simulator)
        {
            _eventAggregator = eventAggregator;
            _simulator = simulator;

            _rndX = new Random();
            _rndY = new Random();
            _corrX = 0;
            _corrY = 55;

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);
        }

        public bool Paused { get; set; } //is paused by the user?
        public bool PleaseWaitVisible { get; set; } //is `please wait` status?

        /// <summary>
        /// controls updated on event handling
        /// </summary>
        public void OnUpdateControls(UiPropertiesModel obj)
        {
            Paused = obj.Paused;
            PleaseWaitVisible = obj.PleaseWaitVisible;
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int posX, int posY);

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        public async Task ClickAudioChallangeInputAsync()
        {
            await MouseLeftButtonClick(236, 892);
        }

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        public async Task ClickCheckboxInputAsync()
        {
            await MouseLeftButtonClick(83, 212);
        }

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        /// <param name="inputCorrection">adds optional input correction</param>
        public async Task ClickNewAudioChallengeInputAsync(bool inputCorrection)
        {
            if (inputCorrection)
            {
                _corrY = _corrY + 75;
            }

            await MouseLeftButtonClick(165, 440);
        }

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        /// <param name="inputCorrection">adds optional input correction</param>
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

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        public async Task ClickSearchBarInputAsync()
        {
            await MouseLeftButtonClick(820, 565);
        }

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        public async Task ClickSearchButtonInputAsync()
        {
            await MouseLeftButtonClick(850, 670);
        }

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        public async Task EnterResulInputAsync(string audioResult)
        {
            await KeyboardTextInput(audioResult);
        }

        /// <summary>
        /// clicks l.mouse btn
        /// </summary>
        public async Task EnterPhraseInputAsync(string phrase)
        {
            await KeyboardTextInput(phrase);
        }

        /// <summary>
        /// DRY moves mouse to X,Y, and clicks l.mouse btn
        /// </summary>
        /// <param name="posX">X coordinate</param>
        /// <param name="posY">Y coordinate</param>
        /// <returns></returns>
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

        /// <summary>
        /// DRY enters text input
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task KeyboardTextInput(string text)
        {
            _log.Called(text);

            Pause(); //if paused

            await Task.Delay(1);

            _simulator.Keyboard.TextEntry(text);

            await Task.Delay(1000);

            _simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        /// <summary>
        /// short pause while paused or wait status
        /// </summary>
        public void Pause()
        {
            while (Paused || PleaseWaitVisible)
                Thread.Sleep(50);
        }

    }
}
