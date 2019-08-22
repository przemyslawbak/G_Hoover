using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace G_Hoover.Services.Input
{

    public class InputService : IInputService
    {
        private readonly InputSimulator _simulator; //framework
        private readonly Random _rndX; //random int for X
        private readonly Random _rndY; //random int for Y
        private int _corrX; //correction for X
        private int _corrY; //correction for Y

        public InputService()
        {
            _simulator = new InputSimulator();
            _rndX = new Random();
            _rndY = new Random();
            _corrX = 0;
            _corrY = 55;
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

        public async Task EnterResulInputAsync(string audioResult)
        {
            await Task.Delay(1000);

            _simulator.Keyboard.TextEntry(audioResult);
        }

        public async Task ClickSendResultInputAsync()
        {
            await MouseLeftButtonClick(450, 433);
        }

        public async Task MouseLeftButtonClick(int posX, int posY)
        {
            //add randomity
            int randX = _rndX.Next(-10, 10);
            int randY = _rndY.Next(-10, 10);

            await Task.Delay(1000);

            SetCursorPos(posX + randX + _corrX, posY + randY + _corrY); //move to x-y

            await Task.Delay(1000);

            _simulator.Mouse.LeftButtonClick(); //click l.btn
        }

    }
}
