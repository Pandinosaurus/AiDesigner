﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace DNN.net.dataset.common
{
    public interface IXDatasetCreator
    {
        string Name { get; }
        void QueryConfiguration(DatasetConfiguration config);
        void Create(DatasetConfiguration config, IXDatasetCreatorProgress progress);
    }

    public interface IXDatasetCreatorSettings
    {
        void VerifyConfiguration(DataConfigSetting[] settings);
        void GetCustomSetting(string strName, DataConfigSetting[] settings);
    }

    public interface IXDatasetCreatorProgress
    {
        void OnProgress(CreateProgressArgs args);
        void OnCompleted(CreateProgressArgs args);
    }

    [Serializable]
    public class DatasetConfiguration
    {
        string m_strName = "";
        int m_nID = 0;
        DataConfigSettingCollection m_rgSettings = new DataConfigSettingCollection();
        string m_strSelectedGroup = "";

        public DatasetConfiguration(string strName, int nID, string strSelectedGroup)
        {
            m_strName = strName;
            m_nID = nID;
            m_strSelectedGroup = strSelectedGroup;
        }

        public int ID
        {
            get { return m_nID; }
            set { m_nID = value; }
        }

        public string Name
        {
            get { return m_strName; }
        }

        public string SelectedGroup
        {
            get { return m_strSelectedGroup; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DataConfigSettingCollection Settings
        {
            get { return m_rgSettings; }
        }

        public void Sort()
        {
            m_rgSettings.Sort();
        }

        public DatasetConfiguration Clone()
        {
            DatasetConfiguration config = new DatasetConfiguration(m_strName, m_nID, m_strSelectedGroup);

            config.m_rgSettings = m_rgSettings.Clone();

            return config;
        }

        public static void LoadFromDirectory(DataConfigSetting[] settings, string strExt, string strPath = "c:\\temp\\configurations")
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.SelectedPath = strPath;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int nFirstGPHFile = -1;

                for (int i = 0; i < settings.Length; i++)
                {
                    if (settings[i].Name.Contains(strExt.ToUpper()))
                    {
                        if (nFirstGPHFile == -1)
                            nFirstGPHFile = i;

                        settings[i].Value = "";
                    }
                }

                string[] rgstrFiles = Directory.GetFiles(dlg.SelectedPath);
                int nIdx = 0;

                while (nIdx < rgstrFiles.Length && !rgstrFiles[nIdx].Contains("." + strExt.ToLower()))
                {
                    nIdx++;
                }

                for (int i = nFirstGPHFile; i < settings.Length; i++)
                {
                    if (nIdx < rgstrFiles.Length && rgstrFiles[nIdx].Contains("." + strExt.ToLower()))
                    {
                        settings[i].Value = rgstrFiles[nIdx];
                        nIdx++;
                    }

                    while (nIdx < rgstrFiles.Length && !rgstrFiles[nIdx].Contains("." + strExt.ToLower()))
                    {
                        nIdx++;
                    }

                    if (nIdx >= rgstrFiles.Length || !rgstrFiles[nIdx].Contains("." + strExt.ToLower()))
                        break;
                }
            }
        }
    }

    [Serializable]
    public class DataConfigSettingCollection : IEnumerable<DataConfigSetting>
    {
        List<DataConfigSetting> m_rgSettings = new List<DataConfigSetting>();

        public DataConfigSettingCollection()
        {
        }

        public void Sort()
        {
            m_rgSettings.Sort(new Comparison<DataConfigSetting>(sort));
        }

        private int sort(DataConfigSetting a, DataConfigSetting b)
        {
            return a.Name.CompareTo(b.Name);
        }

        public int Count
        {
            get { return m_rgSettings.Count; }
        }

        public DataConfigSetting[] Items
        {
            get { return m_rgSettings.ToArray(); }
            set
            {
                m_rgSettings.Clear();

                foreach (DataConfigSetting s in value)
                {
                    m_rgSettings.Add(s.Clone());
                }
            }
        }

        public DataConfigSetting this[int nIdx]
        {
            get { return m_rgSettings[nIdx]; }
            set { m_rgSettings[nIdx] = value; }
        }

        public void Add(DataConfigSetting s)
        {
            m_rgSettings.Add(s);
        }

        public bool Remove(DataConfigSetting s)
        {
            return m_rgSettings.Remove(s);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgSettings.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgSettings.Clear();
        }

        public static DataConfigSetting Find(DataConfigSetting[] rgSettings, string strName)
        {
            foreach (DataConfigSetting s in rgSettings)
            {
                if (s.Name == strName)
                    return s;
            }

            return null;
        }

        public DataConfigSetting Find(string strName)
        {
            return DataConfigSettingCollection.Find(m_rgSettings.ToArray(), strName);
        }

        public DataConfigSettingCollection Clone()
        {
            DataConfigSettingCollection col = new DataConfigSettingCollection();

            foreach (DataConfigSetting s in m_rgSettings)
            {
                col.Add(s.Clone());
            }

            return col;
        }

        public IEnumerator<DataConfigSetting> GetEnumerator()
        {
            return m_rgSettings.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_rgSettings.GetEnumerator();
        }
    }

    [Serializable]
    [EditorAttribute(typeof(DataConfigSettingEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class DataConfigSetting
    {
        string m_strName = "";
        object m_objValue = null;
        TYPE m_type = TYPE.TEXT;

        [Browsable(false)]
        string m_strExtra = "";

        [Browsable(false)]
        [NonSerialized]
        IXDatasetCreatorSettings m_iverify = null;


        public enum TYPE
        {
            TEXT,
            FILENAME,
            DIRECTORY,
            LIST,
            DATETIME,
            INTEGER,
            REAL,
            CUSTOM,
            HELP
        }

        public DataConfigSetting(string strName = "", object objValue = null, TYPE type = TYPE.TEXT, string strExtra = "", IXDatasetCreatorSettings iverify = null)
        {
            m_strName = strName;
            m_objValue = objValue;
            m_strExtra = strExtra;
            m_type = type;
            m_iverify = iverify;
        }

        public IXDatasetCreatorSettings VerifyInterface
        {
            get { return m_iverify; }
        }

        public string Name
        {
            get { return m_strName; }
        }

        public string Extra
        {
            get { return m_strExtra; }
        }

        public object Value
        {
            get { return m_objValue; }
            set { m_objValue = value; }
        }

        public TYPE Type
        {
            get { return m_type; }
        }

        public DataConfigSetting Clone()
        {
            return new DataConfigSetting(m_strName, m_objValue, m_type, m_strExtra, m_iverify);
        }

        public override string ToString()
        {
            return m_strName + ": " + m_objValue.ToString();
        }
    }

    public class OptionItem
    {
        string m_strName = "";
        int m_nIdx = 0;
        OptionItemList m_rgOptionItemList = new OptionItemList();

        public OptionItem(string strName, int nIdx, OptionItemList items = null)
        {
            m_strName = strName;
            m_nIdx = nIdx;

            if (items != null)
                m_rgOptionItemList = items;
        }

        public string Name
        {
            get { return m_strName; }
        }

        public int Index
        {
            get { return m_nIdx; }
        }

        public OptionItemList Options
        {
            get { return m_rgOptionItemList; }
            set { m_rgOptionItemList = value; }
        }

        public OptionItem Clone()
        {
            OptionItem item = new OptionItem(m_strName, m_nIdx, Options);
            return item;
        }

        public override string ToString()
        {
            return m_strName;
        }
    }

    public class OptionItemList : IEnumerable<OptionItem>
    {
        List<OptionItem> m_rgItems = new List<OptionItem>();

        public OptionItemList()
        {
        }

        public int Count
        {
            get { return m_rgItems.Count; }
        }

        public OptionItem this[int nIdx]
        {
            get { return m_rgItems[nIdx]; }
        }

        public OptionItem Find(string strItem)
        {
            foreach (OptionItem item in m_rgItems)
            {
                if (item.Name == strItem)
                    return item;
            }

            return null;
        }

        public OptionItemList Clone()
        {
            OptionItemList list = new OptionItemList();

            foreach (OptionItem item in m_rgItems)
            {
                list.Add(item);
            }

            return list;
        }

        public void Add(OptionItem item)
        {
            m_rgItems.Add(item);
        }

        public void Clear()
        {
            m_rgItems.Clear();
        }

        public IEnumerator<OptionItem> GetEnumerator()
        {
            return m_rgItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_rgItems.GetEnumerator();
        }
    }

    public class DataConfigSettingEditor : UITypeEditor
    {
        public DataConfigSettingEditor()
            : base()
        {
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.PropertyDescriptor != null)
            {
                int nIdx = getIndex(context.PropertyDescriptor.Name);
                DataConfigSetting[] config = context.Instance as DataConfigSetting[];

                if (config[nIdx].Type == DataConfigSetting.TYPE.FILENAME)
                    return UITypeEditorEditStyle.Modal;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.DIRECTORY)
                    return UITypeEditorEditStyle.Modal;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.CUSTOM)
                    return UITypeEditorEditStyle.Modal;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.LIST)
                    return UITypeEditorEditStyle.DropDown;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.DATETIME)
                    return UITypeEditorEditStyle.DropDown;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.INTEGER)
                    return UITypeEditorEditStyle.DropDown;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.REAL)
                    return UITypeEditorEditStyle.DropDown;
                else if (config[nIdx].Type == DataConfigSetting.TYPE.TEXT)
                    return UITypeEditorEditStyle.DropDown;
            }

            return UITypeEditorEditStyle.None;
        }

        private int getIndex(string strName)
        {
            strName = strName.Trim('[', ']');
            return int.Parse(strName);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            DataConfigSetting setting = value as DataConfigSetting;
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (setting.Type == DataConfigSetting.TYPE.FILENAME)
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Filter = "Data Files (*." + setting.Extra + ")|*." + setting.Extra + "||";
                dlg.Title = "Select the " + setting.Name;
                dlg.DefaultExt = setting.Extra;
                dlg.FileName = (string)setting.Value;

                if (dlg.ShowDialog() == DialogResult.OK)
                    setting.Value = dlg.FileName;
                else
                    setting.Value = "";
            }
            else if (setting.Type == DataConfigSetting.TYPE.DIRECTORY)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();

                dlg.RootFolder = Environment.SpecialFolder.MyComputer;
                dlg.SelectedPath = setting.Value.ToString();
                dlg.ShowNewFolderButton = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    setting.Value = dlg.SelectedPath;
            }
            else if (setting.Type == DataConfigSetting.TYPE.LIST)
            {
                if (edSvc != null)
                {
                    ListBox list = new ListBox();

                    list.SelectedIndexChanged += new EventHandler(list_SelectedIndexChanged);

                    OptionItem item = setting.Value as OptionItem;

                    foreach (OptionItem option in item.Options)
                    {
                        list.Items.Add(option.Name);
                    }

                    list.Tag = edSvc;
                    edSvc.DropDownControl(list);

                    if (list.SelectedItem != null)
                    {
                        OptionItem selectedItem = item.Options.Find(list.SelectedItem.ToString());
                        selectedItem.Options = item.Options;

                        setting.Value = selectedItem;
                    }
                }
            }
            else if (setting.Type == DataConfigSetting.TYPE.DATETIME)
            {
                if (edSvc != null)
                {
                    MonthCalendar calendar = new MonthCalendar();
                    string strDate = setting.Value.ToString();
                    DateTime dt = DateTime.Parse(strDate);

                    calendar.ShowTodayCircle = true;
                    calendar.ShowWeekNumbers = true;
                    calendar.MaxSelectionCount = 1;
                    calendar.SelectionStart = dt;
                    calendar.SelectionEnd = dt;

                    calendar.DateSelected += new DateRangeEventHandler(calendar_DateSelected);
                    calendar.Tag = edSvc;

                    edSvc.DropDownControl(calendar);

                    setting.Value = calendar.SelectionStart.ToShortDateString();
                }
            }
            else if (setting.Type == DataConfigSetting.TYPE.CUSTOM)
            {
                if (setting.VerifyInterface != null)
                {
                    setting.VerifyInterface.GetCustomSetting(setting.Name, (DataConfigSetting[])context.Instance);
                }
            }
            else
            {
                if (edSvc != null)
                {
                    TextBox edt = new TextBox();

                    edt.Text = setting.Value.ToString();
                    edt.Tag = edSvc;

                    edSvc.DropDownControl(edt);

                    if (setting.Type == DataConfigSetting.TYPE.INTEGER)
                    {
                        int nVal;

                        if (!int.TryParse(edt.Text, out nVal))
                            throw new Exception("The value specified for '" + setting.Name + "' is invalid.  Please enter a valid INTEGER number.");
                    }
                    else if (setting.Type == DataConfigSetting.TYPE.REAL)
                    {
                        double dfVal;

                        if (!double.TryParse(edt.Text, out dfVal))
                            throw new Exception("The value specified for '" + setting.Name + "' is invalid.  Please enter a valid REAL number.");
                    }

                    setting.Value = edt.Text;
                }
            }

            if (setting.VerifyInterface != null)
                setting.VerifyInterface.VerifyConfiguration((DataConfigSetting[])context.Instance);

            return value;
        }

        void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = sender as ListBox;
            IWindowsFormsEditorService edSvc = list.Tag as IWindowsFormsEditorService;

            edSvc.CloseDropDown();
        }

        void calendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            MonthCalendar calendar = sender as MonthCalendar;
            IWindowsFormsEditorService edSvc = calendar.Tag as IWindowsFormsEditorService;

            edSvc.CloseDropDown();
        }
    }
}
