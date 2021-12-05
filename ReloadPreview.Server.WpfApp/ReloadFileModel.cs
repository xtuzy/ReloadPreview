using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReloadPreview.Server.WpfApp
{
    public class ReloadFileModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 需要监视的文件路径
        /// </summary>
        public string Path { set; get; }
        /// <summary>
        /// 为该Reload文件设置的端口
        /// </summary>
        public string Port { set; get; }
        bool state;
        /// <summary>
        /// 该监视执行的状态,即是否已经开启监视
        /// </summary>
        [JsonIgnore]
        public bool State
        {
            set
            {
                state = value;
                NotifyPropertyChanged("State");//https://stackoverflow.com/questions/45382997/listview-not-updating-on-propertychange
            }

            get
            {
                return state;
            }
        }

        int linkCount;
        /// <summary>
        /// 是否连接
        /// </summary>
        [JsonIgnore]
        public int LinkCount 
        {
            set
            {
                linkCount = value;
                NotifyPropertyChanged("LinkCount");//https://stackoverflow.com/questions/45382997/listview-not-updating-on-propertychange
            }

            get
            {
                return linkCount;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
