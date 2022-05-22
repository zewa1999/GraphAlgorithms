using System.ComponentModel.DataAnnotations;
using Tema_Grafuri.Dtos;

namespace Tema_Grafuri.ScheduleItems;

public class Schedule
{
    public List<ScheduleItem> ScheduleItems { get; set; } = new();

    public bool IsItemValid(List<string> groups, string teacher, string classroom, string interval)
    {
        // students are free
        foreach (string subgroup in groups)
        {
            List<ScheduleItem> subgroupTimetable
                = ScheduleItems.Where((item) => item.Group == subgroup).ToList();

            if (subgroupTimetable.Where((item) => item.Interval == interval).ToList().Count > 0)
            {
                return false;
            }
        }

        // teacher is free

        List<ScheduleItem> teacherSchedule
            = ScheduleItems.Where((item) => item.Teacher == teacher).ToList();

        if (teacherSchedule.Where((item) => item.Interval == interval).ToList().Count > 0)
        {
            return false;
        }

        // classroom is free

        List<ScheduleItem> classroomTimetable
                = ScheduleItems.Where((item) => item.Classroom == classroom).ToList();

        if (classroomTimetable.Where((item) => item.Interval == interval).ToList().Count > 0)
        {
            return false;
        }

        return true;
    }

    public void PrintSchedule()
    {
        Console.WriteLine();

        foreach (ScheduleItem item in ScheduleItems)
        {
            Console.WriteLine(" " + item.Interval + " " + item.Classroom + " " + item.Group + " " + item.Subject + " " + item.Teacher);
        }
    }
}