using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Entity : ValidationBase
    {
        private static int count = 0;

        private int id;
        private string name;
        private string ipAddress;
        private string image;
        private double val;

        public int Id
        {
            get { return id; } 
            set
            {
                if(id != value)
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
                if(name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string IpAddress
        {
            get { return ipAddress; }
            set
            {
                if (ipAddress != value)
                {
                    ipAddress = value;
                    OnPropertyChanged("IpAddress");
                }
            }
        }

        public string Image
        {
            get { return image; }
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        public double Val
        {
            get { return val; }
            set
            {
                if (val != value)
                {
                    val = value;
                    OnPropertyChanged("Val");
                }
            }
        }

        public Entity()
        {

        }

        public Entity(string name)
        {
            this.name = name;
            count++;
            this.id = count;
            this.ipAddress = "192.168.0." + id.ToString();
            this.image = "pack://application:,,,/NetworkService;component/Images/" + name + ".png";
        }

        protected override void ValidateSelf()
        {
            if (string.IsNullOrWhiteSpace(this.name))
            {
                this.ValidationErrors["Name"] = "Name is required";
            }
        }
    }
}
