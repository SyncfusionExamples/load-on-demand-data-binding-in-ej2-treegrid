using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LoadChildOnDemand.Models;
using System.Collections;
using Syncfusion.EJ2.Base;
using Syncfusion.EJ2.Linq;

namespace LoadChildOnDemand.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult DataSource([FromBody] DataManagerRequest dm)
    {
        List<TreeData> data = new List<TreeData>();
        data = TreeData.GetTree();
        DataOperations operation = new DataOperations();
        IEnumerable<TreeData> DataSource = data;
        if (dm.Expand != null && dm.Expand[0] == "CollapsingAction") // setting the ExpandStateMapping property whether is true or false
        {
            var val = TreeData.GetTree().Where(ds => ds.TaskID == int.Parse(dm.Expand[1])).FirstOrDefault();
            val.IsExpanded = false;
        }
        else if (dm.Expand != null && dm.Expand[0] == "ExpandingAction")
        {
            var val = TreeData.GetTree().Where(ds => ds.TaskID == int.Parse(dm.Expand[1])).FirstOrDefault();
            val.IsExpanded = true;
        }
        if (!(dm.Where != null && dm.Where.Count > 1))
        {
            data = data.Where(p => p.ParentValue == null).ToList();
        }
        DataSource = data;
        if (dm.Search != null && dm.Search.Count > 0) // Searching
        {
            DataSource = operation.PerformSearching(DataSource, dm.Search);
        }
        if (dm.Sorted != null && dm.Sorted.Count > 0 && dm.Sorted[0].Name != null) // Sorting
        {
            DataSource = operation.PerformSorting(DataSource, dm.Sorted);
        }
        if (dm.Where != null && dm.Where.Count > 1) //filtering
        {
            DataSource = operation.PerformFiltering(DataSource, dm.Where, "and");
        }
        data = new List<TreeData>();
        foreach (var rec in DataSource)
        {
            data.Add(rec as TreeData);
        }
        var GroupData = TreeData.GetTree().ToList().GroupBy(rec => rec.ParentValue)
                           .Where(g => g.Key != null).ToDictionary(g => g.Key?.ToString(), g => g.ToList());
        foreach (var Record in data.ToList())
        {
            if (GroupData.ContainsKey(Record.TaskID.ToString()))
            {
                var ChildGroup = GroupData[Record.TaskID.ToString()];
                if (dm.Sorted != null && dm.Sorted.Count > 0 && dm.Sorted[0].Name != null) // Sorting the child records
                {
                    IEnumerable ChildSort = ChildGroup;
                    ChildSort = operation.PerformSorting(ChildSort, dm.Sorted);
                    ChildGroup = new List<TreeData>();
                    foreach (var rec in ChildSort)
                    {
                        ChildGroup.Add(rec as TreeData);
                    }
                }
                if (dm.Search != null && dm.Search.Count > 0) // Searching the child records
                {
                    IEnumerable ChildSearch = ChildGroup;
                    ChildSearch = operation.PerformSearching(ChildSearch, dm.Search);
                    ChildGroup = new List<TreeData>();
                    foreach (var rec in ChildSearch)
                    {
                        ChildGroup.Add(rec as TreeData);
                    }
                }
                if (ChildGroup?.Count > 0)
                    AppendChildren(dm, ChildGroup, Record, GroupData, data);
            }
        }
        DataSource = data;
        if (dm.Expand != null && dm.Expand[0] == "CollapsingAction") // setting the skip index based on collapsed parent
        {
            string IdMapping = "TaskID";
            List<WhereFilter> CollapseFilter = new List<WhereFilter>();
            CollapseFilter.Add(new WhereFilter() { Field = IdMapping, value = dm.Where[0].value, Operator = dm.Where[0].Operator });
            var CollapsedParentRecord = operation.PerformFiltering(DataSource, CollapseFilter, "and");
            var index = data.Cast<object>().ToList().IndexOf(CollapsedParentRecord.Cast<object>().ToList()[0]);
            dm.Skip = index;
        }
        else if (dm.Expand != null && dm.Expand[0] == "ExpandingAction") // setting the skip index based on expanded parent
        {
            string IdMapping = "TaskID";
            List<WhereFilter> ExpandFilter = new List<WhereFilter>();
            ExpandFilter.Add(new WhereFilter() { Field = IdMapping, value = dm.Where[0].value, Operator = dm.Where[0].Operator });
            var ExpandedParentRecord = operation.PerformFiltering(DataSource, ExpandFilter, "and");
            var index = data.Cast<object>().ToList().IndexOf(ExpandedParentRecord.Cast<object>().ToList()[0]);
            dm.Skip = index;
        }
        int count = data.Count;
        DataSource = data;
        if (dm.Skip != 0)
        {
            DataSource = operation.PerformSkip(DataSource, dm.Skip);   //Paging
        }
        if (dm.Take != 0)
        {
            DataSource = operation.PerformTake(DataSource, dm.Take);
        }
        return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(DataSource);

    }

    private void AppendChildren(DataManagerRequest dm, List<TreeData> ChildRecords, TreeData ParentValue, Dictionary<string, List<TreeData>> GroupData, List<TreeData> data) // Getting child records for the respective parent
    {
        string TaskId = ParentValue.TaskID.ToString();
        var index = data.IndexOf(ParentValue);
        DataOperations operation = new DataOperations();
        foreach (var Child in ChildRecords)
        {
            if (ParentValue.IsExpanded)
            {
                string ParentId = Child.ParentValue.ToString();
                if (TaskId == ParentId)
                {
                    ((IList)data).Insert(++index, Child);
                    if (GroupData.ContainsKey(Child.TaskID.ToString()))
                    {
                        var DeepChildRecords = GroupData[Child.TaskID.ToString()];
                        if (DeepChildRecords?.Count > 0)
                        {
                            if (dm.Sorted != null && dm.Sorted.Count > 0 && dm.Sorted[0].Name != null) // sorting the child records
                            {
                                IEnumerable ChildSort = DeepChildRecords;
                                ChildSort = operation.PerformSorting(ChildSort, dm.Sorted);
                                DeepChildRecords = new List<TreeData>();
                                foreach (var rec in ChildSort)
                                {
                                    DeepChildRecords.Add(rec as TreeData);
                                }
                            }
                            if (dm.Search != null && dm.Search.Count > 0) // searching the child records
                            {
                                IEnumerable ChildSearch = DeepChildRecords;
                                ChildSearch = operation.PerformSearching(ChildSearch, dm.Search);
                                DeepChildRecords = new List<TreeData>();
                                foreach (var rec in ChildSearch)
                                {
                                    DeepChildRecords.Add(rec as TreeData);
                                }
                            }
                            AppendChildren(dm, DeepChildRecords, Child, GroupData, data);
                            if (Child.IsExpanded)
                            {
                                index += DeepChildRecords.Count;
                            }
                        }
                    }
                }
            }
        }

    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
