using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace 出租车轨迹数据计算
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        public string path;
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择你要打开的文件";
            ofd.Filter = "txt|*.txt|All|*.*";
            ofd.ShowDialog();

            path = ofd.FileName;

            if (path == "")
            {
                return;
            }

            using (FileStream FsRead = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = FsRead.Read(buffer, 0, buffer.Length);
                txtSource.Text = Encoding.UTF8.GetString(buffer, 0, r);


            }
        }

        private void 导出结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "请选择保存路径";
            sfd.Filter = "tx|*.txt|All|*.*";
            sfd.ShowDialog();

            string outcome = sfd.FileName;
            if (outcome == "")
            {
                return;
            }
            using (FileStream FsWrite = new FileStream(outcome, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(txtResult.Text);
                FsWrite.Write(buffer, 0, buffer.Length);

            }
                
        }


        List<string> marklist = new List<string>();//创建车辆标识数组
        List<string> Statelist = new List<string>();//创建运行状态数组
        List<string> Timelist = new List<string>();//创建时间数组
        List<string> XCoordinate = new List<string>();//创建X坐标数组
        List<string> YCoordinate = new List<string>();//创建Y坐标数组

        private void 计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                #region 计算过程
                string Paragraph = null;
                StreamReader sr = new StreamReader(path, Encoding.UTF8);
                int Str_length = 0;
                while ((Paragraph = sr.ReadLine()) != null)
                {
                    string[] strCollection = Paragraph.Split(',');
                    marklist.Add(strCollection[0]);
                    Statelist.Add(strCollection[1]);
                    Timelist.Add(strCollection[2]);
                    XCoordinate.Add(strCollection[3]);
                    YCoordinate.Add(strCollection[4]);
                    Str_length++;
                }

                //创建数组
                List<double> Year = new List<double>();
                List<double> month = new List<double>();
                List<double> Day = new List<double>();
                List<double> hour = new List<double>();
                List<double> minute = new List<double>();
                List<double> second = new List<double>();
                List<double> X_list = new List<double>();
                List<double> Y_list = new List<double>();

                List<string> MJD = new List<string>(); //mjd
                List<string> SPEED = new List<string>(); //mjd
                List<string> AZIMUTH = new List<string>();//

                int index = 0;
                for (int i = 0; i < Str_length; i++)
                {
                    if (marklist[i] == "T2")
                    {
                        Year.Add(Convert.ToDouble(Timelist[i].Substring(0, 4)));
                        month.Add(Convert.ToDouble(Timelist[i].Substring(4, 2)));
                        Day.Add(Convert.ToDouble(Timelist[i].Substring(6, 2)));
                        hour.Add(Convert.ToDouble(Timelist[i].Substring(8, 2)));
                        minute.Add(Convert.ToDouble(Timelist[i].Substring(10, 2)));
                        second.Add(Convert.ToDouble(Timelist[i].Substring(12, 2)));
                        X_list.Add(Convert.ToDouble(XCoordinate[i]));
                        Y_list.Add(Convert.ToDouble(YCoordinate[i]));
                        index++;

                    }
                }

                for (int j = 0; j < index; j++)//进行MJD计算
                {
                    //txtResult.Text = Year[j] + " " + month[j] + " " + Day[j] + " " + hour[j] + " " + minute[j] + " " + second[j] + " " + txtResult.Text;
                    double mjd = -678987 + 367.0 * Year[j];
                    mjd -= Convert.ToInt32(7.0 / 4.0 * (Year[j] + Convert.ToInt32((month[j] + 9.0) / 12.0)));
                    mjd += Convert.ToInt32((275.0 * month[j]) / 9.0);
                    mjd += Day[j] + (hour[j] - 8) / 24.0 + minute[j] / 1440.0 + second[j] / 86400.0;
                    //MessageBox.Show((second[i] / 86400).ToString());
                    // MessageBox.Show(n.ToString());
                    MJD.Add(mjd.ToString("#0.00000"));
                    //txtResult.Text = txtResult.Text + MJD[j] + "    ";

                }

                double Cumulative_distance = 0.0;
                for (int k = 1; k < index; k++)//计算速度
                {
                    double time1 = (hour[k] + (minute[k] / 60) + (second[k] / 3600));
                    double time2 = (hour[k - 1] + (minute[k - 1] / 60) + (second[k - 1] / 3600));
                    double distance = Math.Sqrt((X_list[k] - X_list[k - 1]) * (X_list[k] - X_list[k - 1]) + (Y_list[k] - Y_list[k - 1]) * (Y_list[k] - Y_list[k - 1]));
                    double speed = distance / (time1 - time2) / 1000;
                    SPEED.Add(speed.ToString("#0.000"));
                    Cumulative_distance += distance;
                    //txtResult.Text = txtResult.Text + SPEED[k - 1] + "    ";
                }



                for (int q = 1; q < index; q++)//计算方位角
                {

                    double dy = (Y_list[q] - Y_list[q - 1]); //定义△Y
                    double dx = (X_list[q] - X_list[q - 1]); //定义△X
                    double azimuth = 0.0;

                    if (dx == 0)
                    {
                        if (dy > 0)
                            azimuth = 0.5 * Math.PI;
                        else
                        {
                            azimuth = 1.5 * Math.PI;
                        }
                    }
                    else
                    {
                        azimuth = Math.Atan2(dy, dx);
                        if (dx < 0)
                        {
                            azimuth += Math.PI;
                        }
                    }

                    if (azimuth < 0)
                    {
                        azimuth += 2 * Math.PI;
                    }
                    if (azimuth > 2 * Math.PI)
                    {
                        azimuth -= 2 * Math.PI;
                    }

                    azimuth = azimuth * 180 / Math.PI;


                    AZIMUTH.Add(azimuth.ToString("#0.000"));
                    // txtResult.Text = txtResult.Text + AZIMUTH[q - 1] + "    ";
                }
                #endregion


                #region 输出过程
                string Start_title = "-------------速度和方位角计算结果---------------" + "\r\n" + "序号——--——时段——————速度——方位角";
                string End_title = "-------------速度和方位角计算结果---------------";

                string out_come = Start_title + "\r\n";
                for (int x = 1; x < index; x++)
                {
                    if (x < 11)
                    {
                        out_come = out_come + "0" + (x - 1) + " ," + MJD[x - 1] + " - " + MJD[x] + " ," + SPEED[x - 1] + " ," + AZIMUTH[x - 1] + "\r\n";
                    }
                    else
                    {
                        out_come = out_come + (x - 1) + " ," + MJD[x - 1] + " - " + MJD[x] + " ," + SPEED[x - 1] + " ," + AZIMUTH[x - 1] + "\r\n";
                    }
                }

                out_come += End_title + "\r\n";
                double linear_distance = Math.Sqrt((X_list[index - 1] - X_list[0]) * (X_list[index - 1] - X_list[0]) + (Y_list[index - 1] - Y_list[0]) * (Y_list[index - 1] - Y_list[0]));
                string distance_title = "累积距离：" + (Cumulative_distance / 1000).ToString("#0.000") + " (km)" + "\r\n" + "首位直线距离：" + (linear_distance / 1000).ToString("#0.000") + " (km)";
                txtResult.Text = out_come + distance_title;
                #endregion
            }
            catch
            {
                MessageBox.Show("操作错误", "提示");
            }

        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("制作者：昆明理工大学_CZY" + "\r\n" + "数据若有错误需要修改" + "\r\n" + "请对导入数据进行修改","帮助");
        }
    }
}

