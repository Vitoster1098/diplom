using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom
{
    class pixelAnalyse
    {
        public Dictionary<string, Bitmap> map = new Dictionary<string, Bitmap>();
        
        public void addPictures(string path, Bitmap picture)
        {
            map.Add(path, picture);
        }

        public Bitmap getPicture(string path)
        {
            return map[path];
        }

        public int[] makeGistogram(Bitmap picture)
        {
            int[] counter = new int[255];

            for(int i = 0; i < picture.Width; ++i)
            {
                for(int j = 0; j < picture.Height; ++j)
                {
                    counter[picture.GetPixel(i, j).R]++;
                }
            }

            return counter;
        }
    }
}
