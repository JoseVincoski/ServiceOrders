using ServiceOrders.Api.Domain.ServiceOrders.ServiceOrderItems;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Domain.ServiceOrders;

public sealed class ServiceOrder
{
    public const int MaxRequestorFailureDescriptionLength = 1000;
    public const int MaxWorkerFixDescriptionLength = 1000;
    private readonly List<ServiceOrderItem> _items = [];

    private ServiceOrder() { }

    private ServiceOrder(
        Guid id,
        Guid equipmentId,
        Guid requestorUserId,
        string requestorFailureDescription)
    {
        Id = id;
        EquipmentId = equipmentId;
        RequestorUserId = requestorUserId;
        RequestorFailureDescription = requestorFailureDescription;
        Status = ServiceOrderStatus.Opened;
        OpeningTime = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public DateTime OpeningTime { get; private set; }
    public DateTime? ClosureTime { get; private set; }
    public Guid EquipmentId { get; private set; }
    public Guid RequestorUserId { get; private set; }
    public Guid? WorkerUserId { get; private set; }
    public string RequestorFailureDescription { get; private set; } = string.Empty;
    public string WorkerFixDescription { get; private set; } = string.Empty;

    public IReadOnlyCollection<ServiceOrderItem> Items => _items.AsReadOnly();

    public static Result<ServiceOrder> Create(Guid equipmentId, Guid requestorUserId, string failureDescription)
    {
        if (equipmentId == Guid.Empty)
        {
            return Result.Failure<ServiceOrder>(Error.Problem("ServiceOrder.EmptyEquipment", "Equipment is required."));
        }

        if (requestorUserId == Guid.Empty)
        {
            return Result.Failure<ServiceOrder>(Error.Problem("ServiceOrder.EmptyRequestor", "Requestor user is required."));
        }

        if (string.IsNullOrWhiteSpace(failureDescription))
        {
            return Result.Failure<ServiceOrder>(Error.Problem("ServiceOrder.EmptyDescription", "Failure description cannot be empty."));
        }

        return new ServiceOrder(Guid.NewGuid(), equipmentId, requestorUserId, failureDescription.Trim());
    }

    public Result AssignWorker(Guid workerUserId)
    {
        if (Status != ServiceOrderStatus.Opened)
        {
            return Result.Failure(Error.Problem("ServiceOrder.InvalidState", "Order must be in Opened state to be assigned."));
        }

        WorkerUserId = workerUserId;
        Status = ServiceOrderStatus.InProgress;

        return Result.Success();
    }

    public Result AddUsedItem(Guid itemId, int quantity)
    {
        if (Status != ServiceOrderStatus.InProgress)
        {
            return Result.Failure(Error.Problem("ServiceOrder.InvalidState", "Can only add items while the order is In Progress."));
        }

        if (quantity <= 0)
        {
            return Result.Failure(Error.Problem("ServiceOrder.InvalidQuantity", "Quantity must be greater than zero."));
        }

        var existingItem = _items.FirstOrDefault(i => i.ItemId == itemId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new ServiceOrderItem(Guid.NewGuid(), Id, itemId, quantity));
        }

        return Result.Success();
    }

    public Result Complete(string fixDescription)
    {
        if (Status != ServiceOrderStatus.InProgress)
        {
            return Result.Failure(Error.Problem("ServiceOrder.InvalidState", "Order must be In Progress to be completed."));
        }

        if (string.IsNullOrWhiteSpace(fixDescription))
        {
            return Result.Failure(Error.Problem("ServiceOrder.EmptyFixDescription", "Fix description cannot be empty."));
        }

        WorkerFixDescription = fixDescription.Trim();
        Status = ServiceOrderStatus.Completed;
        ClosureTime = DateTime.UtcNow;

        return Result.Success();
    }
}