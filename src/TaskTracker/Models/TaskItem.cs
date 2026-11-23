
namespace TaskTracker.Models;

public class TaskItem
{
    public int Id {get;set;}
    public string Description {get;set;} = "";
    public int Status {get;set;}= 1;
    public DateTime CreatedAt{get;set;} = DateTime.Now;
    public DateTime? UpdatedAt {get;set;}

}