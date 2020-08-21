using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace SearchDBF.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private DataTable _dtcurrent;
        public DataTable DTCurrent
        {
            get { return _dtcurrent; }
            set { _dtcurrent = value; OnPropertyChanged("DTCurrent"); }
        }
        private bool _isvisiblelist;

        public bool IsVisibleList
        {
            get { return _isvisiblelist; }
            set { _isvisiblelist = value; OnPropertyChanged("IsVisibleList"); }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; OnPropertyChanged("Path"); }
        }

        private string _selectfind = "";
        public string SelectFind
        {
            get { return _selectfind; }
            set { _selectfind = value; OnPropertyChanged("SelectFind"); }
        }


        private bool _isclickenabled = true;
        public bool IsClickEnabled
        {
            get { return _isclickenabled; }
            set { _isclickenabled = value; IsVisibleAnimation=!_isclickenabled; OnPropertyChanged("IsClickEnabled"); }
        }

        private bool _isvisibleanimation = true;
        public bool IsVisibleAnimation
        {
            get { return _isvisibleanimation; }
            set { _isvisibleanimation = value; OnPropertyChanged("IsVisibleAnimation"); }
        }

        private string _findtext;
        public string FindText
        {
            get { return _findtext; }
            set { _findtext = value; OnPropertyChanged("FindText"); }
        }

        private ObservableCollection<string> _finds;
        public ObservableCollection<string> Finds
        {
            get { return _finds; }
            set { _finds = value; OnPropertyChanged("Finds"); }
        }

        private ObservableCollection<string> _files;
        public ObservableCollection<string> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        private ObservableCollection<int> _listindex;
        public ObservableCollection<int> ListIndex
        {
            get { return _listindex; }
            set { _listindex = value; }
        }

        private WorkDBF _dbf;
        public WorkDBF DBF
        {
            get { return _dbf; }
            set { _dbf = value; OnPropertyChanged("DBF"); }
        }

        public MainViewModel()
        {
            _files = new ObservableCollection<string>();
            _finds = new ObservableCollection<string>();
            _dtcurrent = new DataTable();
            _listindex = new ObservableCollection<int>();
            if (!string.IsNullOrWhiteSpace(SearchDBF.Properties.Settings.Default.pathDBF))
                GetListDBFAsync(SearchDBF.Properties.Settings.Default.pathDBF);

        }

        public ICommand OpenCommand => new Command(obj =>
      {
          if (obj != null)
          {
              FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
              folderBrowser.SelectedPath = obj.ToString();
              DialogResult result = folderBrowser.ShowDialog();

              if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
              {
                  SearchDBF.Properties.Settings.Default.pathDBF = folderBrowser.SelectedPath;
                  GetListDBFAsync(folderBrowser.SelectedPath);
              }
          }
      }, (obj) => { return IsClickEnabled; });

        public ICommand SearchCommand => new Command((obj) =>
        {
            SearchAsync(obj.ToString());

        }, (obj) => { return (Files.Count != 0 && IsClickEnabled && obj != null && obj.ToString() != ""); });

        public ICommand ReadCommand => new Command((obj) =>
        {
            ReadDataAsync(obj.ToString());

        }, (obj) => { return (Files.Count != 0 && IsClickEnabled && FindText != null && FindText != ""); });



        public async void GetListDBFAsync(string path)
        {
            IsClickEnabled = false;
            IsVisibleList = false;
            DBF = new WorkDBF(path);
            List<string> ls = await DBF.GetFilesAsync(path);
            _files.Clear();
            _finds.Clear();
            foreach (string l in ls)
            {
                _files.Add(l);
            }
            IsClickEnabled = true;
        }

        public async void SearchAsync(string strfind)
        {
            IsVisibleList = false;
            _findtext = strfind;
            if (_findtext != null)
            {
                IsClickEnabled = false;
                _finds.Clear();
                _finds.Add($"Начало поиска: `{_findtext}`");
                foreach (string mfile in _files)
                {
                    if ((mfile.IndexOf("Файл:") != -1))
                    {
                        string filename = mfile.Substring(5).Trim();
                        int nIndex = await DBF.FindTextAsync(_findtext, filename);
                        if (nIndex > 0)
                        {
                            string numrecstr = $"позиция {nIndex} (всего {DBF.Indexes.Count}) ";
                            if ((mfile.ToLower().IndexOf(".dbf") != -1))
                            {
                                DBFReader dbffile = new DBFReader(filename, Encoding.GetEncoding("cp866"));
                                numrecstr = $"запись {dbffile.GetRecordNumber(nIndex)} (всего {DBF.Indexes.Count})";
                            }
                            _finds.Add($"{filename} - {numrecstr}");
                        }
                    }
                }
                _finds.Add("Конец");
                IsClickEnabled = true;
            }
        }

        public async void ReadDataAsync(string filename)
        {
            filename = filename.Substring(0, filename.IndexOf(" - ")).Trim();
            int nIndex = await DBF.FindTextAsync(_findtext, filename);

            DBFReader dbffile = new DBFReader(filename, Encoding.GetEncoding("cp866"));

            _listindex.Clear();
            foreach (int index in DBF.Indexes)
            {
                _listindex.Add(dbffile.IsDBFFile() ? dbffile.GetRecordNumber(index) : index  );
            }
            IsVisibleList = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
