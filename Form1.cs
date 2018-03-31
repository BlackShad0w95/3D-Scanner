using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;


namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public DirectX.Capture.Filter Camera;
        public DirectX.Capture.Capture CaptureInfo;
        public DirectX.Capture.Filters CamContainer;
        Image captureImage;
        private readonly SynchronizationContext synchronizationContext;
        private DateTime previousTime = DateTime.Now;

        int nr,dup;

        public Form1()
        {
            InitializeComponent();
            nr =dup= 1;

            serialPort1.BaudRate = 9600;
            serialPort1.PortName = "COM10";
           serialPort1.Open();
            serialPort1.DataReceived += serialPort1_DataReceived;


        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string line = serialPort1.ReadLine();
            this.BeginInvoke(new LineReceivedEvent(LineReceived), line);
        }


        private delegate void LineReceivedEvent(string line);
        private void LineReceived(string line)
        {

            label3.Text = line;
            using (StreamWriter writetext = new StreamWriter("dane_pomiarowe.txt", true))
            {
                writetext.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + " Dane:" + line);

            }


        }








        private void Form1_Load(object sender, EventArgs e)
        {
              CamContainer = new DirectX.Capture.Filters();  
 
       try
       {
            int no_of_cam = CamContainer.VideoInputDevices.Count;
 
            for (int i = 0; i< no_of_cam; i++)
            {
                try
                {
                       
                        Camera = CamContainer.VideoInputDevices[i];

                        CaptureInfo = new DirectX.Capture.Capture(Camera, null);

                        CaptureInfo.PreviewWindow = this.pictureBox1;

                        CaptureInfo.FrameCaptureComplete += RefreshImage;


                        CaptureInfo.Width = 800;
                        CaptureInfo.Height = 600;

                        CaptureInfo.CaptureFrame();
                         
 
                        break;
                  }
                    catch (Exception ex) { }
            }
        }
        catch (Exception ex)
        {                
                MessageBox.Show(this, "No Video Device Found", "Error:");
         }
        }

        public void RefreshImage(PictureBox frame)
        {
            //nr = 100;


            captureImage = frame.Image;
            String nazwa = "obraz" + nr + ".jpg";   
          
            captureImage.Save(nazwa ,System.Drawing.Imaging.ImageFormat.Jpeg);   

            this.pictureBox2.Image = captureImage;


         
            this.label1.Text = "Zapisano zdjęcie nr " + nr.ToString()+ "   Dane " +frame.Width.ToString() + "x" +frame.Height.ToString();
        

            nr++;

        }

     

        private async void button1_Click_1(object sender, EventArgs e)
        {

            button1.Enabled = false;

            int ilosc = 0;

            nr = 1;
            int Iilosc_krokow =  Convert.ToInt32(ilosc_krokow.Text);
                
            int pelen_obrot = 270;

            serialPort1.Write("ON");
            System.Threading.Thread.Sleep(400);
 

            for (int i = 0; i < pelen_obrot; i+=Iilosc_krokow)
                {

                    CaptureInfo.CaptureFrame();
                    ilosc++;

                    await Task.Delay(500);
                
                   serialPort1.Write("Kroki");
                    System.Threading.Thread.Sleep(500);


                //    await Task.Delay(500);
                    
                   serialPort1.Write(Iilosc_krokow.ToString());


                //    await Task.Delay(500);
                   System.Threading.Thread.Sleep(1000);
 

                }


            using (StreamWriter writetext = new StreamWriter("info.txt"))
            {
                writetext.WriteLine(ilosc.ToString());
            }

            serialPort1.Write("OFF");
            System.Threading.Thread.Sleep(400);
 

            button1.Enabled = true;

        

            

            
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            String path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*.jpg"); 

           
            String ilosc_obr = "";
   
            Double  thetaDegrees= 30.0;
            Double thetaRadians =( Math.PI * thetaDegrees) /180.0;
            Double rotationAngleDegrees = 0.0;
                        


            using (StreamReader readtext = new StreamReader("info.txt"))
            {
                 ilosc_obr = readtext.ReadLine();
            }

            using (StreamWriter writetext = new StreamWriter("3dplik.txt"))
            {
               


                for (int obraz = 1; obraz <= Int16.Parse(ilosc_obr); obraz++)
                {
                    String nazwa = "obraz" + obraz.ToString() + ".jpg";


                    label1.Text= "Analiza obrazu nr "+ obraz.ToString()+"/"+ilosc_obr.ToString();
                    await Task.Delay(500);


                    using (Stream BitmapStream = System.IO.File.Open(nazwa, System.IO.FileMode.Open))
                    {
                        Image img = Image.FromStream(BitmapStream);

                        Bitmap bmap = new Bitmap(img);
                       

                        int poz = 0;

                        Color[] tab = new Color[9];
                        byte[] pik = new byte[640 * 480];

                        Color c;
                        for (int i = 0; i < bmap.Width; i++)
                        {
                            for (int j = 0; j < bmap.Height; j++)
                            {
                                c = bmap.GetPixel(i, j);
                                byte gray = (byte)(.6299 * c.R + .02 * c.G + .02 * c.B);

                                bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                            }
                        }

                        
                        poz = 0;
                        for (int i = 1; i < bmap.Width - 1; i++)
                        {


                            for (int j = 1; j < bmap.Height - 1; j++)
                            {
                                int ww = 0;
                                double suma = 0.0;
                  

                                for (int r = i - 1; r <= 1 + i; r++)
                                    for (int t = j - 1; t <= 1 + j; t++)
                                    {
                                        suma += (double)bmap.GetPixel(r, t).R;



                                    }

                                suma /= 9.0;
                                if (suma > 255) suma = 255;
                                if (suma < 0) suma = 0;
                                pik[poz++] = (byte)(suma);



                            }
                        }


                        poz = 0;
                        for (int i = 1; i < bmap.Width - 1; i++)
                            for (int j = 1; j < bmap.Height - 1; j++)
                                bmap.SetPixel(i, j, Color.FromArgb(pik[poz], pik[poz], pik[poz++]));



                        //  progroawnie


                        poz = 0;

                        for (int i = 1; i < bmap.Width - 1; i++)
                        {


                            for (int j = 1; j < bmap.Height - 1; j++)
                            {

                                c = bmap.GetPixel(i, j);
                                byte gray;
                                if (c.R > 150)
                                    gray = (byte)255;//bialy
                                else gray = 0;

                                bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));

                            }
                        }



                        //generowanie danych 3D
                        //przelicznik krok na stopien
                        Double degreesPerStep = 5.0;
                        Double rotationAngleRadians = Math.PI* rotationAngleDegrees/180.0;
                        Double xPixelsPerMM, yPixelsPerMM;
                        yPixelsPerMM = xPixelsPerMM = 5.5;
                        Double rowBrightestPosition = -1;

                        Double xPosition, yPosition, zPosition;
                        Double xRotated, yRotated, zRotated;
                        Double centerLine = 320.0; // ?

                        for (int wys = 0; wys < bmap.Height; wys++)
                        {
                            int first, last;
                            first = last = -1;
                            rowBrightestPosition = -1;


                            for (int szer = 0; szer < bmap.Width; szer++)
                            { 
                                 c = bmap.GetPixel(szer, wys);
                                 if (c.R > 200 )
                                 {
                                     if (first == -1)   first = last=szer;
                                     
                                     else last = szer;

                                 }
                            
                            }

                            if (first != -1 & last != -1)  rowBrightestPosition = first + ((last - first) / 2.0);

                            yPosition = ((bmap.Height * 1.00) - wys) * yPixelsPerMM;  // #this will need to be scaled
                            xPosition = ((rowBrightestPosition - centerLine)/(Math.Sin(thetaRadians))) * xPixelsPerMM; 
                            zPosition = 0.00; 
                        
                  
                             rotationAngleRadians = Math.PI * (rotationAngleDegrees) / 180.0;
                          zRotated = (zPosition * Math.Cos(rotationAngleRadians)) - (xPosition * Math.Sin(rotationAngleRadians));
                          xRotated = (zPosition * Math.Sin(rotationAngleRadians)) + (xPosition * Math.Cos(rotationAngleRadians));
                          yRotated = yPosition;

                          if (rowBrightestPosition!= -1)
                              writetext.WriteLine(xRotated.ToString()+ " " +yRotated.ToString()+ " " +zRotated.ToString());

                          
                        
                        }

                       rotationAngleDegrees = rotationAngleDegrees + degreesPerStep;

                        pictureBox2.Image = (Bitmap)bmap.Clone();
                        await Task.Delay(500);


                    }
                }
            }

            button2.Enabled = true;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Laser ON")
            {
                serialPort1.Write("Laser");
                button3.Text = "Laser OFF";
            }
            else
            {
                serialPort1.Write("Laser");
                button3.Text = "Laser ON";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            serialPort1.Write("Kierunek");
        }

      




    }
}
