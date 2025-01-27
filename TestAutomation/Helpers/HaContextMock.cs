using System.Reactive.Linq;
using System.Reactive.Subjects;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace TestAutomation.Helpers;

public class HaContextMock
{
    public HaContextMock()
    {
        HaContext = Substitute.For<IHaContext>();
        HaContext.StateAllChanges().Returns(
            StateChangeSubject
        );
        HaContext.StateChanges().Returns(
            StateChangeSubject.Where(n => n.New?.State != n.Old?.State)
        );
    }

    public IHaContext HaContext { get; init; }
    public Subject<StateChange> StateChangeSubject { get; } = new();
}
