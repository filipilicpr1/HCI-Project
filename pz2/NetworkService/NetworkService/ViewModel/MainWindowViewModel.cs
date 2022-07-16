using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        //private int count = 15; // Inicijalna vrednost broja objekata u sistemu
                                // ######### ZAMENITI stvarnim brojem elemenata
                                //           zavisno od broja entiteta u listi
        
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand<UserControl> UCLoadedCommand { get; set; }
        private NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        private NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        private MeasurementGraphViewModel measurementGraphViewModel = new MeasurementGraphViewModel();
        private BindableBase currentViewModel;
        private int lastSentCount = -1;
        private const string filePath = "..//Debug//Log.txt";

        public MainWindowViewModel()
        {
            NavCommand = new MyICommand<string>(OnNav);
            UCLoadedCommand = new MyICommand<UserControl>(OnUCLoad);
            CurrentViewModel = networkEntitiesViewModel;
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            ci.DateTimeFormat.DateSeparator = ".";
            Thread.CurrentThread.CurrentCulture = ci;
            createListener(); //Povezivanje sa serverskom aplikacijom
        }

        private void OnUCLoad(UserControl uc)
        {
            uc.Focus();
        }

        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "entities":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "display":
                    CurrentViewModel = networkDisplayViewModel;
                    break;
                case "graph":
                    CurrentViewModel = measurementGraphViewModel;
                    break;
            }
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            //Byte[] data = System.Text.Encoding.ASCII.GetBytes(count.ToString());
                            lastSentCount = NetworkEntitiesViewModel.Entities.Count;
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(NetworkEntitiesViewModel.Entities.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"
                            if (lastSentCount == NetworkEntitiesViewModel.Entities.Count)
                            {
                                char[] delimiters = new char[] { '_', ':' };
                                string[] res = incomming.Split(delimiters);
                                int ind = int.Parse(res[1]);
                                double value = double.Parse(res[2]);

                                App.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    if (ind >= 0 && ind < NetworkEntitiesViewModel.Entities.Count)
                                    {
                                        NetworkEntitiesViewModel.Entities[ind].Val = value;
                                        if (!File.Exists(filePath))
                                        {
                                            using (StreamWriter sw = new StreamWriter(filePath))
                                            {
                                                sw.WriteLine(DateTime.Now.ToString() + " : " + NetworkEntitiesViewModel.Entities[ind].Name + " " + NetworkEntitiesViewModel.Entities[ind].Id.ToString() + " , " + NetworkEntitiesViewModel.Entities[ind].Val.ToString());

                                            }
                                        }
                                        else
                                        {
                                            using (StreamWriter sw = File.AppendText(filePath))
                                            {
                                                sw.WriteLine(DateTime.Now.ToString() + " : " + NetworkEntitiesViewModel.Entities[ind].Name + " " + NetworkEntitiesViewModel.Entities[ind].Id.ToString() + " , " + NetworkEntitiesViewModel.Entities[ind].Val.ToString());

                                            }
                                        }
                                        if (MeasurementGraphViewModel.UpdateTextBox != null)
                                            MeasurementGraphViewModel.UpdateTextBox.Text = DateTime.Now.ToString();
                                        if (NetworkDisplayViewModel.UpdateTextBox != null)
                                            NetworkDisplayViewModel.UpdateTextBox.Text = DateTime.Now.ToString();
                                    }
                                });
                            }
                            
                             
                            
                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji

                        }
                    }, null);
                }
            });
            listeningThread.IsBackground = true;
            listeningThread.Start();
            
        }




    }
}
