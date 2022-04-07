using CSharpZapoctak.Others;
using CSharpZapoctak.ViewModels;

namespace CSharpZapoctak.Models
{
    public class Competition : NotifyPropertyChanged, IHasImage, IEntity
    {
        public int ID { get; set; } = SportsData.NOID;

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string info;
        public string Info
        {
            get => info;
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        private string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                OnPropertyChanged();
            }
        }
    }
}
