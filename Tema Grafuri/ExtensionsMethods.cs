using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tema_Grafuri.Dtos;

namespace Tema_Grafuri
{
    public static class ExtensionsMethods
    {
        public static void AddNodes(this Dictionary<int, (string, NodeType)> dictionary, IEnumerable<ClassroomDataDto> enumerableList)
        {
            SortedSet<string> enumerableListSet = new(enumerableList.Select(x => x.ClassroomName));
            foreach (var item in enumerableList)
            {
                int key = dictionary.Count;
                dictionary.Add(key, (item.ClassroomName, NodeType.Classroom));
            }
        }

        public static void AddNodes(this Dictionary<int, (string, NodeType)> dictionary, IEnumerable<GroupDataDto> enumerableList)
        {
            SortedSet<string> enumerableListSet = new(enumerableList.Select(x => x.GroupName));

            foreach (var item in enumerableListSet)
            {
                int key = dictionary.Count;
                dictionary.Add(key, (item, NodeType.Group));
            }
        }

        public static void AddNodes(this Dictionary<int, (string, NodeType)> dictionary, IEnumerable<IntervalsDataDto> enumerableList)
        {
            SortedSet<string> enumerableListSet = new(enumerableList.Select(x => x.Interval));

            foreach (var item in enumerableListSet)
            {
                int key = dictionary.Count;
                dictionary.Add(key, (item, NodeType.Interval));
            }
        }

        public static void AddNodes(this Dictionary<int, (string, NodeType)> dictionary, IEnumerable<TeachersDataDto> enumerableList)
        {
            SortedSet<string> enumerableTeachersListSet = new(enumerableList.Select(x => x.Teacher));
            SortedSet<string> enumerableSubjectsListSet = new(enumerableList.Select(x => x.Subject));

            foreach (var item in enumerableTeachersListSet)
            {
                int key = dictionary.Count;
                dictionary.Add(key, (item, NodeType.Teacher));
            }

            foreach (var item in enumerableSubjectsListSet)
            {
                int key = dictionary.Count;
                dictionary.Add(key, (item, NodeType.Subject));
            }
        }
    }
}