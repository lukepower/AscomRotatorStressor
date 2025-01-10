using System;
using System.Windows.Forms;

namespace ASCOM.AscomRotatorTorture
{
    public partial class Form1 : Form
    {

        private ASCOM.DriverAccess.Rotator driver;
        private float start_position;
        private float step_size;
        private int count_ticks;

        private int total_calls;

        private bool waiting_for_move;

        public Form1()
        {
            InitializeComponent();
            SetUIState();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DriverId = ASCOM.DriverAccess.Rotator.Choose(Properties.Settings.Default.DriverId);
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
            }
            else
            {
                driver = new ASCOM.DriverAccess.Rotator(Properties.Settings.Default.DriverId);
                driver.Connected = true;
                this.textBox2.Text = driver.StepSize.ToString();
            }
            SetUIState();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";
        }

        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.IsConnected)
            {
                // Start stress test
                if (this.timer1.Enabled == false)
                {
                    this.listBox1.Items.Clear();
                    this.waiting_for_move = false;
                    this.total_calls = 0;
                    
                    this.listBox1.Items.Add(this.driver.Name);

                    this.start_position = this.driver.Position;
                    this.step_size = this.driver.StepSize;
                    this.timer1.Interval = Int32.Parse(this.textBox1.Text);

                    this.listBox1.Items.Add("Started at " + DateTime.Now.ToShortTimeString() + " with Interval " + this.timer1.Interval.ToString());
                    this.timer1.Enabled = true;
                    this.button1.Text = "Disable Stresstest";
                }
                else
                {
                    this.timer1.Enabled = false;
                    this.button1.Text = "Enable Stresstest";
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.IsConnected)
            {
                count_ticks++;
                try
                {


                    var tmp = this.driver.IsMoving;
                    var tmp2 = this.driver.CanReverse;
                    var tmp3 = this.driver.Position;
                    var tmp4 = this.driver.StepSize;


                    if (count_ticks >= 10)
                    {
                        if (this.checkBox1.Checked && this.driver.IsMoving)
                        {
                            if (this.waiting_for_move == false)
                            {


                                this.listBox1.Items.Add("Still moving, will not send a new move command");
                            }

                            this.waiting_for_move = true;
                        }
                        else
                        {
                            this.waiting_for_move = false;

                            float new_position;
                            // Move position
                            if (this.driver.Position > this.start_position)
                            {
                                new_position = this.start_position -2;
                            }
                            else
                            {
                                new_position = this.start_position + 2;
                            }
                            if (new_position > 360)
                            {
                                new_position = 360;
                            } else if (new_position < 0)
                            {
                                new_position = 0;
                            }
                            // ToDo: Change StepSize

                            this.driver.MoveAbsolute(new_position);
                            count_ticks = 0;
                            this.listBox1.Items.Add(DateTime.Now.ToLongTimeString() + ": Position: " + this.driver.Position.ToString() + ", IsMoving: " + tmp.ToString() + ", CanReverse: " + tmp2.ToString() + ", StepSize: " + tmp3.ToString() + " ( done 10 calls in between)");
                        }
                    }
                    this.total_calls++;
                }
                catch (Exception ex)
                {
                    this.timer1.Enabled = false;
                    this.listBox1.Items.Add(ex.Message + " (total calls " + this.total_calls.ToString() + ")");
                }

                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;

                // Move position up a little
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.driver.Connected)
            {
                float new_stepsize = float.Parse(this.textBox2.Text);
                this.driver. = new_stepsize;
            }
        }
    }
}
