namespace restapi.Models
{
    public abstract class Event
    {
        public int Resource { get; set; }
    }

    public class Entered : Event
    {
    }

    public class Submittal : Event
    {
    }

    public class Cancellation : Event
    {
        public string Reason { get; set; }        
    }

    public class Rejection : Event
    {
        public string Reason { get; set; }        
    }

    public class Approval : Event
    {
    }

    /// <summary>
    /// Event to delete the time card
    /// </summary>
    public class Deletion : Event
    {
        public string Reason { get; set; }
    }

    /// <summary>
    /// Event to show the transition of added timecard line to a timecard
    /// </summary>
    public class Addition: Event
    {
        public string Reason { get; set; }
    }

    /// <summary>
    /// Event to show the transition of updated timecard line to a timecard
    /// </summary>
    public class Update: Event
    {
        public string Reason { get; set; }
    }
}