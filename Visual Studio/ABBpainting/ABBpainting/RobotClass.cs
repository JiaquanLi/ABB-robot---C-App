using System;
using System.Windows.Forms;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.IOSystemDomain;
using System.Collections.Generic;
using System.Diagnostics;

namespace ABBpainting
{
    class RobotClass
    {
        private Controller controller = null;

        private RapidData rd_targets = null;
        private ArrayData targets = null;

        private RapidData rd_repeat = null;
        private Bool repeatBool;

        private RapidData rd_start = null;
        private const int SHUT_DOWN = -1;
        private const int TO_WAIT = 0;
        private const int TO_PAINT = 1;
        private const int TO_INTRODUCE = 2;
        private Num processFlag;

        private RapidData rd_targetsNum;
        private int maxLength;
        private int targetsNum;
        private Num rd_tgNumValue;

        private RapidData rd_speedLevel;
        private Num speedLevel;

        public RobotClass(Controller controller)
        {
            this.controller = controller;
            this.controller.Logon(UserInfo.DefaultUser);
            InitDataStream();
        }

        public void InitDataStream()
        {
            Task tRob1 = controller.Rapid.GetTask("T_ROB1");
            if (tRob1 != null)
            {
                rd_repeat = tRob1.GetRapidData("Module1", "repeat");
                if (rd_repeat.Value is Bool)
                {
                    repeatBool = (Bool)rd_repeat.Value;
                }

                rd_start = tRob1.GetRapidData("Module1", "flag");
                if (rd_start.Value is Num)
                {
                    processFlag = (Num)rd_start.Value;
                }

                rd_targetsNum = tRob1.GetRapidData("Module1", "targetsNum");
                if (rd_targetsNum.Value is Num)
                {
                    processFlag = (Num)rd_start.Value;
                }

                rd_speedLevel = tRob1.GetRapidData("Module1", "speedLevel");
                if (rd_speedLevel.Value is Num)
                {
                    speedLevel = (Num)rd_speedLevel.Value;
                }

                rd_targets = tRob1.GetRapidData("Module1", "tgPos");
                if (rd_targets.IsArray)
                {
                    targets = (ArrayData)rd_targets.Value;
                    int aRank = targets.Rank;
                    maxLength = targets.GetLength(aRank-1);
                    ArrayModes am = targets.Mode;
                    Debug.WriteLine("ARRAY MODE! ::: "+am.ToString());
                    targets.Mode = ArrayModes.Dynamic;
                }
                else MessageBox.Show("'targets' data is not array!");
                if (rd_targets == null) MessageBox.Show("'targets' data does not exist!");
            }
        }

        public void StartRapidProgram()
        {
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    using (Mastership m = Mastership.Request(controller.Rapid))
                    {
                        //Perform operation
                        Debug.WriteLine("Exec status of the controller ::: " + controller.Rapid.ExecutionStatus);
                        Debug.WriteLine("Controller State ::: " + controller.State);
                        controller.Rapid.Start(true);
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }

        public void StopRapidProgram()
        {
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    using (Mastership m = Mastership.Request(controller.Rapid))
                    {
                        controller.Rapid.Stop();
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }

        public void MoveAlong(List<BrushStatus> nativTargets, ProgressBar pb)
        {
            SetPath(nativTargets, pb);
            StartProcess();
        }

        public void StopProcess()
        {
            //repeatBool.FillFromString2("FALSE");
            processFlag.FillFromString2(SHUT_DOWN.ToString());
            using (Mastership m = Mastership.Request(controller.Rapid))
            {
                //rd_repeat.Value = repeatBool;
                rd_start.Value = processFlag;
            }
        }

        public void StartProcess()
        {
            processFlag.FillFromString2(TO_PAINT.ToString());
            using (Mastership m = Mastership.Request(controller.Rapid))
            {
                rd_start.Value = processFlag;
            }
        }

        public void SetDrawingSpeed(int level)
        {
            speedLevel.FillFromString2(level.ToString());
            using (Mastership m = Mastership.Request(controller.Rapid))
            {
                rd_speedLevel.Value = speedLevel;
            }
        }

        public void SetPath(List<BrushStatus> nativTargets, ProgressBar pb)
        {
            if (nativTargets.Count > maxLength) targetsNum = maxLength;
            else targetsNum = nativTargets.Count;

            rd_tgNumValue.FillFromString2(targetsNum.ToString());
            using (Mastership m = Mastership.Request(controller.Rapid))
            {
                rd_targetsNum.Value = rd_tgNumValue;
            }

            Pos rt;
            for (int i = 0; i < targetsNum; i++)
            {
                rt = new Pos();
                rt.FillFromString2("[" + nativTargets[i].X + "," + nativTargets[i].Y + "," + nativTargets[i].Z + "]");
                Debug.WriteLine(rt.ToString());
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    rd_targets.WriteItem(rt, i);
                }
                pb.Increment(1);
            }
            pb.Value = 0;
        }

        public void Dispose()
        {
            if (controller.Rapid.ExecutionStatus == ExecutionStatus.Running)
            {
                StopProcess();
            }
            this.controller.Logoff();
            this.controller.Dispose();
            this.controller = null;
        }

        public Controller Controller
        {
            get
            {
                return controller;
            }
        }

    }
}
