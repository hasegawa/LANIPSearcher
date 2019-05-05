using System;
using System.Collections.Generic;

namespace LANIPSearcher.Bean
{
    class LANSearchBean
    {
        private List<string> _selfIPList;
        private List<string> _allLANIPList;

        public LANSearchBean()
        {
            this._selfIPList = new List<string>();
            this._allLANIPList = new List<string>();
        }

        public string ShowText
        {
            get
            {
                // ローカルマシンのIPリストと突合
                this.SelfIPList.ForEach(
                    x =>
                    {
                        int index = this._allLANIPList.IndexOf(x);
                        if (index > -1)
                        {
                            this._allLANIPList.RemoveAt(index);
                            this._allLANIPList.Insert(index, x + " (this PC)");
                        }
                    });

                return string.Join(Environment.NewLine, this._allLANIPList);
            }
        }

        public List<string> SelfIPList { get { return this._selfIPList; } set { this._selfIPList = value; } }

        public List<string> AllLANIPList { get { return this._allLANIPList; } set { this._allLANIPList = value; } }

        public void AddSelfIP(string str)
        {
            this.SelfIPList.Add(str);
        }

        public void AddLANIP(string str)
        {
            this._allLANIPList.Add(str);
        }


    }
}
