using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.IO;
using System.IO.Ports;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Input = Microsoft.Xna.Framework.Input; // to provide shorthand to clear up ambiguities

//for voice recognition
using System.Speech;
using System.Speech.Synthesis;
using System.Speech.Recognition;



namespace RS232_KHR1_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
       

        //initialiation for pc to robot communication using serial port 
        System.IO.Ports.SerialPort port = new SerialPort();

        //for voice recognition
        //speech
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        //create the recognition engine
        SpeechRecognitionEngine sre = new SpeechRecognitionEngine();
        //create a new GrammerBuilder to specify which commands we want to use
        GrammarBuilder grammarBuilder = new GrammarBuilder();
        //text-to-speech

        #region Servo Control method
        /*
        1.c++ unsigned char is c# byte..
        2. single servo control instruction
        3. 個別サーボ駆動 送信 [255][ID][長(4)][命令(2)][サーボ番号][サーボ位置][Speed]
        4. byte lrBody means.. (left body is 1 and Right Body is 0)
        */ 
        private bool comportCheck()
        {
            if (port.IsOpen == false)
            {
                MessageBox.Show("COM port is not open.. please check your com Port");
            }
            return port.IsOpen;
        }

        private void singleServoControl(byte lrBody, byte idNum, byte pose)
        {
            if (comportCheck())
            {
                byte[] servo = new byte[7];

                servo[0] = 255;
                servo[1] = lrBody;
                servo[2] = 4;
                servo[3] = 2;
                servo[4] = idNum;
                servo[5] = pose;
                servo[6] = Convert.ToByte(textBox4.Text);

                port.DiscardInBuffer(); //buffer clear
                port.DiscardOutBuffer(); //buffer clear
                port.Write(servo, 0, servo.Length);
            }
        }

        /*
        1. excute all of servo
        2. 全サーボ駆動 送信 [255][ID][長(14)][命令(1)][P0][P1]...[P10][P11][Speed] 
        */
        static byte[,] botMotion = new byte[255, 34];

        private void saveMotion(int i)
        {
            //left body
            botMotion[i, 0] = 255;
            botMotion[i, 1] = 1;
            botMotion[i, 2] = 14;
            botMotion[i, 3] = 1;
            botMotion[i, 4] = 0;
            botMotion[i, 5] = Convert.ToByte(numericUpDown1.Value);
            botMotion[i, 6] = Convert.ToByte(numericUpDown2.Value);
            botMotion[i, 7] = Convert.ToByte(numericUpDown3.Value);
            botMotion[i, 8] = Convert.ToByte(numericUpDown4.Value);
            botMotion[i, 9] = Convert.ToByte(numericUpDown5.Value);
            botMotion[i, 10] = Convert.ToByte(numericUpDown6.Value);
            botMotion[i, 11] = Convert.ToByte(numericUpDown7.Value);
            botMotion[i, 12] = Convert.ToByte(numericUpDown8.Value);
            botMotion[i, 13] = 0;
            botMotion[i, 14] = 0;
            botMotion[i, 15] = 0;
            botMotion[i, 16] = Convert.ToByte(textBox4.Text);

            //right body
            botMotion[i, 17] = 255;
            botMotion[i, 18] = 0;
            botMotion[i, 19] = 14;
            botMotion[i, 20] = 1;
            botMotion[i, 21] = 0;
            botMotion[i, 22] = Convert.ToByte(numericUpDown9.Value);
            botMotion[i, 23] = Convert.ToByte(numericUpDown10.Value);
            botMotion[i, 24] = Convert.ToByte(numericUpDown11.Value);
            botMotion[i, 25] = Convert.ToByte(numericUpDown12.Value);
            botMotion[i, 26] = Convert.ToByte(numericUpDown13.Value);
            botMotion[i, 27] = Convert.ToByte(numericUpDown14.Value);
            botMotion[i, 28] = Convert.ToByte(numericUpDown15.Value);
            botMotion[i, 29] = Convert.ToByte(numericUpDown16.Value);
            botMotion[i, 30] = Convert.ToByte(numericUpDown17.Value);
            botMotion[i, 31] = 0;
            botMotion[i, 32] = 0;
            botMotion[i, 33] = Convert.ToByte(textBox4.Text);
        }

        private void excuteMotion()
        {
            byte[] leftServo = new byte[17];
            byte[] rightServo = new byte[17];

            leftServo.Initialize();
            rightServo.Initialize();

            for (int i = 0; i < 255; i++)
            {
                if (botMotion[i, 0] != 255)
                {
                    break;
                }//end if

                for (int j = 0; j < 34; j++)
                {
                    if (j < 17)
                    {
                        leftServo[j] = botMotion[i, j];
                    }
                    
                    if(j >= 17)
                    {
                        rightServo[j - 17] = botMotion[i, j];
                    }                   
                }//end for j

                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                port.Write(leftServo, 0, leftServo.Length);

                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                port.Write(rightServo, 0, rightServo.Length);
                Thread.Sleep(rightServo[16] * 20);  //interval Time
            }//end for i
        }

        private void initializePosition()
        {

            numericUpDown1.Value = 128;
            numericUpDown2.Value = 128;
            numericUpDown3.Value = 128;
            numericUpDown4.Value = 128;
            numericUpDown5.Value = 128;
            numericUpDown6.Value = 128;
            numericUpDown7.Value = 128;
            numericUpDown8.Value = 128;

            numericUpDown9.Value = 128;
            numericUpDown10.Value = 128;
            numericUpDown11.Value = 128;
            numericUpDown12.Value = 128;
            numericUpDown13.Value = 128;
            numericUpDown14.Value = 128;
            numericUpDown15.Value = 128;
            numericUpDown16.Value = 128;
            numericUpDown17.Value = 128;

            singleServoControl(1, 1, Convert.ToByte(numericUpDown1.Value));
            singleServoControl(1, 2, Convert.ToByte(numericUpDown2.Value));
            singleServoControl(1, 3, Convert.ToByte(numericUpDown3.Value));
            singleServoControl(1, 4, Convert.ToByte(numericUpDown4.Value));
            singleServoControl(1, 5, Convert.ToByte(numericUpDown5.Value));
            singleServoControl(1, 6, Convert.ToByte(numericUpDown6.Value));
            singleServoControl(1, 7, Convert.ToByte(numericUpDown7.Value));
            singleServoControl(1, 8, Convert.ToByte(numericUpDown8.Value));

            singleServoControl(0, 1, Convert.ToByte(numericUpDown9.Value));
            singleServoControl(0, 2, Convert.ToByte(numericUpDown10.Value));
            singleServoControl(0, 3, Convert.ToByte(numericUpDown11.Value));
            singleServoControl(0, 4, Convert.ToByte(numericUpDown12.Value));
            singleServoControl(0, 5, Convert.ToByte(numericUpDown13.Value));
            singleServoControl(0, 6, Convert.ToByte(numericUpDown14.Value));
            singleServoControl(0, 7, Convert.ToByte(numericUpDown15.Value));
            singleServoControl(0, 8, Convert.ToByte(numericUpDown16.Value));
            singleServoControl(0, 9, Convert.ToByte(numericUpDown17.Value)); 
        }

        private void MotionLoad()
        {
            int i =Convert.ToInt32(numericUpDown18.Value);

            numericUpDown1.Value = botMotion[i, 5];
            numericUpDown2.Value = botMotion[i, 6];
            numericUpDown3.Value = botMotion[i, 7];
            numericUpDown4.Value = botMotion[i, 8];
            numericUpDown5.Value = botMotion[i, 9];
            numericUpDown6.Value = botMotion[i, 10];
            numericUpDown7.Value = botMotion[i, 11];
            numericUpDown8.Value = botMotion[i, 12];

            numericUpDown9.Value = botMotion[i, 22];
            numericUpDown10.Value = botMotion[i, 23];
            numericUpDown11.Value = botMotion[i, 24];
            numericUpDown12.Value = botMotion[i, 25];
            numericUpDown13.Value = botMotion[i, 26];
            numericUpDown14.Value = botMotion[i, 27];
            numericUpDown15.Value = botMotion[i, 28];
            numericUpDown16.Value = botMotion[i, 29];
            numericUpDown17.Value = botMotion[i, 30];
          
            singleServoControl(1, 1, Convert.ToByte(numericUpDown1.Value));
            singleServoControl(1, 2, Convert.ToByte(numericUpDown2.Value));
            singleServoControl(1, 3, Convert.ToByte(numericUpDown3.Value));
            singleServoControl(1, 4, Convert.ToByte(numericUpDown4.Value));
            singleServoControl(1, 5, Convert.ToByte(numericUpDown5.Value));
            singleServoControl(1, 6, Convert.ToByte(numericUpDown6.Value));
            singleServoControl(1, 7, Convert.ToByte(numericUpDown7.Value));
            singleServoControl(1, 8, Convert.ToByte(numericUpDown8.Value));

            singleServoControl(0, 1, Convert.ToByte(numericUpDown9.Value));
            singleServoControl(0, 2, Convert.ToByte(numericUpDown10.Value));
            singleServoControl(0, 3, Convert.ToByte(numericUpDown11.Value));
            singleServoControl(0, 4, Convert.ToByte(numericUpDown12.Value));
            singleServoControl(0, 5, Convert.ToByte(numericUpDown13.Value));
            singleServoControl(0, 6, Convert.ToByte(numericUpDown14.Value));
            singleServoControl(0, 7, Convert.ToByte(numericUpDown15.Value));
            singleServoControl(0, 8, Convert.ToByte(numericUpDown16.Value));
            singleServoControl(0, 9, Convert.ToByte(numericUpDown17.Value));

            textBox4.Text = Convert.ToString(botMotion[i, 33]);
        }

        private void showDebug()
        {
            textBox5.Clear();

            string debug = "";

            for (int i = 0; i < 255; i++)
            {
                if (botMotion[i, 0] != 255)
                {
                    break;
                }
                for (int j = 0; j < 34; j++)
                {
                    if(j == 33)
                    {
                        debug += botMotion[i, j];
                    }
                    else
                    {
                        debug += botMotion[i, j] + ",";
                    }
                }
                debug += "\r\n";
            }

            textBox5.Text = debug; 
        }

        private void SaveFile1()
        {
            string debug = "";
            Stream myStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog.OpenFile()) != null)
                {
                    //code to write the stream goes here
                    StreamWriter saveFile = new StreamWriter(myStream);
                    
                    for (int i = 0; i < 255; i++)
                    {
                        if (botMotion[i, 0] != 255)
                        {
                            break;
                        }
                        for (int j = 0; j < 34; j++)
                        {
                            if (j == 33)
                            {
                                debug += botMotion[i, j];
                            }
                            else
                            {
                                debug += botMotion[i, j] + ",";
                            }

                        }
                        debug += "\r\n";
                    }

                    saveFile.WriteLine(debug);
                    
                    saveFile.Close();
                    myStream.Close();
                }
            }
        }

        private void ReadFile1()
        {
            Stream myStream;
            System.IO.StreamReader readFile;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            string line = "";
            char[] charSeparators = new char[] { ',' };
            int cnt = 0;
            int tmp = 0;
            botMotion.Initialize();

            for (int i = 0; i < 255; i++)
            {
                botMotion[i, 0] = 0;
            }

            openFileDialog.InitialDirectory = @"c:w";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files(*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = openFileDialog.OpenFile()) != null)
                {
                    readFile = new System.IO.StreamReader(myStream);

                    //Insert code to read the stream here
                    while ((line = readFile.ReadLine()) != null)
                    {

                        string[] split = line.Split(new char[] { ',' });

                        foreach (string s in split)
                        {
                            if (s.Trim() != "")
                            {
                                botMotion[cnt, tmp] = Convert.ToByte(s);
                            }
                            tmp++;
                        }
                        tmp = 0;
                        cnt++;
                    }
                    int i = cnt - 2;

                    textBox6.Text = Convert.ToString(i);

                    showDebug();

                    readFile.Close();
                    myStream.Close();
                }
            }
        }
        
        static int curMotion = 0;

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            port.PortName = comboBox1.SelectedItem.ToString();
            port.BaudRate = Convert.ToInt32(textBox1.Text);
            port.Parity = Parity.None;
            port.DataBits = Convert.ToInt32(textBox2.Text);
            port.StopBits = StopBits.One;

            try
            {
                port.Open();
                textBox3.Text = port.PortName + " Port open is success";
            }
            catch
            {
                textBox3.Text = port.PortName + " Port open is fail";
            }
        }


        #region left body single control event
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 1, Convert.ToByte(numericUpDown1.Value));
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 2, Convert.ToByte(numericUpDown2.Value));
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 3, Convert.ToByte(numericUpDown3.Value));
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 4, Convert.ToByte(numericUpDown4.Value));
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 5, Convert.ToByte(numericUpDown5.Value));
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 6, Convert.ToByte(numericUpDown6.Value));
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 7, Convert.ToByte(numericUpDown7.Value));
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(1, 8, Convert.ToByte(numericUpDown8.Value));
        }
        #endregion

        #region right body and head single control event
        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 1, Convert.ToByte(numericUpDown9.Value));
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 2, Convert.ToByte(numericUpDown10.Value));
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 3, Convert.ToByte(numericUpDown11.Value));
        }

        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 4, Convert.ToByte(numericUpDown12.Value));
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 5, Convert.ToByte(numericUpDown13.Value));
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 6, Convert.ToByte(numericUpDown14.Value));
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 7, Convert.ToByte(numericUpDown15.Value));
        }

        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 8, Convert.ToByte(numericUpDown16.Value));
        }

        //head control
        private void numericUpDown17_ValueChanged(object sender, EventArgs e)
        {
            singleServoControl(0, 9, Convert.ToByte(numericUpDown17.Value));
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            port.Close();
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveMotion(Convert.ToInt32(numericUpDown18.Value));

            if (Convert.ToInt32(numericUpDown18.Value) > Convert.ToInt32(textBox6.Text))
            {
                curMotion = Convert.ToInt32(textBox6.Text) + 1;
                textBox6.Text = Convert.ToString(curMotion);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            excuteMotion();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            showDebug();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            initializePosition();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SaveFile1();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ReadFile1();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MotionLoad();
        }

        #region Xbox360 Controller
        //To keep track of the current and previous state of the gamepad
        /// <summary>
        /// The current state of the controller
        /// </summary>
        GamePadState gamePadState;
        /// <summary>
        /// The previous state of the controller
        /// </summary>
        GamePadState previousState;

        /// <summary>
        /// Keeps track of the current controller
        /// </summary>
        PlayerIndex playerIndex = PlayerIndex.One;

        /// <summary>
        /// Counter for limiting the time for which the vibration motors are on.
        /// </summary>
        int vibrationCountdown = 0;

                /// <summary>
        /// When a new controller is selected from the drop down
        /// update the player index and turn off all the vibration motors. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ddlController_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.ddlController.SelectedIndex)
            {
                case 0: playerIndex = PlayerIndex.One; break;
                case 1: playerIndex = PlayerIndex.Two; break;
                case 2: playerIndex = PlayerIndex.Three; break;
                case 3: playerIndex = PlayerIndex.Four; break;
                default: playerIndex = PlayerIndex.One; break;
            }
            this.StopAllVibration();

        }

        private void StopAllVibration()
        {
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
            GamePad.SetVibration(PlayerIndex.Two, 0.0f, 0.0f);
            GamePad.SetVibration(PlayerIndex.Three, 0.0f, 0.0f);
            GamePad.SetVibration(PlayerIndex.Four, 0.0f, 0.0f);

        }

        private void CheckVibrationTimeout()
        {
            if (vibrationCountdown > 0)
            {
                --vibrationCountdown;
                if (vibrationCountdown == 0.0f)
                {
                    GamePad.SetVibration(playerIndex, 0.0f, 0.0f);
                }
            }

        }

        private void UpdateControllerState()
        {
            //Get the new gamepad state and save the old state.
            this.previousState = this.gamePadState;
            this.gamePadState = GamePad.GetState(this.playerIndex);
            //If the controller is not connected, let the user know
            this.lblNotConnected.Visible = !this.gamePadState.IsConnected;
            //I personally prefer to only update the buttons if their state has been changed. 
            if (!this.gamePadState.Buttons.Equals(this.previousState.Buttons))
            {
                this.buttonA.Checked = (this.gamePadState.Buttons.A == Input.ButtonState.Pressed);
                this.buttonB.Checked = (this.gamePadState.Buttons.B == Input.ButtonState.Pressed);
                this.buttonX.Checked = (this.gamePadState.Buttons.X == Input.ButtonState.Pressed);
                this.buttonY.Checked = (this.gamePadState.Buttons.Y == Input.ButtonState.Pressed);
                this.buttonLeftShoulder.Checked = (this.gamePadState.Buttons.LeftShoulder == Input.ButtonState.Pressed);
                this.buttonRightShoulder.Checked = (this.gamePadState.Buttons.RightShoulder == Input.ButtonState.Pressed);
                this.buttonStart.Checked = (this.gamePadState.Buttons.Start == Input.ButtonState.Pressed);
                this.buttonBack.Checked = (this.gamePadState.Buttons.Back == Input.ButtonState.Pressed);
                this.buttonLeftStick.Checked = (this.gamePadState.Buttons.LeftStick == Input.ButtonState.Pressed);
                this.buttonRightStick.Checked = (this.gamePadState.Buttons.RightStick == Input.ButtonState.Pressed);
            }
            if (!this.gamePadState.DPad.Equals(this.previousState.DPad))
            {
                this.buttonUp.Checked = (this.gamePadState.DPad.Up == Input.ButtonState.Pressed);
                this.buttonDown.Checked = (this.gamePadState.DPad.Down == Input.ButtonState.Pressed);
                this.buttonLeft.Checked = (this.gamePadState.DPad.Left == Input.ButtonState.Pressed);
                this.buttonRight.Checked = (this.gamePadState.DPad.Right == Input.ButtonState.Pressed);
            }

            //Update the position of the thumb sticks
            //since the thumbsticks can return a number between -1.0 and +1.0 I had to shift (add 1.0)
            //and scale (mutiplication by 100/2, or 50) to get the numbers to be in the range of 0-100
            //for the progress bar
            this.x1Position.Value = (int)((this.gamePadState.ThumbSticks.Left.X + 1.0f) * 100.0f / 2.0f);
            this.y1Position.Value = (int)((this.gamePadState.ThumbSticks.Left.Y + 1.0f) * 100.0f / 2.0f);
            this.x2position.Value = (int)((this.gamePadState.ThumbSticks.Right.X + 1.0f) * 100.0f / 2.0f);
            this.y2position.Value = (int)((this.gamePadState.ThumbSticks.Right.Y + 1.0f) * 100.0f / 2.0f);

            //The triggers return a value between 0.0 and 1.0.  I only needed to scale these values for
            //the progress bar
            this.leftTriggerPosition.Value = (int)((this.gamePadState.Triggers.Left * 100));
            this.rightTriggerPosition.Value = (int)(this.gamePadState.Triggers.Right * 100);

            //R1- control
            if (this.gamePadState.Buttons.RightShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.A == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown9.Value < 255))
                {
                    numericUpDown9.Value++;
                    singleServoControl(0, 1, Convert.ToByte(numericUpDown9.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown9.Value > 0))
                {
                    numericUpDown9.Value--;
                    singleServoControl(0, 1, Convert.ToByte(numericUpDown9.Value));
                }
            }

            //R2- control
            if (this.gamePadState.Buttons.RightShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.B == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown10.Value < 255))
                {
                    numericUpDown10.Value++;
                    singleServoControl(0, 2, Convert.ToByte(numericUpDown10.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown10.Value > 0))
                {
                    numericUpDown10.Value--;
                    singleServoControl(0, 2, Convert.ToByte(numericUpDown10.Value));
                }
            }

            //R3- control
            if (this.gamePadState.Buttons.RightShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.X == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown11.Value < 255))
                {
                    numericUpDown11.Value++;
                    singleServoControl(0, 3, Convert.ToByte(numericUpDown11.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown11.Value > 0))
                {
                    numericUpDown11.Value--;
                    singleServoControl(0, 3, Convert.ToByte(numericUpDown11.Value));
                }
            }

            //R4- control
            if (this.gamePadState.Buttons.RightShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.Y == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown12.Value < 255))
                {
                    numericUpDown12.Value++;
                    singleServoControl(0, 4, Convert.ToByte(numericUpDown12.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown12.Value > 0))
                {
                    numericUpDown12.Value--;
                    singleServoControl(0, 4, Convert.ToByte(numericUpDown12.Value));
                }
            }

            //R5- control
            if (this.rightTriggerPosition.Value > 10
                && this.gamePadState.Buttons.A == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown13.Value < 255))
                {
                    numericUpDown13.Value++;
                    singleServoControl(0, 5, Convert.ToByte(numericUpDown13.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown13.Value > 0))
                {
                    numericUpDown13.Value--;
                    singleServoControl(0, 5, Convert.ToByte(numericUpDown13.Value));
                }
            }

            //R6- control
            if (this.rightTriggerPosition.Value > 10
                && this.gamePadState.Buttons.B == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown14.Value < 255))
                {
                    numericUpDown14.Value++;
                    singleServoControl(0, 6, Convert.ToByte(numericUpDown14.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown14.Value > 0))
                {
                    numericUpDown14.Value--;
                    singleServoControl(0, 6, Convert.ToByte(numericUpDown14.Value));
                }
            }

            //R7- control
            if (this.rightTriggerPosition.Value > 10
                && this.gamePadState.Buttons.X == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown15.Value < 255))
                {
                    numericUpDown15.Value++;
                    singleServoControl(0, 7, Convert.ToByte(numericUpDown15.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown15.Value > 0))
                {
                    numericUpDown15.Value--;
                    singleServoControl(0, 7, Convert.ToByte(numericUpDown15.Value));
                }
            }

            //R8- control
            if (this.rightTriggerPosition.Value > 10
                && this.gamePadState.Buttons.Y == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown16.Value < 255))
                {
                    numericUpDown16.Value++;
                    singleServoControl(0, 8, Convert.ToByte(numericUpDown16.Value));
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown16.Value > 0))
                {
                    numericUpDown16.Value--;
                    singleServoControl(0, 8, Convert.ToByte(numericUpDown16.Value));
                }
            }

            //L1- control
            if (this.gamePadState.Buttons.LeftShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.A == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown1.Value < 255))
                {
                    numericUpDown1.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown1.Value > 0))
                {
                    numericUpDown1.Value--;
                }

                singleServoControl(1, 1, Convert.ToByte(numericUpDown1.Value));
            }

            //L2- control
            if (this.gamePadState.Buttons.LeftShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.B == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown2.Value < 255))
                {
                    numericUpDown2.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown2.Value > 0))
                {
                    numericUpDown2.Value--;
                }

                singleServoControl(1, 2, Convert.ToByte(numericUpDown2.Value));
            }

            //L3- control
            if (this.gamePadState.Buttons.LeftShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.X == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown3.Value < 255))
                {
                    numericUpDown3.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown3.Value > 0))
                {
                    numericUpDown3.Value--;
                }

                singleServoControl(1, 3, Convert.ToByte(numericUpDown3.Value));
            }

            //L4- control
            if (this.gamePadState.Buttons.LeftShoulder == Input.ButtonState.Pressed
                && this.gamePadState.Buttons.Y == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown4.Value < 255))
                {
                    numericUpDown4.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown4.Value > 0))
                {
                    numericUpDown4.Value--;
                }

                singleServoControl(1, 4, Convert.ToByte(numericUpDown4.Value));
            }

            //L5- control
            if (this.leftTriggerPosition.Value > 10
                && this.gamePadState.Buttons.A == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown5.Value < 255))
                {
                    numericUpDown5.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown5.Value > 0))
                {
                    numericUpDown5.Value--;
                }

                singleServoControl(1, 5, Convert.ToByte(numericUpDown5.Value));
            }

            //L6- control
            if (this.leftTriggerPosition.Value > 10
                && this.gamePadState.Buttons.B == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown6.Value < 255))
                {
                    numericUpDown6.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown6.Value > 0))
                {
                    numericUpDown6.Value--;
                }

                singleServoControl(1, 6, Convert.ToByte(numericUpDown6.Value));
            }

            //L7- control
            if (this.leftTriggerPosition.Value > 10
                && this.gamePadState.Buttons.X == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown7.Value < 255))
                {
                    numericUpDown7.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown7.Value > 0))
                {
                    numericUpDown7.Value--;
                }

                singleServoControl(1, 7, Convert.ToByte(numericUpDown7.Value));
            }

            //L8- control
            if (this.leftTriggerPosition.Value > 10
                && this.gamePadState.Buttons.Y == Input.ButtonState.Pressed)
            {
                if ((this.gamePadState.DPad.Up == Input.ButtonState.Pressed) && (numericUpDown8.Value < 255))
                {
                    numericUpDown8.Value++;
                }

                if ((this.gamePadState.DPad.Down == Input.ButtonState.Pressed) && (numericUpDown8.Value > 0))
                {
                    numericUpDown8.Value--;
                }

                singleServoControl(1, 8, Convert.ToByte(numericUpDown8.Value));
            }

            //kick action
            if ((this.gamePadState.Buttons.Back == Input.ButtonState.Pressed) &&
                (this.gamePadState.Buttons.A == Input.ButtonState.Pressed))
            {
                readExcute("kick2.txt");
                excuteMotion();
            }

            //Dance and Push Up
            if ((this.gamePadState.Buttons.Back == Input.ButtonState.Pressed) &&
                (this.gamePadState.Buttons.B == Input.ButtonState.Pressed))
            {
                readExcute("MyFile.txt");
                excuteMotion();
            }

            //test1
            if ((this.gamePadState.Buttons.Back == Input.ButtonState.Pressed) &&
                (this.gamePadState.Buttons.X == Input.ButtonState.Pressed))
            {
                readExcute("bye2.txt");
                excuteMotion();
            }

            //intitialization position
            if (this.gamePadState.Buttons.Start == Input.ButtonState.Pressed)
            {
                initializePosition();
            }

            //move to haed
            if (this.x1Position.Value != 50)
            {
                if ( (this.x1Position.Value > 50) && (numericUpDown17.Value<255) )
                {
                    numericUpDown17.Value++;
                }
                else if ( (this.x1Position.Value < 50) && (numericUpDown17.Value>0) )
                {
                    numericUpDown17.Value--;
                }

                singleServoControl(0, 9, Convert.ToByte(numericUpDown17.Value));
            }
        }

        //I'm updating the controller display on a timed interval. 

        private void button10_Click(object sender, EventArgs e)
        {
            GamePad.SetVibration(playerIndex, (float)this.leftMotor.Value, (float)this.rightMotor.Value);
            vibrationCountdown = 30;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopAllVibration();
        }

        private void controllerTimer_Tick(object sender, EventArgs e)
        {
            this.CheckVibrationTimeout();
            this.UpdateControllerState();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ddlController.SelectedIndex = 0;
            this.controllerTimer.Start();
        }
        #endregion

        private void button11_Click(object sender, EventArgs e)
        {
            //append all the choice we want for commands.
            //we want to be able to one, two, three..it's only test..           
            grammarBuilder.Append(new Choices("hello", "ready", "action", "what is your name", "good bye"));


            //create the Grammar from the grammarBuilder
            Grammar customGrammar = new Grammar(grammarBuilder);

            //unload any grammars from the recognition engine
            sre.UnloadAllGrammars();

            //load out new grammar
            sre.LoadGrammar(customGrammar);

            //set our recognition engine to use the default audio device
            sre.SetInputToDefaultAudioDevice();

            Thread t1 = new Thread(delegate()
            {
                //sre.SetInputToDefaultAudioDevice();
                sre.RecognizeAsync(RecognizeMode.Multiple);
            });

            t1.Start();

            //add an event handler so we get events whenever the engine recognizes spoken commands
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);

            //set the recognition engine to keep running after recognition a command.
            //if we had used RecognizeMode.single, the engine would quite listening after
            //the first recognized command.

            //sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //simple check to see what the result of the recognition was
            switch (e.Result.Text)
            {
                //check for the count
                //we have to make sure we aren't in checking mode first.

                case "hello":
                    textBox7.Text = "Hello";
                    textBox8.Text = "welcome to robot stage";
                    synthesizer.SpeakAsync("welcome to robot stage");
                    break;

                case "action":
                    textBox7.Text = "Action";
                    textBox8.Text = "Yes Sir!! Here we go!!";
                    synthesizer.SpeakAsync("yes sir   Here we go");
                    excuteMotion();
                    break;

                case "what is your name":
                    textBox7.Text = "Your name";
                    textBox8.Text = "My name is Tom";
                    synthesizer.SpeakAsync("my name is tom");
                    break;

                case "good bye":
                    textBox7.Text = "Good Bye";
                    textBox8.Text = "Ok! Bye Bye";
                    synthesizer.SpeakAsync("ok bye bye");
                    readExcute("bye2.txt");
                    excuteMotion();
                    initializePosition();
                    break;

                case "ready":
                    textBox7.Text = "Ready";
                    textBox8.Text = "I am ready please be select the action";
                    synthesizer.SpeakAsync("I am ready please be select the action");
                    ReadFile1();
                    break;

            }
        }//end function

        private void numericUpDown19_ValueChanged(object sender, EventArgs e)
        {
            synthesizer.Rate = Convert.ToInt32(numericUpDown19.Value);
        }

        private void numericUpDown20_ValueChanged(object sender, EventArgs e)
        {
            synthesizer.Volume = Convert.ToInt32(numericUpDown20.Value);
        }

        private void numericUpDown21_ValueChanged(object sender, EventArgs e)
        {
            
        }//end function

        private void readExcute(string filename)
        {
            FileInfo file = new FileInfo(filename);
            FileStream stream = file.Open(FileMode.Open);
            StreamReader readFile = new StreamReader(stream);

            string line = "";
            char[] charSeparators = new char[] { ',' };
            int cnt = 0;
            int tmp = 0;
            botMotion.Initialize();
            textBox6.Clear();

            for (int j = 0; j < 255; j++)
            {
                botMotion[j, 0] = 0;
            }

            //Insert code to read the stream here
            while ((line = readFile.ReadLine()) != null)
            {

                string[] split = line.Split(new char[] { ',' });

                foreach (string s in split)
                {
                    if (s.Trim() != "")
                    {
                        botMotion[cnt, tmp] = Convert.ToByte(s);
                    }
                    tmp++;
                }
                tmp = 0;
                cnt++;
            }
            int i = cnt - 2;

            textBox6.Text = Convert.ToString(i);

            showDebug();

            readFile.Close();
            stream.Close();
        }
    }
}
