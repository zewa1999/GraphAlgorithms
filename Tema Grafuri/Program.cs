using AutoMapper;
using Tema_Grafuri;
using Tema_Grafuri.Dtos;

var c = JsonDeserializer.ToJson<ClassroomRoot>("ClassroomData.json");
var g = JsonDeserializer.ToJson<GroupDataRoot>("GroupsData.json");
var i = JsonDeserializer.ToJson<IntervalsRoot>("IntervalsData.json");
var t = JsonDeserializer.ToJson<TeachersDataRoot>("TeachersData.json");

var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<ClassroomData, ClassroomDataDto>();
    cfg.CreateMap<IntervalsData, IntervalsDataDto>();
    cfg.CreateMap<GroupData, GroupDataDto>().ConvertUsing<GroupData>();
    cfg.CreateMap<TeachersData, TeachersDataDto>().ConvertUsing<TeachersData>();
});

config.AssertConfigurationIsValid();
var mapper = new Mapper(config);

var groups = mapper.Map<List<GroupData>, List<GroupDataDto>>(g.Groups);
var classrooms = mapper.Map<List<ClassroomData>, List<ClassroomDataDto>>(c.Classrooms);
var intervals = mapper.Map<List<IntervalsData>, List<IntervalsDataDto>>(i.Intervals);
var teachers = mapper.Map<List<TeachersData>, List<TeachersDataDto>>(t.Teachers);

Console.WriteLine();

var network = new Network(classrooms, groups, intervals, teachers);