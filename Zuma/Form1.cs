using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Zuma
{
    public partial class Form1 : Form
    {
        class Frog
        {
            public int x, y;
            public int index = 0;
        }
        Random R = new Random();
        Timer T = new Timer();
        Stack<Ball.Color> ListC = new Stack<Ball.Color>();
        int index = 0;
        Bitmap off;
        PointF prevPoint = new PointF(-1, -1);
        Bitmap frog;
        Bitmap background;
        Bitmap rotatedFrog;
        LinkedList<Ball> Lballs = new LinkedList<Ball>();
        PointF ballPoint;
        float t = 0f;
        float Sim = 0.001f;
        int SSIZE = 46;
        BezierCurve mycurve = new BezierCurve();
        float ref_T = 0f;
        List<Ball> ListSB = new List<Ball>();
        int currX, currY;
        LinkedListNode<Ball> Catch;
        Ball ShotBall;
        int ct = 0;
        int portalIndex = 0;
        Reader Sheet1;
        Reader Sheet2;
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.MouseDown += Form1_MouseDown;
            this.Paint += Form1_Paint;
            T.Tick += T_Tick;
            this.KeyDown += Form1_KeyDown;

        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                ShootBall();
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

        }
        private void T_Tick(object sender, EventArgs e)
        {

            ballPoint = mycurve.CalcCurvePointAtTime(t);
            t += Sim;
            BallsMoving();
            if (index < 47)
            {
                index++;
            }
            else
            {
                index = 0;
            }
            ShootMoving();
            TrueCatchy();

            OpenPortal();
            DrawDubb(CreateGraphics());

            DetectGameEnd();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }
        public void RandomColors(int max)
        {
            for (int i=0;i<200;i++)
            {
                int rand = R.Next(max);
                switch(rand)
                {
                    case 0:
                        ListC.Push(Ball.Color.Blue);
                        break;
                    case 1:
                        ListC.Push(Ball.Color.Red);
                        break;
                    case 2:
                        ListC.Push(Ball.Color.Green);
                        break;
                    case 3:
                        ListC.Push(Ball.Color.Yellow);
                        break;
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;

            off = new Bitmap(this.Width, this.Height);

            SetSheet1();

            SetSheet2();

            background = new Bitmap("background.jpg");

            background = new Bitmap(background, new Size(Width, Height));

            this.MouseMove += Form1_MouseMove;

            frog = new Bitmap(Sheet2.atlas.Clone(Sheet2.getRectangle(0), Sheet2.atlas.PixelFormat));

            MakePath();
            RandomColors(4);
            RandomBallsL();
            T.Start();
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (rotatedFrog != null)
            {
                int xCenter = 630 + (rotatedFrog.Width) / 2;
                int yCenter = 300 + (rotatedFrog.Height) / 2;

                float diffY = (e.Y - yCenter);
                float diffX = (e.X - xCenter);

                float m = diffY / diffX;
                m = 1 / m;
                double f = Math.Atan(m);
                float rad = (float)(f * 180 / Math.PI);
                if (e.Y < yCenter)
                    rad -= 180;
                rotatedFrog = rotateImage(frog, rad);
            }
            else
            {
                int xCenter = 630 + (frog.Width) / 2;
                int yCenter = 300 + (frog.Height) / 2;
                float m = (e.Y - yCenter) / (e.X - xCenter);
                rotatedFrog = rotateImage(frog, (float)Math.Atan(1 / m));
            }
            currX = e.X;
            currY = e.Y;
           // RotateZumaWithMouse(e.X, e.Y);
        }
        public void DetectMatchingBall(LinkedListNode<Ball> InsertedBall)
        {

            LinkedListNode<Ball> ptEMP;
            bool del = false;
            List<LinkedListNode<Ball>> Ldeletednodes = new List<LinkedListNode<Ball>>();
            for (LinkedListNode<Ball> pFWD = InsertedBall.Next; pFWD !=null; pFWD = pFWD.Next)
            {
                if (pFWD.Value.color == InsertedBall.Value.color)
                {

                    Ldeletednodes.Add(pFWD);
                }
                else
                {
                    break;
                }
            }
            for (LinkedListNode<Ball> pBCK = InsertedBall.Previous; pBCK != null; pBCK = pBCK.Previous)
            {
                if (pBCK.Value.color == InsertedBall.Value.color)
                {

                    Ldeletednodes.Add(pBCK);
                }
                else
                {
                    break;
                }
            }

            if (Ldeletednodes.Count>0)
                Lballs.Remove(InsertedBall);
            for (int i=0;i<Ldeletednodes.Count;i++)
            {
                Lballs.Remove(Ldeletednodes[i]);
            }
        }
        public void Tcatchy(LinkedListNode<Ball> ball, Ball shotBall)
        {
            LinkedListNode<Ball> ptrav = ball;
            shotBall.currT = ptrav.Value.currT+ 0.007f;


            Lballs.AddBefore(ball, shotBall);
            LinkedListNode<Ball> temp = new     LinkedListNode<Ball> (shotBall); 
            ListSB.Remove(shotBall);
            DetectMatchingBall(ball.Previous);
            BallsPosition();
        }
        public void MakeBall()
        {

            //not proud of this
            Ball ball = ColoredBall();

            if (Lballs.First == null)
            {
                ball.ballPosition.X = mycurve.CalcCurvePointAtTime(ref_T).X;

                ball.ballPosition.Y = mycurve.CalcCurvePointAtTime(ref_T).Y;
                Lballs.AddFirst(ball);
                ref_T -= 0.007f;
            }
            else
            {
                
                ball.currT = ref_T;

                ball.ballPosition.X = mycurve.CalcCurvePointAtTime(ref_T).X;

                ball.ballPosition.Y = mycurve.CalcCurvePointAtTime(ref_T).Y;
                Lballs.AddLast(ball);
                ref_T -= 0.007f;
            }
        }
        public Ball ColoredBall()
        {

            Ball ball = new Ball();
            switch (ListC.Pop())
            {
                case Ball.Color.Blue:
                    ball = BlueBall();
                    break;
                case Ball.Color.Green:
                    ball = GreenBall();
                    break;
                case Ball.Color.Yellow:
                    ball = YellowBall();
                    break;
                case Ball.Color.Red:
                    ball = RedBall();
                    break;


            }
            return ball;
        }
        public Ball BlueBall()
        {
            Ball ball = new Ball(0, 0, 48, 48, 0, SSIZE, Ball.Color.Blue);
            return ball;
        }
        public Ball GreenBall()
        {
            Ball ball = new Ball(0, 0, 48, 48, SSIZE+1, 1+(SSIZE*2), Ball.Color.Green);
            return ball;

        }
        public Ball YellowBall()
        {
            Ball ball = new Ball(0, 0, 48, 48, 2+ (SSIZE*2), 2+ (SSIZE*3), Ball.Color.Yellow);
            return ball;
        }
        public Ball RedBall()
        {
            Ball ball = new Ball(0, 0, 48, 48, 3 + (SSIZE * 3), 3 + (SSIZE * 4), Ball.Color.Red);
            return ball;
        }
        public void TrueCatchy()
        {
            for (int i = 0; i < ListSB.Count; i++)
            {
                if (Lballs.Count > 0)
                {
                    LinkedListNode<Ball> ptrav = Lballs.First;
                    while (ptrav != null)
                    {
                        if (ListSB[i].IsCollid(ptrav.Value))
                        {
                            ListSB[i].straightPath.Speed = 5;

                            PointF currP = new PointF(ListSB[i].straightPath.currX, ListSB[i].straightPath.currY);
                            PointF nextP = mycurve.CalcCurvePointAtTime(ptrav.Value.currT + 0.007f);
                            ListSB[i].straightPath = new DDA();
                            ListSB[i].straightPath.SetVals(currP.X, currP.Y, nextP.X, nextP.Y);
                            ShotBall = ListSB[i];


                            Catch = ptrav;

                            break;
                        }
                        else
                            ptrav = ptrav.Next;
                    }
                }
            }
        }
        private Bitmap rotateImage(Bitmap b, float angle)
        {
            Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
            Graphics g = Graphics.FromImage(returnBitmap);
            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            g.DrawImage(b, new Point(0, 0));
            return returnBitmap;
        }
        public void SetSheet1()
        {
            Sheet1 = new Reader("assets/images/gameobjects.png");
            int keyId = 0;
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 47; i++)
                {
                    Sheet1.addSubImageToDict(keyId, new Rectangle(j * 48, i * 48, 48, 48));
                    keyId++;
                }
            }
        }
        public void SetSheet2()
        {
            Sheet2 = new Reader(new Bitmap("assets/images/frog.png"));
            int keyId = 0;

            Sheet2.addSubImageToDict(keyId, new Rectangle(0, 0, 160, 160));
        }
        void curve()
        {
            mycurve.ControlPoints.Add(new Point(1250, -10));
            mycurve.ControlPoints.Add(new Point(1250, 200));
            mycurve.ControlPoints.Add(new Point(1350, 320));
            mycurve.ControlPoints.Add(new Point(1450, 600));
            mycurve.ControlPoints.Add(new Point(1500, 600));
            mycurve.ControlPoints.Add(new Point(1450, 680));
            mycurve.ControlPoints.Add(new Point(1400, 700));
            mycurve.ControlPoints.Add(new Point(800, 930));
            mycurve.ControlPoints.Add(new Point(600, 760));
            mycurve.ControlPoints.Add(new Point(450, 760));
            mycurve.ControlPoints.Add(new Point(430, 700));
            mycurve.ControlPoints.Add(new Point(400, 700));
            mycurve.ControlPoints.Add(new Point(380, 700));
            mycurve.ControlPoints.Add(new Point(370, 700));
            mycurve.ControlPoints.Add(new Point(250, 690));
            mycurve.ControlPoints.Add(new Point(250, 650));
            mycurve.ControlPoints.Add(new Point(150, 620));
            mycurve.ControlPoints.Add(new Point(150, 640));
            mycurve.ControlPoints.Add(new Point(120, 540));
            mycurve.ControlPoints.Add(new Point(110, 520));
            mycurve.ControlPoints.Add(new Point(100, 520));
            mycurve.ControlPoints.Add(new Point(80, 520));
            mycurve.ControlPoints.Add(new Point(50, 470));
            mycurve.ControlPoints.Add(new Point(50, 400));
            mycurve.ControlPoints.Add(new Point(60, 390));
            mycurve.ControlPoints.Add(new Point(80, 380));
            mycurve.ControlPoints.Add(new Point(110, 350));
            mycurve.ControlPoints.Add(new Point(130, 300));
            mycurve.ControlPoints.Add(new Point(170, 260));
            mycurve.ControlPoints.Add(new Point(180, 200));
            mycurve.ControlPoints.Add(new Point(200, 150));
            mycurve.ControlPoints.Add(new Point(250, 100));
            mycurve.ControlPoints.Add(new Point(300, 80));
            mycurve.ControlPoints.Add(new Point(350, 90));
            mycurve.ControlPoints.Add(new Point(400, 80));
        }
        List<Point>  ReadPoints()
        {
            List<Point> points = new List<Point>();
            string[] readText = File.ReadAllLines("points.txt");
            int[] values = new int[readText.Length/2];
            int ct = 0;
            foreach (string s in readText)
            {
                if (s != ",")
                {
                    values[ct] = Int32.Parse(s);
                    ct++;
                }
            }
            int j = 1;
            for (int i=0;i<ct-1;i+=2)
            {
                Point pn = new Point(values[i], values[j]);

                j+=2;

                points.Add(pn);
            }
            return points;
            curve();
        }
        void MakePath()
        {
            List<Point> points = ReadPoints();
            curve();
          // for (int i=0;i<points.Count;i++)
          // {
          //     mycurve.SetControlPoint(points[i]);
          //
          //
          // }


        }
        void BallsPosition()
        {

            LinkedListNode<Ball> ptrav = Lballs.Last;
            float LastT = ptrav.Value.currT;
            ptrav = ptrav.Previous;
            while (ptrav!=null)
            {
                ptrav.Value.currT = LastT + 0.007f;
                LastT = ptrav.Value.currT;
                ptrav = ptrav.Previous;
            }
        }
        public void BallsMoving()
        {
            int flag = 0;
            for (LinkedListNode<Ball> ptrav = Lballs.First; ptrav != null; ptrav = ptrav.Next)
            {

                PointF point = mycurve.CalcCurvePointAtTime(ptrav.Value.currT);
                ptrav.Value.ballPosition.X = point.X;
                ptrav.Value.ballPosition.Y = point.Y;
                ptrav.Value.currT += Sim;
                




            }


            for (LinkedListNode<Ball> ptrav = Catch; ptrav != null; ptrav = ptrav.Previous)
            {

                if (flag == 0 && Catch != null && ptrav != Catch)
                {
                    ptrav.Value.currT += 0.0028f;

                }
                if (ptrav.Previous == null)
                {
                    if (ct == 2)
                    {
                        flag = 1;
                        Tcatchy(Catch, ShotBall);
                        Catch = null;
                        
                        ct = 0;



                    }
                    else
                    {
                        ct++;
                    }

                }
            }

           if (Lballs.Last.Value.currT > 0.007f)
            {
                //MakeBalls();
            }

          
        }
        public void ShootMoving()
        {
            for (int i=0;i<ListSB.Count;i++)
            {
                ListSB[i].straightPath.MoveStep();
                ListSB[i].ballPosition.X = ListSB[i].straightPath.currX;

                ListSB[i].ballPosition.Y = ListSB[i].straightPath.currY;
            }
        }
        public void ShootBall()
        {

            int xCenter = 630 + (rotatedFrog.Width) / 2;
            int yCenter = 300 + (rotatedFrog.Height) / 2;
            Ball ball = ColoredBall();
            DDA line = new DDA();
            line.SetVals(xCenter, yCenter, xCenter - (2 * (currX - (650 + (rotatedFrog.Width / 2)))), currY);
            ball.straightPath = line;
            ListSB.Add(ball);
        }
        public void RandomBallsL()
        {
            int NofBalls = 30;
            float diff = Sim * 30;
            for (int i=0;i<NofBalls;i++)
            {
                MakeBall();
            }


           
        }
        public void OpenPortal()
        {


            float  t_Portal =  0.9f ;
            float t_Curr = Lballs.First.Value.currT;
            //double distance = Math.Sqrt(Math.Pow(pPortal.Y - pCurr.Y, 2) + Math.Pow(pPortal.X - pCurr.X, 2));

            float distance = Math.Abs(t_Portal - t_Curr);
            if (distance <0.1)
            {
                portalIndex = 1;
            }
            if (distance < 0.05)
            {
                portalIndex = 3;
            }
            if (distance < 0.025)
            {
                portalIndex =   5;
            }
            


        }
        public void RotateZumaWithMouse(int x, int y)
        {
            if (rotatedFrog != null)
            {
                int xCenter = 630 + (rotatedFrog.Width) / 2;
                int yCenter = 300 + (rotatedFrog.Height) / 2;

                float diffY = (y - yCenter);
                float diffX  =(x - xCenter);

                float m = diffY / diffX;
                m = 1 / m;
                double f = Math.Atan(m);
                float rad = (float)(f * 180 / Math.PI);
                if (y < yCenter)
                    rad -= 180;
                rotatedFrog = rotateImage(frog, rad);
            }
            else
            {
                int xCenter = 630 + (frog.Width) / 2;
                int yCenter = 300 + (frog.Height) / 2;
                float m = (y - yCenter) / (x - xCenter);
                rotatedFrog = rotateImage(frog, (float)Math.Atan(1 / m));
            }
        }
        public void DetectGameEnd()
        {
            if (Lballs == null)
                Gameover();
            if (Lballs.First.Value.currT >= 0.9999f)
            {
                Gameover();
            }
        }
        public void Gameover()
        {
            Dispose();
        }
        public void DrawScene(Graphics g)
        {

            g.Clear(Color.White);
            g.DrawImage(background, 1, 1);
            Bitmap atlas = new Bitmap(Sheet1.imgPath);
            Rectangle rect;
            for (LinkedListNode<Ball> ptrav = Lballs.First; ptrav != null;ptrav = ptrav.Next)
            {            
                 rect = Sheet1.getRectangle(ptrav.Value.currIndex);

                g.DrawImage(atlas, new RectangleF(ptrav.Value.ballPosition.X - (rect.Width / 2)

               , ptrav.Value.ballPosition.Y - (rect.Height / 2)
               , rect.Width, rect.Height), rect, GraphicsUnit.Pixel);


                ptrav.Value.ChangeImgFrame();
            }
            if (rotatedFrog != null)
            {
                g.DrawImage(rotatedFrog, 630, 300);            
            }

            g.FillEllipse(Brushes.Red, (630+(rotatedFrog.Width / 2)) - (2 * (currX - (650+(rotatedFrog.Width / 2)))),currY, 20, 20);
            

            for (int i = 0; i < ListSB.Count; i++)
            {


                  rect = Sheet1.getRectangle(ListSB[i].currIndex);


                g.DrawImage(atlas, new RectangleF(ListSB[i].ballPosition.X - (rect.Width / 2)

               , ListSB[i].ballPosition.Y - (rect.Height / 2)
               , rect.Width, rect.Height), rect, GraphicsUnit.Pixel);


                ListSB[i].ChangeImgFrame();
            }


            Ball.Color next = ListC.Peek();
            int ind = 0;
            switch (next)
            {
                case Ball.Color.Yellow:
                    ind = 2 + (SSIZE * 2);
                    break;
                case Ball.Color.Green:
                    ind = SSIZE + 1;
                    break;
                case Ball.Color.Blue:
                    ind = 0;
                    break;
                case Ball.Color.Red:
                    ind = 3 + (SSIZE * 3);
                    break;
            }


            rect = Sheet1.getRectangle(ind);

            g.DrawImage(atlas, new Rectangle(800,400,48,48), rect, GraphicsUnit.Pixel);

            atlas.Dispose();


        }
        void DrawDubb(Graphics g)
        {
            Graphics g2 = Graphics.FromImage(off);
            DrawScene(g2);
            g.DrawImage(off, 0, 0);
        }
    }
}
