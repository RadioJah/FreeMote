﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FreeMote.Psb
{
    public class RlCompress
    {
        public enum PsbImageFormat
        {
            Bmp,
            Png,
        }

        public static byte[] Compress()
        {
            return null;
        }

        public static void ConvertToImageFile(byte[] data, string path, int height, int width, int align = 4, PsbImageFormat format = PsbImageFormat.Bmp)
        {
            byte[] bytes;
            try
            {
                bytes = Uncompress(data, height, width, align);
            }
            catch (Exception e)
            {
                throw new BadImageFormatException("data incorrect", e);
            }
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            //// 获取图像参数
            int stride = bmpData.Stride;  // 扫描线的宽度
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置
            int scanBytes = stride * height;   // 用stride宽度，表示这是内存区域的大小

            if (scanBytes >= bytes.Length)
            {
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, iptr, bytes.Length);
                bmp.UnlockBits(bmpData);  // 解锁内存区域
                switch (format)
                {
                    case PsbImageFormat.Bmp:
                        bmp.Save(path, ImageFormat.Bmp);
                        break;
                    case PsbImageFormat.Png:
                        bmp.Save(path, ImageFormat.Png);
                        break;
                }

                return;
            }
            throw new BadImageFormatException("data may not corresponding");
        }

        public static void ConvertToWinFormatFile()
        { }

        private static byte[] Uncompress(Stream stream, int height, int width, int align = 4)
        {
            var realLength = height * width * align;
            return PixelCompress.Uncompress(stream, realLength, align);
        }

        private static byte[] Uncompress(byte[] data, int height, int width, int align = 4)
        {
            using (var stream = new MemoryStream(data))
            {
                return Uncompress(stream, height, width, align);
            }
        }
    }
}