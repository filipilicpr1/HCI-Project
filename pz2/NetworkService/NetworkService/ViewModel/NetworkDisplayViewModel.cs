using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        private bool dragging = false;
        private Entity draggedItem = null;
        private Canvas sourceCanvas = null;
        private const int canvasCount = 16;
        private static Dictionary<int, Entity> takenCanvases = new Dictionary<int, Entity>();
        private bool connecting = false;
        private Canvas connectingCanvas = null;
        private static List<MyLine> existingLines = new List<MyLine>();
        public static TextBox UpdateTextBox { get; set; }
        public ObservableCollection<Entity> EntityList { get; set; } 

        public MyICommand<ListBox> SelectionChangedCommand { get; set; }

        public MyICommand<Canvas> MouseLeftButtonUpCommand { get; set; }

        public MyICommand<Canvas> CanvasDropCommand { get; set; }

        public MyICommand ListBoxDropCommand { get; set; }

        public MyICommand<Canvas> MouseLeftButtonDownCommand { get; set; }

        public MyICommand<Canvas> AutoPlaceEntitiesCommand { get; set; }

        public MyICommand<Canvas> LoadedCommand { get; set; }

        public MyICommand<ToggleButton> ConnectCommand { get; set; }

        public MyICommand<UserControl> UCLoadedCommand { get; set; }

        public MyICommand<Grid> GridLoadedCommand { get; set; }

        public MyICommand<Canvas> TextChangedCommand { get; set; }

        public NetworkDisplayViewModel()
        {
            EntityList = new ObservableCollection<Entity>();
            foreach(Entity e in NetworkEntitiesViewModel.Entities)
            {
                EntityList.Add(e);
            }
            SelectionChangedCommand = new MyICommand<ListBox>(OnSelectionChanged);
            MouseLeftButtonUpCommand = new MyICommand<Canvas>(OnMouseLeftButtonUp);
            CanvasDropCommand = new MyICommand<Canvas>(OnCanvasDrop);
            ListBoxDropCommand = new MyICommand(OnListBoxDrop);
            MouseLeftButtonDownCommand = new MyICommand<Canvas>(OnMouseLeftButtonDown);
            AutoPlaceEntitiesCommand = new MyICommand<Canvas>(OnAutoPlaceEntities);
            LoadedCommand = new MyICommand<Canvas>(OnLoaded);
            ConnectCommand = new MyICommand<ToggleButton>(OnConnect);
            UCLoadedCommand = new MyICommand<UserControl>(OnUCLoad);
            GridLoadedCommand = new MyICommand<Grid>(OnGridLoad);
            TextChangedCommand = new MyICommand<Canvas>(OnTextChanged);
        }

        private void OnGridLoad(Grid g)
        {
            TextBox tb = (TextBox)g.Children[5];
            UpdateTextBox = tb;
        }

        private void OnUCLoad(UserControl uc)
        {
            uc.Focus();
        }

        private void OnSelectionChanged(ListBox elName)
        {
            if(!dragging)
            {
                dragging = true;
                draggedItem = (Entity)elName.SelectedItem;
                DragDrop.DoDragDrop(elName.Parent, draggedItem, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void OnMouseLeftButtonUp(Canvas elName)
        {
            if (connecting && connectingCanvas != null && elName != null && elName.Resources["taken"] != null
                && NotConnected(FindIndex(elName),FindIndex(connectingCanvas)) && FindIndex(elName) != FindIndex(connectingCanvas))
            {
                int ind1 = FindIndex(connectingCanvas);
                int ind2 = FindIndex(elName);
                Border b = (Border)elName.Parent;
                Canvas mc = (Canvas)b.Parent;
                MyLine newLine = new MyLine(ind1, ind2);
                mc.Children.Add(newLine.Line);
                existingLines.Add(newLine);
            }
            else
            {
                dragging = false;
                draggedItem = null;
                sourceCanvas = null;
            }
        }

        private void OnCanvasDrop(Canvas elName)
        {
            if(draggedItem != null)
            {
                if(elName.Resources["taken"] == null)
                {
                    BitmapImage background = new BitmapImage();
                    background.BeginInit();
                    background.UriSource = new Uri(draggedItem.Image);
                    background.EndInit();
                    elName.Background = new ImageBrush(background);
                    ((TextBlock)elName.Children[0]).Text = draggedItem.Name + " " + draggedItem.Id;
                    elName.Resources.Add("taken", true);
                    ((ListBox)elName.Children[1]).Items.Add(draggedItem);
                    takenCanvases.Add(FindIndex(elName), draggedItem);
                    if (draggedItem.Val < 45 || draggedItem.Val > 75)
                    {
                        ((TextBlock)elName.Children[2]).Text = "INVALID";
                    }
                    if (sourceCanvas != null)
                    {
                        sourceCanvas.Resources.Remove("taken");
                        ((ListBox)sourceCanvas.Children[1]).Items.Remove(draggedItem);
                        ((TextBlock)sourceCanvas.Children[0]).Text = "";
                        ((TextBlock)sourceCanvas.Children[2]).Text = "";
                        sourceCanvas.Background = Brushes.GhostWhite;
                        takenCanvases.Remove(FindIndex(sourceCanvas));
                        UpdateLines(sourceCanvas, FindIndex(elName));
                    } else
                    {
                        EntityList.Remove(draggedItem);
                    }
                }
                sourceCanvas = null;
                dragging = false;
                draggedItem = null;
            }
        }

        private void OnListBoxDrop()
        {
            if (draggedItem != null && sourceCanvas != null)
            {
                EntityList.Add(draggedItem);
                sourceCanvas.Resources.Remove("taken");
                ((ListBox)sourceCanvas.Children[1]).Items.Remove(draggedItem);
                ((TextBlock)sourceCanvas.Children[0]).Text = "";
                ((TextBlock)sourceCanvas.Children[2]).Text = "";
                sourceCanvas.Background = Brushes.GhostWhite;
                takenCanvases.Remove(FindIndex(sourceCanvas));
                RemoveLines(sourceCanvas);
                sourceCanvas = null;
                dragging = false;
                draggedItem = null;

            }
        }

        private void OnMouseLeftButtonDown(Canvas elName)
        {
            if(connecting && elName.Resources["taken"] != null)
            {
                connectingCanvas = elName;
            }
            else if (!dragging && elName.Resources["taken"] != null)
            {
                dragging = true;
                draggedItem = (Entity)((ListBox)elName.Children[1]).Items[0];
                sourceCanvas = elName;
                DragDrop.DoDragDrop(elName.Parent, draggedItem, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void OnAutoPlaceEntities(Canvas elName)
        {
            bool hasRoom = true;
            while(EntityList.Count != 0 && hasRoom)
            {
                Entity e = EntityList[0];
                hasRoom = false;
                for(int i = 0; i < canvasCount; i++)
                {
                    Border b = (Border)elName.Children[i];
                    Canvas c = (Canvas)b.Child;
                    if (c.Resources["taken"] == null)
                    {
                        hasRoom = true;
                        BitmapImage background = new BitmapImage();
                        background.BeginInit();
                        background.UriSource = new Uri(e.Image);
                        background.EndInit();
                        c.Background = new ImageBrush(background);
                        ((TextBlock)c.Children[0]).Text = e.Name + " " + e.Id;
                        if (e.Val < 45 || e.Val > 75)
                        {
                            ((TextBlock)c.Children[2]).Text = "INVALID";
                        }
                        c.Resources.Add("taken", true);
                        ((ListBox)c.Children[1]).Items.Add(e);
                        takenCanvases.Add(FindIndex(c), e);
                        break;
                    }
                }
                EntityList.Remove(e);
            }
        }

        private void OnLoaded(Canvas elName)
        {
            List<int> removeList = new List<int>();
            for(int i = 0; i < canvasCount; i++)
            {
                Border b = (Border)elName.Children[i];
                Canvas c1 = (Canvas)b.Child;
                foreach (int ind in takenCanvases.Keys)
                {
                    if (i == ind)
                    {
                        Entity e = takenCanvases[i];
                        if (EntityList.Contains(e))
                        {
                            BitmapImage background = new BitmapImage();
                            background.BeginInit();
                            background.UriSource = new Uri(e.Image);
                            background.EndInit();
                            c1.Background = new ImageBrush(background);
                            ((TextBlock)c1.Children[0]).Text = e.Name + " " + e.Id;
                            if (e.Val < 45 || e.Val > 75)
                            {
                                ((TextBlock)c1.Children[2]).Text = "INVALID";
                            }
                            if (c1.Resources["taken"] == null)
                            {
                                c1.Resources.Add("taken", true);
                            }
                            ((ListBox)c1.Children[1]).Items.Add(e);
                            EntityList.Remove(e);
                            break;
                        }
                        else
                        {
                            removeList.Add(ind);
                        }
                    }
                }
                foreach(int ind in removeList)
                {
                    takenCanvases.Remove(ind);
                }
            }
            

            List<MyLine> removeLineList = new List<MyLine>();
            List<MyLine> replacedLineList = new List<MyLine>();
            List<MyLine> replacingLineList = new List<MyLine>();
            foreach (MyLine ml in existingLines)
            {
                if(elName.Children.Contains(ml.Line))
                {
                    elName.Children.Remove(ml.Line);
                }
                Canvas c1 = FindCanvas(elName, ml.Ind1);
                Canvas c2 = FindCanvas(elName, ml.Ind2);
                if (((ListBox)c1.Children[1]).Items.Count != 0 && ((ListBox)c2.Children[1]).Items.Count != 0)
                {
                    MyLine nl = new MyLine(ml.Ind1, ml.Ind2);
                    replacedLineList.Add(ml);
                    replacingLineList.Add(nl);
                    elName.Children.Add(nl.Line);
                } 
                else
                {
                    removeLineList.Add(ml);
                }
            }
            
            foreach(MyLine ml in replacedLineList)
            {
                existingLines.Remove(ml);
            }

            foreach(MyLine ml in replacingLineList)
            {
                existingLines.Add(ml);
            }

            foreach(MyLine ml in removeLineList)
            {
                existingLines.Remove(ml);
            }

        }

        private int FindIndex(Canvas c)
        {
            Border b = (Border)c.Parent;
            Canvas mc = (Canvas)b.Parent;
            for(int i = 0; i < canvasCount; i++)
            {
                if(b.Equals(mc.Children[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private Canvas FindCanvas(Canvas mc,int id)
        {
            if(id < 0 || id > canvasCount)
            {
                return null;
            }
            Border b = (Border)mc.Children[id];
            Canvas c = (Canvas)b.Child;
            return c;
        }

        private void OnConnect(ToggleButton elName)
        {
            if(connecting)
            {
                connecting = false;
                elName.Foreground = Brushes.Black;
                elName.Content = "CONNECT";
            } else
            {
                connecting = true;
                elName.Foreground = Brushes.Blue;
                elName.Content = "CONNECTING";
            }
        }

        private void UpdateLines(Canvas elName, int newInd)
        {
            int ind = FindIndex(elName);
            Border b = (Border)elName.Parent;
            Canvas mc = (Canvas)b.Parent;
            foreach(MyLine ml in existingLines)
            {
                if(ml.Ind1 == ind || ml.Ind2 == ind)
                {
                    if(ml.Ind1 == ind)
                    {
                        ml.Ind1 = newInd;
                    } else
                    {
                        ml.Ind2 = newInd;
                    }
                    mc.Children.Remove(ml.Line);
                    ml.UpdateLine();
                    mc.Children.Add(ml.Line);
                }

            }

        }

        private bool NotConnected(int ind1, int ind2)
        {
            foreach(MyLine ml in existingLines)
            {
                if(ml.Ind1 == ind1 && ml.Ind2 == ind2)
                {
                    return false;
                }

                if (ml.Ind1 == ind2 && ml.Ind2 == ind1)
                {
                    return false;
                }
            }

            return true;
        }

        private void RemoveLines(Canvas elName)
        {
            int ind = FindIndex(elName);
            Border b = (Border)elName.Parent;
            Canvas mc = (Canvas)b.Parent;
            List<MyLine> removeList = new List<MyLine>();
            foreach(MyLine ml in existingLines)
            {
                if(ml.Ind1 == ind || ml.Ind2 == ind)
                {
                    mc.Children.Remove(ml.Line);
                    removeList.Add(ml);

                }

            }

            foreach(MyLine ml in removeList)
            {
                existingLines.Remove(ml);
            }

        }

        private void OnTextChanged(Canvas elName)
        {
            foreach(int i in takenCanvases.Keys)
            {
                Border b = (Border)elName.Children[i];
                Canvas c1 = (Canvas)b.Child;
                Entity e = takenCanvases[i];
                if (e.Val < 45 || e.Val > 75)
                {
                    ((TextBlock)c1.Children[2]).Text = "INVALID";
                } else
                {
                    ((TextBlock)c1.Children[2]).Text = "";
                }
            }
        }

    }
}
