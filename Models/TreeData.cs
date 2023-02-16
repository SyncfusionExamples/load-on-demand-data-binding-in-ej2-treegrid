namespace LoadChildOnDemand.Models
{
    public class TreeData
    {
        public static List<TreeData> tree = new List<TreeData>();
        [System.ComponentModel.DataAnnotations.Key]
        public int TaskID { get; set; }
        public string TaskName { get; set; }

        public int Duration { get; set; }
        public int? ParentValue { get; set; }
        public bool? isParent { get; set; }

        public bool IsExpanded { get; set; }
        public TreeData() { }
        public static List<TreeData> GetTree()
        {
            if (tree.Count == 0)  // nested child sample
            {
                int root = -1;
                for (var t = 1; t <= 2500; t++)
                {
                    Random ran = new Random();
                    string math = (ran.Next() % 3) == 0 ? "High" : (ran.Next() % 2) == 0 ? "Release Breaker" : "Critical";
                    string progr = (ran.Next() % 3) == 0 ? "Started" : (ran.Next() % 2) == 0 ? "Open" : "In Progress";
                    root++;
                    int rootItem = tree.Count + root + 1;
                    tree.Add(new TreeData() { TaskID = rootItem, TaskName = "Parent task " + rootItem.ToString(), isParent = true, IsExpanded = true, ParentValue = null, Duration = ran.Next(1, 50) });
                    int parent = tree.Count;
                    for (var c = 0; c < 6; c++)
                    {
                        root++;
                        string val = ((parent + c + 1) % 3 == 0) ? "Low" : "Critical";
                        int parn = parent + c + 1;
                        progr = (ran.Next() % 3) == 0 ? "In Progress" : (ran.Next() % 2) == 0 ? "Open" : "Validated";
                        int iD = tree.Count + root + 1;
                        tree.Add(new TreeData() { TaskID = iD, TaskName = "Child task " + iD.ToString(), isParent = (((parent + c + 1) % 3) == 0), IsExpanded = true, ParentValue = rootItem, Duration = ran.Next(1, 50) });
                        if ((((parent + c + 1) % 3) == 0))
                        {
                            int immParent = tree.Count;
                            for (var s = 0; s < 2; s++)
                            {
                                root++;
                                string Prior = (immParent % 2 == 0) ? "Validated" : "Normal";
                                tree.Add(new TreeData() { TaskID = tree.Count + root + 1, TaskName = "Sub task " + (tree.Count + root + 1).ToString(), isParent = false, ParentValue = iD, Duration = ran.Next(1, 50) });
                            }
                        }
                    }
                }
            }
            return tree;
        }
    }

}
