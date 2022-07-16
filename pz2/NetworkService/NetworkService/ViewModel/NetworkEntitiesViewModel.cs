using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        public ObservableCollection<string> NameList { get; set; }

        public ObservableCollection<string> HistoryList { get; set; }

        public static ObservableCollection<Entity> Entities { get; set; }

        public  ObservableCollection<Entity> FilteredEntities { get; set; }

        public MyICommand AddEntityCommand { get; set; }

        public MyICommand DeleteEntityCommand { get; set; }

        public MyICommand FilterEntitiesCommand { get; set; }

        public MyICommand LowerThanCheckedCommand { get; set; }

        public MyICommand GreaterThanCheckedCommand { get; set; }

        public MyICommand AddFilterCommand { get; set; }

        public MyICommand RemoveFilterCommand { get; set; }

        public MyICommand<UserControl> UCLoadedCommand { get; set; }

        private Entity currentEntity = new Entity();

        private Filter currentFilter = new Filter();

        private int selectedIndex = -1;

        private bool lowerThan = true;
        
        public NetworkEntitiesViewModel()
        {
            NameList = new ObservableCollection<string>() {"Server", "Client", "DNS" };
            HistoryList = new ObservableCollection<string>();
            if(Entities == null)
                Entities = new ObservableCollection<Entity>();
            FilteredEntities = new ObservableCollection<Entity>();
            Transfer(FilteredEntities, Entities);
            AddEntityCommand = new MyICommand(OnAdd);
            DeleteEntityCommand = new MyICommand(OnDelete, CanDelete);
            FilterEntitiesCommand = new MyICommand(OnFilter);
            LowerThanCheckedCommand = new MyICommand(OnLowerThanChecked);
            GreaterThanCheckedCommand = new MyICommand(OnGreaterThanChecked);
            AddFilterCommand = new MyICommand(OnAddFilter);
            RemoveFilterCommand = new MyICommand(OnRemoveFilter);
            UCLoadedCommand = new MyICommand<UserControl>(OnUCLoad);
        }

        private void OnUCLoad(UserControl uc)
        {
            uc.Focus();
        }

        public Entity CurrentEntity
        {
            get { return currentEntity; }
            set
            {
                currentEntity = value;
                OnPropertyChanged("CurrentEntity");
            }
        }

        public Filter CurrentFilter
        {
            get { return currentFilter; }
            set
            {
                currentFilter = value;
                OnPropertyChanged("CurrentFilter");
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                    DeleteEntityCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public void OnAdd()
        {
            CurrentEntity.Validate();
            if (CurrentEntity.IsValid)
            {
                Entities.Add(new Entity(CurrentEntity.Name));
                Transfer(FilteredEntities, Entities);
            }
        }
        private bool CanDelete()
        {
            return selectedIndex != -1;
        }

        private void OnDelete()
        {
            int id = FilteredEntities[SelectedIndex].Id;
            for(int i = 0; i < Entities.Count; i++)
            {
                if(Entities[i].Id == id)
                {
                    Entities.RemoveAt(i);
                    break;
                }
            }
            //Entities.RemoveAt(SelectedIndex);
            Transfer(FilteredEntities, Entities);
        }

        private void OnFilter()
        {
            try
            {
                currentFilter.Validate();
                if (currentFilter.IsValid)
                {
                    Transfer(FilteredEntities, Entities);
                    if (!string.IsNullOrWhiteSpace(currentFilter.History))
                    {
                        string id_pom;
                        string name;
                        bool lower;
                        GetNameAndId(currentFilter.History, out name, out id_pom, out lower);
                        if (!string.IsNullOrWhiteSpace(id_pom))
                        {
                            int id = int.Parse(id_pom);
                            for (int i = 0; i < FilteredEntities.Count; i++)
                            {
                                if (lower)
                                {
                                    if (FilteredEntities[i].Id >= id)
                                    {
                                        FilteredEntities.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    if (FilteredEntities[i].Id <= id)
                                    {
                                        FilteredEntities.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            for (int i = 0; i < FilteredEntities.Count; i++)
                            {
                                if (!String.Equals(name, FilteredEntities[i].Name))
                                {
                                    FilteredEntities.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                        currentFilter.Name = null;
                        currentFilter.Id = "";
                    }
                    else
                    {

                        if (!string.IsNullOrWhiteSpace(currentFilter.Id))
                        {
                            int id = int.Parse(currentFilter.Id);
                            for (int i = 0; i < FilteredEntities.Count; i++)
                            {
                                if (lowerThan)
                                {
                                    if (FilteredEntities[i].Id >= id)
                                    {
                                        FilteredEntities.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    if (FilteredEntities[i].Id <= id)
                                    {
                                        FilteredEntities.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(currentFilter.Name))
                        {
                            for (int i = 0; i < FilteredEntities.Count; i++)
                            {
                                if (!String.Equals(currentFilter.Name, FilteredEntities[i].Name))
                                {
                                    FilteredEntities.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void Transfer(ObservableCollection<Entity> first, ObservableCollection<Entity> second)
        {
            first.Clear();
            for(int i = 0; i < second.Count; i++)
            {
                first.Add(second[i]);
            }
        }

        private void OnLowerThanChecked()
        {
            lowerThan = true;
        }

        private void OnGreaterThanChecked()
        {
            lowerThan = false;
        }

        private void OnAddFilter()
        {
            currentFilter.Validate();
            if(currentFilter.IsValid)
            {
                string res = "";
                bool hasName = false;
                if (!string.IsNullOrWhiteSpace(currentFilter.Name))
                {
                    res += "Name: " + currentFilter.Name;
                    hasName = true;
                }

                if (!string.IsNullOrWhiteSpace(currentFilter.Id))
                {
                    if(hasName)
                    {
                        res += " , ";
                    }

                    res += "ID ";

                    if (lowerThan)
                    {
                        res += "< ";
                    } else
                    {
                        res += "> ";
                    }
                    res += currentFilter.Id;

                }
                if (!HistoryList.Contains(res) && !string.IsNullOrWhiteSpace(res))
                    HistoryList.Add(res);
            }
        }

        private void GetNameAndId(string input,out string name, out string id, out bool lower)
        {
            name = "";
            id = "";
            lower = false;
            char[] delimiters = new char[] { ' '};
            string[] res = input.Split(delimiters);
            if(String.Equals(res[0],"Name:"))
            {
                // Name: Server , ID < 4
                name = res[1];
                if(res.Length == 6)
                {
                    id = res[5];
                    if(String.Equals(res[4],"<"))
                    {
                        lower = true;
                    }
                }
            } else
            {
                id = res[2];
                if (String.Equals(res[1], "<"))
                {
                    lower = true;
                }
            }

        }

        private void OnRemoveFilter()
        {
            Transfer(FilteredEntities, Entities);
            currentFilter.Name = null;
            currentFilter.Id = "";
            currentFilter.History = null;
        }

    }
}
