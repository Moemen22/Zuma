using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Zuma
{
    class Reader
    {
        // x,y,w,h for sub images in atlas
        public IDictionary<int, Rectangle> subImagesPosition  = new Dictionary<int, Rectangle>();

        public Bitmap atlas;
        public string imgPath;
        public Reader(Bitmap img)
        {
            atlas = img;

        }
        public Reader(string path)
        {
            imgPath = path;

        }
        public void addSubImageToDict(int id, Rectangle rect)
        {

           
            subImagesPosition.Add(id, rect);

        }

        public Rectangle getRectangle(int id)
        {
            Rectangle pn = subImagesPosition[id];
            return pn;
        }




    }
}
