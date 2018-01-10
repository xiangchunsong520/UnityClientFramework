﻿using System;

namespace ImageHelper
        
        #region 缩略图

            int towidth = width;

            int x = 0;

            switch (mode)
                    break;
                    toheight = originalImage.Height * width / originalImage.Width;
                    towidth = originalImage.Width * height / originalImage.Height;
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight), new System.Drawing.Rectangle(x, y, ow, oh), System.Drawing.GraphicsUnit.Pixel);

            try
                //以jpg格式保存缩略图
                bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
        #endregion
        #region 图片水印

        /// <summary>

            if (location == "LT")
        #endregion
        #region 文字水印
            #region
            string kz_name = Path.GetExtension(path);
            #endregion

        /// <summary>
            #region
            ArrayList loca = new ArrayList();  //定义数组存储位置
            float x = 10;

            if (location == "LT")
            #endregion
        #endregion
        #region 调整光暗
            int x, y, resultR, resultG, resultB;//x、y是循环次数，后面三个是记录红绿蓝三个值的
            Color pixel;
                    resultR = pixel.R + val;//检查红色值会不会超出[0, 255]
                    resultG = pixel.G + val;//检查绿色值会不会超出[0, 255]
                    resultB = pixel.B + val;//检查蓝色值会不会超出[0, 255]
                    bm.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));//绘图
                }
        #endregion
        #region 反色处理
            int x, y, resultR, resultG, resultB;
                    resultR = 255 - pixel.R;//反红
                    resultG = 255 - pixel.G;//反绿
                    resultB = 255 - pixel.B;//反蓝
                    bm.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));//绘图
                }
        #endregion
        #region 浮雕处理
        #endregion
        #region 拉伸图片
        #endregion
        #region 滤色处理
            int x, y;

            for (x = 0; x < width; x++)
                    bm.SetPixel(x, y, Color.FromArgb(0, pixel.G, pixel.B));//绘图
                }
        #endregion
        #region 左右翻转
            Color pixel;
                    bm.SetPixel(z++, y, Color.FromArgb(pixel.R, pixel.G, pixel.B));//绘图
                }
        #endregion
        #region 上下翻转
                    bm.SetPixel(x, z++, Color.FromArgb(pixel.R, pixel.G, pixel.B));//绘图
                }
        #endregion
        #region 压缩图片
                        break;
        #endregion
        #region 图片灰度化
        #endregion
        #region 转换为黑白图片
            Color pixel;
                    result = (pixel.R + pixel.G + pixel.B) / 3;//取红绿蓝三色的平均值
                    bm.SetPixel(x, y, Color.FromArgb(result, result, result));
        #endregion
        #region 获取图片中的各帧
            for (int i = 0; i < count; i++)    //以Jpeg格式保存各帧
            {
        #endregion