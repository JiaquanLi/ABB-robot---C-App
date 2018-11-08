using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABBpainting
{
    public partial class Form1 : Form
    {
        private bool paint = false;
        private SolidBrush color;
        private List<BrushStatus> posList;

        private NetworkScanner scanner = null;
        private RobotClass myRobot = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnClearClick(object sender, EventArgs e)
        {
            Graphics g1 = panel1.CreateGraphics();
            g1.Clear(panel1.BackColor);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (myRobot != null)
            {
                paint = true;
                posList = new List<BrushStatus>();
                posList.Add(new BrushStatus(e.X, e.Y, 0));
                richTextBox1.Text = "";
                if(myRobot.Controller.Rapid.ExecutionStatus == ABB.Robotics.Controllers.RapidDomain.ExecutionStatus.Stopped)
                {
                    myRobot.StartRapidProgram();
                }
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (myRobot != null)
            {
                paint = false;
                posList.Add(new BrushStatus(e.X, e.Y, -10));
                var index = 0;
                foreach (var item in posList)
                {
                    richTextBox1.Text += (index++) + ".     " + item.ToString();
                }
                progressBar1.Maximum = posList.Count;
                myRobot.MoveAlong(posList, progressBar1);
                posList.Clear();
                posList = null;
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                color = new SolidBrush(Color.Black);
                Graphics g = panel1.CreateGraphics();
                g.FillEllipse(color, e.X, e.Y, 10, 10);
                posList.Add(new BrushStatus(e.X, e.Y, 0));
                g.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.scanner = new NetworkScanner();
            this.scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;
            foreach (ControllerInfo info in controllers)
            {
                comboBox1.Items.Add(info.ControllerName + " / " + info.IPAddress.ToString());
            }
        }

        private void connect_Click(object sender, EventArgs e)
        {
            ControllerInfoCollection controllers = scanner.Controllers;
            foreach (ControllerInfo info in controllers)
            {
                if(comboBox1.Text.Equals(info.ControllerName + " / " + info.IPAddress.ToString()))
                {
                    if (info.Availability == Availability.Available)
                    {
                        if (myRobot != null)
                        {
                            myRobot.Dispose(); // = LogOff
                            myRobot = null;
                        }
                        myRobot = new RobotClass(ControllerFactory.CreateFrom(info));
                        myRobot.StartRapidProgram();
                        myRobot.Controller.ConnectionChanged += new EventHandler<ConnectionChangedEventArgs>(ConnectionChanged);
                        connect_btn.BackColor = Color.Green;
                        break;
                    }
                }
                {
                    MessageBox.Show("Selected controller not available.");
                }
            }
            if(myRobot == null) MessageBox.Show("Selected controller not available. (comboBox String != controller info)");
        }

        public void ConnectionChanged(object sender, EventArgs e)
        {
            if(myRobot.Controller.Connected == true) connect_btn.BackColor = Color.Green;
            else connect_btn.BackColor = Color.Red;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.Dispose(); // = LogOff
                myRobot = null;
            }
        }

        private void shutDown_click(object sender, EventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.StopProcess();
            }
            //myRobot.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.StartProcess();
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.SetDrawingSpeed(trackBar1.Value);
            }
        }
    }
}
