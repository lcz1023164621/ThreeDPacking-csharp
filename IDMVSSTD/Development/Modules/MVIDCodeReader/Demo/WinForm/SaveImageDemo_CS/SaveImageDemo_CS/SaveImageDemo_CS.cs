using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MVIDCodeReaderNet;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;

namespace SaveImageDemo_CS
{
    public partial class SaveImageDemo_CS : Form
    {
        MVIDCodeReader MyCodeReader = new MVIDCodeReader();     // 设备句柄
        System.DateTime CurrentTime = new System.DateTime();    // 当前时间

        string filepath = "";   // 文件路径

        // ch:获得语言版本 | en:Get language version
        int nLCID = 0;

        public SaveImageDemo_CS()
        {
            InitializeComponent();
            comboBoxImageType.Items.Insert(0, "JPG");
            comboBoxImageType.Items.Insert(1, "BMP");
            comboBoxImageType.SelectedIndex = 0;
            comboBoxConvertType.Items.Insert(0, "MONO8");
            comboBoxConvertType.Items.Insert(1, "BGR24");
            comboBoxConvertType.SelectedIndex = 0;
            textBoxWidth.Text = "0";
            textBoxHeight.Text = "0";

            nLCID = System.Globalization.CultureInfo.CurrentCulture.LCID;
        }

        // 选择raw图片
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;                          //ch:该值确定是否可以选择多个文件 | en:This value determines whether it supports select multiple files

            if (0x0804 == nLCID)
            {
                dialog.Title = "请选择文件夹";
                dialog.Filter = "图片文件(*.raw)|*.raw";
            }
            else
            {
                dialog.Title = "Select folder";
                dialog.Filter = "Picture file(*.raw)|*.raw";
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filepath = dialog.FileName;
                TextFilePath.Text = filepath;
            }
        }

        // 初始化资源
        private void ButtonInit_Click(object sender, EventArgs e)
        {
            int nRet = MyCodeReader.MVID_CR_CreateHandle_NET(0);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CreateHandle failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }
        }

        // 存图
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            string csMessage;
            Int32 nRet = MVIDCodeReader.MVID_CR_OK;

            MVIDCodeReader.MVID_IMAGE_INFO stInputImage = new MVIDCodeReader.MVID_IMAGE_INFO();
            MVIDCodeReader.MVID_IMAGE_INFO stOutputImage = new MVIDCodeReader.MVID_IMAGE_INFO();
            MVIDCodeReader.MVID_IMAGE_TYPE enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_JPEG;

            try
            {
                stInputImage.nWidth = Convert.ToUInt16(textBoxWidth.Text.ToString());
                stInputImage.nHeight = Convert.ToUInt16(textBoxHeight.Text.ToString());

                FileStream fs = new FileStream(filepath, FileMode.Open);
                byte[] Rawfile = new byte[fs.Length];
                fs.Read(Rawfile, 0, Rawfile.Length);
                fs.Close();

                stInputImage.pImageBuf = Marshal.AllocHGlobal(Rawfile.Length);
                Marshal.Copy(Rawfile, 0, stInputImage.pImageBuf, Rawfile.Length);

                switch (comboBoxConvertType.SelectedIndex)
                {
                    case 0:
                        stInputImage.enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_MONO8;
                        stInputImage.nImageLen = (UInt32)stInputImage.nWidth * (UInt32)stInputImage.nHeight;

                        if (stInputImage.nImageLen != Rawfile.Length)
                        {
                            csMessage = "Input Width/Height Error or the Image is not Mono8!";
                            MessageBox.Show(csMessage);

                            stInputImage.nWidth = 0;
                            stInputImage.nHeight = 0;
                            stInputImage.nImageLen = 0;
                            if (IntPtr.Zero != stInputImage.pImageBuf)
                            {
                                Marshal.FreeHGlobal(stInputImage.pImageBuf);
                                stInputImage.pImageBuf = IntPtr.Zero;
                            }
                            return;
                        }
                        break;
                    case 1:
                        stInputImage.enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_BGR24;
                        stInputImage.nImageLen = (UInt32)stInputImage.nWidth * (UInt32)stInputImage.nHeight * 3;

                        if (stInputImage.nImageLen != Rawfile.Length)
                        {
                            csMessage = "Input Width/Height Error or the Image is not BGR24!";
                            MessageBox.Show(csMessage);

                            stInputImage.nWidth = 0;
                            stInputImage.nHeight = 0;
                            stInputImage.nImageLen = 0;
                            if (IntPtr.Zero != stInputImage.pImageBuf)
                            {
                                Marshal.FreeHGlobal(stInputImage.pImageBuf);
                                stInputImage.pImageBuf = IntPtr.Zero;
                            }
                            return;
                        }
                        break;
                    default:
                        if (IntPtr.Zero != stInputImage.pImageBuf)
                        {
                            Marshal.FreeHGlobal(stInputImage.pImageBuf);
                            stInputImage.pImageBuf = IntPtr.Zero;
                        }
                        return;
                }
            }
            catch
            {
                textBoxWidth.Text = "0";
                textBoxHeight.Text = "0";
                csMessage = "Input Image Width or Height Error !";
                MessageBox.Show(csMessage);
                if (IntPtr.Zero != stInputImage.pImageBuf)
                {
                    Marshal.FreeHGlobal(stInputImage.pImageBuf);
                    stInputImage.pImageBuf = IntPtr.Zero;
                }
                return;
            }

            switch (comboBoxImageType.SelectedIndex)
            {
                case 0:
                    enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_JPEG;
                    break;
                case 1:
                    enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_BMP;
                    break;
                default:
                    if (IntPtr.Zero != stInputImage.pImageBuf)
                    {
                        Marshal.FreeHGlobal(stInputImage.pImageBuf);
                        stInputImage.pImageBuf = IntPtr.Zero;
                    }
                    return;
            }

            if (0 == stInputImage.nWidth || 0 == stInputImage.nHeight)
            {
                csMessage = "error image width/height";
                MessageBox.Show(csMessage);
                if (IntPtr.Zero != stInputImage.pImageBuf)
                {
                    Marshal.FreeHGlobal(stInputImage.pImageBuf);
                    stInputImage.pImageBuf = IntPtr.Zero;
                }
                return;
            }

            CurrentTime = System.DateTime.Now;

            nRet = MyCodeReader.MVID_CR_SaveImage_NET(ref stInputImage, enImageType, ref stOutputImage, 80);
            if (MVIDCodeReader.MVID_CR_OK == nRet)
            {
                byte[] buffer = new byte[stOutputImage.nImageLen];
                Marshal.Copy(stOutputImage.pImageBuf, buffer, 0, (int)stOutputImage.nImageLen);

                String FilePath = null;

                if (MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_JPEG == enImageType)
                {
                    FilePath = CurrentTime.ToString("yyyyMMddHHmmss") + ".jpg";
                    FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
                    file.Write(buffer, 0, (int)(stOutputImage.nImageLen));
                    file.Close();
                }
                else
                {
                    FilePath = CurrentTime.ToString("yyyyMMddHHmmss") + ".bmp";
                    FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
                    file.Write(buffer, 0, (int)(stOutputImage.nImageLen));
                    file.Close();
                }

                csMessage = "success save image";
                MessageBox.Show(csMessage);
                if (IntPtr.Zero != stInputImage.pImageBuf)
                {
                    Marshal.FreeHGlobal(stInputImage.pImageBuf);
                    stInputImage.pImageBuf = IntPtr.Zero;
                }
                return;
            }
            else
            {
                csMessage = "fail save image " + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                if (IntPtr.Zero != stInputImage.pImageBuf)
                {
                    Marshal.FreeHGlobal(stInputImage.pImageBuf);
                    stInputImage.pImageBuf = IntPtr.Zero;
                }
                return;
            }
        }

        private void SaveImageDemo_CS_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyCodeReader.MVID_CR_DestroyHandle_NET();
        }
    }
}
