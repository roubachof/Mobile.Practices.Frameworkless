using System;

using Android.Content;
using Android.Graphics;
using Android.Runtime;

using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load;
using Com.Bumptech.Glide.Load.Engine.Bitmap_recycle;
using Com.Bumptech.Glide.Load.Resource.Bitmap;

namespace SeLoger.Lab.Playground.Droid
{
    public class CropCircleTransformation : BitmapTransformation
    {
        public CropCircleTransformation(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public CropCircleTransformation(Context p0)
            : base(p0)
        {
        }

        public CropCircleTransformation(IBitmapPool p0)
            : base(p0)
        {
        }

        protected override Bitmap Transform(IBitmapPool bitmapPool, Bitmap source, int p2, int p3)
        {
            int size = Math.Min(source.Width, source.Height);

            int width = (source.Width - size) / 2;
            int height = (source.Height - size) / 2;

            Bitmap bitmap = bitmapPool.Get(size, size, Bitmap.Config.Argb8888)
                            ?? Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);

            Canvas canvas = new Canvas(bitmap);
            Paint paint = new Paint();
            BitmapShader shader =
                new BitmapShader(source, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
            if (width != 0 || height != 0)
            {
                // source isn't square, move viewport to center
                Matrix matrix = new Matrix();
                matrix.SetTranslate(-width, -height);
                shader.SetLocalMatrix(matrix);
            }
            paint.SetShader(shader);
            paint.AntiAlias = true;

            float r = size / 2f;
            canvas.DrawCircle(r, r, r, paint);

            return BitmapResource.Obtain(bitmap, bitmapPool).Get();
        }

        public override string Id => "CropCircleTransformation()";
    }
}