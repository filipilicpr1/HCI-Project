using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        private string updateText;
        private static Entity selectedEntity;
        private bool chartCreated = false;
        private const string filePath = "..//Debug//Log.txt";
        private string time1;
        private string time2;
        private string time3;
        private string time4;
        private string time5;
        private int[] values = new int[5];

        public string Time1
        {
            get { return time1; }
            set
            {
                if (time1 != value)
                {
                    time1 = value;
                    OnPropertyChanged("Time1");
                }
            }
        }

        public string Time2
        {
            get { return time2; }
            set
            {
                if (time2 != value)
                {
                    time2 = value;
                    OnPropertyChanged("Time2");
                }
            }
        }

        public string Time3
        {
            get { return time3; }
            set
            {
                if (time3 != value)
                {
                    time3 = value;
                    OnPropertyChanged("Time3");
                }
            }
        }

        public string Time4
        {
            get { return time4; }
            set
            {
                if (time4 != value)
                {
                    time4 = value;
                    OnPropertyChanged("Time4");
                }
            }
        }

        public string Time5
        {
            get { return time5; }
            set
            {
                if (time5 != value)
                {
                    time5 = value;
                    OnPropertyChanged("Time5");
                }
            }
        }

        public ObservableCollection<Entity> EntityList { get; set; }
        public MyICommand<Grid> LoadedCommand { get; set; }

        public MyICommand<ComboBox> SelectionChangedCommand { get; set; }

        public MyICommand<Grid> ShowCommand { get; set; }

        public MyICommand<Grid> TextChangedCommand { get; set; }

        public MyICommand<UserControl> UCLoadedCommand { get; set; }

        public static TextBox UpdateTextBox { get; set; }

        public MeasurementGraphViewModel()
        {
            EntityList = new ObservableCollection<Entity>();
            foreach (Entity e in NetworkEntitiesViewModel.Entities)
            {
                EntityList.Add(e);
            }
            LoadedCommand = new MyICommand<Grid>(OnLoaded);
            SelectionChangedCommand = new MyICommand<ComboBox>(OnSelectionChanged);
            ShowCommand = new MyICommand<Grid>(OnShow, CanShow);
            TextChangedCommand = new MyICommand<Grid>(OnTextChanged);
            UCLoadedCommand = new MyICommand<UserControl>(OnUCLoad);
        }

        private void OnUCLoad(UserControl uc)
        {
            uc.Focus();
        }

        public  string UpdateText
        {
            get { return updateText; }
            set
            {
                if (updateText != value)
                {
                    updateText = value;
                    OnPropertyChanged("UpdateText");
                }
            }
        }

        private void OnLoaded(Grid mainGrid)
        {
            TextBox tb = (TextBox)mainGrid.Children[0];
            UpdateTextBox = tb;
            if(selectedEntity != null && EntityList.Contains(selectedEntity))
            {
                ComboBox cb = (ComboBox)mainGrid.Children[1];
                cb.SelectedItem = selectedEntity;
                Grid g = (Grid)mainGrid.Children[2];
                CreateChart(g);
            }
        }

        private void OnSelectionChanged(ComboBox cb)
        {
            selectedEntity = (Entity)cb.SelectedItem;
            ShowCommand.RaiseCanExecuteChanged();
        }

        private void PlaceSingleColumn(Grid grid, Color color, int height, int colNum, int maxHeight, int br, Visibility isVisible)
        {
            Brush brush = new SolidColorBrush(color);
            Ellipse e = new Ellipse();
            e.Fill = brush;
            e.Width = 30;
            e.Height = 30;
            e.Visibility = isVisible;
            Grid.SetColumn(e, colNum);
            Grid.SetRow(e, maxHeight - height - 22);
            Grid.SetRowSpan(e, 20);
            grid.Children.Add(e);
            TextBlock tb = new TextBlock();
            tb.Text = br.ToString();
            tb.FontSize = 14;
            tb.FontWeight = FontWeights.Bold;
            tb.Foreground = new SolidColorBrush(Colors.White);
            int left = 52;
            if (br < 10)
                left = 55;
            if (br == 100)
                left = 47;
            tb.Margin = new Thickness(left, -58, 0, 0);
            tb.Visibility = isVisible;
            Grid.SetColumn(tb, colNum);
            Grid.SetRow(tb, maxHeight - height);
            Grid.SetRowSpan(tb, 20);
            grid.Children.Add(tb);
        }

        private void CreateChart(Grid myGrid)
        {
            GetValues();
            for (int i = 0; i < 124; i++)
            {
                RowDefinition rowDef = new RowDefinition();
                myGrid.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < 5; i++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                myGrid.ColumnDefinitions.Add(colDef);
            }

            int br = 0;


            for (int i = 0; i < CountValues(); i++)
            {

                Color color = (values[i] < 45 || values[i] > 74) ? Colors.Red : Colors.Blue;

                PlaceSingleColumn(myGrid, color, values[i], i, 124, values[i], Visibility.Visible);
                br++;
            }
            while(br < 5)
            {
                PlaceSingleColumn(myGrid, Colors.Red, 0, br, 124, 0, Visibility.Hidden);
                SetTime(br, "NO TIME");
                br++;
            }
            chartCreated = true;
        }

        private void UpdateChart(Grid myGrid)
        {
            GetValues();
            int br = 0;
            for(int i = 0; i < CountValues(); i++)
            {
                Ellipse e = (Ellipse)myGrid.Children[2 * i];
                TextBlock tb = (TextBlock)myGrid.Children[2 * i + 1];
                Color color = (values[i] < 45 || values[i] > 74) ? Colors.Red : Colors.Blue;
                UpdateColumn(e,tb,color,values[i],i,124,values[i],Visibility.Visible);
                br++;
            }
            while(br < 5)
            {
                TextBlock tb = (TextBlock)myGrid.Children[2 * br + 1];
                Ellipse e = (Ellipse)myGrid.Children[2 * br];
                UpdateColumn(e,tb, Colors.Red, 0, br, 124, 0, Visibility.Hidden);
                SetTime(br, "NO TIME");
                br++;
            }
        }

        private void UpdateColumn(Ellipse e,TextBlock tb, Color color ,int height, int colNum, int maxHeight, int br, Visibility isVisible)
        {
            e.Visibility = isVisible;
            Brush brush = new SolidColorBrush(color);
            e.Fill = brush;
            Grid.SetColumn(e, colNum);
            Grid.SetRow(e, maxHeight - height - 22);
            tb.Text = br.ToString();
            int left = 52;
            if (br < 10)
                left = 55;
            if (br == 100)
                left = 47;
            tb.Margin = new Thickness(left, -58, 0, 0);
            tb.Visibility = isVisible;
            Grid.SetColumn(tb, colNum);
            Grid.SetRow(tb, maxHeight - height);
        }

        private void OnShow(Grid myGrid)
        {
            if (!chartCreated)
            {
                CreateChart(myGrid);
            } else
            {
                UpdateChart(myGrid);
            }
        }

        private bool CanShow(Grid g)
        {
            return selectedEntity != null;
        }

        private void GetValues()
        {
            if(File.Exists(filePath))
            {
                Time1 = "";
                Time2 = "";
                Time3 = "";
                Time4 = "";
                Time5 = "";
                using (StreamReader sr = new StreamReader(filePath))
                {
                    int i = 0;
                    string line = sr.ReadLine();
                    while(line != null)
                    {
                        // 14.01.2022 10:13:09 AM : Client 2 , 13
                        string[] res = line.Split(' ');
                        if(String.Equals(res[4],selectedEntity.Name) && int.Parse(res[5]) == selectedEntity.Id)
                        {
                            if (i == 5)
                            {
                                Time1 = Time2;
                                Time2 = Time3;
                                Time3 = Time4;
                                Time4 = Time5;
                                //times[0] = times[1];
                                //times[1] = times[2];
                                //times[2] = times[3];
                                values[0] = values[1];
                                values[1] = values[2];
                                values[2] = values[3];
                                values[3] = values[4];
                                SetTime(4, res[0] + " " + res[1] + " " + res[2]);
                                //times[3] = res[0] + " " + res[1] + " " + res[2];
                                values[4] = int.Parse(res[7]);

                            } else
                            {
                                SetTime(i, res[0] + " " + res[1] + " " + res[2]);
                                //times[i] = res[0] + " " + res[1] + " " + res[2];
                                values[i] = int.Parse(res[7]);
                                i++;
                            }

                        }

                        line = sr.ReadLine();
                    }
                }
            }
        }

        private int CountValues()
        {
            int n1 = String.IsNullOrEmpty(Time1) ? 0 : 1;
            int n2 = String.IsNullOrEmpty(Time2) ? 0 : 1;
            int n3 = String.IsNullOrEmpty(Time3) ? 0 : 1;
            int n4 = String.IsNullOrEmpty(Time4) ? 0 : 1;
            int n5 = String.IsNullOrEmpty(Time5) ? 0 : 1;
            return n1 + n2 + n3 + n4 + n5;
        }

        private void SetTime(int i, string s)
        {
            switch (i)
            {
                case 0:
                    Time1 = s;
                    break;
                case 1:
                    Time2 = s;
                    break;
                case 2:
                    Time3 = s;
                    break;
                case 3:
                    Time4 = s;
                    break;
                case 4:
                    Time5 = s;
                    break;
            }
        }

        private void OnTextChanged(Grid myGrid)
        {
            if(chartCreated && selectedEntity != null)
            {
                UpdateChart(myGrid);
            }
        }

        

    }
}
