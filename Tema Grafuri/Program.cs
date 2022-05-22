using AutoMapper;
using Tema_Grafuri;
using Tema_Grafuri.Dtos;

var mata = JsonDeserializer.ToJson<ClassroomRoot>("ClassroomData.json");
var mata1 = JsonDeserializer.ToJson<GroupDataRoot>("GroupsData.json");
var mata2 = JsonDeserializer.ToJson<IntervalsRoot>("IntervalsData.json");
var mata3 = JsonDeserializer.ToJson<TeachersDataRoot>("TeachersData.json");

var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<ClassroomData, ClassroomDataDto>();
    cfg.CreateMap<IntervalsData, IntervalsDataDto>();
    cfg.CreateMap<GroupData, GroupDataDto>().ConvertUsing<GroupData>();
    cfg.CreateMap<TeachersData, TeachersDataDto>().ConvertUsing<TeachersData>();
});

config.AssertConfigurationIsValid();
var mapper = new Mapper(config);

var groups = mapper.Map<List<GroupData>, List<GroupDataDto>>(mata1.Groups);
var classrooms = mapper.Map<List<ClassroomData>, List<ClassroomDataDto>>(mata.Classrooms);
var intervals = mapper.Map<List<IntervalsData>, List<IntervalsDataDto>>(mata2.Intervals);
var teachers = mapper.Map<List<TeachersData>, List<TeachersDataDto>>(mata3.Teachers);

Console.WriteLine();

var network = new Network(classrooms, groups, intervals, teachers);