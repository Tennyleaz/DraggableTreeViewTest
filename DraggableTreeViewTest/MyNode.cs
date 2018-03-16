using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace DraggableTreeViewTest
{
    public class MyNode : INotifyPropertyChanged, IEquatable<MyNode>
    {
        public MyNode(MyNode Parent, string guid = null, string name = null)
        {
            this.Members = new ObservableCollection<MyNode>();
            if (guid != null)
                this.ID = guid;
            else
                this.ID = Guid.NewGuid().ToString();

            if (name != null)
                Name = name;
            allChilrenID = new HashSet<string>();
            IsExpand = false;
            allChilrenID.Add(this.ID);
            _parent = Parent;
        }

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string ID { get; set; }
        public string ParentID { get; set; }
        public MyNode _parent;
        public bool IsExpand
        {
            get { return _isExpand; }
            set
            {
                _isExpand = value;
                OnPropertyChanged("IsExpand");
            }
        }
        public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
                _isPinned = value;
                OnPropertyChanged("IsPinned");
            }
        }

        private HashSet<string> allChilrenID;
        //private int position = 0;
        private string _name;
        private bool _isExpand;
        private bool _isPinned;

        public ObservableCollection<MyNode> Members { get; set; }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool Equals(MyNode other)
        {
            if (other == null)
                return false;
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as MyNode);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public void AddMember(MyNode childNode)
        {
            Members.Add(childNode);
            childNode.ParentID = ID;
            childNode._parent = this;
            foreach (string guid in childNode.allChilrenID)
            {
                allChilrenID.Add(guid);
            }
        }

        public bool MoveUp(string parentID, MyNode child)
        {
            if (ID == parentID)
            {
                // 移動自己的直屬child
                return MoveChildUp(child);
            }
            else if (!allChilrenID.Contains(parentID))
                return false;
            else
            {
                // 叫自己的Child想辦法移動
                for (int i = 0; i < Members.Count; i++)
                {
                    // 尋找包含id的child
                    if (Members[i].ContainsNode(parentID))
                    {
                        return Members[i].MoveUp(parentID, child);
                    }
                }
            }
            return false;
        }

        public bool MoveDown(string parentID, MyNode child)
        {
            if (ID == parentID)
            {
                // 移動自己的直屬child
                return MoveChildDown(child);
            }
            else if (!allChilrenID.Contains(parentID))
                return false;
            else
            {
                // 叫自己的Child想辦法移動
                for (int i = 0; i < Members.Count; i++)
                {
                    // 尋找包含id的child
                    if (Members[i].ContainsNode(parentID))
                    {
                        return Members[i].MoveDown(parentID, child);
                    }
                }
            }
            return false;
        }

        public MyNode GetNode(string guid)
        {
            if (ID == guid)
            {
                return this;
            }
            else if (!allChilrenID.Contains(guid))
                return null;
            else
            {
                // 如果HashSet有，再從所有Child裡面找
                foreach (MyNode child in Members)
                {
                    // 尋找包含id的child
                    if (child.ContainsNode(guid))
                    {
                        return child.GetNode(guid);
                    }
                }
            }
            return null;
        }

        private bool MoveChildUp(MyNode child)
        {
            if (ID == child.ID)
                return false; // 不能動自己
            else if (!Members.Contains(child))
                return false;
            else
            {
                int index = Members.IndexOf(child);
                if (index - 1 >= 0)
                {
                    Members.Move(index, index - 1);
                    return true;
                }
            }
            return false;
        }

        private bool MoveChildDown(MyNode child)
        {
            if (ID == child.ID)
                return false; // 不能動自己
            else if (!Members.Contains(child))
                return false;
            else
            {
                int index = Members.IndexOf(child);
                if (index + 1 < Members.Count)
                {
                    Members.Move(index, index + 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 所有分支內children中是否包含id，利用HashSet快速搜尋
        /// </summary>
        public bool ContainsNode(string id)
        {
            if (ID == id)
                return true;
            else
                return allChilrenID.Contains(id);
        }

        public bool AddNodeAt(string guid, MyNode newNode)
        {
            if (ID == guid)
            {
                // 加在自己下面
                AddMember(newNode);
                return true;
            }
            else if (!allChilrenID.Contains(guid))
                return false;
            else
            {
                // 加在自己的Child內
                foreach (MyNode child in Members)
                {
                    // 尋找包含id的child
                    if (child.ContainsNode(guid))
                    {
                        if (child.AddNodeAt(guid, newNode))
                        {
                            allChilrenID.Add(newNode.ID); // 加成功了再更新ID
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool DeleteChild(string guid)
        {
            if (ID == guid)
            {
                return false;  // 不能刪除自己
            }
            else if (!allChilrenID.Contains(guid))
                return false;
            else
            {
                // 從自己的child刪除
                for (int i = Members.Count - 1; i >= 0; i--)
                {
                    // 直接刪除某個child
                    if (Members[i].ID == guid)
                    {
                        if (Members[i].Members.Count > 0)  // 仍然有子類別就不能刪除
                            return false;
                        else
                        {
                            Members.RemoveAt(i);
                            return allChilrenID.Remove(guid);
                        }
                    }

                    // 尋找包含id的child
                    if (Members[i].ContainsNode(guid))
                    {
                        if (Members[i].DeleteChild(guid))
                            return allChilrenID.Remove(guid); // 加成功了再更新ID
                    }
                }
            }
            return false;
        }

        public bool DeleteChildBranch(MyNode child)
        {
            if (ID == child.ID)
            {
                return false;  // 不能刪除自己
            }
            else if (!allChilrenID.Contains(child.ID))
                return false;
            else
            {
                try
                {
                    // 從自己的child刪除
                    for (int i = Members.Count - 1; i >= 0; i--)
                    {
                        // 直接刪除某個child
                        if (Members[i].ID == child.ID)
                        {
                            foreach (string id in child.allChilrenID)
                                allChilrenID.Remove(id);  // 移除所有child ID
                            Members.RemoveAt(i);
                            return true;
                        }

                        // 尋找包含id的child
                        if (Members[i].ContainsNode(child.ID))
                        {
                            if (Members[i].DeleteChildBranch(child))
                            {
                                foreach (string id in child.allChilrenID)
                                    allChilrenID.Remove(id);  // 移除所有child ID
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return false;
        }

        public bool IsDirectChild(string id)
        {
            foreach (var child in Members)
            {
                if (child.ID == id)
                    return true;
            }
            return false;
        }

        public bool RenameNode(string guid, string newName)
        {
            if (ID == guid)
            {
                // 重新命名自己
                this.Name = newName;
                return true;
            }
            else if (!allChilrenID.Contains(guid))
                return false;
            else
            {
                // 重新命名自己的Child
                foreach (MyNode child in Members)
                {
                    // 尋找包含id的child
                    if (child.ContainsNode(guid))
                    {
                        return child.RenameNode(guid, newName);
                    }
                }
            }
            return false;
        }

        public void CollapseOldNode(string oldGuid, string newGuid)
        {
            if (allChilrenID.Contains(oldGuid))
            {
                // 不含newGuid，直接關閉整條Branch
                if (IsExpand)
                {
                    if (oldGuid != ID)
                    {
                        // 含oldGuid，且是自己的child，要關掉它
                        foreach (MyNode child in Members)
                        {
                            // 尋找包含id的child
                            if (child.ContainsNode(oldGuid))
                            {
                                child.CollapseOldNode(oldGuid, newGuid);
                                break;
                            }
                        }
                    }

                    if (allChilrenID.Contains(newGuid))
                    {
                        // 含newGuid，則不關閉自己
                    }
                    else
                    {
                        IsExpand = false;  // 關閉自己
                    }
                }
            }
        }

        public int GetSubTreeHeight()
        {
            if (allChilrenID == null || allChilrenID.Count == 0)
                return 1;

            // 遞迴問每個child的高度
            int tallestChild = 0;
            foreach (MyNode child in Members)
            {
                int childHeight = child.GetSubTreeHeight();
                if (childHeight > tallestChild)
                    tallestChild = childHeight;
            }
            
            // 回傳最高的child+1
            return tallestChild + 1;
        }

        /// <summary>
        /// root node沒有parent，高度會是零
        /// </summary>
        /// <returns></returns>
        public int GetDepth()
        {
            if (_parent == null)
                return 0;
            // 遞迴問parent的深度
            return _parent.GetDepth() + 1;
        }
    }
}
