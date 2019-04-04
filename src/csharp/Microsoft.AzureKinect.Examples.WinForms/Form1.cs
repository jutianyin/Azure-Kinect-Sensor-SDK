﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AzureKinect;

namespace Microsoft.AzureKinect.Examples.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            using (Device device = Device.Open(0))
            {
                device.StartCameras(new DeviceConfiguration
                {
                    color_format = ImageFormat.ColorBGRA32,
                    color_resolution = ColorResolution.r1080p,
                    depth_mode = DepthMode.NFOV_2x2Binned,
                    synchronized_images_only = true
                });

                while (true)
                {
                    using (Capture capture = await Task.Run(() => { return device.GetCapture(); }))
                    {
                        pictureBoxColor.Image = new Bitmap(capture.Color.WidthPixels,
                            capture.Color.HeightPixels,
                            capture.Color.StrideBytes,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                            ((UnsafeImage)capture.Color).UnsafeBufferPointer);


                        pictureBoxDepth.Image = await Task.Run(() =>
                        {
                            Bitmap depthVisualization = new Bitmap(capture.Depth.WidthPixels, capture.Depth.HeightPixels, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                            
                            //BitmapData d = depthVisualization.LockBits(new Rectangle(0, 0, depthVisualization.Width, depthVisualization.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            try
                            {
                                UInt16[] depthValues = new UInt16[capture.Depth.WidthPixels * capture.Depth.HeightPixels];


                                capture.Depth.CopyTo(depthValues, 0, 0, depthValues.Length);
                                for (int y = 0; y < capture.Depth.HeightPixels; y++)
                                {
                                    for (int x = 0; x < capture.Depth.WidthPixels; x++)
                                    {
                                        ushort depthValue = depthValues[y * capture.Depth.WidthPixels + x];

                                        if (depthValue == 0)
                                        {
                                            depthVisualization.SetPixel(x, y, Color.Red);
                                        }
                                        else if (depthValue == ushort.MaxValue)
                                        {
                                            depthVisualization.SetPixel(x, y, Color.Green);
                                        }
                                        else
                                        {

                                            float brightness = (float)depthValue / (float)2000;

                                            if (brightness > 1.0f)
                                                depthVisualization.SetPixel(x, y, Color.White);
                                            else
                                            {
                                                int c = (int)(brightness * 250);
                                                depthVisualization.SetPixel(x, y, Color.FromArgb(c, c, c));
                                            }

                                        }
                                    }
                                }
                            }
                            finally
                            {
                                //depthVisualization.UnlockBits(d);
                            }
                            return depthVisualization;
                        });

                        this.Invalidate();
                    }
                }
            }
        }
    }
}
