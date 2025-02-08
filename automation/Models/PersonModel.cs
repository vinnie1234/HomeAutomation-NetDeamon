using System.Reactive.Disposables;

public class PersonModel
{
    private readonly IEntities _entities;
    private readonly IDisposable _subscriptions;

    public bool IsSleeping { get; private set; }
    public bool IsDriving { get; private set; }
    public bool IsHome { get; private set; }
    public string? DirectionOfTravel { get; private set; }
    public string? State { get; private set; }

    public PersonModel(IEntities entities)
    {
        _entities = entities;

        _subscriptions = new CompositeDisposable(
            _entities.InputBoolean.Sleeping.StateChanges().Subscribe(x => IsSleeping = x.New.IsOn()),
            _entities.BinarySensor.VincentPhoneAndroidAuto.StateChanges().Subscribe(x => IsDriving = x.New.IsOn()),
            _entities.InputBoolean.Away.StateChanges().Subscribe(x => IsHome = x.New.IsOff()),
            _entities.Sensor.ThuisSmS938bDirectionOfTravel.StateChanges().Subscribe(x =>
            {
                if (x.New?.State != null) DirectionOfTravel = x.New.State;
            }),
            _entities.Person.VincentMaarschalkerweerd.StateChanges().Subscribe(x => State = x.New?.State)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}