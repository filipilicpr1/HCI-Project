using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Filter : ValidationBase
    {

        private string id;
        private string name;
        private string history;

        public string Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string History
        {
            get { return history; }
            set
            {
                if (history != value)
                {
                    history = value;
                    OnPropertyChanged("History");
                }
            }
        }

        protected override void ValidateSelf()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.id) && string.IsNullOrWhiteSpace(this.history))
                {
                    int.Parse(this.id);
                }
            }
            catch
            {
                this.ValidationErrors["Id"] = "Id must be an integer";
            }
        }
    }
}
