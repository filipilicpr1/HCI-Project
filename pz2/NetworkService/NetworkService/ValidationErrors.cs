using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService
{
    public class ValidationErrors : BindableBase
    {
        private readonly Dictionary<string, string> validationErrors = new Dictionary<string, string>();

        public bool IsValid
        {
            get
            {
                return validationErrors.Count < 1; // ako je validan, nema nikakvu gresku
            }
        }

        // prosledimo naziv propertija, pa gledamo da li u recniku postoji taj property , a to znaci da taj propery nije validan
        public string this[string fieldName]
        {
            get
            {
                return this.validationErrors.ContainsKey(fieldName) ? this.validationErrors[fieldName] : string.Empty;
            }

            set
            {
                if (this.validationErrors.ContainsKey(fieldName))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        this.validationErrors.Remove(fieldName);  // ako je poruka o gresci prazan string, onda to znaci da je property validan i izbacujemo ga
                    }
                    else
                    {
                        this.validationErrors[fieldName] = value;
                    }
                }
                else
                {
                    // polje nije u recniku
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        this.validationErrors.Add(fieldName, value);
                    }
                }
                this.OnPropertyChanged("IsValid");
            }
        }

        public void Clear()
        {
            validationErrors.Clear();
        }

    }
}
