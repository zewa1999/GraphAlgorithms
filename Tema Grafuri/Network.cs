using System;
using System.Collections.Generic;
using System.Xml;
using Tema_Grafuri.Dtos;
using Tema_Grafuri.ScheduleItems;

namespace Tema_Grafuri;

public class Network
{
    private int maxFlow = 0;
    private int[,] _capacityNetwork;
    private int[,] _residualNetwork;
    private int[,] _flowNetwork;
    private List<ClassroomDataDto> _classrooms { get; set; }
    private List<GroupDataDto> _groups { get; set; }
    private List<IntervalsDataDto> _intervals { get; set; }
    private List<TeachersDataDto> _scheduleData { get; set; }
    private Schedule _schedule { get; set; } = new();

    public Dictionary<int, (string, NodeType)> Nodes = new();

    public Network(List<ClassroomDataDto> classrooms, List<GroupDataDto> groups, List<IntervalsDataDto> intervals, List<TeachersDataDto> timetableData)
    {
        _classrooms = classrooms;
        _groups = groups;
        _intervals = intervals;
        _scheduleData = timetableData;

        CreateNodes(classrooms, groups, intervals, timetableData);
        _capacityNetwork = new int[Nodes.Count, Nodes.Count];
        _residualNetwork = new int[Nodes.Count, Nodes.Count];
        _flowNetwork = new int[Nodes.Count, Nodes.Count];

        CreateSourceToTeacherConnections(timetableData);
        CreateTeachersToSubjectConnections(timetableData);
        CreateSubjectToStudentGroupsConnections(timetableData);
        CreateStudentGroupsToClassroomsConnection(groups, classrooms);
        CreateClassroomToIntervalsConnections(classrooms, intervals);
        CreateIntervalsToTConnections(intervals, classrooms);
        _residualNetwork = (int[,])_capacityNetwork.Clone();

        //PrintResidualNetwork();
        //asta merge
        //AODS_MaxFlow();
        //_schedule.PrintSchedule();
        //PrintResidualNetwork();
        //_residualNetwork = (int[,])_capacityNetwork.Clone();
        FifoPreflow_MaxFlow();
        _schedule.PrintSchedule();

        Console.WriteLine("\n GATA!");
    }

    private int GetTotalNoOfHoursOfTeachers(List<TeachersDataDto> teachers) => teachers.Sum(x => int.Parse(x.Hours.Remove(x.Hours.Length - 1)));

    private int GetNodeId(string nodeName) => Nodes.Where(x => x.Value.Item1 == nodeName).FirstOrDefault().Key;

    private string GetNodeName(int nodeId) => Nodes.Where(x => x.Key == nodeId).FirstOrDefault().Value.Item1;

    private void CreateNodes(List<ClassroomDataDto> classrooms, List<GroupDataDto> groups, List<IntervalsDataDto> intervals, List<TeachersDataDto> timetableData)
    {
        Nodes.TryAdd(0, ("s", NodeType.s));

        Nodes.AddNodes(timetableData);
        Nodes.AddNodes(groups);
        Nodes.AddNodes(classrooms);
        Nodes.AddNodes(intervals);

        Nodes.TryAdd(Nodes.Count, ("t", NodeType.t));
    }

    private void PrintResidualNetwork()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            for (int j = 0; j < Nodes.Count; j++)
            {
                if (_residualNetwork[i, j] != 0)
                {
                    Console.WriteLine(Nodes[i].Item1 + " -(" + _residualNetwork[i, j] + ") ->" + Nodes[j].Item1);
                }
            }
        }
    }

    private void PrintFlowNetwork()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            for (int j = 0; j < Nodes.Count; j++)
            {
                if (_residualNetwork[i, j] != 0)
                {
                    Console.WriteLine(Nodes[i].Item1 + " -(" + _residualNetwork[i, j] + ") ->" + Nodes[j].Item1);
                }
            }
        }
    }

    #region CreateResidualMatrix

    private void CreateSourceToTeacherConnections(List<TeachersDataDto> timetableData)
    {
        var teacherData = timetableData.Select(x => x.Teacher);

        foreach (var teacher in teacherData)
        {
            var currentTeacherHours = timetableData.Where(x => x.Teacher == teacher)
                .Sum(x => int.Parse(x.Hours.Remove(x.Hours.Length - 1)));

            _capacityNetwork[GetNodeId("s"), GetNodeId(teacher)] = currentTeacherHours;
        }
    }

    private void CreateTeachersToSubjectConnections(List<TeachersDataDto> timetableData)
    {
        var teacherData = timetableData.Select(x => x.Teacher);

        foreach (var teacher in teacherData)
        {
            var currentTeacherData = timetableData.Where(x => x.Teacher == teacher);

            foreach (var currentTeacher in currentTeacherData)
            {
                _capacityNetwork[GetNodeId(teacher), GetNodeId(currentTeacher.Subject)] =
                    int.Parse(currentTeacher.Hours.Remove(currentTeacher.Hours.Length - 1));
            }
        }
    }

    private void CreateSubjectToStudentGroupsConnections(List<TeachersDataDto> timetableData)
    {
        foreach (var data in timetableData)
        {
            var teacherData = timetableData.Where(x => x.Teacher == data.Teacher);

            foreach (var teacherSubjectData in teacherData)
            {
                foreach (var studentGroup in teacherSubjectData.Groups)
                {
                    _capacityNetwork[GetNodeId(teacherSubjectData.Subject), GetNodeId(studentGroup)] = 2;
                }
            }
        }
    }

    private void CreateStudentGroupsToClassroomsConnection(List<GroupDataDto> studentGroups, List<ClassroomDataDto> classrooms)
    {
        foreach (var studentGroup in studentGroups)
        {
            foreach (var classroom in classrooms)
            {
                if (classroom.ClassroomCapacity >= studentGroup.GroupSize)
                {
                    _capacityNetwork[GetNodeId(studentGroup.GroupName), GetNodeId(classroom.ClassroomName)] = 60;
                }
            }
        }
    }

    private void CreateClassroomToIntervalsConnections(List<ClassroomDataDto> classrooms, List<IntervalsDataDto> intervals)
    {
        foreach (var classroom in classrooms)
        {
            foreach (var interval in intervals)
            {
                _capacityNetwork[GetNodeId(classroom.ClassroomName), GetNodeId(interval.Interval)] = 2;
            }
        }
    }

    private void CreateIntervalsToTConnections(List<IntervalsDataDto> intervals, List<ClassroomDataDto> classrooms)
    {
        foreach (var interval in intervals)
        {
            _capacityNetwork[GetNodeId(interval.Interval), GetNodeId("t")] = classrooms.Count * 2;
        }
    }

    #endregion CreateResidualMatrix

    #region Algorithms

    public void AODS_MaxFlow()
    {
        _schedule.ScheduleItems.Clear();

        Queue<int> q = new();
        int x = 0, y = 0;

        int[] d = Enumerable.Repeat(-1, Nodes.Count).ToArray();
        int[] p = Enumerable.Repeat(-1, Nodes.Count).ToArray();

        p[GetNodeId("s")] = GetNodeId("t");
        d[GetNodeId("t")] = 0;
        q.Enqueue(GetNodeId("t"));

        while (q.Count() > 0)
        {
            y = q.Dequeue();

            for (x = 0; x < Nodes.Count; x++)
            {
                if (_residualNetwork[x, y] > 0 && !q.Contains(x) && x != GetNodeId("s"))
                {
                    d[x] = d[y] + 1;
                    q.Enqueue(x);
                }
            }
        }

        x = GetNodeId("s");

        while (d[GetNodeId("s")] < Nodes.Count)
        {
            if (FindNextNode(d, p, x) != -1)
            {
                y = FindNextNode(d, p, x);

                Console.WriteLine(" (" + GetNodeName(x) + ", " + GetNodeName(y) + ") ");

                p[y] = x;
                x = y;
                if (x == GetNodeId("t"))
                {
                    IncreaseFlow(p);
                    x = GetNodeId("s");
                    p = Enumerable.Repeat(-1, Nodes.Count).ToArray();
                    p[GetNodeId("s")] = GetNodeId("t");
                }
            }
            else
            {
                List<int> D = new();
                for (y = 0; y < Nodes.Count; y++)
                {
                    if (_residualNetwork[x, y] > 0)
                    {
                        D.Add(d[y]);
                    }
                }
                if (D.Any())
                {
                    d[x] = D.Min() + 1;
                }
                else
                {
                    d[x] = Nodes.Count();
                }

                if (x != GetNodeId("s"))
                {
                    x = p[x];
                }
            }
        }
    }

    public void FifoPreflow_MaxFlow()
    {
        //_schedule.ScheduleItems.Clear();

        int x = 0, y = 0;

        int[] d = Enumerable.Repeat(-1, Nodes.Count).ToArray();
        int[] e = new int[Nodes.Count];

        Queue<int> q = new();

        d[GetNodeId("t")] = 0;
        q.Enqueue(GetNodeId("t"));

        while (q.Any())
        {
            y = q.Dequeue();

            for (x = 0; x < Nodes.Count; x++)
            {
                if (_residualNetwork[x, y] > 0 && d[x] == -1)
                {
                    d[x] = d[y] + 1;
                    q.Enqueue(x);
                }
            }
        }

        x = GetNodeId("s");

        e = CalculateExcess();

        Queue<int> C = new Queue<int>();

        //Console.Write(" --> ");

        for (y = 0; y < Nodes.Count; y++)
        {
            if (_residualNetwork[GetNodeId("s"), y] > 0)
            {
                _flowNetwork[GetNodeId("s"), y] = _capacityNetwork[GetNodeId("s"), y];
                e = CalculateExcess();

                if (y != GetNodeId("t"))
                {
                    C.Enqueue(y);
                    //Console.Write(GetNodeName(y) + " ");
                }
            }
        }

        //Console.WriteLine();

        d[GetNodeId("s")] = Nodes.Count;
        e = CalculateExcess();

        while (C.Any())
        {
            x = C.Dequeue();
            //Console.WriteLine(" <-- " + GetNodeName(x));

            while (e[x] > 0 && FindNextNode(d, x) != -1)
            {
                y = FindNextNode(d, x);

                int flow = e[x] >= _residualNetwork[x, y] ? _residualNetwork[x, y] : e[x];
                Console.WriteLine(" (" + GetNodeName(x) + ", " + GetNodeName(y) + ")");
                _flowNetwork[x, y] += flow;
                _residualNetwork[x, y] -= flow;
                _residualNetwork[y, x] += flow;

                if (!C.Contains(y) && y != GetNodeId("s") && y != GetNodeId("t"))
                {
                    C.Enqueue(y);
                    //Console.WriteLine(" --> " + GetNodeName(y));
                }

                e = CalculateExcess();
            }

            if (e[x] > 0)
            {
                List<int> D = new List<int>();

                for (y = 0; y < Nodes.Count; y++)
                {
                    if (_residualNetwork[x, y] > 0)
                    {
                        D.Add(d[y]);
                    }
                }

                if (D.Any())
                {
                    d[x] = D.Min() + 1;
                }

                C.Enqueue(x);
                //Console.WriteLine(" --> " + GetNodeName(x));
            }
        }

        e = CalculateExcess();

        maxFlow = e[GetNodeId("t")];

        Console.WriteLine(maxFlow);

        //PrintFlowNetwork();

        CreateSchedule();
    }

    private void CreateSchedule()
    {
        Console.WriteLine("CreateSchedule()\n");

        Queue<int> V = new Queue<int>();
        int[] p = Enumerable.Repeat(-1, Nodes.Count).ToArray();

        do
        {
            p = Enumerable.Repeat(-1, Nodes.Count).ToArray();

            V.Clear();
            V.Enqueue(GetNodeId("s"));
            p[GetNodeId("s")] = GetNodeId("t");

            while (V.Count > 0 && p[GetNodeId("t")] == -1)
            {
                int x = V.Dequeue();

                if (FindNextNode(_flowNetwork, p, x) != -1)
                {
                    int y = FindNextNode(_flowNetwork, p, x);
                    if (p[y] == -1)
                    {
                        p[y] = x;
                        V.Enqueue(y);
                    }
                }
            }
            if (p[GetNodeId("t")] != -1)
            {
                List<(int, int)> road = new();
                int y = GetNodeId("t");
                int x = p[y];

                ScheduleItem scheduleItem = new();

                while (x != GetNodeId("t"))
                {
                    road.Add((x, y));

                    switch (Nodes[x].Item2)
                    {
                        case NodeType.Teacher:
                            {
                                scheduleItem.Teacher = GetNodeName(x);
                                break;
                            }
                        case NodeType.Subject:
                            {
                                scheduleItem.Subject = GetNodeName(x);
                                break;
                            }
                        case NodeType.Group:
                            {
                                scheduleItem.Group = GetNodeName(x);
                                break;
                            }
                        case NodeType.Classroom:
                            {
                                scheduleItem.Classroom = GetNodeName(x);
                                break;
                            }
                        case NodeType.Interval:
                            {
                                scheduleItem.Interval = GetNodeName(x);
                                break;
                            }
                    }

                    y = x;
                    x = p[y];
                }

                var subgroups = _groups.Where(x => x.GroupName == scheduleItem.Group).FirstOrDefault()?.GroupComponents;

                if (subgroups != null)
                {
                    foreach (var subgroup in subgroups)
                    {
                        ScheduleItem item = new();
                        item.Teacher = scheduleItem.Teacher;
                        item.Subject = scheduleItem.Subject;
                        item.Group = subgroup;
                        item.Classroom = scheduleItem.Classroom;
                        item.Interval = scheduleItem.Interval;
                        _schedule.ScheduleItems.Add(item);
                    }

                    while (road.Any())
                    {
                        (int x, int y) arrow = road.FirstOrDefault();
                        Console.WriteLine(" (" + GetNodeName(arrow.x) + ", " + GetNodeName(arrow.y) + ")");
                        road.Remove(road.FirstOrDefault());
                        _flowNetwork[arrow.x, arrow.y] -= 2;
                    }
                    Console.WriteLine();
                }
            }
        } while (p[GetNodeId("t")] != -1);
    }

    private int[] CalculateExcess()
    {
        int[] e = new int[Nodes.Count];

        for (int x = 0; x < Nodes.Count; x++)
        {
            for (int N = 0; N < Nodes.Count; N++)
            {
                if (_flowNetwork[N, x] > 0)
                {
                    e[x] = e[x] + _flowNetwork[N, x];
                }

                if (_flowNetwork[x, N] > 0)
                {
                    e[x] = e[x] - _flowNetwork[x, N];
                }
            }
        }

        return e;
    }

    private void IncreaseFlow(int[] p)
    {
        int x, y, r;
        List<(int, int)> road = new();
        ScheduleItem scheduleItem = new();

        y = GetNodeId("t");
        x = p[y];
        r = 0;

        while (x != GetNodeId("t"))
        {
            road.Add((x, y));

            if (x != GetNodeId("s"))
                Console.Write(GetNodeName(x) + " ");

            if (r == 0 || _residualNetwork[x, y] < r)
            {
                r = _residualNetwork[x, y];
            }

            switch (Nodes[x].Item2)
            {
                case NodeType.Teacher:
                    {
                        scheduleItem.Teacher = GetNodeName(x);
                        break;
                    }
                case NodeType.Subject:
                    {
                        scheduleItem.Subject = GetNodeName(x);
                        break;
                    }
                case NodeType.Group:
                    {
                        scheduleItem.Group = GetNodeName(x);
                        break;
                    }
                case NodeType.Classroom:
                    {
                        scheduleItem.Classroom = GetNodeName(x);
                        break;
                    }
                case NodeType.Interval:
                    {
                        scheduleItem.Interval = GetNodeName(x);
                        break;
                    }
            }

            y = x;
            x = p[y];
        }
        var subgroups = _groups.Where(x => x.GroupName == scheduleItem.Group).FirstOrDefault()?.GroupComponents;

        if (subgroups != null)
        {
            foreach (var subgroup in subgroups)
            {
                ScheduleItem item = new();
                item.Teacher = scheduleItem.Teacher;
                item.Subject = scheduleItem.Subject;
                item.Group = subgroup;
                item.Classroom = scheduleItem.Classroom;
                item.Interval = scheduleItem.Interval;
                _schedule.ScheduleItems.Add(item);
            }
        }
        Console.WriteLine();

        while (road.Any())
        {
            var arrow = road.FirstOrDefault();
            road.Remove(road.FirstOrDefault());

            _residualNetwork[arrow.Item1, arrow.Item2] -= r;
            _residualNetwork[arrow.Item2, arrow.Item1] += r;
        }

        //PrintResidualNetwork();

        maxFlow += r;

        //Console.WriteLine(maxFlow);
    }

    private int FindNextNode(int[] distances, int[] p, int x)
    {
        var groupInfos = Nodes.Where(x => x.Value.Item2 == NodeType.Group).ToDictionary(x => x.Key, x => x.Value);
        var classroomInfos = Nodes.Where(x => x.Value.Item2 == NodeType.Classroom).ToDictionary(x => x.Key, x => x.Value);

        for (int y = 0; y < Nodes.Count; y++)
        {
            if (_residualNetwork[x, y] > 0 && distances[x] == distances[y] + 1)
            {
                switch (Nodes[x].Item2)
                {
                    case NodeType.Group:
                        if (Nodes[y].Item2 == NodeType.Classroom)
                        {
                            if (GetNodeName(p[x]).Contains("Laborator"))
                            {
                                var classroom = _classrooms.Where(classroom => classroom.ClassroomName == GetNodeName(y)).FirstOrDefault();
                                if (classroom != null)
                                {
                                    if (classroom.ClassroomType.Contains("Lab"))
                                    {
                                        return y;
                                    }
                                }
                            }
                            else
                            {
                                return y;
                            }
                        }
                        break;

                    case NodeType.Classroom:
                        if (Nodes[y].Item2 == NodeType.Interval)
                        {
                            var subgroups = _groups.Where(group => group.GroupName == GetNodeName(p[x])).FirstOrDefault();

                            if (subgroups != null)
                            {
                                if (_schedule.IsItemValid(subgroups.GroupComponents, GetNodeName(p[p[p[x]]]), GetNodeName(x), GetNodeName(y)))
                                {
                                    return y;
                                }
                            }
                        }
                        break;

                    default:
                        return y;
                }

                return y;
            }
        }
        return -1;
    }

    private int FindNextNode(int[,] matrix, int[] p, int x)
    {
        var groupInfos = Nodes.Where(x => x.Value.Item2 == NodeType.Group).ToDictionary(x => x.Key, x => x.Value);
        var classroomInfos = Nodes.Where(x => x.Value.Item2 == NodeType.Classroom).ToDictionary(x => x.Key, x => x.Value);

        for (int y = 0; y < Nodes.Count; y++)
        {
            if (matrix[x, y] > 0)
            {
                switch (Nodes[x].Item2)
                {
                    case NodeType.Group:
                        if (Nodes[y].Item2 == NodeType.Classroom)
                        {
                            if (GetNodeName(p[x]).Contains("Laborator"))
                            {
                                var classroom = _classrooms.Where(classroom => classroom.ClassroomName == GetNodeName(y)).FirstOrDefault();
                                if (classroom != null)
                                {
                                    if (classroom.ClassroomType.Contains("Lab"))
                                    {
                                        return y;
                                    }
                                }
                            }
                            else
                            {
                                return y;
                            }
                        }
                        break;

                    case NodeType.Classroom:
                        if (Nodes[y].Item2 == NodeType.Interval)
                        {
                            var subgroups = _groups.Where(group => group.GroupName == GetNodeName(p[x])).FirstOrDefault();

                            if (subgroups != null)
                            {
                                if (_schedule.IsItemValid(subgroups.GroupComponents, GetNodeName(p[p[p[x]]]), GetNodeName(x), GetNodeName(y)))
                                {
                                    return y;
                                }
                            }
                        }
                        break;

                    default:
                        return y;
                }
            }
        }
        return -1;
    }

    private int FindNextNode(int[] distances, int x)
    {
        var groupInfos = Nodes.Where(x => x.Value.Item2 == NodeType.Group).ToDictionary(x => x.Key, x => x.Value);
        var classroomInfos = Nodes.Where(x => x.Value.Item2 == NodeType.Classroom).ToDictionary(x => x.Key, x => x.Value);

        for (int y = 0; y < Nodes.Count; y++)
        {
            if (_residualNetwork[x, y] > 0 && distances[x] == distances[y] + 1)
            {
                return y;
            }
        }
        return -1;
    }

    #endregion Algorithms
}